using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Eto.Drawing;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Rhino.Render.DataSources;
using Color = System.Drawing.Color;

namespace Monkey
{
    public class MK_Counter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MK_Counter()
          : base("MK_Counter", "Counter",
              "Count number at a given range",
              "Monkey", "Utility")
        {
        }

        private GH_Document GrasshopperDocument;
        private IGH_Component Component;
        private int counter;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Executing the component", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Start Number", "start #", "The number to start with.\nDefault 0.", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Target Number", "end #",
                "The maximum number to count until it is stopped.\nDefault 15", GH_ParamAccess.item, 15);
            pManager.AddIntegerParameter("Interval", "interval",
                "The interval in millisecond (ms) between each count.\nDefault 150.", GH_ParamAccess.item, 150);
;        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Count", "C", "Current count", GH_ParamAccess.item);
        }

        
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region Variables

            Component = this;
            GrasshopperDocument = this.OnPingDocument();

            #endregion

            #region Inputs

            bool run = false;
            int startNum = 0;
            int targetNum = 15;
            int interval = 150;

            if (!DA.GetData(0, ref run)) return;
            if (!DA.GetData(1, ref startNum)) return;
            if (!DA.GetData(2, ref targetNum)) return;
            if (!DA.GetData(3, ref interval)) return;

            #endregion

            ChangeInputSlider("Interval", 50, 15000, "interval", "interval");
            #region Error Handeling

            if (interval < 50)
            {
                interval = 50;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "WARNING: Interval must not be less than 50ms. Value is set to 50ms.");
            }

            if (startNum == targetNum)
            {
                targetNum = startNum + 1;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "WARNING: Start number and Target number must not be equal. Please re-adjust number.");
            }

            if (startNum > targetNum)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "ERROR: Start number must be greater than Target number. Please re-adjust number.");
                return;
            }

            #endregion

            if (run)
            {
                if (counter < targetNum)
                {
                    GrasshopperDocument.ScheduleSolution(interval, d =>
                    {
                        ExpireSolution(false);
                        counter++;
                        Message = $"Count: {counter.ToString()} / {targetNum.ToString()}";
                    });
                }
                else
                {
                    ChangeInputToggle("Run", false);
                }
            }
            else
            {
                counter = startNum;
                Message = $"Stopped.";
            }

            // output
            DA.SetData(0, counter);
        }

        public void ChangeInputSlider(string targetInputName, int min, int max, string inputAlteredName,
            string inputAlterNickname)
        {
            List<IGH_Param> inputs = Component.Params.Input;
            foreach (IGH_Param input in inputs)
            {
                if (input.Name != targetInputName) continue;
                // fetch sources, anything that connects to this input
                IList<IGH_Param> sources = input.Sources;
                foreach (IGH_Param source in sources)
                {
                    IGH_Attributes attributes = source.Attributes;
                    IGH_DocumentObject comp_obj = attributes.GetTopLevel.DocObject;

                    // find if type of specified type (change here)
                    if (comp_obj.GetType() == typeof(Grasshopper.Kernel.Special.GH_NumberSlider))
                    {
                        // cast type (change here)
                        var slider = source as Grasshopper.Kernel.Special.GH_NumberSlider;

                        if (slider == null || slider.Slider == null) continue;
                        if (slider.Slider.Maximum == max && slider.Slider.Minimum == min) continue;
                        slider.Slider.Maximum = max;
                        slider.Slider.Minimum = min;
                        slider.Slider.ControlEdgeColour = Color.Blue;
                        slider.Slider.DecimalPlaces = 0;

                        comp_obj.Name = inputAlteredName;
                        comp_obj.NickName = inputAlterNickname;
                        // re-computer to refresh
                        comp_obj.ExpireSolution(true);
                    }
                }
            }
        }

        public void ChangeInputToggle(string targetInputName, bool state)
        {
            List<IGH_Param> inputs = Component.Params.Input;
            foreach (IGH_Param input in inputs)
            {
                if (input.Name != targetInputName) continue;
                // fetch sources, anything that connects to this input
                IList<IGH_Param> sources = input.Sources;
                foreach (IGH_Param source in sources)
                {
                    IGH_Attributes attributes = source.Attributes;
                    IGH_DocumentObject comp_obj = attributes.GetTopLevel.DocObject;

                    // find if type of specified type (change here)
                    if (comp_obj.GetType() == typeof(Grasshopper.Kernel.Special.GH_BooleanToggle))
                    {
                        // cast type (change here)
                        var toggle = source as Grasshopper.Kernel.Special.GH_BooleanToggle;

                        if (toggle == null) continue;
                        if (toggle.Value == state) continue;
                        toggle.Value = state;
                        
                        // re-computer to refresh
                        comp_obj.ExpireSolution(true);
                    }
                }
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
                return Properties.Resources.Monkey___Counter;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("285A1064-3416-43A2-A7FC-AED8732A1A1A"); }
        }
    }
}