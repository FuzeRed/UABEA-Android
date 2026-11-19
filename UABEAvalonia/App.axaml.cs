using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace UABEAvalonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Current.RequestedThemeVariant = ThemeVariant.Light;
#if ANDROID
            AddTouchStyles();
#endif
        }

        public override void OnFrameworkInitializationCompleted()
        {
#if !ANDROID
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
#else
            if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            {
                // android has no real windows: UABEA's "windows" are stacked inside one view
                var host = new WindowHost();
                WindowHost.Current = host;
                host.ExitRequested += () => AppPaths.ExitApp?.Invoke();
                host.Push(new MainWindow());
                singleView.MainView = host;
            }
#endif

            base.OnFrameworkInitializationCompleted();
        }

#if ANDROID
        // bigger tap targets than the desktop theme has
        private void AddTouchStyles()
        {
            var button = new Style(x => x.OfType<Button>());
            button.Setters.Add(new Setter(Avalonia.Layout.Layoutable.MinHeightProperty, 40.0));
            Styles.Add(button);

            var menuItem = new Style(x => x.OfType<MenuItem>());
            menuItem.Setters.Add(new Setter(Avalonia.Layout.Layoutable.MinHeightProperty, 44.0));
            Styles.Add(menuItem);

            var listItem = new Style(x => x.OfType<ListBoxItem>());
            listItem.Setters.Add(new Setter(Avalonia.Layout.Layoutable.MinHeightProperty, 40.0));
            Styles.Add(listItem);
        }
#endif
    }
}
