using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;

namespace Monkey.src.Components
{
    public class CurvesContainment : GH_Component
    {
        #region Metadata

        public CurvesContainment()
        : base("CurvesContainment", "CrvContain",
              "Partition a list of curves into interior and exterior curves.",
              "Monkey", "Geometry")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "containcurve", "contain curve", "crvcontain" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.CurveContainment; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("3a2c7cb5-70da-4401-8ce5-efe8630d2111");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "Crvs", "The curves to analyze.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Interior Curves", "IntC", "The interior curves.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Exterior Curves", "ExtC", "The exterior curves.", GH_ParamAccess.list);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Retrieve input curves from the Grasshopper component
            List<Curve> inputCurves = new List<Curve>();
            DA.GetDataList(0, inputCurves);

            // Validate input curves for self-intersections and mutual intersections
            if (!ValidateInputCurves(inputCurves))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input curves contain self-intersections or mutual intersections.");
                return;
            }

            // Initialize lists to store partitioned curves
            List<Curve> exteriorCrvs = new List<Curve>();
            List<Curve> interiorCrvs = new List<Curve>();

            // Partition input curves into exterior and interior curves
            PartitionCurves(inputCurves, ref exteriorCrvs, ref interiorCrvs);

            // Set output data for the Grasshopper component
            DA.SetDataList(0, interiorCrvs);
            DA.SetDataList(1, exteriorCrvs);
        }

        #region Additional

        private bool ValidateInputCurves(List<Curve> curves)
        {
            // Iterate through input curves to check for self-intersections
            for (int i = 0; i < curves.Count; i++)
            {
                if (Intersection.CurveSelf(curves[i], 0.001).Count > 0)
                {
                    return false;
                }

                // Check for mutual intersections between input curves
                for (int j = i + 1; j < curves.Count; j++)
                {
                    if (Intersection.CurveCurve(curves[i], curves[j], 0.001, 0.001).Count > 0)
                    {
                        return false;
                    }
                }
            }

            // Return true if no self-intersections or mutual intersections are found
            return true;
        }

        private void PartitionCurves(List<Curve> inputCurves, ref List<Curve> exteriorCrvs, ref List<Curve> interiorCrvs)
        {
            // Initialize a list of unprocessed curves, starting with all input curves
            List<Curve> unprocessedCrvs = new List<Curve>(inputCurves);

            // Find the most exterior curve
            Curve mostExteriorCurve = FindMostExteriorCurve(inputCurves);

            // Add the most exterior curve to the exterior curves list
            exteriorCrvs.Add(mostExteriorCurve);
            unprocessedCrvs.Remove(mostExteriorCurve);

            // Iterate through unprocessed curves to determine their containment relationships
            foreach (Curve curve in unprocessedCrvs)
            {
                // Determine if the curve is inside the most exterior curve
                Point3d curveCenter = curve.GetBoundingBox(false).Center;
                if (mostExteriorCurve.Contains(curveCenter, Plane.WorldXY, 0.001) == PointContainment.Inside)
                {
                    interiorCrvs.Add(curve);
                }
                else
                {
                    exteriorCrvs.Add(curve);
                }
            }
        }

        private Curve FindMostExteriorCurve(List<Curve> curves)
        {
            Curve largestCurve = null;
            double largestArea = double.MinValue;

            foreach (Curve curve in curves)
            {
                double area = AreaMassProperties.Compute(curve).Area;
                if (area > largestArea)
                {
                    largestArea = area;
                    largestCurve = curve;
                }
            }

            return largestCurve;
        }
        #endregion
    }
}