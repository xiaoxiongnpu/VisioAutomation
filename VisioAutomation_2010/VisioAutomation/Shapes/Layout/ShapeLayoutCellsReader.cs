using VisioAutomation.ShapeSheet.CellGroups;
using VisioAutomation.ShapeSheet.Query;
using SRCCON = VisioAutomation.ShapeSheet.SRCConstants;

namespace VisioAutomation.Shapes.Layout
{
    class ShapeLayoutCellsReader : SingleRowReader<Shapes.Layout.ShapeLayoutCells>
    {
        public CellColumn ConFixedCode { get; set; }
        public CellColumn ConLineJumpCode { get; set; }
        public CellColumn ConLineJumpDirX { get; set; }
        public CellColumn ConLineJumpDirY { get; set; }
        public CellColumn ConLineJumpStyle { get; set; }
        public CellColumn ConLineRouteExt { get; set; }
        public CellColumn ShapeFixedCode { get; set; }
        public CellColumn ShapePermeablePlace { get; set; }
        public CellColumn ShapePermeableX { get; set; }
        public CellColumn ShapePermeableY { get; set; }
        public CellColumn ShapePlaceFlip { get; set; }
        public CellColumn ShapePlaceStyle { get; set; }
        public CellColumn ShapePlowCode { get; set; }
        public CellColumn ShapeRouteStyle { get; set; }
        public CellColumn ShapeSplit { get; set; }
        public CellColumn ShapeSplittable { get; set; }
        public CellColumn DisplayLevel { get; set; }
        public CellColumn Relationships { get; set; }

        public ShapeLayoutCellsReader() 
        {
            this.ConFixedCode = this.query.AddCell(SRCCON.ConFixedCode, nameof(SRCCON.ConFixedCode));
            this.ConLineJumpCode = this.query.AddCell(SRCCON.ConLineJumpCode, nameof(SRCCON.ConLineJumpCode));
            this.ConLineJumpDirX = this.query.AddCell(SRCCON.ConLineJumpDirX, nameof(SRCCON.ConLineJumpDirX));
            this.ConLineJumpDirY = this.query.AddCell(SRCCON.ConLineJumpDirY, nameof(SRCCON.ConLineJumpDirY));
            this.ConLineJumpStyle = this.query.AddCell(SRCCON.ConLineJumpStyle, nameof(SRCCON.ConLineJumpStyle));
            this.ConLineRouteExt = this.query.AddCell(SRCCON.ConLineRouteExt, nameof(SRCCON.ConLineRouteExt));
            this.ShapeFixedCode = this.query.AddCell(SRCCON.ShapeFixedCode, nameof(SRCCON.ShapeFixedCode));
            this.ShapePermeablePlace = this.query.AddCell(SRCCON.ShapePermeablePlace, nameof(SRCCON.ShapePermeablePlace));
            this.ShapePermeableX = this.query.AddCell(SRCCON.ShapePermeableX, nameof(SRCCON.ShapePermeableX));
            this.ShapePermeableY = this.query.AddCell(SRCCON.ShapePermeableY, nameof(SRCCON.ShapePermeableY));
            this.ShapePlaceFlip = this.query.AddCell(SRCCON.ShapePlaceFlip, nameof(SRCCON.ShapePlaceFlip));
            this.ShapePlaceStyle = this.query.AddCell(SRCCON.ShapePlaceStyle, nameof(SRCCON.ShapePlaceStyle));
            this.ShapePlowCode = this.query.AddCell(SRCCON.ShapePlowCode, nameof(SRCCON.ShapePlowCode));
            this.ShapeRouteStyle = this.query.AddCell(SRCCON.ShapeRouteStyle, nameof(SRCCON.ShapeRouteStyle));
            this.ShapeSplit = this.query.AddCell(SRCCON.ShapeSplit, nameof(SRCCON.ShapeSplit));
            this.ShapeSplittable = this.query.AddCell(SRCCON.ShapeSplittable, nameof(SRCCON.ShapeSplittable));
            this.DisplayLevel = this.query.AddCell(SRCCON.DisplayLevel, nameof(SRCCON.DisplayLevel));
            this.Relationships = this.query.AddCell(SRCCON.Relationships, nameof(SRCCON.Relationships));


        }

        public override Shapes.Layout.ShapeLayoutCells CellDataToCellGroup(VisioAutomation.Utilities.ArraySegment<ShapeSheet.CellData> row)
        {
            var cells = new Shapes.Layout.ShapeLayoutCells();
            cells.ConFixedCode = row[this.ConFixedCode];
            cells.ConLineJumpCode = row[this.ConLineJumpCode];
            cells.ConLineJumpDirX = row[this.ConLineJumpDirX];
            cells.ConLineJumpDirY = row[this.ConLineJumpDirY];
            cells.ConLineJumpStyle = row[this.ConLineJumpStyle];
            cells.ConLineRouteExt = row[this.ConLineRouteExt];
            cells.ShapeFixedCode = row[this.ShapeFixedCode];
            cells.ShapePermeablePlace = row[this.ShapePermeablePlace];
            cells.ShapePermeableX = row[this.ShapePermeableX];
            cells.ShapePermeableY = row[this.ShapePermeableY];
            cells.ShapePlaceFlip = row[this.ShapePlaceFlip];
            cells.ShapePlaceStyle = row[this.ShapePlaceStyle];
            cells.ShapePlowCode = row[this.ShapePlowCode];
            cells.ShapeRouteStyle = row[this.ShapeRouteStyle];
            cells.ShapeSplit = row[this.ShapeSplit];
            cells.ShapeSplittable = row[this.ShapeSplittable];
            cells.DisplayLevel = row[this.DisplayLevel];
            cells.Relationships = row[this.Relationships];
            return cells;
        }
    }
}