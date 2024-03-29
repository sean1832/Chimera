﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Chimera.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Chimera.Components.Geometry
{
    public class PolylineMesh : GH_Component
    {
        #region Metadata

        public PolylineMesh()
            : base("PolylineMesh", "PolyMesh",
                "Using polyline to construct a mesh",
                "Chimera", "Geometry")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "polylinemesh", "polylinemesh" };
        public override Guid ComponentGuid => new Guid("7E65648F-386D-49DF-AC3E-1125A0020331");
        protected override Bitmap Icon => Resources.PolyMesh;

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "input curve", GH_ParamAccess.item);
            pManager.AddIntegerParameter("PolylineSegment", "S", "Optional segment for the curve to divide.",
                GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "output mesh", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            int numSegments = 0;
            if (!DA.GetData(0, ref curve)) return;
            DA.GetData(1, ref numSegments);

            Polyline polyline;
            if (!curve.TryGetPolyline(out polyline))
            {
                // If the curve is not a polyline, divide it into points and re-construct polyline
                if (numSegments < 3)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Segment cannot be smaller than 3! Reset it to 3.");
                    numSegments = 3;
                }

                // Divide the curve into points
                Point3d[] points;
                curve.DivideByCount(numSegments, true, out points);
                polyline = new Polyline(points);
            }
            else
            {
                if (numSegments > 3)
                {
                    Point3d[] points;
                    polyline.ToNurbsCurve().DivideByCount(numSegments, true, out points);
                    polyline = new Polyline(points);
                }
            }
            if (!polyline.IsClosed)
            {
                // close the polyline
                polyline.Add(polyline[0]);
            }


            Mesh mesh = Rhino.Geometry.Mesh.CreateFromClosedPolyline(polyline);
            
            DA.SetData(0, mesh);
        }
    }
}