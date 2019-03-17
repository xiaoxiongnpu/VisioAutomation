﻿using VASS = VisioAutomation.ShapeSheet;
using IVisio = Microsoft.Office.Interop.Visio;
using System.Collections.Generic;
using System.Linq;

namespace VisioAutomation.ShapeSheet.Query
{

    public class ShapeCellsRow<T> : IEnumerable<T>
    {
        public int ShapeID { get; private set; }
        private readonly VASS.Internal.ArraySegment<T> Cells;

        internal ShapeCellsRow(int shapeid, VASS.Internal.ArraySegment<T> cells)
        {
            this.ShapeID = shapeid;
            this.Cells = cells;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.Cells.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.Cells.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                return this.Cells[index];
            }
        }
    }

    public class ShapeSectionCellsRow<T> : ShapeCellsRow<T>
    {
        public readonly IVisio.VisSectionIndices SectionIndex;
        public readonly int RowIndex;

        internal ShapeSectionCellsRow(int shapeid, VASS.Internal.ArraySegment<T> cells, IVisio.VisSectionIndices secindex, int rowindex) : base(shapeid,cells)
        {
            this.SectionIndex = secindex;
            this.RowIndex = rowindex;
        }

    }
}