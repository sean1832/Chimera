using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Monkey.src.Components
{
    public class DLAVisualzier : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DLAVisualzier class.
        /// </summary>
        public DLAVisualzier()
          : base("DLA Visualzier", "Visualizer",
              "Visualization data from DLA lines. Plug with a lineweight preview.",
              "Monkey", "DLA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("DLA Lines", "Ln", "List of DLA line.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Parameters", "P", "Parameters. *Plug with a 'Gradient' component.",
                GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness", "T",
                "Thickness for each lines. *Plug with a 'Custome Preview Lineweights' component.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.Monkey___DLA_Visualizer;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("27899113-CC84-425E-A89D-E74048F8CA3A"); }
        }
    }
}