using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Monkey.src.InputComponents;
using Monkey.src.UI;
using Rhino.Collections;
using Rhino.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Monkey.src.IO;

namespace Monkey.src.Components
{
    public class MK_DLACrawl : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MK_DLACrawl class.
        /// </summary>
        public MK_DLACrawl()
          : base("DLA Crawl", "DLA",
              "Diffusion Limited Aggregation walk on mesh surface. Attach a trigger to run.",
              "Monkey", "DLA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Geo", "G", "Base mesh for the curve to crawl on.", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "Starting location.", GH_ParamAccess.item);
            pManager.AddCurveParameter("Attractor Curve", "C", "*Optional: Attractor curve to guide the growth of curves.",
                GH_ParamAccess.item);

            // parameters
            pManager.AddTextParameter("[]Simulation Attribute", "[]A", "Attributes to control the simulation.", GH_ParamAccess.item,
                "{ \"interval\": 10, \"maxStep\": 200 }");
            pManager.AddNumberParameter("Scale", "S", "Scale of the branches.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Attractor Weight", "W",
                "Determines the influence of an attractor on the growth pattern of a DLA curve. " +
                "A higher value results in the DLA curve being more strongly drawn towards the attractor, " +
                "while a lower value reduces its influence on the growth pattern.", GH_ParamAccess.item);


            pManager[2].Optional = true; // make the attractor curve optional
            pManager[5].Optional = true; // make the attractor weight optional
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("DLA Line", "Ln", "Output of DLA Lines.", GH_ParamAccess.item);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region Inputs

            Mesh inGeo = new Mesh();
            Point3d pt = new Point3d();
            Curve attractor = new ArcCurve();
            string simParams = "";
            double scale = 0;
            double attractorWeight = 0;
            


            if (!DA.GetData(0, ref inGeo)) return;
            if (!DA.GetData(1, ref pt)) return;
            DA.GetData(2, ref attractor); // optional attractor input.
            if (!DA.GetData(3, ref simParams)) return;
            if (!DA.GetData(4, ref scale)) return;
            DA.GetData(5, ref attractorWeight); // optional attractor input.


            JObject jsonObject = JObject.Parse(simParams);
            int interval = jsonObject["interval"].Value<int>();
            int maxStep = jsonObject["maxStep"].Value<int>();


            #endregion

            bool hasAttrCrv = false;


            #region Input Error Handeling

            bool valid;
            try
            {
                valid = inGeo.IsValid;
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No mesh input detected, please plugin a mesh.");
                DA.AbortComponentSolution();
                return;
            }

            try
            {
                valid = pt.IsValid;
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No point input detected, please plugin a point.");
                DA.AbortComponentSolution();
                return;
            }

            // if this component have attractor
            if (Util.InputHasData(this, 2))
            {
                hasAttrCrv = true;
            }

            if (interval <= 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid interval. Must be greater than 1ms.");
                DA.AbortComponentSolution();
                return;
            }

            if (maxStep < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Target Number. Must be greater than 0.");
                DA.AbortComponentSolution();
                return;
            }

            #endregion

            #region Main

