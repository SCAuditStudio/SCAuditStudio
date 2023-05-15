using Avalonia.Controls;
using Avalonia.Interactivity;
using SCAuditStudio.Classes.CustomElements;
using SCAuditStudio.Classes.ProjectFile;
using SCAuditStudio.ViewModels;
using System.IO;
using System.Threading.Tasks;

namespace SCAuditStudio.Views
{
    public partial class StartMenu : UserControl
    {
        bool mouseDownForWindowMoving = false;

        public StartMenu()
        {
            DataContext = new StartMenuViewModel();
            InitializeComponent();
        }
        StartMenuViewModel? GetViewModel()
        {
            if (DataContext == null)
            {
                return null;
            }

            return (StartMenuViewModel)DataContext;
        }

        public void TreeView_DoubleTapped(object sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            ProjectNode? selectedNode = GetViewModel()?.projectFileTree.RowSelection?.SelectedItem;
            if (selectedNode == null) return;


            //Try Open Tab Page of file

            MainWindow.Instance?.GetViewModel()?.SetJudgingEditorActive();
            if (!Directory.Exists(selectedNode.path)) return;
            MainWindow.Instance?.GetViewModel()?.LoadProject(selectedNode.path);

            e.Handled = true;
        }
        public void RemoveFromListClick(object sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            ProjectNode? selectedNode = GetViewModel()?.projectFileTree.RowSelection?.SelectedItem;
            if (selectedNode == null) return;
            ProjectFileReader.RemoveProjectFile(selectedNode.path);
            GetViewModel()?.LoadProjectItems();
            e.Handled = true;
        }
    }
}
