using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using DynamicData;
using ReactiveUI;
using SCAuditStudio.Classes.CustomElements;
using SCAuditStudio.Classes.ProjectFile;
using SCAuditStudio.Design;
using SCAuditStudio.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCAuditStudio.ViewModels
{
    public class StartMenuViewModel : ReactiveObject
    {
        public HierarchicalTreeDataGridSource<ProjectNode> projectFileTree { get; }
        public ObservableCollection<ProjectNode> projectFileTreeItems { get; }
        public AppTheme? selectedTheme
        {
            get { return MainWindow.Instance?.GetViewModel()?.selectedTheme; }
        }

        public StartMenuViewModel()
        {
            projectFileTreeItems = new();
            projectFileTree = new(projectFileTreeItems);
            projectFileTree.Columns.Add(new HierarchicalExpanderColumn<ProjectNode>(new TextColumn<ProjectNode, string>("Project path", f => f.path), f => f.subNodes));
            projectFileTree.Columns.Add(new TextColumn<ProjectNode, string>("Project Name", f => f.Name));
            projectFileTree.Columns.SetColumnWidth(0, GridLength.Parse("*"));
            projectFileTree.Columns.SetColumnWidth(1, GridLength.Parse("*"));

            LoadProjectItems();
        }

        public void SetJudgingEditorActive()
        {
            MainWindow.Instance?.GetViewModel()?.SetJudgingEditorActive();
        }
        public void LoadProjectItems()
        {
  
            projectFileTreeItems.Clear();

            //Load remaining files in root
            ProjectFile[] projects = ProjectFileReader.ReadProjects();

            foreach (ProjectFile project in projects)
            {
                ProjectNode projectNode = new(project.path);

                projectNode.Name = project?.Name ?? projectNode.Name;

                projectFileTreeItems.Add(projectNode);
            }

            //Update File Tree
            projectFileTree.Items = projectFileTreeItems;
            Console.WriteLine(projectFileTree.Items.Count());
        }
    }
}
