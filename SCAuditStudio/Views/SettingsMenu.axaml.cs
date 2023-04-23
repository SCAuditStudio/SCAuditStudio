using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace SCAuditStudio.Views
{
    public partial class SettingsMenu : UserControl
    {
        public SettingsMenu()
        {
            InitializeComponent();
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
