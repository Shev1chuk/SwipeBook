namespace SwipeBook;

public partial class IntroPage : ContentPage
{
    private readonly bool _fromApp;

    // fromApp = true, коли сторінку відкрито повторно зсередини застосунку
    public IntroPage(bool fromApp = false)
    {
        InitializeComponent();
        _fromApp = fromApp;
        if (fromApp) StartButton.Text = "Закрити";
    }

    private async void OnStartClicked(object? sender, EventArgs e)
    {
        if (_fromApp)
        {
            await Navigation.PopModalAsync();
            return;
        }

        Preferences.Default.Set("intro_seen", true);

        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null) window.Page = new AppShell();
    }
}