using SRCCON = VisioAutomation.ShapeSheet.SRCConstants;
using IVisio = Microsoft.Office.Interop.Visio;

namespace VisioAutomation.ShapeSheetQuery.CommonQueries
{
    class ControlCellsQuery : CellQuery
    {
        public SubQueryCellColumn CanGlue { get; set; }
        public SubQueryCellColumn Tip { get; set; }
        public SubQueryCellColumn X { get; set; }
        public SubQueryCellColumn Y { get; set; }
        public SubQueryCellColumn YBehavior { get; set; }
        public SubQueryCellColumn XBehavior { get; set; }
        public SubQueryCellColumn XDynamics { get; set; }
        public SubQueryCellColumn YDynamics { get; set; }

        public ControlCellsQuery()
        {
            var sec = this.AddSection(IVisio.VisSectionIndices.visSectionControls);

            this.CanGlue = sec.AddCell(SRCCON.Controls_CanGlue, nameof(SRCCON.Controls_CanGlue));
            this.Tip = sec.AddCell(SRCCON.Controls_Tip, nameof(SRCCON.Controls_Tip));
            this.X = sec.AddCell(SRCCON.Controls_X, nameof(SRCCON.Controls_X));
            this.Y = sec.AddCell(SRCCON.Controls_Y, nameof(SRCCON.Controls_Y));
            this.YBehavior = sec.AddCell(SRCCON.Controls_YCon, nameof(SRCCON.Controls_YCon));
            this.XBehavior = sec.AddCell(SRCCON.Controls_XCon, nameof(SRCCON.Controls_XCon));
            this.XDynamics = sec.AddCell(SRCCON.Controls_XDyn, nameof(SRCCON.Controls_XDyn));
            this.YDynamics = sec.AddCell(SRCCON.Controls_YDyn, nameof(SRCCON.Controls_YDyn));

        }

        public Shapes.Controls.ControlCells GetCells(ShapeSheet.CellData<double>[] row)
        {
            var cells = new Shapes.Controls.ControlCells();
            cells.CanGlue = Extensions.CellDataMethods.ToInt(row[this.CanGlue]);
            cells.Tip = Extensions.CellDataMethods.ToInt(row[this.Tip]);
            cells.X = row[this.X];
            cells.Y = row[this.Y];
            cells.YBehavior = Extensions.CellDataMethods.ToInt(row[this.YBehavior]);
            cells.XBehavior = Extensions.CellDataMethods.ToInt(row[this.XBehavior]);
            cells.XDynamics = Extensions.CellDataMethods.ToInt(row[this.XDynamics]);
            cells.YDynamics = Extensions.CellDataMethods.ToInt(row[this.YDynamics]);
            return cells;
        }
    }
}