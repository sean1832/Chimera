using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Monkey.src.Components
{
    public class ListSegment : GH_Component
    {
        #region Metadata

        public ListSegment()
          : base("ListSegment", "ListSeg",
              "Extracts first, last, and middle n items from a list.",
              "Monkey", "Data")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "listsegement", "firstlast" };
        protected override System.Drawing.Bitmap Icon => null; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("2ee651b3-12ca-4842-9f30-b55b65ae24a3");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("List", "L", "Input list", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Number", "N", "Number of items to get from the list", GH_ParamAccess.item, 1);

            Params.Input[0].Optional = true;
            Params.Input[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("First N Items", "F", "First N items of the input list", GH_ParamAccess.list);
            pManager.AddGenericParameter("Middle Items", "M", "Middle items of the input list", GH_ParamAccess.list);
            pManager.AddGenericParameter("Last N Items", "L", "Last N items of the input list", GH_ParamAccess.list);
        }

        #endregion



        #region ClassLevelVariables

        private string _mode = "First/Last";
        public string Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                Message = _mode;
            }
        }

        #endregion

        #region (De)Serialization

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // First add our own field.
            writer.SetString("Mode", Mode);
            // Then call the base class implementation.
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // First read our own field.
            Mode = reader.GetString("Mode") ?? _mode;
            // Then call the base class implementation.
            return base.Read(reader);
        }

        #endregion


        #region Context Menu

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "First/Last", ToggleFirstLast, true).Checked = Mode == "First/Last";
            Menu_AppendItem(menu, "Half", ToggleHalf, true).Checked = Mode == "Half";
        }

        private void ToggleFirstLast(object sender, EventArgs e)
        {
            RecordUndoEvent("ToggleFirstLast");
            Mode = "First/Last";
            ExpireSolution(true);
        }

        private void ToggleHalf(object sender, EventArgs e)
        {
            RecordUndoEvent("ToggleHalf");
            Mode = "Half";
            ExpireSolution(true);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<object> inputList = new List<object>();
            int n = 1;

            if (!DA.GetDataList(0, inputList)) return;
            DA.GetData(1, ref n);

            List<object> firstNItems = new List<object>();
            List<object> middleItems = new List<object>();
            List<object> lastNItems = new List<object>();

            switch (Mode)
            {
                case "First/Last":
                    (firstNItems, middleItems, lastNItems) = SolveFirstLast(DA, inputList, n);
                    break;
                case "Half":
                    (firstNItems, middleItems, lastNItems) = SolveHalf(DA, inputList, n);
                    break;
            }

            DA.SetDataList(0, firstNItems);
            DA.SetDataList(1, middleItems);
            DA.SetDataList(2, lastNItems);
        }

        #region Additional

        private (List<object>, List<object>, List<object>) SolveFirstLast(IGH_DataAccess DA, List<object> inputList, int n)
        {
            if (n < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Number(N) cannot be less than 1. reset to 1.");
                n = 1;
            }

            if (n > inputList.Count / 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Number(N) cannot be greater than the half length of the list. reset to half of the list count.");
                n = inputList.Count / 2;
            }

            int listLength = inputList.Count;

            List<object> firstNItems = inputList.GetRange(0, Math.Min(n, listLength));
            List<object> lastNItems = inputList.GetRange(listLength - n, Math.Min(n, listLength));

            int middleItemsStart = Math.Min(n, listLength);
            int middleItemsCount = Math.Max(0, listLength - 2 * n);
            List<object> middleItems = inputList.GetRange(middleItemsStart, middleItemsCount);

            return (firstNItems, middleItems, lastNItems);
        }

        private (List<object>, List<object>, List<object>) SolveHalf(IGH_DataAccess DA, List<object> inputList, int n)
        {
            if (n < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Number(N) cannot be less than 1. reset to 1.");
                n = 0;
            }

            else if (n > inputList.Count / 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Number(N) cannot be greater than the half length of the list. reset to half of the list count.");
                n = inputList.Count / 2;
            }

            int listLength = inputList.Count;
            int middleIndex = listLength / 2;

            List<object> middleItems;
            List<object> frontSection;
            List<object> backSection;

            if (n == 0)
            {
                if (listLength % 2 == 0)
                {
                    // even
                    middleItems = new List<object> { inputList[middleIndex - 1], inputList[middleIndex] };
                    frontSection = inputList.GetRange(0, middleIndex - 1);
                    backSection = inputList.GetRange(middleIndex + 1, listLength - middleIndex - 1);
                }
                else
                {
                    middleItems = new List<object> { inputList[middleIndex] };
                    frontSection = inputList.GetRange(0, middleIndex);
                    backSection = inputList.GetRange(middleIndex + 1, listLength - middleIndex - 1);
                }
            }
            else
            {
                if (listLength % 2 == 0)
                {
                    // even
                    middleItems = inputList.GetRange(middleIndex - 1, n+1);
                    frontSection = inputList.GetRange(0, middleIndex - n);
                    backSection = inputList.GetRange(middleIndex + n, listLength - middleIndex - n);
                }
                else
                {
                    // odd
                    middleItems = inputList.GetRange(middleIndex - n, n + 2);
                    frontSection = inputList.GetRange(0, middleIndex - n);
                    backSection = inputList.GetRange(middleIndex + n + 1, listLength - middleIndex - n - 1);
                }
            }

            return (frontSection, middleItems, backSection);
        }
        #endregion
    }
}