            if (run)
            {

                if (count < maxStep)
                {
                    if (rerun)
                    {
                        Reset();
                        rerun = false;
                    }

                    OnPingDocument().ScheduleSolution(interval, d =>
                    {
                        ExpireSolution(false);

                        // start of the algorithm
                        if (count == 0)
                        {
                            // create a vertical line
                            Vector3d motion = new Vector3d(0, 0, 1);
                            Point3d ptA = pt;
                            pt.Transform(Transform.Translation(motion));
                            Point3d ptB = pt;
                            Line startLine = new Line(ptA, ptB);
                            calculatedLines.Add(startLine);
                        }

                        // populate the mesh with points
                        List<Point3d> populatedPts = PopulateMesh(inGeo, 1, SeedGenerator(count));
                        Point3d populatedPt = populatedPts[0];
                        Point3d closestPt = GetClosestPoint(populatedPt, calculatedLines);


                        // base dla direction
                        Line baseLine = new Line(closestPt, populatedPt);
                        Vector3d baseDir = baseLine.Direction;
                        Line sdlLine = new Line(closestPt, baseDir, scale);
                        baseDir = sdlLine.Direction;
                        baseDir.Unitize();

                        Vector3d finalDir = Vector3d.Unset;

                        if (hasAttrCrv)
                        {
                            // divide attractor curve by length and get the points
                            Point3d[] dividedPts;
                            attractor.DivideByLength(0.1, true, out dividedPts);
                            // rebuild the curve as polyline
                            Polyline atrPoly = new Polyline(dividedPts);
                            // rebuild the polyline into segments
                            Line[] atrSegmentLines = atrPoly.GetSegments();
                            // get the center points of the segments
                            List<Point3d> atrCenterPts = new List<Point3d>();
                            foreach (Line line in atrSegmentLines)
                            {
                                Point3d centerPt = line.PointAt(0.5);
                                atrCenterPts.Add(centerPt);
                            }

                            // find closest attractor
                            int atrCPIndex;
                            double atrCPDist;
                            GetClosestPoint(closestPt, atrCenterPts, out atrCPIndex, out atrCPDist);
                            Vector3d closestAtrSegmentDirection = atrSegmentLines[atrCPIndex].Direction;
                            closestAtrSegmentDirection.Unitize();
                            double t = GH_Operations.Remap(atrCPDist, 0, 52, 0, 1);
                            double bezierMultiplier = GH_Operations.Bezier(t, 1.0, 0.8, 0.2, 0) * attractorWeight;

                            // add vector together
                            finalDir = baseDir * (1 - bezierMultiplier) + closestAtrSegmentDirection * bezierMultiplier * scale;
                        }
                        else
                        {
                            finalDir = baseDir * scale;
                        }

                        // pull point back to mesh
                        Point3d closestPtAdjusted = closestPt;
                        closestPtAdjusted.Transform(Transform.Translation(finalDir));
                        Point3d meshClosestPoint = GetClosestPoint(closestPtAdjusted, inGeo);


                        // create line
                        Line lineToDraw = new Line(closestPt, meshClosestPoint);

                        count++;
                        calculatedLines.Add(lineToDraw);


                        Message = $"Step: {count.ToString()}";
                    });
                }
                else
                {
                    run = false;
                    rerun = true;
                    count = 0;
                }
            }

            #endregion


            // output
            DA.SetDataList(0, calculatedLines);
        }

        #region Additional

        private int count = 0;
        private List<Line> calculatedLines = new List<Line>();
        private bool rerun = false;
        private bool run = false;

        private void Run()
        {
            run = true;
            ExpireSolution(true);
        }
        private void Reset()
        {
            calculatedLines.Clear();
            count = 0;
        }

        Point3d GetClosestPoint(Point3d targetPoint, List<Line> lines)
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

        Point3d GetClosestPoint(Point3d targetPoint, Mesh mesh)
        {
            Point3d closestPoint = Point3d.Unset;
            Vector3d normal;
            mesh.ClosestPoint(targetPoint, out closestPoint, out normal, double.MaxValue);
            return closestPoint;
        }



        private Point3d GetClosestPoint(Point3d targetPt, List<Point3d> cloudPts, out int index, out double distance)
        {
            index = Point3dList.ClosestIndexInList(cloudPts, targetPt);
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Closest Point index out of range.");
            }
            distance = targetPt.DistanceTo(cloudPts[index]);
            return cloudPts[index];
        }

        private int SeedGenerator(int seed)
        {
            Random rnd = new Random(seed);
            return rnd.Next(0, 700);
        }

        /// <summary>
        /// Populate a mesh geometry with points
        /// </summary>
        /// <param name="inputMesh">mesh to populate</param>
        /// <param name="num">numbers of points</param>
        /// <param name="seed">seed value</param>
        /// <returns>list of randomly populated points on geometry</returns>
        private List<Point3d> PopulateMesh(Mesh inputMesh, int num, int seed = -1)
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

        #endregion



        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "Simulate", Run);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.Monkey___DLA_Crawl;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A541F904-CCBE-4087-9E4E-3256CA2369E1"); }
        }
    }
}