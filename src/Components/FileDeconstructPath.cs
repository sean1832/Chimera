using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Grasshopper.Kernel;

namespace Chimera.Components
{
    public class FileDeconstructPath : GH_Component
    {
        #region Metadata

        public FileDeconstructPath()
            : base("Deconstruct Path", "DePath",
                "Deconstruct a path to its elements",
                "Chimera", "File")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "deconstruct path", "depath" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.DeconstructPath;
        public override Guid ComponentGuid => new Guid("67FC6FCE-8664-45A0-AEFD-D04096485E18");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "File path", GH_ParamAccess.item);

            this.Params.Input[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Directory", "D", "Directory for the file.", GH_ParamAccess.item);
            pManager.AddTextParameter("Filename", "N", "File name for the file.", GH_ParamAccess.item);
            pManager.AddTextParameter("File Extension", "E", "Extension of the file", GH_ParamAccess.item);
        }

        #endregion

        #region Context Menu

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Open Directory", OpenDirectory, Properties.Resources.Window_New_Open);
        }

        private void OpenDirectory(object sender, EventArgs e)
        {
            if (path == null)
            {
                MessageBox.Show("Warning: path is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string directory = Path.GetDirectoryName(path);
                System.Diagnostics.Process.Start(directory);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref path)) return;

            string extension = Path.GetExtension(path);
            string directory = null;
            string filename = null;

            if (string.IsNullOrEmpty(extension))
            {
                directory = path;
            }
            else
            {
                directory = Path.GetDirectoryName(path);
                filename = Path.GetFileNameWithoutExtension(path);
            }


            DA.SetData(0, directory);
            DA.SetData(1, filename);
            DA.SetData(2, extension);
        }

        #region Additional

        private string path;

        #endregion



    }
}