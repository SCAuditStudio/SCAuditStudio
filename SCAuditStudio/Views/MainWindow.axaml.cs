using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using SCAuditStudio.ViewModels;
using System;

namespace SCAuditStudio.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
        }
        void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public MainWindowViewModel? GetViewModel()
        {
            if (DataContext == null)
            {
                return null;
            }
            
            return (MainWindowViewModel)DataContext;
        }

        public void AutoInvalidateIssueClicked(object sender, RoutedEventArgs e)
        {
            MDFile[] mDFiles = GetViewModel()?.mdManager.mdFiles ?? Array.Empty<MDFile>();
            foreach (MDFile mDFile in mDFiles)
            {
                if (mDFile.score < 12)
                {
                    GetViewModel()?.mdManager.MoveFileToInvalid(mDFile.fileName);
                }
            }
            GetViewModel()?.LoadMDFileItems();
            GetViewModel()?.LoadMDFileContext();
        }
        public void AutoSortIssuesClicked(object sender, RoutedEventArgs e)
        {
            GetViewModel()?.StaticSortIssues(GetViewModel()?.mdManager.mdFiles);
        }

        /* MOVE WINDOW EVENTS */
        void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Source == null) return;

            Menu? menu = ((IVisual)e.Source).GetSelfAndVisualAncestors()
            .OfType<Menu>()
            .FirstOrDefault();
            if (menu == null) return;

            PointerPoint currentPoint = e.GetCurrentPoint(this);
            if (currentPoint.Position.Y > menu.Height) return;

            BeginMoveDrag(e);
        }
        void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            e.Handled = true;
        }

        public async void OpenProject_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new();
            string? directory = await dialog.ShowAsync(this);

            if (directory == null) return;
            Task? loadProjectTask = GetViewModel()?.LoadProject(directory);
            if (loadProjectTask == null) return;
            await loadProjectTask;
        }
        public void OpenOptionsMenu(object sender, RoutedEventArgs e)
        {
            GetViewModel()?.OpenOptionsPage();
        }    
        public void ExitProgram_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}