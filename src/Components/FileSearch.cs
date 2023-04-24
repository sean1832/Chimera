using System;
using System.Collections.Generic;
using Chimera.UI;
using Grasshopper.Kernel;

namespace Chimera.Components
{
    public class FileSearch : GH_Component
    {
        #region Metadata

        public FileSearch()
            : base("File Search", "Search",
                "Search for files in a specified directory based on a given file name pattern.",
                "Chimera", "File")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { "search", "search file" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.FileSearch;
        public override Guid ComponentGuid => new Guid("2E7A9B25-4A2D-4E6F-A6B0-F3AF853735CA");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Directory", "Dir", "The target directory where the search will be performed.",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Pattern", "Pat",
                "The file name pattern to search for within the target directory.", GH_ParamAccess.item, "*.*");
            pManager.AddBooleanParameter("Subfolder", "Sub", "Whether the search should include subdirectories or not.",
                GH_ParamAccess.item, false);

            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Source", "S", "Found files path.", GH_ParamAccess.list);
        }

        #endregion

        #region Button

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "Refresh", Refresh);
        }
        private void Refresh()
        {
            ExpireSolution(true);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare and initialize input variables
            string inputDirectory = null;
            string filePattern = "*.*";
            bool searchSubfolders = false;

            // Get input data, with default values if not provided
            if (!DA.GetData(0, ref inputDirectory)) return;
            DA.GetData(1, ref filePattern);
            DA.GetData(2, ref searchSubfolders);

            // Determine the search option based on the 'searchSubfolders' input
            System.IO.SearchOption searchOption = searchSubfolders ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;

            // Get the list of files in the directory
            List<string> fileList = new List<string>(System.IO.Directory.GetFiles(inputDirectory, filePattern, searchOption));

            // Set the output data
            DA.SetDataList(0, fileList);
        }
    }
}