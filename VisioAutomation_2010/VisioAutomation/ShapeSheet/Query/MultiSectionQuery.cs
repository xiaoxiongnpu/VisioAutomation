using System.Collections.Generic;
using System.Linq;
using IVisio = Microsoft.Office.Interop.Visio;
using VASS = VisioAutomation.ShapeSheet;

namespace VisioAutomation.ShapeSheet.Query
{
    public class MultiSectionQuery
    {
        public SectionQueryList SectionQueries { get; }

        private SectionCache _sectioncache;

        public MultiSectionQuery()
        {
            this.SectionQueries = new SectionQueryList(0);
        }

        private static void RestrictToShapesOnly(SurfaceTarget surface)
        {
            if (surface.Shape == null)
            {
                string msg = "Target must be Shape not Page or Master";
                throw new System.ArgumentException(msg);
            }
        }

        public MultiSectionOutput<string> GetFormulas(SurfaceTarget surface)
        {
            RestrictToShapesOnly(surface);

            this.CacheSectionInfoForAllShapes(surface, new[] { surface.Shape.ID });

            var srcstream = this._build_src_stream();
            var values = surface.GetFormulasU(srcstream);
            var shape_index = 0;
            var shape_cache_item = _sectioncache[shape_index];
            var reader = new VASS.Internal.ArraySegmentReader<string>(values);
            var output_for_shape = this._create_output_for_shape(surface.ID16, shape_cache_item, reader);

            return output_for_shape;
        }

        public MultiSectionOutput<TResult> GetResults<TResult>(IVisio.Shape shape)
        {
            var surface = new SurfaceTarget(shape);
            return GetResults<TResult>(surface);
        }

        public MultiSectionOutput<TResult> GetResults<TResult>(SurfaceTarget surface)
        {
            RestrictToShapesOnly(surface);

            this.CacheSectionInfoForAllShapes(surface, new[] { surface.Shape.ID });

            var srcstream = this._build_src_stream();
            const object[] unitcodes = null;
            var values = surface.GetResults<TResult>(srcstream, unitcodes);
            var shape_index = 0;
            var sectioncache = _sectioncache[shape_index];
            var reader = new VASS.Internal.ArraySegmentReader<TResult>(values);
            var output_for_shape = this._create_output_for_shape(surface.ID16, sectioncache, reader);
            return output_for_shape;
        }

        public MultiSectionOuputList<string> GetFormulas(IVisio.Page page, IList<int> shapeids)
        {
            var surface = new SurfaceTarget(page);
            return this.GetFormulas(surface, shapeids);
        }


        public MultiSectionOuputList<TResult> GetResults<TResult>(IVisio.Page page, IList<int> shapeids)
        {
            var surface = new SurfaceTarget(page);
            return this.GetResults<TResult>(surface, shapeids);
        }

        public MultiSectionOuputList<TResult> GetResults<TResult>(SurfaceTarget surface, IList<int> shapeids)
        {
            // Store information about the sections we need to query
            CacheSectionInfoForAllShapes(surface, shapeids);

            // Perform the query
            var srcstream = this._build_sidsrc_stream(shapeids);
            const object[] unitcodes = null;
            var values = surface.GetResults<TResult>(srcstream, unitcodes);
            var reader = new VASS.Internal.ArraySegmentReader<TResult>(values);
            var list_sectionoutput = this._create_outputs_for_shapes(shapeids, _sectioncache, reader);
            return list_sectionoutput;
        }
        public MultiSectionOuputList<string> GetFormulas(SurfaceTarget surface, IList<int> shapeids)
        {
            // Store information about the sections we need to query
            CacheSectionInfoForAllShapes(surface, shapeids);

            // Perform the query
            var srcstream = this._build_sidsrc_stream(shapeids);
            var values = surface.GetFormulasU(srcstream);
            var reader = new VASS.Internal.ArraySegmentReader<string>(values);
            var list_sectionoutput = this._create_outputs_for_shapes(shapeids, _sectioncache, reader);
            return list_sectionoutput;
        }

        private void CacheSectionInfoForAllShapes(SurfaceTarget surface, IList<int> shape_ids)
        {
            // Prepare a cache object
            if (this.SectionQueries.Count < 1)
            {
                this._sectioncache = new SectionCache(0);
            }
            this._sectioncache = new SectionCache();

            // For each shape, for each section find the number of rows
            foreach (var shape_id in shape_ids)
            {
                // Retrieve the actual shape object from the surface. 
                // this is needed to find the number of rows for sections i that shape
                var shape = surface.Shapes.ItemFromID16[(short)shape_id];

                // For that shape, fill in the section cache for each section that
                // needs to be queried
                var shapecacheitems = new ShapeCacheItemList(this.SectionQueries.Count);
                foreach (var section_query in this.SectionQueries)
                {
                    var shapecacheitem = section_query.GetShapeCacheItem(shape);
                    shapecacheitems.Add(shapecacheitem);
                }

                // For this shape, add the accumulated info into the cache
                _sectioncache.AddSectionInfosForShape(shapecacheitems);
            }

            // Ensure that we have created a cache for eash shapes
            if (shape_ids.Count != _sectioncache.CountShapes)
            {
                string msg = string.Format("mismatch in number of shapes and information collected for shapes");
                throw new Exceptions.InternalAssertionException(msg);
            }
        }


