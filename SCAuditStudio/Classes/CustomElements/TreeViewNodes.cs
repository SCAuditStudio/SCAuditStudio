using Avalonia.Media;
using System.Collections.ObjectModel;
using System.IO;

#pragma warning disable IDE1006
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
        public int searchDiff { get; set; }

        public Node(string path)
        {
            subNodes = new();
            fileName = Path.GetFileName(path);

            title = "untitled";
        }
    }
    public class ProjectNode
    {
        public string path { get; }
        public string Name;

        public ProjectNode(string path)
        {
            this.path = path;
            Name = "untitled";
        }
    }
}
