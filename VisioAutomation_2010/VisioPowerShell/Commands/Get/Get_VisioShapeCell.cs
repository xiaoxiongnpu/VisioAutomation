﻿using System.Linq;
using System.Management.Automation;
using VisioPowerShell.ShapeSheet;
using IVisio = Microsoft.Office.Interop.Visio;

namespace VisioPowerShell.Commands.Get
{
    [Cmdlet(VerbsCommon.Get, VisioPowerShell.Nouns.VisioShapeCell)]
    public class Get_VisioShapeCell : VisioCmdlet
    {
        [Parameter(Mandatory = false, Position = 0)]
        public string[] Cells { get; set; }

        [Parameter(Mandatory = false)]
        public IVisio.Shape[] Shapes { get; set; }

        [Parameter(Mandatory = false)] 
        public SwitchParameter GetResults;

        [Parameter(Mandatory = false)] 
        public Model.ResultType ResultType = Model.ResultType.String;

        protected override void ProcessRecord()
        {
            var cellmap = CellSRCDictionary.GetCellMapForShapes();
            if (this.Cells == null || this.Cells.Length < 1 || this.Cells.Contains("*"))
            {
                this.Cells = cellmap.GetNames().ToArray();
            }

            Get_VisioPageCell.EnsureEnoughCellNames(this.Cells);
            var target_shapes = this.Shapes ?? this.Client.Selection.GetShapes();
            var v = string.Join(",", cellmap.GetNames());
            this.WriteVerbose(string.Format("Valid Names: {0}", v));
            var query = cellmap.CreateQueryFromCellNames(this.Cells);
            var surface = this.Client.ShapeSheet.GetShapeSheetSurface();
            var target_shapeids = target_shapes.Select(s => s.ID).ToList();
            var dt = Helpers.QueryToDataTable(query, this.GetResults, this.ResultType, target_shapeids, surface);
            this.WriteObject(dt);
        }
    }
}