using IVisio = Microsoft.Office.Interop.Visio;
using VA = VisioAutomation;
using TABLE = VisioAutomation.ShapeSheet.Data.Table<VisioAutomation.ShapeSheet.CellData<double>>;

namespace VisioAutomation.ShapeSheet.CellGroups
{
    public abstract class BaseCellGroup
    {
        public delegate void ApplyFormula(VA.ShapeSheet.SRC src, VA.ShapeSheet.FormulaLiteral formula);
    }
}