using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Monkey.src.Components
{
    public class FileSearch : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FileSearch class.
        /// </summary>
        public FileSearch()
          : base("File Search", "Search",
              "Search for files in a specified directory based on a given file name pattern.",
              "Monkey", "File")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Overrides the Description property to include the desired keywords.
        /// </summary>
        public override IEnumerable<string> Keywords
        {
            get
            {
                return new string[] { "search", "search file" };
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Directory", "Dir", "The target directory where the search will be performed.",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Pattern", "Pat",
                "The file name pattern to search for within the target directory.", GH_ParamAccess.item, "*.*");
            pManager.AddBooleanParameter("Subfolder", "Sub", "Whether the search should include subdirectories or not.",
                GH_ParamAccess.item, false);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Source", "S", "Found files path.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string directory = null;
            string pattern = "*.*";
            bool subfolder = false;
            if (!DA.GetData(0, ref directory)) return;
            if (!DA.GetData(0, ref pattern)) pattern = "*.*";
            if (!DA.GetData(0, ref subfolder)) return;
            List<string> source = new List<string>();
            if (directory != null)
            {
                source.AddRange(System.IO.Directory.GetFiles(directory, pattern, subfolder ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly));
            }
            DA.SetDataList(0, source);
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
                return Properties.Resources.Monkey_FileSearch;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2E7A9B25-4A2D-4E6F-A6B0-F3AF853735CA"); }
        }
    }
}