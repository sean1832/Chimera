using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Chimera.Components
{
    public class CurvesContainment : GH_Component
    {
        #region Metadata

        public CurvesContainment()
        : base("CurvesContainment", "CrvContain",
              "Partition a list of curves into interior and exterior curves.",
              "Chimera", "Geometry")
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

            Params.Input[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Interior Curves", "IntC", "The interior curves.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Exterior Curves", "ExtC", "The exterior curves.", GH_ParamAccess.list);
        }

        #endregion

        #region ClassVariables

        private int _sampleCount = 2;
        public int SampleCount
        {
            get => _sampleCount;
            set
            {
                _sampleCount = value;
                Message = $"SampleCount: {_sampleCount}";
            }
        }

        #endregion

        #region (De)Serialization

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // First add our own field.
            writer.SetInt32("SampleCount", SampleCount);
            // Then call the base class implementation.
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // First read our own field.
            try
            {
                SampleCount = reader.GetInt32("SampleCount");
            }
            catch (Exception e)
            {
                // default value
                SampleCount = 2;
            }
            // Then call the base class implementation.
            return base.Read(reader);
        }

        #endregion

        #region Context Menu

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "SamplePoints 2", Toggle2, true).Checked = SampleCount == 2;
            Menu_AppendItem(menu, "SamplePoints 20", Toggle20, true).Checked = SampleCount == 20;
            Menu_AppendItem(menu, "SamplePoints 100", Toggle100, true).Checked = SampleCount == 100;
        }

        private void Toggle2(object sender, EventArgs e)
        {
            RecordUndoEvent("SamplePoints 2");
            SampleCount = 2;
            ExpireSolution(true);
        }
        private void Toggle20(object sender, EventArgs e)
        {
            RecordUndoEvent("SamplePoints 20");
            SampleCount = 20;
            ExpireSolution(true);
        }
        private void Toggle100(object sender, EventArgs e)
        {
            RecordUndoEvent("SamplePoints 100");
            SampleCount = 100;
            ExpireSolution(true);
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

            (List<Curve> cleanInteriorCrvs, List<Curve> cleanExteriorCrvs) = RemoveDuplicates(interiorCrvs, exteriorCrvs);

            // Set output data for the Grasshopper component
            DA.SetDataList(0, cleanInteriorCrvs);
            DA.SetDataList(1, cleanExteriorCrvs);
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
            List<Curve> parallelCrvs = new List<Curve>(inputCurves);
            // test containment
            for (int i = 0; i < inputCurves.Count; i++)
            {
                Curve crvA = inputCurves[i];
                for (int j = i + 1; j < inputCurves.Count; j++)
                {
                    Curve crvB = inputCurves[j];
                    RegionContainment containment = Curve.PlanarClosedCurveRelationship(crvA, crvB, Plane.WorldXY, 0.001);
                    switch (containment)
                    {
                        case RegionContainment.AInsideB:
                            interiorCrvs.Add(crvA);
                            exteriorCrvs.Add(crvB);

                            // remove crvA from parallelCrvs
                            parallelCrvs.Remove(crvA);
                            // remove crvB from parallelCrvs
                            parallelCrvs.Remove(crvB);

                            break;
                        case RegionContainment.BInsideA:
                            interiorCrvs.Add(crvB);
                            exteriorCrvs.Add(crvA);

                            // remove crvA from parallelCrvs
                            parallelCrvs.Remove(crvA);
                            // remove crvB from parallelCrvs
                            parallelCrvs.Remove(crvB);
                            break;
                        case RegionContainment.Disjoint:
                            break;
                        case RegionContainment.MutualIntersection:
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input curves contain mutual intersections.");
                            break;
                    }
                }
            }
            exteriorCrvs.AddRange(parallelCrvs);
            
        }

        private (List<Curve>, List<Curve>) RemoveDuplicates(List<Curve> curvesA, List<Curve> curvesB)
        {
            List<Curve> uniqueCurves = new List<Curve>();
            List<Curve> cleanedCurvesA = new List<Curve>();
            List<Curve> cleanedCurvesB = new List<Curve>();
            double tolerance = 0.001;

            // Add unique curves from the first list
            foreach (Curve curve in curvesA)
            {
                if (!IsDuplicate(curve, uniqueCurves, tolerance))
                {
                    uniqueCurves.Add(curve);
                    cleanedCurvesA.Add(curve);
                }
            }

            // Add unique curves from the second list
            foreach (Curve curve in curvesB)
            {
                if (!IsDuplicate(curve, uniqueCurves, tolerance))
                {
                    uniqueCurves.Add(curve);
                    cleanedCurvesB.Add(curve);
                }
            }

            return (cleanedCurvesA, cleanedCurvesB);
        }

        private bool IsDuplicate(Curve curve, List<Curve> uniqueCurves, double tolerance)
        {
            foreach (Curve uniqueCurve in uniqueCurves)
            {
                if (AreCurvesAlmostEqual(curve, uniqueCurve, tolerance))
                {
                    return true;
                }
            }
            return false;
        }


        private bool AreCurvesAlmostEqual(Curve curve1, Curve curve2, double tolerance)
        {
            if (curve1 == null || curve2 == null) return false;

            if (curve1.IsPeriodic || curve2.IsPeriodic)
            {
                curve1 = curve1.ToNurbsCurve();
                curve2 = curve2.ToNurbsCurve();
            }

            for (int i = 0; i < SampleCount; i++)
            {
                double t = (double)i / (SampleCount - 1);
                Point3d pt1 = curve1.PointAtNormalizedLength(t);
                Point3d pt2 = curve2.PointAtNormalizedLength(t);

                if (pt1.DistanceTo(pt2) > tolerance)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}