using SwipeBook.Models;
using SwipeBook.Services;

namespace SwipeBook;

public partial class CatalogPage : ContentPage
{
    private readonly List<Book> _all = BookRepository.GetBooks();

    public CatalogPage()
    {
        InitializeComponent();
        GenrePicker.ItemsSource = new[] { "Усі жанри" }
            .Concat(_all.Select(b => b.Genre).Distinct().OrderBy(g => g)).ToList();
        GenrePicker.SelectedIndex = 0;
        ApplyFilter();
    }

    private void OnFilterChanged(object? sender, EventArgs e) => ApplyFilter();

    private void ApplyFilter()
    {
        var query = SearchBar.Text?.Trim() ?? "";
        var genre = GenrePicker.SelectedItem as string;
        var comparison = StringComparison.CurrentCultureIgnoreCase;

        var result = _all.Where(b =>
            (genre is null || genre == "Усі жанри" || b.Genre == genre) &&
            (query == "" || b.Title.Contains(query, comparison) || b.Author.Contains(query, comparison)))
            .ToList();

        BooksView.ItemsSource = result;
        CountLabel.Text = $"Знайдено: {result.Count}";
    }

    private async void OnBookSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Book book) return;
        BooksView.SelectedItem = null;
        await Shell.Current.GoToAsync($"{nameof(BookDetailPage)}?id={book.Id}");
    }
}