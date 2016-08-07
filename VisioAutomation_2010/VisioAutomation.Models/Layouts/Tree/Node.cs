using IVisio = Microsoft.Office.Interop.Visio;
using VA=VisioAutomation;

namespace VisioAutomation.Models.Layouts.Tree
{
    public class Node
    {
        private readonly NodeList _children;
        internal Node parent;

        public Node Parent
        {
            get { return this.parent; }
        }

        public NodeList Children
        {
            get { return this._children; }
        }

        public VisioAutomation.Models.Text.TextElement Text { get; set;}
        public IVisio.Shape VisioShape { get; set; }
        public DOM.Node DOMNode { get; set; }
        public VA.Drawing.Size? Size { get; set; }
        public DOM.ShapeCells Cells { get; set; }

        public Node()
        {
            this._children = new NodeList(this);
        }

        public Node(string name)
            : this()
        {
            this.Text = new VisioAutomation.Models.Text.TextElement(name);
        }
    }
}