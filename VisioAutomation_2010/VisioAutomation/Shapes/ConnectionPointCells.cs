using IVisio = Microsoft.Office.Interop.Visio;
using System.Collections.Generic;
using VisioAutomation.ShapeSheet.CellGroups;
using VASS=VisioAutomation.ShapeSheet;

namespace VisioAutomation.Shapes
{
    public class ConnectionPointCells : CellGroup
    {
        public VASS.CellValueLiteral X { get; set; }
        public VASS.CellValueLiteral Y { get; set; }
        public VASS.CellValueLiteral DirX { get; set; }
        public VASS.CellValueLiteral DirY { get; set; }
        public VASS.CellValueLiteral Type { get; set; }

        public override IEnumerable<CellMetadataItem> CellMetadata
        {
            get
            {
                yield return this.Create(nameof(this.X), VASS.SrcConstants.ConnectionPointX, this.X);
                yield return this.Create(nameof(this.Y), VASS.SrcConstants.ConnectionPointY, this.Y);
                yield return this.Create(nameof(this.DirX), VASS.SrcConstants.ConnectionPointDirX, this.DirX);
                yield return this.Create(nameof(this.DirY), VASS.SrcConstants.ConnectionPointDirY, this.DirY);
                yield return this.Create(nameof(this.Type), VASS.SrcConstants.ConnectionPointType, this.Type);
            }
        }

        public static List<List<ConnectionPointCells>> GetCells(IVisio.Page page, IList<int> shapeids, VASS.CellValueType type)
        {
            var reader = ConnectionPointCells_lazy_builder.Value;
            return reader.GetCellsMultiRow(page, shapeids, type);
        }

        public static List<ConnectionPointCells> GetCells(IVisio.Shape shape, VASS.CellValueType type)
        {
            var reader = ConnectionPointCells_lazy_builder.Value;
            return reader.GetCellsMultiRow(shape, type);
        }

        private static readonly System.Lazy<ConnectionPointCellsBuilder> ConnectionPointCells_lazy_builder = new System.Lazy<ConnectionPointCellsBuilder>();

        class ConnectionPointCellsBuilder : CellGroupBuilder<ConnectionPointCells>
        {

            public ConnectionPointCellsBuilder() : base(CellGroupBuilderType.MultiRow)
            {
            }

            public override ConnectionPointCells ToCellGroup(ShapeSheet.Internal.ArraySegment<string> row, VisioAutomation.ShapeSheet.Query.ColumnList cols)
            {
                var cells = new ConnectionPointCells();
                var getcellvalue = VisioAutomation.ShapeSheet.CellGroups.CellGroup.gcf(row, cols);

                cells.X = getcellvalue(nameof(ConnectionPointCells.X));
                cells.Y = getcellvalue(nameof(ConnectionPointCells.Y));
                cells.DirX = getcellvalue(nameof(ConnectionPointCells.DirX));
                cells.DirY = getcellvalue(nameof(ConnectionPointCells.DirY));
                cells.Type = getcellvalue(nameof(ConnectionPointCells.Type));

                return cells;
            }
        }

    }
}