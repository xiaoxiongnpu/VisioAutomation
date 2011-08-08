using VA=VisioAutomation;
using System.Collections.Generic;
using IVisio = Microsoft.Office.Interop.Visio;
using System.Linq;
using VisioAutomation.Extensions;

namespace VisioAutomation.Connections
{
    public class ConnectionPointCells : VA.ShapeSheet.CellSectionDataGroup
    {
        public VA.ShapeSheet.CellData<double> X { get; set; }
        public VA.ShapeSheet.CellData<double> Y { get; set; }
        public VA.ShapeSheet.CellData<int> DirX { get; set; }
        public VA.ShapeSheet.CellData<int> DirY { get; set; }
        public VA.ShapeSheet.CellData<int> Type { get; set; }

        [System.Obsolete]
        public static IList<ConnectionPointCells> GetConnectionPoints(IVisio.Shape shape)
        {
            return ConnectionPointCells.GetCells(shape);
        }

        protected override void _Apply(VA.ShapeSheet.CellSectionDataGroup.ApplyFormula func, short row)
        {
            func(VA.ShapeSheet.SRCConstants.Connections_X.ForRow(row), this.X.Formula);
            func(VA.ShapeSheet.SRCConstants.Connections_Y.ForRow(row), this.Y.Formula);
            func(VA.ShapeSheet.SRCConstants.Connections_DirX.ForRow(row), this.DirX.Formula);
            func(VA.ShapeSheet.SRCConstants.Connections_DirY.ForRow(row), this.DirY.Formula);
            func(VA.ShapeSheet.SRCConstants.Connections_Type.ForRow(row), this.Type.Formula);
        }

        private static ConnectionPointCells get_cells_from_row(ConnectionPointQuery query, VA.ShapeSheet.Query.QueryDataSet<double> qds, int row)
        {
            var cells = new ConnectionPointCells();
            cells.X = qds.GetItem(row, query.X);
            cells.Y = qds.GetItem(row, query.Y);
            cells.DirX = qds.GetItem(row, query.DirX).ToInt();
            cells.DirY = qds.GetItem(row, query.DirY).ToInt();
            cells.Type = qds.GetItem(row, query.Type).ToInt();

            return cells;
        }

        internal static IList<List<ConnectionPointCells>> GetCells(IVisio.Page page, IList<int> shapeids)
        {
            var query = new ConnectionPointQuery();
            return VA.ShapeSheet.CellSectionDataGroup._GetCells(page, shapeids, query, get_cells_from_row);
        }

        internal static IList<ConnectionPointCells> GetCells(IVisio.Shape shape)
        {
            var query = new ConnectionPointQuery();
            return VA.ShapeSheet.CellSectionDataGroup._GetCells(shape, query, get_cells_from_row);
        }
    }
}