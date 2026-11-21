using SwipeBook.Models;
using SwipeBook.Services;

namespace SwipeBook;

public partial class LikedPage : ContentPage
{
    private readonly string[] _filters = { "Вподобані", "Хочу прочитати", "Читаю", "Прочитано" };

    public LikedPage()
    {
        InitializeComponent();
        FilterPicker.ItemsSource = _filters;
        FilterPicker.SelectedIndex = 0;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ThemeSwitch.Toggled -= OnThemeToggled;
        ThemeSwitch.IsToggled = Application.Current!.UserAppTheme == AppTheme.Dark;
        ThemeSwitch.Toggled += OnThemeToggled;

        RefreshProfile();
        RefreshList();
    }

    private void OnFilterChanged(object? sender, EventArgs e) => RefreshList();

    // ---------- Тема ----------
    private void OnThemeToggled(object? sender, ToggledEventArgs e)
    {
        Application.Current!.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
        Preferences.Default.Set("theme", e.Value ? "dark" : "light");
    }

    // ---------- Полиці ----------
    private async void OnShelvesClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(ShelvesPage));

    // ---------- Профіль і діаграма ----------
    private void RefreshProfile()
    {
        var books = BookRepository.GetBooks();
        var likedIds = SwipeStorage.LoadLiked();
        var dislikedIds = SwipeStorage.LoadDisliked();
        var liked = books.Where(b => likedIds.Contains(b.Id)).ToList();

        int rated = likedIds.Count + dislikedIds.Count;
        RatedValue.Text = rated.ToString();
        LikePercentValue.Text = rated == 0 ? "—" : $"{likedIds.Count * 100 / rated}%";

        var genres = liked
            .GroupBy(b => b.Genre)
            .Select(g => new { Genre = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        FavGenreValue.Text = genres.Count > 0 ? genres[0].Genre : "—";

        // Статуси читання
        var statuses = SwipeStorage.LoadStatuses();
        int want = statuses.Count(s => s.Status == ReadingStatus.WantToRead);
        int reading = statuses.Count(s => s.Status == ReadingStatus.Reading);
        int done = statuses.Count(s => s.Status == ReadingStatus.Done);
        int doneThisYear = statuses.Count(s => s.Status == ReadingStatus.Done && s.Date.Year == DateTime.Now.Year);

        ReadThisYearValue.Text = doneThisYear.ToString();
        StatusCountsLabel.Text = $"Хочу прочитати: {want}  ·  Читаю: {reading}  ·  Прочитано: {done}";

        BuildChart(genres.Select(g => (g.Genre, g.Count)).ToList());
    }

    private void BuildChart(List<(string Genre, int Count)> genres)
    {
        GenreChart.Children.Clear();

        if (genres.Count == 0)
        {
            var hint = new Label
            {
                Text = "Постав кілька ♥ на вкладці «Свайп», і тут з'явиться діаграма.",
                FontSize = 14
            };
            hint.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#777"), Color.FromArgb("#B3A99C"));
            GenreChart.Children.Add(hint);
            return;
        }

        int max = genres.Max(g => g.Count);

        foreach (var (genre, count) in genres)
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection(
                    new ColumnDefinition(new GridLength(120)),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(new GridLength(28))),
                ColumnSpacing = 8
            };

            var name = new Label { Text = genre, FontSize = 14, VerticalOptions = LayoutOptions.Center };
            name.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#3B2F2F"), Color.FromArgb("#F2E9DD"));

            var bar = new ProgressBar
            {
                Progress = (double)count / max,
                ProgressColor = Color.FromArgb("#B5651D"),
                VerticalOptions = LayoutOptions.Center
            };

            var num = new Label
            {
                Text = count.ToString(),
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.End,
                VerticalOptions = LayoutOptions.Center
            };
            num.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#777"), Color.FromArgb("#B3A99C"));

            row.Add(name, 0, 0);
            row.Add(bar, 1, 0);
            row.Add(num, 2, 0);
            GenreChart.Children.Add(row);
        }
    }

    // ---------- Список книжок ----------
    private void RefreshList()
    {
        if (FilterPicker.SelectedIndex < 0) return;

        var books = BookRepository.GetBooks();
        List<Book> source;
        string emptyText;

        if (FilterPicker.SelectedIndex == 0)
        {
            var ids = SwipeStorage.LoadLiked();
            source = books.Where(b => ids.Contains(b.Id)).ToList();
            emptyText = "Тут з'являться книжки, які тобі сподобались ♥";
        }
        else
        {
            var status = FilterPicker.SelectedIndex switch
            {
                1 => ReadingStatus.WantToRead,
                2 => ReadingStatus.Reading,
                _ => ReadingStatus.Done
            };
            var ids = SwipeStorage.LoadStatuses().Where(s => s.Status == status).Select(s => s.BookId).ToList();
            source = books.Where(b => ids.Contains(b.Id)).ToList();
            emptyText = "Поки що порожньо. Статус книжки можна змінити на її сторінці в каталозі.";
        }

        var items = source.Select(b => new BookListItem
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            Genre = b.Genre,
            Cover = b.Cover,
            StatusText = StatusText.ToText(SwipeStorage.GetStatus(b.Id))
        }).ToList();

        BindableLayout.SetItemsSource(BooksList, items);
        EmptyLabel.Text = emptyText;
        EmptyLabel.IsVisible = items.Count == 0;
    }

    private async void OnBookTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not BookListItem item) return;
        await Shell.Current.GoToAsync($"{nameof(BookDetailPage)}?id={item.Id}");
    }

    private async void OnResetClicked(object? sender, EventArgs e)
    {
        if (!await DisplayAlertAsync("Скинути свайпи?",
                "Усі вподобані й відхилені книжки буде очищено. Статуси читання залишаться.", "Так", "Ні"))
            return;

        SwipeStorage.Clear();
        RefreshProfile();
        RefreshList();
    }

    private async void OnIntroClicked(object? sender, EventArgs e)
        => await Navigation.PushModalAsync(new IntroPage(true));
}