        private MultiSectionOuputList<T> _create_outputs_for_shapes<T>(IList<int> shapeids, SectionCache sectioncache, VASS.Internal.ArraySegmentReader<T> segReader)
        {
            var output_for_all_shapes = new MultiSectionOuputList<T>();

            for (int shape_index = 0; shape_index < shapeids.Count; shape_index++)
            {
                var shapeid = shapeids[shape_index];
                var shapecacheitems = sectioncache[shape_index];
                var output_for_shape = this._create_output_for_shape((short)shapeid, shapecacheitems, segReader);
                output_for_all_shapes.Add(output_for_shape);
            }

            return output_for_all_shapes;
        }

        private MultiSectionOutput<T> _create_output_for_shape<T>(short shapeid, ShapeCacheItemList shapecacheitems, VASS.Internal.ArraySegmentReader<T> segReader)
        {
            int original_seg_size = segReader.Count;

            if (shapecacheitems==null)
            {
                throw new VisioAutomation.Exceptions.InternalAssertionException();
            }

            int results_cell_count = shapecacheitems.CountCells(); 

            List<SectionOutput<T>> sections = null;
            if (shapecacheitems != null)
            {
                sections = new List<SectionOutput<T>>(shapecacheitems.Count);
                foreach (var shapecacheitem in shapecacheitems)
                {
                    var section_output = new SectionOutput<T>(shapecacheitem.RowCount, shapecacheitem.SectionQuery.SectionIndex);

                    int num_cols = shapecacheitem.SectionQuery.Columns.Count;
                    foreach (int row_index in shapecacheitem.RowIndexes)
                    {
                        var segment = segReader.GetNextSegment(num_cols);
                        var sec_res_row = new SectionOutputRow<T>(segment, shapecacheitem.SectionQuery.SectionIndex, row_index);
                        section_output.Rows.Add(sec_res_row);
                    }

                    sections.Add(section_output);
                }
            }

            var output = new MultiSectionOutput<T>(shapeid, results_cell_count, sections);

            int final_seg_size = segReader.Count;

            if ((final_seg_size - original_seg_size) != output.__totalcellcount)
            {
                throw new Exceptions.InternalAssertionException("Unexpected cursor");
            }

            return output;
        }

        private Streams.StreamArray _build_src_stream()
        {
            int dummy_shapeid = -1;
            int shapeindex = 0;
            int numcells = this._sectioncache.CountCells();
            var stream = new VASS.Streams.SrcStreamArrayBuilder(numcells);
            var sidsrcs = this._enum_sidsrcs(dummy_shapeid, shapeindex);
            var srcs = sidsrcs.Select(i => i.Src);
            stream.AddRange(srcs);

            return stream.ToStreamArray();
        }

        private VASS.Streams.StreamArray _build_sidsrc_stream(IList<int> shapeids)
        {
            int numcells = this._sectioncache.CountCells();

            var stream = new VASS.Streams.SidSrcStreamArrayBuilder(numcells);

            for (int shapeindex = 0; shapeindex < shapeids.Count; shapeindex++)
            {
                // For each shape add the cells to query
                var shapeid = shapeids[shapeindex];
                var sidsrcs = this._enum_sidsrcs(shapeid, shapeindex);
                stream.AddRange(sidsrcs);
            }

            return stream.ToStreamArray();
        }

        private IEnumerable<SidSrc> _enum_sidsrcs(int shape_id, int shapeindex)
        {
            if (this._sectioncache.CountShapes < 1)
            {
                yield break;
            }

            var shapecacheitems = _sectioncache[shapeindex];
            foreach (var shapecacheitem in shapecacheitems)
            {
                foreach (int row_index in shapecacheitem.RowIndexes)
                {
                    var cols = shapecacheitem.SectionQuery.Columns;
                    var section_index = shapecacheitem.SectionQuery.SectionIndex;
                    foreach (var col in cols)
                    {
                        var sidsrc = new VASS.SidSrc(
                            (short)shape_id,
                            (short)section_index,
                            (short)row_index,
                            col.Src.Cell);
                        yield return sidsrc;
                    }
                }
            }
        }
    }
}