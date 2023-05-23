using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SCAuditStudio.Views.Editor
{
    public partial class SettingsMenu : UserControl
    {
        public SettingsMenu()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public async void OpenBlacklistFolder_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            FileDialogFilter filter = new()
            {
                Name = ".txt",
                Extensions = new System.Collections.Generic.List<string>() {"txt"}
            };
            dialog.Filters = new System.Collections.Generic.List<FileDialogFilter> { filter };
            dialog.AllowMultiple = false;

            MainWindow? window = MainWindow.Instance;
            if (window == null) return;

            string[]? file = await dialog.ShowAsync(window);
            if (file == null) return;
            if (file[0] != null) ConfigFile.Write("BlackList", file[0]);
        }
        public void SetDarkMode(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            MainWindow? window = MainWindow.Instance;
            if (window == null) return;

            if (e.Property.Name == "IsChecked")
            {
                ConfigFile.Write("AppTheme_UseDarkMode", e.NewValue);
                window.GetViewModel()?.LoadTheme();
            }
        }
        public async void SetBackgroundImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            FileDialogFilter filter = new()
            {
                Name = "Images",
                Extensions = new System.Collections.Generic.List<string>() { "jpg", "jpeg", "png", "webp", "gif" },
            };
            dialog.Filters = new System.Collections.Generic.List<FileDialogFilter> { filter };
            dialog.AllowMultiple = false;

            MainWindow? window = MainWindow.Instance;
            if (window == null) return;

            string[]? file = await dialog.ShowAsync(window);
            if (file == null) return;
            if (file[0] != null) ConfigFile.Write("AppTheme_BackgroundImagePath", file[0]);

            window.GetViewModel()?.LoadTheme();
        }
        public void RemoveBackgroundImage(object sender, RoutedEventArgs e)
        {
            MainWindow? window = MainWindow.Instance;
            if (window == null) return;

            ConfigFile.Write("AppTheme_BackgroundImagePath", null);
            window.GetViewModel()?.LoadTheme();
        }
        public void StretchModeChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            MainWindow? window = MainWindow.Instance;
            if (window == null) return;

            if (e.Property.Name == "SelectionBoxItem")
            {
                ConfigFile.Write("AppTheme_BackgroundStretchMode", e.NewValue);
                window.GetViewModel()?.LoadTheme();
            }
        }
        public void BorderThicknessChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            MainWindow? window = MainWindow.Instance;
            if (window == null) return;

            if (e.Property.Name == "Value")
            {
                ConfigFile.Write("AppTheme_BackgroundBorderThickness", e.NewValue);
                window.GetViewModel()?.LoadTheme();
            }
        }
        public void BackgroundOpacityChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            MainWindow? window = MainWindow.Instance;
            if (window == null) return;

            if (e.Property.Name == "Value")
            {
                ConfigFile.Write("AppTheme_BackgroundOpacity", e.NewValue);
                window.GetViewModel()?.LoadTheme();
            }
        }
    }
}
