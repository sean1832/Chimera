using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Chimera.Components.DLA
{
    public class DLAVisualizer : GH_Component
    {
        public DLAVisualizer()
          : base("DLA Visualizer", "Visualizer",
              "Visualization data from DLA lines. Plug with a line-weight preview.",
              "Chimera", "DLA")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "dlavisualizer" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.DLA_Visualizer;
        public override Guid ComponentGuid => new Guid("27899113-CC84-425E-A89D-E74048F8CA3A");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("DLA Lines", "Ln", "List of DLA line.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Parameters", "P", "Parameters. *Plug with a 'Gradient' component.",
                GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness", "T",
                "Thickness for each lines. *Plug with a 'Custom Preview Line-weights' component.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> lines = new List<Curve>();
            List<double> remaped = new List<double>();
            List<double> paramList = new List<double>();
            if (!DA.GetDataList(0, lines)) return;
            if (Util.InputHasData(this, 0))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    paramList.Add(i);
                }
                GH_Operations operations = new GH_Operations(OnPingDocument(), this);
                paramList = operations.NormalizeList(paramList);

                foreach (var t in paramList)
                {
                    remaped.Add(operations.Remap(t, 0, 1, 7, 1));
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No DLA lines input detected.");
                DA.AbortComponentSolution();
                return;
            }

            DA.SetDataList(0, paramList);
            DA.SetDataList(1, remaped);

        }

        
    }
}