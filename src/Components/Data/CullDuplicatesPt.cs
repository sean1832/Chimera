using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Chimera.src.Components.Data
{
    public class CullDuplicatesPt : GH_Component
    {
        #region Metadata

        public CullDuplicatesPt()
            : base("CullDuplicatesPt", "CullDupPt",
                "Removes duplicates from a list of points. Optimized for points.",
                "Chimera", "Data")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "cullduplicatespt", "cullpt" };
        protected override Bitmap Icon => Properties.Resources.CullDuplicate;
        public override Guid ComponentGuid => new Guid("78c5253f-4767-498c-9b2a-f9bafffd2ce6");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "List of points to remove duplicates from", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "T", "Tolerance for culling duplicates", GH_ParamAccess.item, 0.001);
            Params.Input[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Cleaned Points", "C", "Cleaned list with duplicates removed", GH_ParamAccess.list);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> points = new List<Point3d>();
            double tol = 0.001;
            if (!DA.GetDataList(0, points)) return;
            DA.GetData(1, ref tol);
            List<Point3d> cleanedPoints = RemoveDuplicatesWithSpatialHashing(points, tol);
            DA.SetDataList(0, cleanedPoints);
        }

        #region Additional

        public static List<Point3d> RemoveDuplicatesWithSpatialHashing(List<Point3d> points, double tolerance)
        {
            if (tolerance <= 0)
            {
                throw new ArgumentException(@"Tolerance must be greater than 0", nameof(tolerance));
            }
            // Define a dictionary to hold points with their bucket keys
            var buckets = new Dictionary<(int, int, int), List<Point3d>>();

            foreach (Point3d point in points)
            {
                // Calculate the bucket key for the current point
                (int, int, int) bucketKey = (
                    GetBucketIndex(point.X, tolerance),
                    GetBucketIndex(point.Y, tolerance),
                    GetBucketIndex(point.Z, tolerance));

                // Check the current bucket and the adjacent buckets for potential duplicates
                bool foundDuplicate = false;
                foreach (var adjacentKey in GetAdjacentKeys(bucketKey))
                {
                    // If the adjacent bucket does not exist, skip it
                    if (!buckets.TryGetValue(adjacentKey, out List<Point3d> bucketPoints)) continue;
                    foreach (Point3d existingPoint in bucketPoints)
                    {
                        if (!(point.DistanceTo(existingPoint) < tolerance)) continue;
                        foundDuplicate = true;
                        break;
                    }
                    if (foundDuplicate) break;
                }

                if (foundDuplicate) continue;
                if (!buckets.ContainsKey(bucketKey))
                {
                    buckets[bucketKey] = new List<Point3d>();
                }
                buckets[bucketKey].Add(point);
            }

            // Flatten the list of unique points from the buckets
            List<Point3d> uniquePoints = buckets.Values.SelectMany(bucket => bucket).ToList();
            return uniquePoints;
        }

        private static int GetBucketIndex(double coordinate, double tolerance)
        {
            return (int)Math.Floor(coordinate / tolerance);
        }

        private static IEnumerable<(int, int, int)> GetAdjacentKeys((int x, int y, int z) key)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        yield return (key.x + dx, key.y + dy, key.z + dz);
                    }
                }
            }
        }

        #endregion
    }
}