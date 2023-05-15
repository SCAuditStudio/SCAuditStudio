using Avalonia.Controls;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Interactivity;
using SCAuditStudio.ViewModels;
using SCAuditStudio.Classes.CustomElements;
using Avalonia.Markup.Xaml;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SCAuditStudio.Views.Editor
{
    public partial class MainEditor : UserControl
    {
        public MainEditor()
        {
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

        public void CloseTab_Clicked(object sender, PointerPressedEventArgs e)
        {
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

            Node? selectedNode = GetViewModel()?.mdFileTree.RowSelection?.SelectedItem;
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
                IReadOnlyList<Node?>? selectedNodes = GetViewModel()?.mdFileTree.RowSelection?.SelectedItems;
                if (selectedNodes == null) return;

                foreach (Node? selectedNode in selectedNodes)
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
                IndexPath? selected = GetViewModel()?.mdFileTree.RowSelection?.SelectedIndex;

                GetViewModel()?.MoveFileToInvalid(sender, e);

                if (selected == null) return;
                GetViewModel()!.mdFileTree.RowSelection!.SelectedIndex = selected ?? IndexPath.Unselected;
            }

            //Move File to root
            if (e.Key == Key.R)
            {
                GetViewModel()?.MoveFileToRoot(sender, e);
            }

            //Select all files
            if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.A)
            {
                for (int i = 0; i < GetViewModel()?.mdFileTree.Items.Count(); i++)
                {
                    GetViewModel()?.mdFileTree.RowSelection?.Select(new IndexPath(i));
                }
            }

            //Open Context Menu
            if (e.Key == Key.C)
            {
                TreeDataGrid? treeDataGrid = ((IVisual)e.Source).GetSelfAndVisualAncestors()
                    .OfType<TreeDataGrid>()
                    .FirstOrDefault();

                ContextMenu? contextMenu = treeDataGrid?.ContextMenu;
                if (contextMenu == null) return;

                contextMenu.PlacementMode = PlacementMode.AnchorAndGravity;
                contextMenu.Open();
                contextMenu.SelectedIndex = 0;
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
        public void StaticSortFile(object sender, RoutedEventArgs e)
        {
            GetViewModel()?.StaticSortFiles(sender,e);
        }
    }
}
