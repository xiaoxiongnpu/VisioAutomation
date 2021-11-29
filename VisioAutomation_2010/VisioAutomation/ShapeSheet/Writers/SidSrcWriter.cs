﻿using IVisio = Microsoft.Office.Interop.Visio;
using VA= VisioAutomation;
using VisioAutomation.Extensions;
using VisioAutomation.Internal;

namespace VisioAutomation.ShapeSheet.Writers
{
    public class SidSrcWriter : WriterBase
    {
        public SidSrcWriter() : base(VA.ShapeSheet.Streams.StreamType.SidSrc)
        {
        }

        public void Commit(IVisio.Page page, Core.CellValueType type)
        {
            var visobjtarget = new VisioObjectTarget(page);
            this.Commit(visobjtarget, type);
        }
        public void SetValue(short id, Core.Src src, Core.CellValue formula)
        {
            var sidsrc = new Core.SidSrc(id, src);
            this.__SetValueIgnoreNull(sidsrc, formula);
        }

        public void SetValue(Core.SidSrc sidsrc, Core.CellValue formula)
        {
            this.__SetValueIgnoreNull(sidsrc, formula);
        }

        public void SetValues(short id, CellGroups.CellGroup cellgroup, short row)
        {
            var pairs = cellgroup.GetSidSrcValuePairs_NewRow(id, row);
            foreach (var pair in pairs)
            {
                this.SetValue(pair.ShapeID, pair.Src, pair.Value);
            }
        }

        public void Commit(IVisio.Page page, object formula)
        {
            throw new System.NotImplementedException();
        }

        public void SetValues(short id, CellGroups.CellGroup cellgroup)
        {
            foreach (var pair in cellgroup.GetSrcValuePairs())
            {
                this.SetValue(id, pair.Src, pair.Value);
            }
        }

        private void __SetValueIgnoreNull(Core.SidSrc sidsrc, Core.CellValue formula)
        {
            if (this._records == null)
            {
                this._records = new WriteRecordList(VA.ShapeSheet.Streams.StreamType.SidSrc);
            }

            if (formula.HasValue)
            {
                this._records.Add(sidsrc, formula.Value);
            }
        }

        public void CommitFormulas(VisioObjectTarget visobjtarget)
        {
            if ((this._records == null || this._records.Count < 1))
            {
                return;
            }

            var stream = this._records.BuildStreamArray(VA.ShapeSheet.Streams.StreamType.SidSrc);
            var formulas = this._records.BuildValuesArray();

            if (stream.Array.Length == 0)
            {
                throw new Exceptions.InternalAssertionException();
            }

            var flags = this._compute_setformula_flags();

            var c = visobjtarget.Dispatch_Func<int>(
                (shape) => (shape.SetFormulas(stream, formulas, (short)flags)),
                (master) => (master.SetFormulas(stream, formulas, (short)flags)),
                (page) => (page.SetFormulas(stream, formulas, (short)flags)));
        }

        public void CommitFormulas(IVisio.Shape shape)
        {
            if ((this._records == null || this._records.Count < 1))
            {
                return;
            }

            var stream = this._records.BuildStreamArray(VA.ShapeSheet.Streams.StreamType.SidSrc);
            var formulas = this._records.BuildValuesArray();

            if (stream.Array.Length == 0)
            {
                throw new Exceptions.InternalAssertionException();
            }

            var flags = this._compute_setformula_flags();

            int c = shape.SetFormulas(stream, formulas, (short)flags);
        }

        public void CommitFormulas(IVisio.Page page)
        {
            if ((this._records == null || this._records.Count < 1))
            {
                return;
            }

            var stream = this._records.BuildStreamArray(VA.ShapeSheet.Streams.StreamType.SidSrc);
            var formulas = this._records.BuildValuesArray();

            if (stream.Array.Length == 0)
            {
                throw new Exceptions.InternalAssertionException();
            }

            var flags = this._compute_setformula_flags();

            int c = page.SetFormulas(stream, formulas, (short)flags);
        }

        public void Commit(VisioObjectTarget visobjtarget, Core.CellValueType type)
        {
            if ((this._records == null || this._records.Count < 1))
            {
                return;
            }

            var stream = this._records.BuildStreamArray(VA.ShapeSheet.Streams.StreamType.SidSrc);
            var values = this._records.BuildValuesArray();

            if (stream.Array.Length == 0)
            {
                throw new Exceptions.InternalAssertionException();
            }

            if (type == Core.CellValueType.Formula)
            {
                var flags = this._compute_setformula_flags();
                var c = visobjtarget.Dispatch_Func<int>(
                    (shape) => (shape.SetFormulas(stream, values, (short)flags)),
                    (master) => (master.SetFormulas(stream, values, (short)flags)),
                    (page) => (page.SetFormulas(stream, values, (short)flags)));
            }
            else
            {
                const object[] unitcodes = null;
                var flags = this._compute_setresults_flags();

                var res = visobjtarget.Dispatch_Func<int>(
                    (shape) => (shape.SetResults(stream, unitcodes, values, (short)flags)),
                    (master) => (master.SetResults(stream, unitcodes, values, (short)flags)),
                    (page) => (page.SetResults(stream, unitcodes, values, (short)flags)));
            }
        }
    }
}