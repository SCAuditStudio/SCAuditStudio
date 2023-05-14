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
using SCAuditStudio.Design;
using SCAuditStudio.Views.Editor;
using System.Reactive.Linq;
using System.Reactive;
using ReactiveUI;

#pragma warning disable IDE1006
namespace SCAuditStudio.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public string ProjectDirectory;
        private bool _startMenuActive = true;
        private AppTheme _selectedTheme;
        public MDManager mdManager { get; private set; }
        public MainWindow? mainWindow { get; private set; }

        public ObservableCollection<TabItem> tabPages { get; }
        public ObservableCollection<Node> mdFileItems { get; }
        public HierarchicalTreeDataGridSource<Node> mdFileTree { get; }
        public ObservableCollection<MenuItem> mdFileIssues { get; private set; }
        public ObservableCollection<MenuItem> highlightBrushes { get; }

        public AppTheme selectedTheme
        {
            get { return _selectedTheme; }
            set { this.RaiseAndSetIfChanged(ref _selectedTheme, value); }
        }
        public bool startmenuactive
        {
            get { return _startMenuActive; }
            set { this.RaiseAndSetIfChanged(ref _startMenuActive, value); }
        }
        public MainWindowViewModel(string directory)
        {
            ProjectDirectory = directory;
            mdManager = new(ProjectDirectory);

            mdFileItems = new();
            mdFileTree = new(mdFileItems);
            mdFileTree.RowSelection!.SingleSelect = false;
            mdFileTree.Columns.Add(new HierarchicalExpanderColumn<Node>(new TextColumn<Node, string>("File Name", f => f.fileName), f => f.subNodes));
            mdFileTree.Columns.Add(new TextColumn<Node, string>("Title", f => f.title));
            mdFileTree.Columns.Add(new TextColumn<Node, uint?>("Score", f => f.score));
            mdFileTree.Columns.SetColumnWidth(0, GridLength.Parse("100"));
            mdFileTree.Columns.SetColumnWidth(1, GridLength.Parse("185"));
            mdFileTree.Columns.SetColumnWidth(2, GridLength.Parse("55"));
            mdFileIssues = new();

            highlightBrushes = new();
            _selectedTheme = new();
            selectedTheme = new();
            LoadTheme();
           
            tabPages = new();
        }
        public MainWindowViewModel() : this("") { }
        public void SetJudgingEditorActive()
        {
            startmenuactive = false;
        }
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
            
            CloseTabPages();
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
                string subNodeTitle = "";
                Node subNode = new(subDir)
                {
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
                    subNodeTitle = subFileNode.title;
                    subNode.subNodes.Add(subFileNode);
                }
                subNode.title = subNodeTitle;
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
                string title = mdFileTree.Items.Where(i => i.fileName == issue).FirstOrDefault()?.title ?? "";
                MenuItem issueItem = new() { Header = issue + " - " + title, Name = issue };
                issueItem.Click += MoveFileToIssue;
                mdFileIssues.Add(issueItem);
            }

            //Load medium issues last
            string[] mediumIssue = mdManager.GetIssues(MDManager.MDFileIssue.Medium);
            foreach (string issue in mediumIssue)
            {
                string title = mdFileTree.Items.Where(i => i.fileName == issue).FirstOrDefault()?.title ?? "";
                MenuItem issueItem = new() { Header = issue + " - " + title, Name = issue };
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
        public void LoadTheme()
        {
            bool darkMode = ConfigFile.Read<bool?>("AppTheme_UseDarkMode") ?? true;
            AppTheme theme = darkMode ? AppTheme.DefaultDark : AppTheme.DefaultLight;
            string? imagePath = ConfigFile.Read<string?>("AppTheme_BackgroundImagePath");
            if (imagePath != null) theme.SetBackgroundImage(imagePath);
            string? stretchMode = ConfigFile.Read<string?>("AppTheme_BackgroundStretchMode");
            if (stretchMode != null) { _ = Enum.TryParse(stretchMode, out Stretch mode); theme.BackgroundStretchMode = mode; }
            float? borderThickness = ConfigFile.Read<float?>("AppTheme_BackgroundBorderThickness");
            if (borderThickness.HasValue) theme.BackgroundBorderThickness = borderThickness.Value;
            float? opacity = ConfigFile.Read<float?>("AppTheme_BackgroundOpacity");
            if (opacity.HasValue) theme.BackgroundOpacity = opacity.Value;
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
                Content = file.rawContent,
            };

            tabPages.Add(page);
            page.IsSelected = true;
        }
        public void OpenOptionsPage()
        {
            if (TabOpen("Options"))
            {
                TabItem? item = GetTab("Options");
                if (item == null) return;
                item.IsSelected = true;
                return;
            }

            TabItem page = new()
            {
                Header = "Options"
            };

            tabPages.Insert(0, page);
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
        public void CloseTabPages()
        {
            for (int t = 0; t < tabPages.Count; t++)
            {
                TabItem? page = tabPages[t];
                if (page.Header?.ToString() != "Options")
                {
                    tabPages.RemoveAt(t);
                    t--;
                }
            }
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
                if(item.subNodes.Count > 0)
                {
                    foreach(Node subnode in item.subNodes)
                    {
                        mdManager.MoveFileToRoot(subnode.fileName);
                    }
                    continue;
                }
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
                if (item.subNodes.Count > 0)
                {
                    foreach (Node subnode in item.subNodes)
                    {
                        mdManager.MoveFileToInvalid(subnode.fileName);
                    }
                    continue;
                }
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
            string issue = menuItem.Name?.ToString() ?? "";

            IReadOnlyList<Node?>? selectedItems = mdFileTree.RowSelection?.SelectedItems;
            if (selectedItems == null) return;

            foreach (Node? item in selectedItems)
            {
                if (item == null) continue;
                if (item.subNodes.Count > 0)
                {
                    foreach (Node subnode in item.subNodes)
                    {
                        mdManager.MoveFileToIssue(subnode.fileName, issue, false);
                    }
                    continue;
                }
                mdManager.MoveFileToIssue(item.fileName, issue, false);
            }

            LoadMDFileItems();
            LoadMDFileContext();
        }
        public void StaticSortFiles(object? sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            IReadOnlyList<Node?>? selectedItems = mdFileTree.RowSelection?.SelectedItems;
            if (selectedItems == null) return;

            List<MDFile> mdfileitems = new();


            foreach (Node? item in selectedItems)
            {
                if (item == null) continue;
                if (item.subNodes.Count > 0)
                {
                    foreach (Node subnode in item.subNodes)
                    {
                        MDFile? mdfile = mdManager.GetFile(subnode.fileName);
                        if (mdfile == null) continue;
                        mdfileitems.Add(mdfile);
                    }
                    continue;
                }
                MDFile? mdfile1 = mdManager.GetFile(item.fileName);
                if (mdfile1 == null) continue;
                mdfileitems.Add(mdfile1);
            }

            StaticSortIssues(mdfileitems.ToArray());
            LoadMDFileItems();
            LoadMDFileContext();
        }
        public void StaticSortIssues(MDFile[]? issuesToCompare)
        {
            if (issuesToCompare == null) return;
            List<List<MDFile>>? groups = AutoDirectorySort.GroupIssues(issuesToCompare, mdManager.mdFiles);//,Math.Min(Environment.ProcessorCount*2, issuesToCompare.Length));
            if (groups == null) return;
            for (int i = 0; i < groups.Count; i++)
            {
                List<MDFile> mdFiles = groups[i];
                int? index = mdManager.GetIssueIndex(MDManager.MDFileIssue.Medium);
                for (int f = 0; f < mdFiles.Count; f++)
                {
                    mdManager.MoveFileToIssue(mdFiles[f].fileName, MDManager.MDFileIssue.Medium, index ?? 0, true);
                }

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
                if (item.subNodes.Count > 0)
                {
                    foreach (Node subnode in item.subNodes)
                    {
                        mdManager.MoveFileToIssue(subnode.fileName, severity, issueIndex, true);
                    }
                    continue;
                }
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
        //Folder or MDfile in Treeview
        public class Node
        {
            public ObservableCollection<Node> subNodes { get; set; }

            public IBrush? Background { get; set; }
            public IBrush? Foreground { get; set; }
            public string fileName { get; }
            public string title;
            public uint? score;

            public Node(string path)
            {
                subNodes = new();
                fileName = Path.GetFileName(path);

                title = "untitled";
            }
        }
    }
}