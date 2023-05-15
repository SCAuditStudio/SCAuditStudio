using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCAuditStudio.Classes.CustomElements
{
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
    public class ProjectNode
    {
        public ObservableCollection<ProjectNode> subNodes { get; set; }

        public IBrush? Background { get; set; }
        public IBrush? Foreground { get; set; }
        public string fileName { get; }
        public string title;

        public ProjectNode(string path)
        {
            subNodes = new();
            fileName = Path.GetFileName(path);

            title = "untitled";
        }
    }
}
