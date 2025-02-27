﻿using VisioAutomation.Exceptions;
using VisioAutomation.Extensions;
using MUT=Microsoft.VisualStudio.TestTools.UnitTesting;
using VADOM = VisioAutomation.Models.Dom;
using VA = VisioAutomation;
using IVisio = Microsoft.Office.Interop.Visio;

namespace VTest.Models
{
    [MUT.TestClass]
    public class Dom_Tests : Framework.VTest
    {
        public string node_stencil_name = "basic_u.vss";
        public string edge_stencil_name = "connec_u.vss";
        public string node_master_name = "Rectangle";
        public string edge_master_name = "Dynamic Connector";

        private VisioAutomation.Core.Size _pagesize;

        [MUT.TestMethod]
        public void Dom_EmptyRendering()
        {
            // Rendering a DOM should not change the page count
            // Empty DOMs do not add any shapes
            var app = this.GetVisioApplication();

            var page_node = new VADOM.Page();
            var doc = this.GetNewDoc();
            page_node.Render(app.ActiveDocument);
            MUT.Assert.AreEqual(0, app.ActivePage.Shapes.Count);
            app.ActiveDocument.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_RenderPageToDocument()
        {
            // Rendering a dom page to a document should create a new page
            var app = this.GetVisioApplication();
            var page_node = new VADOM.Page();
            var visdoc = this.GetNewDoc();
            MUT.Assert.AreEqual(1, visdoc.Pages.Count);
            var page = page_node.Render(app.ActiveDocument);
            MUT.Assert.AreEqual(2, visdoc.Pages.Count);
            app.ActiveDocument.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_RenderDocumentToApplication()
        {
            // Rendering a dom document to an appliction instance should create a new document
            var app = this.GetVisioApplication();
            var doc_node = new VADOM.Document();
            var page_node = new VADOM.Page();
            doc_node.Pages.Add(page_node);
            int old_count = app.Documents.Count;
            var newdoc = doc_node.Render(app);
            MUT.Assert.AreEqual(old_count + 1, app.Documents.Count);
            MUT.Assert.AreEqual(1, newdoc.Pages.Count);
            app.ActiveDocument.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_DrawSimpleShape()
        {
            // Create the doc
            var page_node = new VADOM.Page();
            var vrect1 = new VADOM.Rectangle(1, 1, 9, 9);
            vrect1.Text = new VisioAutomation.Models.Text.Element("HELLO WORLD");
            vrect1.Cells.FillForeground = "rgb(255,0,0)";
            page_node.Shapes.Add(vrect1);

            // Render it
            var app = this.GetVisioApplication();
            var doc = this.GetNewDoc();
            this._pagesize = new VA.Core.Size(10, 10);
            Framework.VTest.SetPageSize(app.ActivePage, this._pagesize);
            var page = page_node.Render(app.ActiveDocument);

            // Verify
            MUT.Assert.IsNotNull(vrect1.VisioShape);
            MUT.Assert.AreEqual("HELLO WORLD", vrect1.VisioShape.Text);

            app.ActiveDocument.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_DropShapes()
        {
            // Render it
            var app = this.GetVisioApplication();
            var doc = this.GetNewDoc();
            var stencil = app.Documents.OpenStencil(this.node_stencil_name);
            var rectmaster = stencil.Masters[this.node_master_name];


            // Create the doc
            var shape_nodes = new VADOM.ShapeList();

            shape_nodes.DrawRectangle(0, 0, 1, 1);
            shape_nodes.Drop(rectmaster, 3, 3);

            shape_nodes.Render(app.ActivePage);

            app.ActiveDocument.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_CustomProperties()
        {
            // Create the doc
            var shape_nodes = new VADOM.ShapeList();
            var vrect1 = new VADOM.Rectangle(1, 1, 9, 9);
            vrect1.Text = new VisioAutomation.Models.Text.Element("HELLO WORLD");

            vrect1.CustomProperties = new VA.Shapes.CustomPropertyDictionary();

            var cp1 = new VA.Shapes.CustomPropertyCells();
            cp1.Value = "\"FOOVALUE\"";
            cp1.Label = "\"Foo Label\"";

            var cp2 = new VA.Shapes.CustomPropertyCells();
            cp2.Value = "\"BARVALUE\"";
            cp2.Label = "\"Bar Label\"";

            vrect1.CustomProperties["FOO"] = cp1;
            vrect1.CustomProperties["BAR"] = cp2;

            shape_nodes.Add(vrect1);

            // Render it
            var app = this.GetVisioApplication();
            var doc = this.GetNewDoc();
            shape_nodes.Render(app.ActivePage);

            // Verify
            MUT.Assert.IsNotNull(vrect1.VisioShape);
            MUT.Assert.AreEqual("HELLO WORLD", vrect1.VisioShape.Text);
            MUT.Assert.IsTrue(VA.Shapes.CustomPropertyHelper.Contains(vrect1.VisioShape, "FOO"));
            MUT.Assert.IsTrue(VA.Shapes.CustomPropertyHelper.Contains(vrect1.VisioShape, "BAR"));

            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_DrawOrgChart()
        {
            var app = this.GetVisioApplication();
            var vis_ver = VA.Application.ApplicationHelper.GetVersion(app);

            // How to draw using a Template instead of a doc and a stencil
            string orgchart_vst = "orgchart.vst";
            string position_master_name = vis_ver.Major >= 15 ? "Position Belt" : "Position";

            var doc_node = new VADOM.Document(orgchart_vst, IVisio.VisMeasurementSystem.visMSUS);
            var page_node = new VADOM.Page();
            doc_node.Pages.Add(page_node);

            // Have to be smart about selecting the right master with Visio 2013

            var s1 = new VADOM.Shape(position_master_name, null, new VA.Core.Point(3, 8));
            page_node.Shapes.Add(s1);

            var s2 = new VADOM.Shape(position_master_name, null, new VA.Core.Point(0, 4));
            page_node.Shapes.Add(s2);

            var s3 = new VADOM.Shape(position_master_name, null, new VA.Core.Point(6, 4));
            page_node.Shapes.Add(s3);

            page_node.Shapes.Connect(this.edge_master_name, this.edge_stencil_name, s1, s2);
            page_node.Shapes.Connect(this.edge_master_name, this.edge_stencil_name, s1, s3);

            var doc = doc_node.Render(app);
            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_DrawEmpty()
        {
            // Verify that an empty DOM page can be created and rendered
            var doc = this.GetNewDoc();
            var page_node = new VADOM.Page();
            page_node.Size = new VA.Core.Size(5, 5);
            var page = page_node.Render(doc);

            MUT.Assert.AreEqual(0, page.Shapes.Count);
            var actual_page_size = Framework.VTest.GetPageSize(page);
            var expected_page_size = new VA.Core.Size(5, 5);
            MUT.Assert.AreEqual(expected_page_size, actual_page_size);

            page.Delete(0);
            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_DrawLine()
        {
            var doc = this.GetNewDoc();
            var page_node = new VADOM.Page();
            var line_node_0 = page_node.Shapes.DrawLine(1, 1, 3, 3);
            var page = page_node.Render(doc);

            MUT.Assert.AreEqual(1, page.Shapes.Count);
            MUT.Assert.AreNotEqual(0, line_node_0.VisioShapeID);
            MUT.Assert.IsNotNull(line_node_0.VisioShape);
            MUT.Assert.AreEqual(2.0, line_node_0.VisioShape.CellsU["PinX"].Result[IVisio.VisUnitCodes.visNumber]);
            page.Delete(0);
            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_DrawBezier()
        {
            var doc = this.GetNewDoc();
            var page_node = new VADOM.Page();
            var bez_node_0 = page_node.Shapes.DrawBezier(new double[] { 1, 2, 3, 3, 6, 3, 3, 4 });

            var page = page_node.Render(doc);

            MUT.Assert.AreEqual(1, page.Shapes.Count);
            MUT.Assert.AreNotEqual(0, bez_node_0.VisioShapeID);
            MUT.Assert.IsNotNull(bez_node_0.VisioShape);

            page.Delete(0);
            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_DropMaster()
        {

            var doc = this.GetNewDoc();
            var page_node = new VADOM.Page();
            var stencil = doc.Application.Documents.OpenStencil(this.node_stencil_name);
            var master1 = stencil.Masters[this.node_master_name];

            var master_node_0 = page_node.Shapes.Drop(master1, 3, 3);
            var master_node_1 = page_node.Shapes.Drop(this.node_master_name, this.node_stencil_name, 5, 5);

            var page = page_node.Render(doc);

            MUT.Assert.AreEqual(2, page.Shapes.Count);

            // Verify that the shapes created both have IDs and shape objects associated with them
            MUT.Assert.AreNotEqual(0, master_node_0.VisioShapeID);
            MUT.Assert.AreNotEqual(0, master_node_1.VisioShapeID);
            MUT.Assert.IsNotNull(master_node_0.VisioShape);
            MUT.Assert.IsNotNull(master_node_1.VisioShape);
            page.Delete(0);
            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_FormatShape()
        {
            var doc = this.GetNewDoc();
            var page_node = new VADOM.Page();
            var stencil = doc.Application.Documents.OpenStencil(this.node_stencil_name);
            var master1 = stencil.Masters[this.node_master_name];

            var master_node_0 = page_node.Shapes.Drop(master1, 3, 3);
            var bez_node_0 = page_node.Shapes.DrawBezier(new double[] { 1, 2, 3, 3, 6, 3, 3, 4 });
            var line_node_0 = page_node.Shapes.DrawLine(1, 1, 3, 3);

            master_node_0.Cells.LineWeight = 0.1;
            bez_node_0.Cells.LineWeight = 0.3;
            line_node_0.Cells.LineWeight = 0.5;

            var page = page_node.Render(doc);

            MUT.Assert.AreEqual(3, page.Shapes.Count);
            page.Delete(0);
            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_ConnectShapes()
        {
            var doc = this.GetNewDoc();
            var page_node = new VADOM.Page();

            var basic_stencil = doc.Application.Documents.OpenStencil(this.node_stencil_name);
            var basic_masters = basic_stencil.Masters;
            var connectors_stencil = doc.Application.Documents.OpenStencil(this.edge_stencil_name);
            var connectors_masters = connectors_stencil.Masters;

            var master1 = basic_masters[this.node_master_name];
            var master2 = connectors_masters[this.edge_master_name];

            var master_node_0 = page_node.Shapes.Drop(master1, 3, 3);
            var master_node_1 = page_node.Shapes.Drop(master1, 6, 5);
            var dc = page_node.Shapes.Connect(master2, master_node_0, master_node_1);

            var page = page_node.Render(doc);

            MUT.Assert.AreEqual(3, page.Shapes.Count);

            page.Delete(0);
            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_ConnectShapes2()
        {
            // Deferred means that the stencils (and thus masters) are loaded when rendering
            // and are no loaded by the caller before Render() is called

            var doc = this.GetNewDoc();
            var page_node = new VADOM.Page();
            var master_node_0 = page_node.Shapes.Drop(this.node_master_name, this.node_stencil_name, 3, 3);
            var master_node_1 = page_node.Shapes.Drop(this.node_master_name, this.node_stencil_name, 6, 5);
            var dc = page_node.Shapes.Connect(this.edge_master_name, this.edge_stencil_name, master_node_0, master_node_1);
            var page = page_node.Render(doc);

            MUT.Assert.AreEqual(3, page.Shapes.Count);

            page.Delete(0);
            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_VerifyThatUnknownMastersAreDetected()
        {
            var doc = this.GetNewDoc();
            var page_node = new VADOM.Page();
            var master_node_0 = page_node.Shapes.Drop("XXX", this.node_stencil_name, 3, 3);

            IVisio.Page page=null;
            bool caught = false;
            try
            {
                page = page_node.Render(doc);
            }
            catch (System.ArgumentException)
            {
                caught = true;
            }
            
            if (caught == false)
            {
                MUT.Assert.Fail("Expected an AutomationException");
            }
            
            if (page!=null)
            {
                page.Delete(0);
            }
            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_VerifyThatUnknownStencilsAreDetected()
        {
            string non_existent_stencil = "foobar.vss";

            var doc = this.GetNewDoc();
            var page_node = new VADOM.Page();
            var master_node_0 = page_node.Shapes.Drop(this.node_master_name, non_existent_stencil, 3, 3);

            IVisio.Page page = null;
            bool caught = false;
            try
            {
                page = page_node.Render(doc);
            }
            catch (AutomationException)
            {
                caught = true;
            }
            
            if (caught == false)
            {
                MUT.Assert.Fail("Expected an AutomationException");
            }

            if (page!=null)
            {
                page.Delete(0);                
            }
            doc.Close(true);
        }

        [MUT.TestMethod]
        public void Dom_DrawAndDrop()
        {
            var doc = this.GetNewDoc();
            var page_node = new VADOM.Page();

            var rect0 = new VisioAutomation.Core.Rectangle(3, 4, 7, 8);
            var rect1 = new VisioAutomation.Core.Rectangle(8, 1, 9, 5);

            // Draw and Drop two rectangles in the same place
            var s0 = page_node.Shapes.Drop(this.node_master_name, this.node_stencil_name, rect0);
            var s1 = page_node.Shapes.DrawRectangle(rect0);

            // Draw and Drop two rectangles in the same place
            var s2 = page_node.Shapes.Drop(this.node_master_name, this.node_stencil_name, rect1);
            var s3 = page_node.Shapes.DrawRectangle(rect1);

            // Render the page
            var page = page_node.Render(doc);

            // Verify the locations and sizes

            var shapeids = new int[] { 
                s0.VisioShapeID,
                s1.VisioShapeID,
                s2.VisioShapeID,
                s3.VisioShapeID};

            var xfrms = VA.Shapes.ShapeXFormCells.GetCells(page, shapeids, VA.Core.CellValueType.Formula);

            MUT.Assert.AreEqual(xfrms[1].PinX, xfrms[0].PinX);
            MUT.Assert.AreEqual(xfrms[1].PinY, xfrms[0].PinY);

            MUT.Assert.AreEqual(xfrms[1].Width, xfrms[0].Width);
            MUT.Assert.AreEqual(xfrms[1].Height, xfrms[0].Height);

            MUT.Assert.AreEqual(xfrms[3].PinX,   xfrms[2].PinX);
            MUT.Assert.AreEqual(xfrms[3].PinY,   xfrms[2].PinY);
            MUT.Assert.AreEqual(xfrms[3].Width,  xfrms[2].Width);
            MUT.Assert.AreEqual(xfrms[3].Height, xfrms[2].Height);

            doc.Close(true);
        }
    }
}