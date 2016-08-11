using IVisio = Microsoft.Office.Interop.Visio;
using System.Collections.Generic;
using VisioAutomation.ShapeSheet;

namespace VisioAutomation.ShapeSheetQuery.CellGroups
{
    public abstract class CellGroup : BaseCellGroup
    {
        private static void check_query(CellQuery query)
        {
            if (query.CellColumns.Count < 1)
            {
                throw new AutomationException("Query must contain at least 1 Column");
            }

            if (query.SectionColumns.Count != 0)
            {
                throw new AutomationException("Query should not contain contain any sections");
            }
        }

        protected static IList<T> _GetCells<T, RT>(
            IVisio.Page page, IList<int> shapeids,
            CellQuery query,
            RowToObject<T, RT> row_to_object)
        {
            check_query(query);

            var surface = new ShapeSheet.ShapeSheetSurface(page);
            var data_for_shapes = query.GetCellData<RT>( surface, shapeids);
            var list = new List<T>(shapeids.Count);
            foreach (var data_for_shape in data_for_shapes)
            {
                var srr = new SectionResultRow<CellData<RT>>(data_for_shape.Cells);
                var cells = row_to_object(srr);
                list.Add(cells);
            }
            return list;
        }

        protected static T _GetCells<T, RT>(
            IVisio.Shape shape,
            CellQuery query,
            RowToObject<T, RT> row_to_object)
        {
            check_query(query);

            QueryResult<CellData<RT>> data_for_shape = query.GetCellData<RT>(shape);
            var srr = new SectionResultRow<CellData<RT>>(data_for_shape.Cells);
            var cells = row_to_object(srr);
            return cells;
        }
    }
}