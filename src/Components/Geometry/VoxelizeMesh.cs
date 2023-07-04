using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Chimera.Components.Utility;
using Grasshopper.Kernel;
using Rhino;
using Rhino.FileIO;
using Rhino.Geometry;

namespace Chimera.Components.Geometry
{
    public class VoxelizeMesh : GH_Component
    {
        #region Metadata

        public VoxelizeMesh()
          : base("Voxelize Mesh", "Voxelize",
              "Create a voxelized grid of points from a given mesh",
              "Chimera", "Geometry")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "voxelize", "Voxelize" };
        protected override System.Drawing.Bitmap Icon => null; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("4fcef930-b12a-4aeb-b814-8d5cb45ac949");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to voxelize", GH_ParamAccess.item);
            pManager.AddNumberParameter("Voxel Size", "S", "Size of the voxels. Resolution of voxels.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Max Voxel Grid Count", "MxC", "Maximum voxel grid allowed. (x1000)", GH_ParamAccess.item, 250);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Voxelized points", GH_ParamAccess.list);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            double size = 1;
            int maxCount = 250;

            DA.GetData(0, ref mesh);
            DA.GetData(1, ref size);
            DA.GetData(2, ref maxCount);

            maxCount *= 1000;

            if (!mesh.IsClosed)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh is not closed.");
                return;
            }
            

            List<Point3d> points = new List<Point3d>();

            // Get the bounding box of the mesh
            BoundingBox bbox = mesh.GetBoundingBox(true);
            bbox.Inflate(1.2);

            // calculate number of voxels in each direction
            int xCount = (int)Math.Ceiling(bbox.Diagonal.X / size);
            int yCount = (int)Math.Ceiling(bbox.Diagonal.Y / size);
            int zCount = (int)Math.Ceiling(bbox.Diagonal.Z / size);

            if (xCount == 0 || yCount == 0 || zCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Mesh is too small for given voxel size.");
                return;
            }

            if (xCount * yCount * zCount > maxCount)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Mesh is too big for given voxel size. " +
                                                                  "Consider increase 'Max Voxel Grid Count'," +
                                                                  "or increase 'Voxel Size'.");
                return;
            }

            // loop through all voxels
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    for (int k = 0; k < zCount; k++)
                    {
                        Point3d voxelCenter = new Point3d(                            
                            bbox.Min.X + i * size + size / 2,
                            bbox.Min.Y + j * size + size / 2,
                            bbox.Min.Z + k * size + size / 2
                            );
                        
                        if (mesh.IsPointInside(voxelCenter, Double.Epsilon, false))
                        {
                            points.Add(voxelCenter);
                        }
                    }
                }
            }

            DA.SetDataList(0, points);
        }

        #region Additional

        #endregion
    }
}