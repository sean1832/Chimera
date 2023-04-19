using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Monkey.src.UI;
using Rhino.Geometry;

namespace Monkey.src.Components
{
    public class FileMove : GH_Component
    {
        public FileMove()
          : base("File Move", "File Move",
              "Move a list of files to a directory.",
              "Monkey", "File")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.FileMove;
        public override Guid ComponentGuid => new Guid("9EF2EF38-3D41-4952-A8DF-316CED99AAE4");


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Source", "S", "Location of the file you want to move.", GH_ParamAccess.list);
            pManager.AddTextParameter("Folder", "F", "Directory you want to move", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Overwrite", "O", "Overwrite existing files", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Copy", "C", "Copy files to new location", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Run", "R", "Executing operation.", GH_ParamAccess.item, false);
        }

        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "I", "Output information", GH_ParamAccess.list);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> source = new List<string>();
            string folder = null;
            bool overwrite = false;
            bool copy = true;

            if (!DA.GetDataList(0, source)) return;
            if (!DA.GetData(1, ref folder)) return;
            if (!DA.GetData(2, ref overwrite)) return;
            if (!DA.GetData(3, ref copy)) return;
            if (!DA.GetData(4, ref run)) return;
            List<string> info = new List<string>();
            if (run)
            {
                if (!System.IO.Directory.Exists(folder))
                {
                    // create the directory
                    System.IO.Directory.CreateDirectory(folder);
                    info.Add("Directory created at " + folder);
                }
                foreach (string s in source)
                {
                    try
                    {
                        if (copy)
                        {
                            System.IO.File.Copy(s, folder + "\\" + System.IO.Path.GetFileName(s), overwrite);
                            info.Add("File copied to " + folder + "\\" + System.IO.Path.GetFileName(s));
                        }
                        else
                        {
                            System.IO.File.Move(s, folder + "\\" + System.IO.Path.GetFileName(s));
                            info.Add("File moved to " + folder + "\\" + System.IO.Path.GetFileName(s));
                        }
                    }
                    catch (Exception e)
                    {
                        info.Add(e.Message);
                        break;
                    }
                }
            }
            DA.SetDataList(0, info);
        }

        #region Addition

        private bool run = false;

        #endregion


        

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "Run", Run);
        }
        private void Run()
        {
            run = true;
            this.ExpireSolution(true);
        }

        
        
    }
}