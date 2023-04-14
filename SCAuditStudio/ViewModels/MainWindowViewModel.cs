using SCAuditStudio.Views;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Interactivity;
using Avalonia.Controls.Models.TreeDataGrid;
using System.Net.Http.Headers;
using SCAuditStudio.Design;

#pragma warning disable IDE1006
namespace SCAuditStudio.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string ProjectDirectory;

        public MDManager mdManager { get; private set; }
        public MainWindow? mainWindow { get; private set; }

        public ObservableCollection<TabItem> tabPages { get; }
        public ObservableCollection<Node> mdFileItems { get; }
        public HierarchicalTreeDataGridSource<Node> mdFileTree { get; }
        public ObservableCollection<MenuItem> mdFileIssues { get; private set; }
        public ObservableCollection<MenuItem> highlightBrushes { get; }

        public AppTheme selectedTheme { get; set; }

        public MainWindowViewModel(string directory)
        {
            ProjectDirectory = directory;
            mdManager = new(ProjectDirectory);

            mdFileItems = new();
            mdFileTree = new(mdFileItems);
            mdFileTree.RowSelection!.SingleSelect = false;
            mdFileTree.Columns.Add(new HierarchicalExpanderColumn<Node>(new TextColumn<Node, string>("File Name", f => f.fileName), f => f.subNodes));
            mdFileTree.Columns.Add(new TextColumn<Node, string>("Title", f => f.title));
            mdFileTree.Columns.Add(new TextColumn<Node, int>("Score", f => f.score));
            mdFileTree.Columns.SetColumnWidth(0, GridLength.Parse("100"));
            mdFileTree.Columns.SetColumnWidth(1, GridLength.Parse("185"));
            mdFileTree.Columns.SetColumnWidth(2, GridLength.Parse("55"));
            mdFileIssues = new();

            highlightBrushes = new();
            selectedTheme = new();
            LoadTheme(AppTheme.DefaultDark);

            tabPages = new();
        }
        public MainWindowViewModel() : this("") { }

        public async Task<MainWindowViewModel> Init(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            await mdManager.LoadFilesAsync();
            LoadMDFileItems();
            LoadMDFileContext();

            return this;
        }
        public async Task LoadProject(string directory)
        {
            ProjectDirectory = directory;

            mdManager = new(ProjectDirectory);
            await mdManager.LoadFilesAsync();
            LoadMDFileItems();
            LoadMDFileContext();
        }

        public void LoadMDFileItems()
        {
            if (!Directory.Exists(ProjectDirectory))
            {
                return;
            }

            mdFileItems.Clear();

            //Load sub-directories and their files
            string[] subDirs = mdManager.GetSubDirectories();
            foreach (string subDir in subDirs)
            {
                Node subNode = new(subDir)
                {
                    title = "",
                    score = 0,
                    Background = selectedTheme.Background,
                    Foreground = selectedTheme.Foreground
                };

                string[] subFiles = Directory.GetFiles(subDir);
                foreach (string subFile in subFiles)
                {
                    Node subFileNode = new(subFile);

                    if (!subFile.EndsWith(".md"))
                    {
                        subFileNode.title = "";
                        subFileNode.score = 0;
                        subFileNode.Background = selectedTheme.Background;
                        subFileNode.Foreground = selectedTheme.Foreground;

                        subNode.subNodes.Add(subFileNode);
                        continue;
                    }

                    MDFile? mdFile = mdManager.GetFile(subFileNode.fileName);
                    subFileNode.title = mdFile?.title ?? subFileNode.title;
                    subFileNode.score = mdFile?.score ?? subFileNode.score;
                    subFileNode.Background = mdFile?.highlight;
                    subFileNode.Foreground = mdFile?.highlight == null ? selectedTheme.Foreground : selectedTheme.SelectedText;
                    subNode.subNodes.Add(subFileNode);
                }

                mdFileItems.Add(subNode);
            }

            //Load remaining files in root
            string[] files = Directory.GetFiles(ProjectDirectory);
            foreach (string file in files)
            {
                Node fileNode = new(file);

                if (!file.EndsWith(".md"))
                {
                    fileNode.title = "";
                    fileNode.score = 0;
                    fileNode.Background = selectedTheme.Background;
                    fileNode.Foreground = selectedTheme.Foreground;

                    mdFileItems.Add(fileNode);
                    continue;
                }

                MDFile? mdFile = mdManager.GetFile(fileNode.fileName);
                fileNode.title = mdFile?.title ?? fileNode.title;
                fileNode.score = mdFile?.score ?? fileNode.score;
                fileNode.Background = mdFile?.highlight;
                fileNode.Foreground = mdFile?.highlight == null ? selectedTheme.Foreground : selectedTheme.SelectedText;
                mdFileItems.Add(fileNode);
            }

            //Update File Tree
            mdFileTree.Items = mdFileItems;
        }
        public void LoadMDFileContext()
        {
            if (!Directory.Exists(ProjectDirectory))
            {
                return;
            }

            mdFileIssues.Clear();

            //Load high issues first
            string[] highIssues = mdManager.GetIssues(MDManager.MDFileIssue.High);
            foreach (string issue in highIssues)
            {
                MenuItem issueItem = new() { Header = issue };
                issueItem.Click += MoveFileToIssue;
                mdFileIssues.Add(issueItem);
            }

            //Load medium issues last
            string[] mediumIssue = mdManager.GetIssues(MDManager.MDFileIssue.Medium);
            foreach (string issue in mediumIssue)
            {
                MenuItem issueItem = new() { Header = issue };
                issueItem.Click += MoveFileToIssue;
                mdFileIssues.Add(issueItem);
            }
        }
        public void LoadContextBrushes()
        {
            highlightBrushes.Clear();

            foreach (AppTheme.ContextBrush brush in selectedTheme.Brushes)
            {
                MenuItem item = new() { Header = brush.Name };
                item.Background = brush.Brush ?? item.Background;
                item.Foreground = brush.TextBrush ?? item.Foreground;
                item.DataContext = brush.Brush ?? null;
                item.Click += new EventHandler<RoutedEventArgs>(HighlightFile);

                highlightBrushes.Add(item);
            }
        }
        public void LoadTheme(AppTheme theme)
        {
            selectedTheme = theme;
            LoadContextBrushes();
        }

        public bool TabOpen(string tabName)
        {
            foreach (TabItem tab in tabPages)
            {
                if (tab.Header?.ToString() == tabName)
                {
                    return true;
                }
            }

            return false;
        }
        public TabItem? GetTab(string tabName)
        {
            foreach (TabItem tab in tabPages)
            {
                if (tab.Header?.ToString() == tabName)
                {
                    return tab;
                }
            }

            return null;
        }
        public TabItem? GetLastTab()
        {
            if (tabPages.Count == 0)
            {
                return null;
            }

            return tabPages[^1];
        }
        public void OpenTabPage(string fileName)
        {
            MDFile? file = mdManager.GetFile(fileName);
            if (file == null) return;

            if (TabOpen(fileName))
            {
                TabItem? item = GetTab(fileName);
                if (item == null) return;
                item.IsSelected = true;
                return;
            }

            TabItem page = new()
            {
                Header = fileName,
                Content = file.rawContent
            };

            tabPages.Add(page);
            page.IsSelected = true;
        }
        public void CloseTabPage(string tabName)
        {
            TabItem? tab = GetTab(tabName);

            if (tab != null)
            {
                tabPages.Remove(tab);
            }

            TabItem? lastTab = GetLastTab();
            if (lastTab == null) return;

            lastTab.IsSelected = true;
        }

        /* EVENTS */
        public void MoveFileToRoot(object sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            IReadOnlyList<Node?>? selectedItems = mdFileTree.RowSelection?.SelectedItems;
            if (selectedItems == null) return;

            foreach (Node? item in selectedItems)
            {
                if (item == null) continue;
                mdManager.MoveFileToRoot(item.fileName);
            }

            LoadMDFileItems();
            LoadMDFileContext();
        }
        public void MoveFileToInvalid(object sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            IReadOnlyList<Node?>? selectedItems = mdFileTree.RowSelection?.SelectedItems;
            if (selectedItems == null) return;

            foreach (Node? item in selectedItems)
            {
                if (item == null) continue;
                mdManager.MoveFileToInvalid(item.fileName);
            }

            LoadMDFileItems();
            LoadMDFileContext();
        }
        public void MoveFileToIssue(object? sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            MenuItem? menuItem = ((IVisual)e.Source).GetSelfAndVisualAncestors()
            .OfType<MenuItem>()
            .FirstOrDefault();

            if (menuItem == null) return;
            string issue = menuItem.Header.ToString() ?? "";

            IReadOnlyList<Node?>? selectedItems = mdFileTree.RowSelection?.SelectedItems;
            if (selectedItems == null) return;

            foreach (Node? item in selectedItems)
            {
                if (item == null) continue;
                mdManager.MoveFileToIssue(item.fileName, issue, false);
            }

            LoadMDFileItems();
            LoadMDFileContext();
        }
        public void MoveFileToNewIssue(object sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            MenuItem? menuItem = ((IVisual)e.Source).GetSelfAndVisualAncestors()
            .OfType<MenuItem>()
            .FirstOrDefault();

            if (menuItem == null) return;
            string? header = menuItem.Header.ToString();
            if (header == null) return;
            MDManager.MDFileIssue severity = header == "High" ? MDManager.MDFileIssue.High : MDManager.MDFileIssue.Medium;
            int issueIndex = mdManager.GetIssueIndex(severity);

            IReadOnlyList<Node?>? selectedItems = mdFileTree.RowSelection?.SelectedItems;
            if (selectedItems == null) return;

            foreach (Node? item in selectedItems)
            {
                if (item == null) continue;
                mdManager.MoveFileToIssue(item.fileName, severity, issueIndex, true);
            }

            LoadMDFileItems();
            LoadMDFileContext();
        }
        public void MarkFileAsBest(object sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            Node? fileNode = mdFileTree.RowSelection?.SelectedItem;
            if (fileNode == null) return;

            MDFile? mdFile = mdManager.GetFile(fileNode.fileName);
            if (mdFile == null) return;
            if (!mdManager.IssueExists(mdFile.subPath)) return;

            MDFile[] subFiles = mdManager.GetFilesInSubPath(mdFile.subPath);
            if (subFiles.Length < 2) return;
            foreach (MDFile subFile in subFiles)
            {
                if (subFile == mdFile) continue;
                mdManager.UnmarkFile(subFile.fileName);
            }

            mdManager.MarkFileAsBest(fileNode.fileName);

            LoadMDFileItems();
        }
        public void MarkFileAsUnmarked(object sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            IReadOnlyList<Node?>? selectedItems = mdFileTree.RowSelection?.SelectedItems;
            if (selectedItems == null) return;

            foreach (Node? item in selectedItems)
            {
                if (item == null) continue;
                mdManager.UnmarkFile(item.fileName);
            }

            LoadMDFileItems();
        }
        public void HighlightFile(object? sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            MenuItem? menuItem = ((IVisual)e.Source).GetSelfAndVisualAncestors()
                .OfType<MenuItem>()
                .FirstOrDefault();
            if (menuItem == null) return;

            IReadOnlyList<Node?>? selectedItems = mdFileTree.RowSelection?.SelectedItems;
            if (selectedItems == null) return;

            foreach (Node? item in selectedItems)
            {
                if (item == null) continue;

                MDFile? mdFile = mdManager.GetFile(item.fileName);
                if (mdFile == null) continue;

                mdFile.highlight = (IBrush?)menuItem.DataContext;
            }

            LoadMDFileItems();
        }

        /* INTERNAL CLASSES */
        public class Node
        {
            public ObservableCollection<Node> subNodes { get; set; }

            public IBrush? Background { get; set; }
            public IBrush? Foreground { get; set; }
            public string fileName { get; }
            public string title;
            public int score;

            public Node(string path)
            {
                subNodes = new();
                fileName = Path.GetFileName(path);

                title = "untitled";
                score = 0;
            }
        }
    }
}