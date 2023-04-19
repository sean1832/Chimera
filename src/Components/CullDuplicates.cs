using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Monkey.src.Components
{
    public class CullDuplicates : GH_Component
    {
        #region Metadata

        public CullDuplicates()
          : base("CullDuplicates", "CullDup",
              "Removes duplicates from a list of data.",
              "Monkey", "Data")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "cullduplicates", "culldup", "removedup" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.CullDuplicate; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("4c23f203-dd49-4446-b307-37957ba875ec");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Data", "D", "List of data to remove duplicates from", GH_ParamAccess.list);

            Params.Input[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Cleaned Data", "C", "Cleaned list with duplicates removed", GH_ParamAccess.list);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> data = new List<string>();
            if (!DA.GetDataList(0, data)) return;

            List<string> cleanedData = RemoveDuplicateData(data);

            DA.SetDataList(0, cleanedData);
        }

        #region Additional

        private List<T> RemoveDuplicateData<T>(List<T> list)
        {

            HashSet<T> uniqueSet = new HashSet<T>(list);
            return new List<T>(uniqueSet);
        }

        #endregion
    }
}