using SwipeBook.Models;
using SwipeBook.Services;

namespace SwipeBook;

public partial class MainPage : ContentPage
{
    private List<Book> _books = new();
    private List<Book> _liked = new();
    private List<int> _dislikedIds = new();
    private int _index = 0;
    private int _total = 0;

    private bool _isAnimating;
    private double _lastTotalX;
    private const double SwipeThreshold = 120;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadQueue();
    }

    private void LoadQueue()
    {
        var all = BookRepository.GetBooks();
        var likedIds = SwipeStorage.LoadLiked();
        _dislikedIds = SwipeStorage.LoadDisliked();

        _liked = all.Where(b => likedIds.Contains(b.Id)).ToList();
        _books = all.Where(b => !likedIds.Contains(b.Id) && !_dislikedIds.Contains(b.Id)).ToList();
        _index = 0;
        _total = _books.Count;

        CardBorder.IsVisible = true;
        ResetCardPosition();
        InfoLabel.IsVisible = false;
        LikeButton.IsEnabled = true;
        DislikeButton.IsEnabled = true;

        ReorderByPreferences();
        ShowCurrentBook();
    }

    private void ResetCardPosition()
    {
        CardBorder.Opacity = 1;
        CardBorder.TranslationX = 0;
        CardBorder.Rotation = 0;
        LikeStamp.Opacity = 0;
        NopeStamp.Opacity = 0;
    }

    private void ShowCurrentBook()
    {
        if (_index >= _books.Count)
        {
            CardBorder.IsVisible = false;
            LikeButton.IsEnabled = false;
            DislikeButton.IsEnabled = false;
            InfoLabel.IsVisible = true;
            InfoLabel.Text = $"Книжки закінчились!\nСподобалось: {_liked.Count}\n\nЗагляни у вкладку «Профіль».";
            CounterLabel.Text = "Усе переглянуто";
            ProgressBar.Progress = 1;
            return;
        }

        var book = _books[_index];

        CounterLabel.Text = $"Книжка {_index + 1} з {_total}";
        ProgressBar.Progress = _total == 0 ? 0 : (double)_index / _total;

        // «обкладинка» за жанром (запасний варіант)
        var style = GenreStyle.Get(book.Genre);
        CoverBorder.Background = new LinearGradientBrush(
            new GradientStopCollection
            {
                new GradientStop(style.From, 0f),
                new GradientStop(style.To, 1f)
            },
            new Point(0, 0), new Point(1, 1));
        CoverEmoji.Text = style.Emoji;
        CoverGenre.Text = book.Genre.ToUpper();

        // справжня обкладинка з Resources\Images (якщо файлу немає, лишається градієнт)
        CoverImage.Source = null;
        CoverImage.Source = book.Cover;

        TitleLabel.Text = book.Title;
        AuthorLabel.Text = book.Author;
        GenreLabel.Text = book.Genre;
        DescriptionLabel.Text = book.Description;
        RecommendationLabel.Text = BuildExplanation(book);
    }

    private string BuildExplanation(Book book)
    {
        int sameGenre = _liked.Count(b => b.Genre == book.Genre);

        if (sameGenre > 0)
        {
            return $"✨ Рекомендовано, бо у жанрі «{book.Genre}» тобі вже сподобалось " +
                   $"{sameGenre} {Plural(sameGenre, "книжка", "книжки", "книжок")}";
        }

        if (_liked.Count == 0)
            return "🔍 Поки що ми збираємо твої вподобання: свайпай, і рекомендації стануть точнішими";

        return $"🧭 Щоб відкрити щось нове: жанр «{book.Genre}» ти ще не вподобав";
    }

    private static string Plural(int n, string one, string few, string many)
    {
        int m100 = n % 100, m10 = n % 10;
        if (m100 is >= 11 and <= 14) return many;
        return m10 switch
        {
            1 => one,
            2 or 3 or 4 => few,
            _ => many
        };
    }

    private void ReorderByPreferences()
    {
        var score = _liked
            .GroupBy(b => b.Genre)
            .ToDictionary(g => g.Key, g => g.Count());

        var rng = new Random();

        _books = _books
            .OrderByDescending(b => score.TryGetValue(b.Genre, out var s) ? s : 0)
            .ThenBy(_ => rng.Next())
            .ToList();
    }
    // Свайп пальцем
    private async void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (_isAnimating || _index >= _books.Count) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _lastTotalX = 0;
                break;

            case GestureStatus.Running:
                _lastTotalX = e.TotalX;
                CardBorder.TranslationX = e.TotalX;
                CardBorder.Rotation = e.TotalX / 20;
                LikeStamp.Opacity = Math.Clamp(e.TotalX / SwipeThreshold, 0, 1);
                NopeStamp.Opacity = Math.Clamp(-e.TotalX / SwipeThreshold, 0, 1);
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (_lastTotalX > SwipeThreshold) await CommitSwipe(true);
                else if (_lastTotalX < -SwipeThreshold) await CommitSwipe(false);
                else await SnapBack();
                _lastTotalX = 0;
                break;
        }
    }

    // Кнопки
    private async void OnLikeClicked(object? sender, EventArgs e)
    {
        if (_isAnimating) return;
        LikeStamp.Opacity = 1;
        await CommitSwipe(true);
    }

    private async void OnDislikeClicked(object? sender, EventArgs e)
    {
        if (_isAnimating) return;
        NopeStamp.Opacity = 1;
        await CommitSwipe(false);
    }

    // Картка повертається на місце, якщо свайп недостатньо довгий
    private async Task SnapBack()
    {
        _isAnimating = true;
        await Task.WhenAll(
            CardBorder.TranslateTo(0, 0, 200, Easing.CubicOut),
            CardBorder.RotateTo(0, 200),
            LikeStamp.FadeTo(0, 150),
            NopeStamp.FadeTo(0, 150));
        _isAnimating = false;
    }

    // Картка відлітає, вибір зберігається, показується наступна книжка
    private async Task CommitSwipe(bool like)
    {
        if (_isAnimating || _index >= _books.Count) return;
        _isAnimating = true;

        var book = _books[_index];
        double distance = (Width > 0 ? Width : 400) * 1.5;

        await Task.WhenAll(
            CardBorder.TranslateTo(like ? distance : -distance, 0, 250, Easing.CubicIn),
            CardBorder.FadeTo(0, 250));

        SaveChoice(book, like);
        _index++;
        ResetCardPosition();
        ShowCurrentBook();

        _isAnimating = false;
    }

    private void SaveChoice(Book book, bool like)
    {
        var liked = SwipeStorage.LoadLiked();
        var disliked = SwipeStorage.LoadDisliked();

        if (like)
        {
            if (!liked.Contains(book.Id)) liked.Add(book.Id);
            disliked.Remove(book.Id);
            _liked.Add(book);
        }
        else
        {
            if (!disliked.Contains(book.Id)) disliked.Add(book.Id);
            liked.Remove(book.Id);
        }

        SwipeStorage.Save(liked, disliked);
    }
}