using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Monkey.src.Components
{
    public class SortCurves : GH_Component
    {
        #region Metadata

        public SortCurves()
          : base("SortCurves", "SortCrv",
              "Sorts curves into X, Y, Z, and other directions",
              "Monkey", "Geometry")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "sortcrv", "sortcrvbydirection" };
        protected override System.Drawing.Bitmap Icon => null; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("1808ae91-ccf7-4706-ad52-c7c49feb2c64");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "C", "The curves to analyze.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "T", "Tolerance of axis alignment.\nBetween 0 to 1", GH_ParamAccess.item, 0.9);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("X Curves", "X", "The curves in the X direction.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Y Curves", "Y", "The curves in the Y direction.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Z Curves", "Z", "The curves in the Z direction.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Other Curves", "O", "The curves in other directions.", GH_ParamAccess.list);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> inputCurves = new List<Curve>();
            double tolerance = 0.9;
            DA.GetDataList(0, inputCurves);
            DA.GetData(1, ref tolerance);

            (List<Curve> xDir, List<Curve> yDir, List<Curve> zDir, List<Curve> otherDir) sortedCurves = SortCurvesByDirection(inputCurves, tolerance);

            DA.SetDataList(0, sortedCurves.xDir);
            DA.SetDataList(1, sortedCurves.yDir);
            DA.SetDataList(2, sortedCurves.zDir);
            DA.SetDataList(3, sortedCurves.otherDir);
        }

        #region Additional

        private (List<Curve> xDir, List<Curve> yDir, List<Curve> zDir, List<Curve> otherDir) SortCurvesByDirection(List<Curve> curves, double tolerance)
        {
            List<Curve> xDir = new List<Curve>();
            List<Curve> yDir = new List<Curve>();
            List<Curve> zDir = new List<Curve>();
            List<Curve> otherDir = new List<Curve>();

            foreach (Curve curve in curves)
            {
                Vector3d tangent = curve.TangentAtStart;
                tangent.Unitize();

                if (Math.Abs(tangent.X) > tolerance)
                    xDir.Add(curve);
                else if (Math.Abs(tangent.Y) > tolerance)
                    yDir.Add(curve);
                else if (Math.Abs(tangent.Z) > tolerance)
                    zDir.Add(curve);
                else
                    otherDir.Add(curve);
            }

            return (xDir, yDir, zDir, otherDir);
        }

        #endregion
    }
}