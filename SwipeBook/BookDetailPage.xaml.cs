using SwipeBook.Models;
using SwipeBook.Services;

namespace SwipeBook;

[QueryProperty(nameof(BookIdText), "id")]
public partial class BookDetailPage : ContentPage
{
    private Book? _book;

    public string BookIdText
    {
        set
        {
            if (!int.TryParse(value, out var id)) return;
            _book = BookRepository.GetBooks().FirstOrDefault(b => b.Id == id);
            Display();
        }
    }

    public BookDetailPage()
    {
        InitializeComponent();
    }

    private void Display()
    {
        if (_book is null) return;
        Title = _book.Title;
        CoverImage.Source = _book.Cover;
        TitleLabel.Text = _book.Title;
        AuthorLabel.Text = _book.Author;
        GenreLabel.Text = _book.Genre;
        DescriptionLabel.Text = _book.Description;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (_book is null) return;

        var isLiked = SwipeStorage.LoadLiked().Contains(_book.Id);
        StatusLabel.Text = isLiked ? "♥ У вподобаних" : "Ще не у вподобаних";
        ToggleButton.Text = isLiked ? "Прибрати з вподобаних" : "Додати до вподобаних";

        var status = SwipeStorage.GetStatus(_book.Id);
        StyleButton(WantButton, status == ReadingStatus.WantToRead);
        StyleButton(ReadingButton, status == ReadingStatus.Reading);
        StyleButton(DoneButton, status == ReadingStatus.Done);
    }

    private static void StyleButton(Button button, bool active)
    {
        button.BackgroundColor = active ? Color.FromArgb("#B5651D") : Colors.Transparent;
        button.TextColor = active ? Colors.White : Color.FromArgb("#B5651D");
    }

    // Повторне натискання на активний статус знімає його
    private void SetStatus(ReadingStatus status)
    {
        if (_book is null) return;
        var current = SwipeStorage.GetStatus(_book.Id);
        SwipeStorage.SetStatus(_book.Id, current == status ? ReadingStatus.None : status);
        UpdateStatus();
    }

    private void OnWantClicked(object? sender, EventArgs e) => SetStatus(ReadingStatus.WantToRead);
    private void OnReadingClicked(object? sender, EventArgs e) => SetStatus(ReadingStatus.Reading);
    private void OnDoneClicked(object? sender, EventArgs e) => SetStatus(ReadingStatus.Done);

    private void OnToggleClicked(object? sender, EventArgs e)
    {
        if (_book is null) return;
        var liked = SwipeStorage.LoadLiked();
        var disliked = SwipeStorage.LoadDisliked();

        if (liked.Contains(_book.Id)) liked.Remove(_book.Id);
        else { liked.Add(_book.Id); disliked.Remove(_book.Id); }

        SwipeStorage.Save(liked, disliked);
        UpdateStatus();
    }

    // Додати книжку на полицю (або зняти, якщо вона там уже є)
    private async void OnShelfClicked(object? sender, EventArgs e)
    {
        if (_book is null) return;

        var shelves = ShelfStorage.Load();
        var options = shelves
            .Select(s => (s.BookIds.Contains(_book.Id) ? "✓ " : "") + s.Name)
            .ToList();
        const string NewShelf = "＋ Нова полиця";
        options.Add(NewShelf);

        var choice = await DisplayActionSheetAsync("Полиця (✓ означає, що книжка вже там)",
            "Скасувати", null, options.ToArray());
        if (choice is null || choice == "Скасувати") return;

        if (choice == NewShelf)
        {
            var name = await DisplayPromptAsync("Нова полиця", "Назва (наприклад, «На літо»)", maxLength: 30);
            if (string.IsNullOrWhiteSpace(name)) return;
            var shelf = ShelfStorage.Create(name.Trim());
            ShelfStorage.AddBook(shelf.Id, _book.Id);
            return;
        }

        var index = options.IndexOf(choice);
        if (index >= 0 && index < shelves.Count)
            ShelfStorage.Toggle(shelves[index].Id, _book.Id);
    }
}