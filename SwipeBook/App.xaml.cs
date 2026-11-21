using Microsoft.Extensions.DependencyInjection;
using SwipeBook.Services;
namespace SwipeBook
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            ThemeService.ApplySaved();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            bool seen = Preferences.Default.Get("intro_seen", false);
            Page first = seen ? new AppShell() : new IntroPage();
            return new Window(first);
        }   
    }
}