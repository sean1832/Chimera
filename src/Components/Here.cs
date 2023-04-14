using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Monkey.src.UI;
using Rhino.Geometry;

namespace Monkey.src.Components
{
    public class Here : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Here class.
        /// </summary>
        public Here()
          : base("Here", "here",
              "Finds the path location of current script.",
              "Monkey", "File")
        {
        }

        /// <summary>
        /// Overrides the Description property to include the desired keywords.
        /// </summary>
        public override IEnumerable<string> Keywords
        {
            get
            {
                return new string[] { "here" };
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Folder Path", "fP", "Current file name as folder path.", GH_ParamAccess.item);
            pManager.AddTextParameter("Directory", "Dir", "Directory of current script.", GH_ParamAccess.item);
            pManager.AddTextParameter("Script Path", "P", "Current script path.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string fullPath = this.OnPingDocument().FilePath;
            directory = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileNameWithoutExtension(fullPath);
            string folderPath = Path.Combine(directory, fileName);


            DA.SetData(0, folderPath);
            DA.SetData(1, directory);
            DA.SetData(2, fullPath);
        }

        private string directory;

        private void OpenDir()
        {
            Process.Start(directory);
        }

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "OpenDir", OpenDir);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("59011623-9050-4E5E-B0D4-C17C20B13929"); }
        }
    }
}