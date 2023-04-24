using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Chimera.Components
{
    public class GroundPlane : GH_Component
    {
        #region Metadata

        public GroundPlane()
          : base("GroundPlane", "Ground",
              "Find the ground plane of a given geometry.",
              "Chimera", "Geometry")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "ground", "ground", "footrec", "getbase" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GroundPlane; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("7bc3efe2-c16c-4168-8aac-d1cec0c080ad");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to get ground plane", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Base Geo", "Base", "Base geometry.", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Ground Plane", "Pln", "Ground plane", GH_ParamAccess.item);
            pManager.AddPointParameter("Ground Center Point", "Pt", "Ground Center Point", GH_ParamAccess.item);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase Geometry = null;
            if (!DA.GetData(0, ref Geometry)) return;

            // Get the bounding box of the geometry
            BoundingBox bbox = Geometry.GetBoundingBox(true);

            // Deconstruct the bounding box into 6 BRep faces
            Box box = new Box(bbox); // Create a Box from the BoundingBox
            List<BrepFace> boxFaces = box.ToBrep().Faces.ToList();

            // Sort BRep faces by their Z value
            boxFaces.Sort((a, b) =>
            {
                Point3d centerA = a.GetBoundingBox(true).Center;
                Point3d centerB = b.GetBoundingBox(true).Center;
                return centerA.Z.CompareTo(centerB.Z);
            });

            // Get the ground geometry with the smallest Z value BRep face
            // cast the BRep face to a Brep
            Brep groundGeo = boxFaces[0].ToBrep();
            Plane groundPlane;
            groundGeo.Faces[0].TryGetPlane(out groundPlane); // Get the plane of the face
            Point3d groundCenterPoint = groundGeo.Faces[0].GetBoundingBox(true).Center;

            // Output the results
            DA.SetData(0, groundGeo);
            DA.SetData(1, groundPlane);
            DA.SetData(2, groundCenterPoint);
        }

        #region Additional



        #endregion
    }
}