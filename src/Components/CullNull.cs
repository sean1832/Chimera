using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Chimera.Components
{
    public class CullNull : GH_Component
    {
        #region Metadata

        public CullNull()
          : base("CullNull", "CullNull",
              "Remove null item from a list",
              "Chimera", "Data")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "cullnull", "removenull" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.CullNull; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("b56ab18b-5b89-448e-803c-b9e6bd340f84");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Input List", "L", "The list to cull null items from.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Clean List", "C", "The list with null items removed.", GH_ParamAccess.list);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<object> inputList = new List<object>();
            if (!DA.GetDataList(0, inputList)) return;

            List<object> cleanList = new List<object>();

            foreach (object item in inputList)
            {
                if (item != null)
                {
                    cleanList.Add(item);
                }
            }

            DA.SetDataList(0, cleanList);
        }

        #region Additional



        #endregion
    }
}