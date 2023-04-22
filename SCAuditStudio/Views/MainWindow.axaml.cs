using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Interactivity;
using SCAuditStudio.ViewModels;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Models.TreeDataGrid;
using System.IO;

namespace SCAuditStudio.Views
{
    public partial class MainWindow : Window
    {
        bool mouseDownForWindowMoving = false;
        PointerPoint? originalPoint;

        public MainWindow()
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

        /* MOVE WINDOW EVENTS */
        void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (e.Source == null) return;
            if (!mouseDownForWindowMoving) return;

            Menu? menu = ((IVisual)e.Source).GetSelfAndVisualAncestors()
            .OfType<Menu>()
            .FirstOrDefault();
            if (menu == null) return;

            PointerPoint currentPoint = e.GetCurrentPoint(this);
            if (currentPoint.Position.Y > menu.Height) return;

            originalPoint ??= currentPoint;
            Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - originalPoint.Position.X),
                Position.Y + (int)(currentPoint.Position.Y - originalPoint.Position.Y));
        }
        void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Source == null) return;
            if (WindowState == WindowState.Maximized || WindowState == WindowState.FullScreen) return;

            Menu? menu = ((IVisual)e.Source).GetSelfAndVisualAncestors()
            .OfType<Menu>()
            .FirstOrDefault();
            if (menu == null) return;

            PointerPoint currentPoint = e.GetCurrentPoint(this);
            if (currentPoint.Position.Y > menu.Height) return;

            mouseDownForWindowMoving = true;
            originalPoint = e.GetCurrentPoint(this);
        }
        void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            mouseDownForWindowMoving = false;
        }

        public async void OpenProject_Clicked(object sender, RoutedEventArgs e)
        {
            mouseDownForWindowMoving = false;

            OpenFolderDialog dialog = new();
            string? directory = await dialog.ShowAsync(this);

            if (directory == null) return;
            Task? loadProjectTask = GetViewModel()?.LoadProject(directory);
            if (loadProjectTask == null) return;
            await loadProjectTask;
        }
        public void ExitProgram_Clicked(object sender, RoutedEventArgs e)
        {
            mouseDownForWindowMoving = false;

            Close();
        }
        public async void AutoInvalidateIssueClicked(object sender, RoutedEventArgs e)
        {
            mouseDownForWindowMoving = false;
            await AutoDirectorySort.GetScore(GetViewModel()?.mdManager.mdFiles,File.ReadAllText(@"C:\\Users\\LinenBox\\Documents\\GitHub\\SCAuditStudio\\SCAuditStudio\\Assets\\SherlockConfig\Criteria.txt"));

        }
        public void CloseTab_Clicked(object sender, PointerPressedEventArgs e)
        {
            mouseDownForWindowMoving = false;

            if (e.Source == null) return;

            TextBlock? itemText = ((IVisual)e.Source).VisualParent?.VisualParent.GetSelfAndVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault();

            if (itemText != null)
            {
                GetViewModel()?.CloseTabPage(itemText.Text);
            }
        }
        public void CloseTab_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Source == null) return;
            if (e.Key != Key.Enter) return;

            TabControl? tabControl = ((IVisual)e.Source).VisualParent?.GetSelfAndVisualAncestors()
                .OfType<TabControl>()
                .FirstOrDefault();

            TextBlock? itemText = ((IVisual)e.Source).VisualParent?.GetSelfAndVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault();

            if (itemText != null)
            {
                GetViewModel()?.CloseTabPage(itemText.Text);
            }

            if (tabControl == null) return;
            tabControl.Focus();
        }
        public void TreeView_DoubleTapped(object sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            MainWindowViewModel.Node? selectedNode = GetViewModel()?.mdFileTree.RowSelection?.SelectedItem;
            if (selectedNode == null) return;

            //Return if not .md file
            if (!selectedNode.fileName.EndsWith(".md")) return;

            //Try Open Tab Page of file
            GetViewModel()?.OpenTabPage(selectedNode.fileName);

            e.Handled = true;
        }
        public void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Source == null) return;

            //Read File
            if (e.Key == Key.Enter)
            {
                IReadOnlyList<MainWindowViewModel.Node?>? selectedNodes = GetViewModel()?.mdFileTree.RowSelection?.SelectedItems;
                if (selectedNodes == null) return;

                foreach (MainWindowViewModel.Node? selectedNode in selectedNodes)
                {
                    if (selectedNode == null) continue;

                    //Return if not .md file
                    if (!selectedNode.fileName.EndsWith(".md")) return;

                    //Try Open Tab Page of file
                    GetViewModel()?.OpenTabPage(selectedNode.fileName);
                }
            }

            //Move File to invalid
            if (e.Key == Key.I)
            {
                GetViewModel()?.MoveFileToInvalid(sender, e);
            }

            //Move File to root
            if (e.Key == Key.R)
            {
                GetViewModel()?.MoveFileToRoot(sender, e);
            }

            e.Handled = true;
        }

        /* EVENTS FOR VIEWMODEL */
        public void MoveFileToRoot(object sender, RoutedEventArgs e)
        {
            GetViewModel()?.MoveFileToRoot(sender, e);
        }
        public void MoveFileToInvalid(object sender, RoutedEventArgs e)
        {
            GetViewModel()?.MoveFileToInvalid(sender, e);
        }
        public void MoveFileToNewIssue(object sender, RoutedEventArgs e)
        {
            GetViewModel()?.MoveFileToNewIssue(sender, e);
        }
        public void MarkFileAsBest(object sender, RoutedEventArgs e)
        {
            GetViewModel()?.MarkFileAsBest(sender, e);
        }
        public void MarkFileAsUnmarked(object sender, RoutedEventArgs e)
        {
            GetViewModel()?.MarkFileAsUnmarked(sender, e);
        }
    }
}