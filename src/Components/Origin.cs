using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Monkey.src.Components
{
    public class Origin : GH_Component
    {
        #region Metadata

        public Origin()
          : base("Origin", "Origin",
              "Get the world origin",
              "Monkey", "Geometry")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "origin" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Origin; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("87342fff-d04e-4c00-83f5-f6236046e5f8");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Origin", "O", "The world origin", GH_ParamAccess.item);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, new Point3d(0, 0, 0));
        }

        #region Additional



        #endregion
    }
}