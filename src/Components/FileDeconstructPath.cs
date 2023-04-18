using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Monkey.src.InputComponents;
using Rhino.Geometry;

namespace Monkey.src.Components
{
    public class FileDeconstructPath : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FileDeconstructPath class.
        /// </summary>
        public FileDeconstructPath()
          : base("Deconstruct Path", "DePath",
              "Deconstruct a path to its elements",
              "Monkey", "File")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public override IEnumerable<string> Keywords
        {
            get
            {
                return new string[] { "deconstruct path", "depath" };
            }
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Open Directory", OpenDirectory);
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

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "File path", GH_ParamAccess.item);

            this.Params.Input[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Directory", "D", "Directory for the file.", GH_ParamAccess.item);
            pManager.AddTextParameter("Filename", "N", "File name for the file.", GH_ParamAccess.item);
            pManager.AddTextParameter("File Extension", "E", "Extension of the file", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
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

        private string path;
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.DeconstructPath;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("67FC6FCE-8664-45A0-AEFD-D04096485E18"); }
        }
    }
}