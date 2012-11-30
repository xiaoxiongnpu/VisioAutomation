using System.Linq;
using VA = VisioAutomation;
using SMA = System.Management.Automation;

namespace VisioPS.Commands
{
    [SMA.Cmdlet(SMA.VerbsCommon.New, "VisioNURBS")]
    public class New_VisioNURBS : VisioPS.VisioPSCmdlet
    {
        [SMA.Parameter(Position = 0, Mandatory = true)]
        public double[] ControlPoints { get; set; }

        [SMA.Parameter(Position = 1, Mandatory = true)]
        public double[] Knots { get; set; }

        [SMA.Parameter(Position = 2, Mandatory = true)]
        public double[] Weights { get; set; }

        [SMA.Parameter(Position = 3, Mandatory = true)]
        public int Degree { get; set; }
        

        protected override void ProcessRecord()
        {
            var scriptingsession = this.ScriptingSession;
            var points = VA.Drawing.Point.FromDoubles(this.ControlPoints).ToList();
            var shape = scriptingsession.Draw.NURBSCurve(points,this.Knots,this.Weights,this.Degree);
            this.WriteObject(shape);
        }
    }
}