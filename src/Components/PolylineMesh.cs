using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Monkey.src.InputComponents;
using Rhino.Geometry;

namespace Monkey.src.Components
{
    public class PolylineMesh : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the PolylineMesh class.
        /// </summary>
        public PolylineMesh()
          : base("PolylineMesh", "PolyMesh",
              "Using polyline to construct a mesh",
              "Monkey", "Geometry")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public override IEnumerable<string> Keywords => new string[] { "polylinemesh", "polylinemesh" };



        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "P", "input polyline", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "output mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            int numSegments = 0;
            if (!DA.GetData(0, ref curve)) return;
            if (!hideSegmentParam)
            {
                DA.GetData(1, ref numSegments);
            }

            Polyline polyline;
            if (!curve.TryGetPolyline(out polyline))
            {
                hideSegmentParam = false;
                ExpireSolution(true);

                if (numSegments < 3)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Segment number cannot be smaller than 3! Reset it to 3.");
                    numSegments = 3;
                }

                // Divide the curve into points
                Point3d[] points;
                curve.DivideByCount(numSegments, true, out points);
                polyline = new Polyline(points);
            }
            else
            {
                hideSegmentParam = true;
                ExpireSolution(true);
            }

            if (!polyline.IsClosed)
            {
                // close the polyline
                polyline.Add(polyline[0]);
            }
            Mesh mesh = Rhino.Geometry.Mesh.CreateFromClosedPolyline(polyline);
            
            DA.SetData(0, mesh);
        }

        #region Additional

        private bool hideSegmentParam = true;

        #endregion

        protected override void AfterSolveInstance()
        {
            VariableParameterMaintenance();
            Params.OnParametersChanged();
        }
        public bool CanInsertParameter(GH_ParameterSide side, int index) { return false; }
        public bool CanRemoveParameter(GH_ParameterSide side, int index) { return false; }
        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var param = new Param_Integer()
            {
                Name = "PolylineSegment",
                NickName = "S",
                Description = "Optional segment for the curve to divide.",
                Access = GH_ParamAccess.item,
                Optional = true
            };

            // Assign the default value
            param.PersistentData.Append(new GH_Integer(10)); // Set the default value

            return param;

        }
        public bool DestroyParameter(GH_ParameterSide side, int index) { return true; }
        public void VariableParameterMaintenance()
        {
            ComponentInput input = new ComponentInput(OnPingDocument(), this);
            if (!hideSegmentParam)
            {
                if (Params.Input.Count == 2) return;
                Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 1));
                input.CreateSliderAt(1, 10, 3, 30, decimalPlace:0, local:true, offsetX: -100, offsetY: -15);
                ExpireSolution(true);
            }
            else
            {
                if (Params.Input.Count != 2) return;

                List<GH_ActiveObject> sources = input.GetSourceObjects(1, typeof(GH_NumberSlider));
                Type type = sources.GetType();
                if (sources.Count > 0)
                {
                    input.RemoveSourceObjects(1, sources);
                    Params.Input[1].Sources.Clear();
                    Params.UnregisterInputParameter(Params.Input[1]);
                }
                ExpireSolution(true);
            }
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7E65648F-386D-49DF-AC3E-1125A0020331"); }
        }
    }
}