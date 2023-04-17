using Grasshopper.Kernel;
using Rhino.Collections;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkey.src
{
    internal class GH_Operations
    {
        public GH_Document Document { get; set; }
        public IGH_Component Component { get; set; }
        private GH_ActiveObject _activeObject;

        public GH_Operations(GH_Document document, GH_Component component)
        {
            Document = document;
            Component = component;
            _activeObject = component as GH_ActiveObject;
        }


        public List<double> NormalizeList<T>(List<T> list)
        {
            if (!Util.IsNumericType(typeof(T)))
            {
                Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid input. Normalize List only accept int and double. " + typeof(T).ToString());
            }
            double minValue = list.Min(x => Convert.ToDouble(x));
            double maxValue = list.Max(x => Convert.ToDouble(x));
            double range = maxValue - minValue;

            List<double> normalizedList = new List<double>(list.Count);

            foreach (T value in list)
            {
                double normalizedValue = (Convert.ToDouble(value) - minValue) / range;
                normalizedList.Add(normalizedValue);
            }

            return normalizedList;
        }


        // Cubic Bezier function
        public double Bezier(double t, double a, double b, double c, double d)
        {
            double s = 1 - t;
            return Math.Pow(s, 3) * a + 3 * Math.Pow(s, 2) * t * b + 3 * s * Math.Pow(t, 2) * c + Math.Pow(t, 3) * d;
        }

        public double Remap(double value, double sourceMin, double sourceMax, double targetMin, double targetMax)
        {
            return (value - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
        }
        public List<double> Remap(List<double> value, double sourceMin, double sourceMax, double targetMin, double targetMax)
        {
            List<double> output = new List<double>();
            foreach (double val in value)
            {
                double remapped = (val - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
                output.Add(remapped);
            }
            return output;
        }
        public Point3d GetClosestPoint(Point3d targetPoint, List<Line> lines)
        {
            Point3dList pointList = new Point3dList();

            foreach (Line line in lines)
            {
                Point3d currentClosestPoint = line.ClosestPoint(targetPoint, true);
                pointList.Add(currentClosestPoint);
            }

            int index = pointList.ClosestIndex(targetPoint);
            Point3d closestPoint = pointList[index];
            return closestPoint;
        }
        public Point3d GetClosestPoint(Point3d targetPoint, Mesh mesh)
        {
            Point3d closestPoint = Point3d.Unset;
            Vector3d normal;
            mesh.ClosestPoint(targetPoint, out closestPoint, out normal, double.MaxValue);
            return closestPoint;
        }
        public Point3d GetClosestPoint(Point3d targetPt, List<Point3d> cloudPts, out int index, out double distance)
        {
            index = Point3dList.ClosestIndexInList(cloudPts, targetPt);
            if (index == -1)
            {
                _activeObject.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Closest Point index out of range.");
            }
            distance = targetPt.DistanceTo(cloudPts[index]);
            return cloudPts[index];
        }
        public int RandomGenerator(int seed, int min, int max)
        {
            Random rnd = new Random(seed);
            return rnd.Next(min, max);
        }

        /// <summary>
        /// Populate a mesh geometry with points
        /// </summary>
        /// <param name="inputMesh">mesh to populate</param>
        /// <param name="num">numbers of points</param>
        /// <param name="seed">seed value</param>
        /// <returns>list of randomly populated points on geometry</returns>
        public List<Point3d> PopulateMesh(Mesh inputMesh, int num, int seed = -1)
        {
            List<Point3d> randomPoints = new List<Point3d>();

            Random random = seed == -1 ? new Random(seed) : new Random(); // if seed exist, add seed to random. Else fully random.

            inputMesh.FaceNormals.ComputeFaceNormals();
            inputMesh.Normals.ComputeNormals();

            for (int i = 0; i < num; i++)
            {
                int faceIndex = random.Next(inputMesh.Faces.Count);
                MeshFace face = inputMesh.Faces[faceIndex];

                Point3d A = inputMesh.Vertices[face.A];
                Point3d B = inputMesh.Vertices[face.B];
                Point3d C = inputMesh.Vertices[face.C];
                Point3d D = face.IsTriangle ? C : inputMesh.Vertices[face.D];

                double u = random.NextDouble();
                double v = random.NextDouble();

                if (u + v > 1)
                {
                    u = 1 - u;
                    v = 1 - v;
                }

                Point3d randomPoint = A + u * (B - A) + v * (C - A);

                if (!face.IsTriangle)
                {
                    double w = random.NextDouble();

                    if (u + v + w > 1)
                    {
                        u = 1 - u;
                        v = 1 - v;
                        w = 1 - w;
                    }

                    randomPoint += w * (D - A);
                }

                randomPoints.Add(randomPoint);
            }

            return randomPoints;
        }
    }
}
