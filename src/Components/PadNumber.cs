using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace Chimera.Components
{
    public class PadNumber : GH_Component
    {
        #region Metadata

        public PadNumber()
          : base("PadNumber", "PadNum",
              "Pads a number with zeros based",
              "Chimera", "Data")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "padnum", "pad" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.PadNumber; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("d5b59b7d-a9a8-4776-9ce8-3719204aa226");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Number", "N", "The number to pad.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Digits", "D", "The number of digits to pad to.", GH_ParamAccess.item, 1);

            Params.Input[0].Optional = true;
            Params.Input[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Padded Number", "P", "The padded number.", GH_ParamAccess.item);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int number = 0;
            int digits = 1;
            if (!DA.GetData(0, ref number)) return;
            DA.GetData(1, ref digits);

            if (digits < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Digits must be greater than 0. reset to 1.");
                digits = 1;
            }

            string paddedNumber = number.ToString().PadLeft(digits, '0');
            DA.SetData(0, paddedNumber);
        }

        #region Additional



        #endregion
    }
}