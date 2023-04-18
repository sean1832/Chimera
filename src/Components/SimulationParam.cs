using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Monkey.src.Components
{
    public class SimulationParam : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MK_SimulationParam class.
        /// </summary>
        public SimulationParam()
          : base("Simulation Attributes", "Attributes",
              "Simulation attributes for Monkey.",
              "Monkey", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Interval", "I", "Speed (ms) of iteration", GH_ParamAccess.item, (int)10);
            pManager.AddIntegerParameter("MaxStep", "MxS", "Maximum step of iteration.", GH_ParamAccess.item, (int)50);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Attributes", "A", "Output attributes for Monkey simulation.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int interval = 0;
            int maxStep = 0;
            if (!DA.GetData(0, ref interval)) return;
            if (!DA.GetData(1, ref maxStep)) return;

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

            var param = new Dictionary<string, int>
            {
                { "interval", interval },
                { "maxStep", maxStep }
            };
            var json = JsonConvert.SerializeObject(param, Formatting.Indented);
            DA.SetData(0, json);
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
                return Properties.Resources.Monkey___Simulation_Parameters;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6424E833-46AE-4A8F-90C9-C6AF9E06EBC4"); }
        }
    }
}