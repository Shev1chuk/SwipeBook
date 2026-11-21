using System.Windows.Input;

namespace SwipeBook.Services;

public static class ThemeService
{
    private const string Key = "theme";

    // Команда для кнопки у верхній панелі
    public static ICommand ToggleCommand { get; } = new Command(Toggle);

    // Викликається при запуску: відновлює збережену тему (за замовчуванням світла)
    public static void ApplySaved()
    {
        var app = Application.Current;
        if (app is null) return;
        app.UserAppTheme = Preferences.Default.Get(Key, "light") == "dark"
            ? AppTheme.Dark
            : AppTheme.Light;
    }

    public static void Toggle()
    {
        var app = Application.Current;
        if (app is null) return;
        var next = app.RequestedTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        app.UserAppTheme = next;
        Preferences.Default.Set(Key, next == AppTheme.Dark ? "dark" : "light");
    }
}