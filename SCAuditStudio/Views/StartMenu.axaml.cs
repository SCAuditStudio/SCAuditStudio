using Avalonia.Controls;
using Avalonia.Interactivity;
using SCAuditStudio.ViewModels;
using System.Threading.Tasks;

namespace SCAuditStudio.Views
{
    public partial class StartMenu : UserControl
    {
        bool mouseDownForWindowMoving = false;

        public StartMenu()
        {
            InitializeComponent();
        }
        MainWindowViewModel? GetViewModel()
        {
            if (DataContext == null)
            {
                return null;
            }

            return (MainWindowViewModel)DataContext;
        }
        public void JudgeProjectClicked(object sender, RoutedEventArgs e)
        {
            mouseDownForWindowMoving = false;
            GetViewModel()?.SetJudgingEditorActive();
        }
            
    }
}
