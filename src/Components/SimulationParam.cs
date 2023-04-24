using System;
using System.Collections.Generic;
using System.Drawing;
using Chimera.Properties;
using Grasshopper.Kernel;
using Newtonsoft.Json;

namespace Chimera.Components
{
    public class SimulationParam : GH_Component
    {
        #region Metadata

        public SimulationParam()
            : base("Simulation Attributes", "Attributes",
                "Simulation attributes for Monkey.",
                "Chimera", "Utility")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "attributes", "simattri" };
        protected override Bitmap Icon => Resources.Simulation_Parameters;
        public override Guid ComponentGuid => new Guid("6424E833-46AE-4A8F-90C9-C6AF9E06EBC4");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Interval", "I", "Speed (ms) of iteration", GH_ParamAccess.item, (int)10);
            pManager.AddIntegerParameter("MaxStep", "MxS", "Maximum step of iteration.", GH_ParamAccess.item, (int)50);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Attributes", "A", "Output attributes for Monkey simulation.", GH_ParamAccess.item);
        }

        #endregion

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
    }
}