using Chimera.UI;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Chimera.src.Components.Utility
{
    public class DeletePlaceHolderComp : GH_Component
    {
        #region Metadata

        public DeletePlaceHolderComp()
            : base("CleanComponent", "Clean",
                "Remove blank placeholder components on canvas",
                "Chimera", "Utility")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "delplaceholder", "clean"};
        protected override Bitmap Icon => Properties.Resources.Clean; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("ef9a2447-f06c-44d5-9fe1-7f63fa2fd0fd");

        #endregion

        #region Button

        private bool _mRun;

        private void Run()
        {
            _mRun = true;
            ExpireSolution(true);
        }

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "Clean", Run);
        }

        #endregion

        #region IO
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Message", "M", "Message", GH_ParamAccess.item);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!_mRun) return;
            string message = string.Empty;
            try
            {
                var doc = OnPingDocument();
                if (doc == null) return;
                var objs = doc.Objects;
                var objsToDelete = objs.Where(o =>
                    o.GetType().ToString() == "Grasshopper.Kernel.Components.GH_PlaceholderComponent").ToList();
                if (objsToDelete.Count == 0 || objsToDelete == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No placeholder components found.");
                    message = "No placeholder components found.";
                    return;
                }
                else
                {
                    doc.ScheduleSolution(20, (d) => doc.RemoveObjects(objsToDelete, false));
                    var count = objsToDelete.Count();
                    message = $"Removed {count} placeholder components.";
                    Message = $"Done ({count})";
                }
            }
            catch (Exception e)
            {
                Message = "Error";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                message = e.Message;
            }
            finally
            {
                _mRun = false;
                DA.SetData(0, message);
            }
            
        }

        #region Additional



        #endregion
    }
}