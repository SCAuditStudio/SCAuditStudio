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
using Avalonia.Markup.Xaml;

namespace SCAuditStudio.Views
{
    public partial class MainWindow : Window
    {
        bool mouseDownForWindowMoving = false;
        PointerPoint? originalPoint;
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        MainWindowViewModel? GetViewModel()
        {
            if (DataContext == null)
            {
                return null;
            }
            
            return (MainWindowViewModel)DataContext;
        }

        public async void AutoInvalidateIssueClicked(object sender, RoutedEventArgs e)
        {
            mouseDownForWindowMoving = false;
            await AutoDirectorySort.GetScore(GetViewModel()?.mdManager.mdFiles, File.ReadAllText(@"C:\\Users\\LinenBox\\Documents\\GitHub\\SCAuditStudio\\SCAuditStudio\\Assets\\SherlockConfig\Criteria.txt"));

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
    }
}