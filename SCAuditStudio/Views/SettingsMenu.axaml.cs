using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

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
            OpenFolderDialog dialog = new();
            string? directory = await dialog.ShowAsync(MainWindow.Instance);

            if (directory == null) return;
            ConfigFile.Write("BlackList", directory);
        }
    }
}
