using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Chimera.Properties;
using Chimera.UI;
using Grasshopper.Kernel;

namespace Chimera.Components.Files
{
    public class Here : GH_Component
    {
        #region Metadata

        public Here()
            : base("Here", "here",
                "Finds the path location of current script.",
                "Chimera", "File")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { "here" };
        protected override Bitmap Icon => Resources.Here;
        public override Guid ComponentGuid => new Guid("59011623-9050-4E5E-B0D4-C17C20B13929");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Folder Path", "fP", "Current file name as folder path.", GH_ParamAccess.item);
            pManager.AddTextParameter("Directory", "Dir", "Directory of current script.", GH_ParamAccess.item);
            pManager.AddTextParameter("Script Path", "P", "Current script path.", GH_ParamAccess.item);
        }

        #endregion

        #region Button

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "OpenDir", OpenDir);
        }
        private void OpenDir()
        {
            Process.Start(directory);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string fullPath = OnPingDocument().FilePath;
            directory = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileNameWithoutExtension(fullPath);
            string folderPath = Path.Combine(directory, fileName);


            DA.SetData(0, folderPath);
            DA.SetData(1, directory);
            DA.SetData(2, fullPath);
        }

        #region Additional

        private string directory;

        #endregion






    }
}