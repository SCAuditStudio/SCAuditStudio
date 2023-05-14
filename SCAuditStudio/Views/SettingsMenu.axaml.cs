using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json.Linq;

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
    }
}
