using SwipeBook.Models;
using SwipeBook.Services;

namespace SwipeBook;

public partial class ShelvesPage : ContentPage
{
    public ShelvesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Refresh();
    }

    private void Refresh()
    {
        var all = BookRepository.GetBooks();
        var items = ShelfStorage.Load().Select(s => new ShelfView
        {
            Id = s.Id,
            Name = s.Name,
            CountText = $"Книжок: {s.BookIds.Count}",
            Books = s.BookIds
                .Select(id => all.FirstOrDefault(b => b.Id == id))
                .Where(b => b != null)
                .Select(b => new BookListItem
                {
                    Id = b!.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre,
                    Cover = b.Cover
                }).ToList()
        }).ToList();

        BindableLayout.SetItemsSource(ShelvesHost, items);
        EmptyLabel.Text = items.Count == 0 ? "Поки що немає полиць. Створи першу!" : "";
    }

    private async void OnNewShelfClicked(object? sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Нова полиця", "Назва (наприклад, «Для школи»)", maxLength: 30);
        if (string.IsNullOrWhiteSpace(name)) return;
        ShelfStorage.Create(name.Trim());
        Refresh();
    }

    private async void OnDeleteShelfClicked(object? sender, EventArgs e)
    {
        if (((Button)sender!).BindingContext is not ShelfView shelf) return;
        bool ok = await DisplayAlertAsync("Видалити полицю?", shelf.Name, "Видалити", "Скасувати");
        if (!ok) return;
        ShelfStorage.Delete(shelf.Id);
        Refresh();
    }

    private async void OnBookTapped(object? sender, TappedEventArgs e)
    {
        if (((BindableObject)sender!).BindingContext is not BookListItem item) return;
        await Shell.Current.GoToAsync($"{nameof(BookDetailPage)}?id={item.Id}");
    }
}