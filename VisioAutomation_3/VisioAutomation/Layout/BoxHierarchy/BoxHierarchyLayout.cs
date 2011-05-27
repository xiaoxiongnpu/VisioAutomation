using VA=VisioAutomation;

namespace VisioAutomation.Layout.BoxHierarchy
{
    public class BoxHierarchyLayout<T>
    {
        public LayoutOptions LayoutOptions;

        private Node<T> _root;

        public BoxHierarchyLayout() :
            this(LayoutDirection.Vertical)
        {
        }

        public BoxHierarchyLayout(LayoutDirection dir)
        {
            this.LayoutOptions = new LayoutOptions(); 
            this._root = new Node<T>(dir);
        }

        public Node<T> Root
        {
            get { return _root; }
            set { _root = value; }
        }

        public void PerformLayout()
        {
            this.CalculateSizes();
            this.Place(this.LayoutOptions.Origin);
        }

        private void CalculateSizes()
        {
            // this method calculates the sizes of nodes
            _CalculateSizeNode(_root);
        }

        private void _CalculateSizeNode(Node<T> node)
        {
            //calculate the size of the children
            foreach (var child_el in node.Children)
            {
                _CalculateSizeNode(child_el);
            }

            double child_height_sum = 0;
            double child_width_max = 0;
            double child_height_max = 0;
            double child_width_sum = 0;
            double h = node.Height.GetValueOrDefault(LayoutOptions.DefaultHeight);
            double w = node.Width.GetValueOrDefault(LayoutOptions.DefaultWidth);

            double padx = node.Padding;
            double pady = node.Padding;

            foreach (var child_el in node.Children)
            {
                child_height_sum += child_el.Height.Value;
                child_height_max = System.Math.Max(child_height_max, child_el.Height.Value);
                child_width_sum += child_el.Width.Value;
                child_width_max = System.Math.Max(child_width_max, child_el.Width.Value);
            }

            // Account for child separation
            int num_seps = System.Math.Max(0, node.ChildCount - 1);
            double total_sepy = (node.Direction == LayoutDirection.Vertical) ? num_seps*node.ChildSeparation : 0.0;
            double total_sepx = (node.Direction == LayoutDirection.Horizonal) ? num_seps*node.ChildSeparation : 0.0;

            child_height_sum += total_sepy;
            child_width_sum += total_sepx;

            if (node.Direction == LayoutDirection.Vertical)
            {
                node.Height = System.Math.Max(h, child_height_sum);
                node.Width = System.Math.Max(w, child_width_max);
            }
            else if (node.Direction == LayoutDirection.Horizonal)
            {
                node.Height = System.Math.Max(h, child_height_max);
                node.Width = System.Math.Max(w, child_width_sum);
            }

            node.Height = node.Height.Value + (2*pady);
            node.Width = node.Width.Value + (2*padx);
        }

        private void Place(VA.Drawing.Point origin)
        {
            // this method calculates the positions on nodes
            _PlaceNode(_root, origin);
        }

        private void _PlaceNode(Node<T> node, VA.Drawing.Point origin)
        {
            if (node == null)
            {
                throw new System.ArgumentNullException("node");
            }

            double signx = (LayoutOptions.DirectionHorizontal == VA.DirectionHorizontal.LeftToRight) ? 1.0 : -1.0;
            double signy = (LayoutOptions.DirectionVertical == VA.DirectionVertical.BottomToTop) ? 1.0 : -1.0;

            // Calculate the final rectangle to place the current node

            double miny = (LayoutOptions.DirectionVertical == VA.DirectionVertical.TopToBottom)
                              ? origin.Y - node.Height.Value
                              : origin.Y;

            double minx = (LayoutOptions.DirectionHorizontal == VA.DirectionHorizontal.LeftToRight)
                              ? origin.X
                              : origin.X - node.Width.Value;

            double maxx = minx + node.Width.Value;

            double maxy = miny + node.Height.Value;

            var rect = new VA.Drawing.Rectangle(minx, miny, maxx, maxy);
            node.Rectangle = rect;

            var current_point = origin;
            double padx = node.Padding;
            double pady = node.Padding;

            foreach (var cur_el in node.Children)
            {
                // Calculate where the child will be placed, taking into account the direction and alignment
                var child_rect = current_point;

                if (node.Direction == LayoutDirection.Vertical)
                {
                    var halign = cur_el.AlignmentHorizontal;

                    double deltawidth = node.Width.Value - (2*padx) - cur_el.Width.Value;
                    double deltax = (halign == VA.Drawing.AlignmentHorizontal.Left) ? 0.0 : deltawidth;
                    double factorx = (halign == VA.Drawing.AlignmentHorizontal.Center) ? 0.5 : 1.0;

                    child_rect = current_point.Add(signx*factorx*deltax, 0);
                }
                else
                {
                    var valign = cur_el.AlignmentVertical;

                    double deltaheight = node.Height.Value - (2*pady) - cur_el.Height.Value;
                    double deltay = (valign == VA.Drawing.AlignmentVertical.Bottom) ? 0.0 : deltaheight;
                    double factory = (valign == VA.Drawing.AlignmentVertical.Center) ? 0.5 : 1.0;
                    child_rect = current_point.Add(0, signy*factory*deltay);
                }

                child_rect = child_rect.Add(signx*padx, signy*pady);

                // render the child
                _PlaceNode(cur_el, child_rect);

                // move to the next place to start placing a child
                if (node.Direction == LayoutDirection.Vertical)
                {
                    current_point = current_point.Add(0, signy*cur_el.Height.Value);
                    current_point = current_point.Add(0, signy*node.ChildSeparation);
                }
                else if (node.Direction == LayoutDirection.Horizonal)
                {
                    current_point = current_point.Add(signx*cur_el.Width.Value, 0);
                    current_point = current_point.Add(signx*node.ChildSeparation, 0);
                }
            }
        }


        private void internal_Render(Node<T> node, RenderOptions<T> options)
        {
            if (node == null)
            {
                throw new System.ArgumentNullException("node");
            }

            if (options== null)
            {
                throw new System.ArgumentException("renderoptions is null");
            }

            if (this.RenderAction == null)
            {
                throw new System.ArgumentException("renderoptions contains a null function");
            }

            this.RenderAction(node, node.Rectangle);

            foreach (var cur_el in node.Children)
            {
                internal_Render(cur_el, options);
            }
        }

        public void Render(RenderOptions<T> options)
        {
            this.internal_Render(this.Root, options);
        }

        public delegate void OnRenderAction(Node<T> node,VA.Drawing.Rectangle rect);

        public event OnRenderAction RenderAction;

    }
}