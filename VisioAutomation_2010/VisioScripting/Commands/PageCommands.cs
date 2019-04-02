using System.Collections.Generic;
using System.Linq;
using VisioAutomation.Extensions;
using VisioAutomation.ShapeSheet;
using IVisio = Microsoft.Office.Interop.Visio;

namespace VisioScripting.Commands
{
    public class PageCommands : CommandSet
    {
        internal PageCommands(Client client) :
            base(client)
        {

        }

        public void SetActivePage(IVisio.Page page)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();
            var app = cmdtarget.Application;
            this._client.Output.WriteVerbose("Setting Active Page to \"{0}\"", page.Name);
            var window = app.ActiveWindow;
            window.Page = page;
        }

        public void SetActivePageByPageName(string name)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();
            var doc = cmdtarget.ActiveDocument;
            this._client.Output.WriteVerbose("Retrieving Page \"{0}\"", name);
            var pages = doc.Pages;
            var page = pages[name];
            this.SetActivePage(page);
        }

        public void SetActivePageByPageNumber(int pagenumber)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();


            var application = cmdtarget.Application;
            var doc = application.ActiveDocument;
            this._client.Output.WriteVerbose("Retrieving Page Number \"{0}\"", pagenumber);
            var pages = doc.Pages;
            var page = pages[pagenumber];
            this.SetActivePage(page);
        }
        
        public IVisio.Page GetActivePage()
        {
            var cmdtarget = this._client.GetCommandTargetDocument();

            var application = cmdtarget.Application;
            return application.ActivePage;
        }

        public void DeletePages(Models.TargetPages targetpages, bool renumber)
        {
            targetpages = targetpages.Resolve(this._client);

            foreach (var page in targetpages.Items)
            {
                page.Delete(renumber ? (short) 1 : (short) 0);
            }
        }

        public VisioAutomation.Geometry.Size GetPageSize(Models.TargetPages targetpages)
        {
            targetpages = targetpages.Resolve(this._client);

            if (targetpages.Items.Count < 1)
            {
                throw new System.ArgumentException("No pages found");
            }

            var query = new VisioAutomation.ShapeSheet.Query.CellQuery();
            var col_height = query.Columns.Add(VisioAutomation.ShapeSheet.SrcConstants.PageHeight, nameof(VisioAutomation.ShapeSheet.SrcConstants.PageHeight));
            var col_width = query.Columns.Add(VisioAutomation.ShapeSheet.SrcConstants.PageWidth, nameof(VisioAutomation.ShapeSheet.SrcConstants.PageWidth));

            var cellqueryresult = query.GetResults<double>(targetpages.Items[0].PageSheet);
            var row = cellqueryresult[0];
            double height = row[col_height];
            double width = row[col_width];
            var s = new VisioAutomation.Geometry.Size(width, height);
            return s;
        }

        public IVisio.Page NewPage(VisioAutomation.Geometry.Size? size, bool isbackgroundpage)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();

            var active_document = cmdtarget.ActiveDocument;
            var pages = active_document.Pages;
            IVisio.Page new_page;

            using (var undoscope = this._client.Undo.NewUndoScope(nameof(NewPage)))
            {
                new_page = pages.Add();

                if (size.HasValue)
                {
                    var targetpages = new Models.TargetPages(new_page);
                    this.SetPageSize(targetpages, size.Value);
                }

                if (isbackgroundpage)
                {
                    new_page.Background = 1;
                }
            }

            return new_page;
        }

        public void SetActivePageBackground(string background_page_name)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();

            if (background_page_name == null)
            {
                throw new System.ArgumentNullException(nameof(background_page_name));
            }

            var app = cmdtarget.Application;
            var application = app;
            var active_document = application.ActiveDocument;
            var pages = active_document.Pages;
            var names = new HashSet<string>(pages.GetNamesU());
            if (!names.Contains(background_page_name))
            {
                string msg = string.Format("Could not find page with name \"{0}\"", background_page_name);
                throw new VisioAutomation.Exceptions.VisioOperationException(msg);
            }

            var bgpage = pages.ItemU[background_page_name];
            var fgpage = application.ActivePage;

            // Set the background page
            // Check that the intended background is indeed a background page
            if (bgpage.Background == 0)
            {
                string msg = string.Format("Page \"{0}\" is not a background page", bgpage.Name);
                throw new VisioAutomation.Exceptions.VisioOperationException(msg);
            }

            // don't allow the page to be set as a background to itself
            if (fgpage == bgpage)
            {
                string msg = "Cannot set page as its own background page";
                throw new VisioAutomation.Exceptions.VisioOperationException(msg);
            }

            using (var undoscope = this._client.Undo.NewUndoScope(nameof(SetActivePageBackground)))
            {
                fgpage.BackPage = bgpage;
            }
        }

        public IVisio.Page DuplicateActivePage()
        {
            var cmdtarget = this._client.GetCommandTargetPage();

            using (var undoscope = this._client.Undo.NewUndoScope(nameof(DuplicateActivePage)))
            {
                var pages = cmdtarget.ActiveDocument.Pages;

                var src_page = cmdtarget.Application.ActivePage;
                var new_page = pages.Add();

                var win = cmdtarget.Application.ActiveWindow;
                win.Page = src_page;
                VisioAutomation.Pages.PageHelper.Duplicate(src_page, new_page);
                win.Page = new_page;
                return new_page;
            }
        }

        public IVisio.Page DuplicatePageToDocument(Models.TargetPage targetpage, IVisio.Document dest_doc)
        {
            targetpage = targetpage.Resolve(this._client);


            if (targetpage.Item == null)
            {
                throw new VisioAutomation.Exceptions.VisioOperationException("No page found to duplicate");
            }

            if (dest_doc == null)
            {
                throw new System.ArgumentNullException(nameof(dest_doc));
            }

            if (targetpage.Item.Document == dest_doc)
            {
                throw new VisioAutomation.Exceptions.VisioOperationException("dest doc is same as pages src doc");
            }

            var dest_pages = dest_doc.Pages;
            var dest_page = dest_pages[1];
            VisioAutomation.Pages.PageHelper.Duplicate(targetpage.Item, dest_page);

            return dest_page;
        }

        public Models.PageOrientation GetPageOrientation( Models.TargetPages targetpages )
        {
            targetpages = targetpages.Resolve(this._client);
            return PageCommands._GetPageOrientation(targetpages.Items[0]);
        }
        
        private static Models.PageOrientation _GetPageOrientation(IVisio.Page page)
        {
            if (page == null)
            {
                throw new System.ArgumentNullException(nameof(page));
            }

            var page_sheet = page.PageSheet;
            var src = VisioAutomation.ShapeSheet.SrcConstants.PrintPageOrientation;
            var orientationcell = page_sheet.CellsSRC[src.Section, src.Row, src.Cell];
            int value = orientationcell.ResultInt[IVisio.VisUnitCodes.visNumber, 0];
            return (Models.PageOrientation)value;
        }

        public void SetPageOrientation(Models.TargetPages targetpages, Models.PageOrientation orientation)
        {
            if (orientation != VisioScripting.Models.PageOrientation.Landscape && orientation != VisioScripting.Models.PageOrientation.Portrait)
            {
                throw new System.ArgumentOutOfRangeException(nameof(orientation), "must be either Portrait or Landscape");
            }

            targetpages = targetpages.Resolve(this._client);
            using (var undoscope = this._client.Undo.NewUndoScope(nameof(SetPageOrientation)))
            {

                foreach (var page in targetpages.Items)
                {
                    var old_orientation = PageCommands._GetPageOrientation(page);

                    if (old_orientation == orientation)
                    {
                        // don't need to do anything
                        return;
                    }

                    var page_tp = new VisioScripting.Models.TargetPages(page);
                    var old_size = this.GetPageSize(page_tp);

                    double new_height = old_size.Width;
                    double new_width = old_size.Height;

                    var writer = new VisioAutomation.ShapeSheet.Writers.SrcWriter();
                    writer.SetValue(VisioAutomation.ShapeSheet.SrcConstants.PageWidth, new_width);
                    writer.SetValue(VisioAutomation.ShapeSheet.SrcConstants.PageHeight, new_height);
                    writer.SetValue(VisioAutomation.ShapeSheet.SrcConstants.PrintPageOrientation, (int)orientation);

                    writer.Commit(page.PageSheet, CellValueType.Formula);
                }

            }

        }
        public void ResizePageToFitContents(Models.TargetPages targetpages, VisioAutomation.Geometry.Size bordersize)
        {
            targetpages = targetpages.Resolve(this._client);

            using (var undoscope = this._client.Undo.NewUndoScope(nameof(ResizePageToFitContents)))
            {
                foreach (var page in targetpages.Items)
                {
                    page.ResizeToFitContents(bordersize);
                }
            }
        }

        public void SetPageFormatCells(Models.TargetPages targetpages, VisioAutomation.Pages.PageFormatCells cells)
        {
            targetpages = targetpages.Resolve(this._client);

            using (var undoscope = this._client.Undo.NewUndoScope(nameof(SetPageFormatCells)))
            {
                foreach (var page in targetpages.Items)
                {
                    var writer = new VisioAutomation.ShapeSheet.Writers.SrcWriter();
                    writer.SetValues(cells);
                    writer.BlastGuards = true;
                    writer.Commit(page, CellValueType.Formula);
                }
            }
        }

        public void SetPageSize(Models.TargetPages targetpages, VisioAutomation.Geometry.Size new_size)
        {
            targetpages = targetpages.Resolve(this._client);

            using (var undoscope = this._client.Undo.NewUndoScope(nameof(SetPageSize)))
            {
                foreach (var page in targetpages.Items)
                {
                    var page_sheet = page.PageSheet;
                    var writer = new VisioAutomation.ShapeSheet.Writers.SrcWriter();
                    writer.SetValue(VisioAutomation.ShapeSheet.SrcConstants.PageWidth, new_size.Width);
                    writer.SetValue(VisioAutomation.ShapeSheet.SrcConstants.PageHeight, new_size.Height);
                    writer.Commit(page_sheet, CellValueType.Formula);
                }
            }
        }

        public void SetPageSize(Models.TargetPage targetpage, double? width, double? height)
        {
            if (!width.HasValue && !height.HasValue)
            {
                // nothing to do
                return;
            }

            var page = this._client.Page.GetActivePage();
            var targetpages = new VisioScripting.Models.TargetPages(page);
            var old_size = this.GetPageSize(targetpages);
            var w = width.GetValueOrDefault(old_size.Width);
            var h = height.GetValueOrDefault(old_size.Height);
            var new_size = new VisioAutomation.Geometry.Size(w, h);
            this.SetPageSize(new Models.TargetPages(targetpage.Item),new_size);
        }

        public void SetActivePageByDirection(Models.PageDirection flags)
        {
            var cmdtarget = this._client.GetCommandTargetPage();

            var docpages = cmdtarget.ActiveDocument.Pages;
            if (docpages.Count < 2)
            {
                return;
            }

            var pages = docpages;
            this._GoTo(pages, flags, cmdtarget);
        }

        public void LayoutPage(Models.TargetPages targetpages, VisioAutomation.Models.LayoutStyles.LayoutStyleBase layout)
        {
            targetpages = targetpages.Resolve(this._client);

            using (var undoscope = this._client.Undo.NewUndoScope(nameof(SetPageSize)))
            {
                foreach (var page in targetpages.Items)
                {
                    layout.Apply(page);
                }
            }
        }

        private void _GoTo(IVisio.Pages pages, Models.PageDirection flags, CommandTarget cmdtarget)
        {
            if (pages == null)
            {
                throw new System.ArgumentNullException(nameof(pages));
            }

            if (pages.Count < 2)
            {
                throw new VisioAutomation.Exceptions.VisioOperationException("Only 1 page available. Navigation not possible.");
            }

            int cur_index = cmdtarget.ActivePage.Index;
            const int min_index = 1;
            int max_index = pages.Count;
            int new_index = PageCommands.move_in_range(cur_index, min_index, max_index, flags);
            if (cur_index != new_index)
            {
                var doc_pages = cmdtarget.ActiveDocument.Pages;
                var page = doc_pages[new_index];

                var active_window = cmdtarget.Application.ActiveWindow;
                active_window.Page = page;
            }
        }

        internal static int move_in_range(int cur, int min, int max, Models.PageDirection direction)
        {
            if (max < min)
            {
                throw new System.ArgumentOutOfRangeException(nameof(max));
            }

            if (cur < min)
            {
                throw new System.ArgumentOutOfRangeException(nameof(cur));
            }

            if (cur > max)
            {
                throw new System.ArgumentOutOfRangeException(nameof(cur));
            }

            switch (direction)
            {
                case VisioScripting.Models.PageDirection.Next:
                    return System.Math.Min(cur + 1, max);
                case VisioScripting.Models.PageDirection.Previous:
                    return System.Math.Max(cur - 1, min);
                case VisioScripting.Models.PageDirection.First:
                    return min;
                case VisioScripting.Models.PageDirection.Last:
                    return max;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(direction));
            }
        }

        public List<IVisio.Shape> GetShapesOnPageByID(Models.TargetPage targetpage, int[] shapeids)
        {
            targetpage = targetpage.Resolve(this._client);
            var shapes = targetpage.Item.Shapes;
            var shapes_list = new List<IVisio.Shape>(shapeids.Length);
            foreach (int id in shapeids)
            {
                var shape = shapes.ItemFromID[id];
                shapes_list.Add(shape);
            }
            return shapes_list;
        }


        public List<IVisio.Shape> GetShapesOnPageByName(Models.TargetPage targetpage, string[] shapenames)
        {
            targetpage = targetpage.Resolve(this._client);

            return this.GetShapesOnPageByName(targetpage, shapenames, false);
        }

        public List<IVisio.Shape> GetShapesOnPageByName(Models.TargetPage targetpage, string[] shapenames, bool ignore_bad_names)
        {
            targetpage = targetpage.Resolve(this._client);

            if (targetpage.Item == null)
            {
                throw new System.ArgumentException("No page available");
            }

            var cmdtarget = this._client.GetCommandTargetDocument();
            var shapes = targetpage.Item.Shapes;
            var cached_shapes_list = new List<IVisio.Shape>(shapes.Count);
            cached_shapes_list.AddRange(shapes.ToEnumerable());
            
            if (shapenames.Contains("*"))
            {
                // if any of the shape names contains a simple wildcard then return all the shapes
                return cached_shapes_list;
            }

            // otherwise we start checking for each name
            var shapes_list = VisioScripting.Helpers.WildcardHelper.FilterObjectsByNames(cached_shapes_list, shapenames, s => s.Name, true, VisioScripting.Helpers.WildcardHelper.FilterAction.Include).ToList();

            return shapes_list;
        }

        public List<IVisio.Page> FindPagesInActiveDocumentByName(string name, Models.PageType pagetype)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();

            var active_document = cmdtarget.ActiveDocument;
            if (VisioScripting.Helpers.WildcardHelper.NullOrStar(name))
            {
                // return all pages
                var all_pages = active_document.Pages.ToList();
                all_pages = filter_pages_by_type(all_pages, pagetype);
                return all_pages;
            }
            else
            {
                // return the named page
                var all_pages = active_document.Pages.ToEnumerable();
                var named_pages= VisioScripting.Helpers.WildcardHelper.FilterObjectsByNames(all_pages, new[] { name }, p => p.Name, true, VisioScripting.Helpers.WildcardHelper.FilterAction.Include).ToList();
                named_pages = filter_pages_by_type(named_pages, pagetype);

                return named_pages;
            }
        }

        private List<IVisio.Page> filter_pages_by_type(List<IVisio.Page> pages, Models.PageType pagetype)
        {
            if (pages == null)
            {
                return null;
            }

            if (pagetype == Models.PageType.Any)
            {
                return pages;
            }
            if (pagetype == Models.PageType.Foreground)
            {
                return pages.Where(p=>p.Background==0).ToList();
            }

            if (pagetype == Models.PageType.Background)
            {
                return pages.Where(p => p.Background != 0).ToList();
            }

            string msg = "Unsupported value for pagetype";
            throw new System.ArgumentOutOfRangeException(nameof(pagetype),msg);
        }

        public List<IVisio.Shape> GetShapesOnActivePage()
        {
            var cmdtarget = this._client.GetCommandTargetPage();
            var shapes = cmdtarget.ActivePage.Shapes.ToList();
            return shapes;
        }
    }
}