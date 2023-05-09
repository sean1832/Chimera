using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chimera.IO;
using Chimera.UI;
using Grasshopper.Kernel;

namespace Chimera.Components.Files
{
    public class FileWrite : GH_Component
    {
        #region Metadata

        public FileWrite()
            : base("File Write", "Write",
                "Write content to a file. Input content must be pre-formatted to correctly export.",
                "Chimera", "File")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { "write", "write file" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.FileWrite;
        public override Guid ComponentGuid => new Guid("08A598CC-471D-4970-AE2C-0EC50E334EF4");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Content", "C", "Content to write to file.", GH_ParamAccess.item);
            pManager.AddTextParameter("[]Path", "[]P", "File path to write.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write", "W", "Write content to file.", GH_ParamAccess.item, false);

            Params.Input[0].Optional = true;
            Params.Input[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "I", "Output information.", GH_ParamAccess.list);
        }

        #endregion

        #region Context Menu

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Create Path", Spawn, Properties.Resources.Add_New);
        }

        private void Spawn(object sender, EventArgs e)
        {
            RecordUndoEvent("CreatePath");
            SpawnComponent();
            ExpireSolution(true);
        }

        private void SpawnComponent()
        {
            if (Params.Input[1].SourceCount != 0) return;

            var input = new ComponentInput(OnPingDocument(), this);
            var filePathComponent = input.CreateCustomComponentAt<FileCreatePath>(1, 0, -40, -70);
            if (filePathComponent is FileCreatePath filePath)
            {
                filePath.ChangeValueList("text");
                filePath.MenuCategory = "text";
                filePath.ExpireSolution(true);
            }
        }

        #endregion

        #region Button

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "Write", Write);
        }
        private void Write()
        {
            writeBut = true;
            ExpireSolution(true);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string content = null;
            string path = null;
            bool write = false;
            DA.GetData(0, ref content);
            DA.GetData(1, ref path);
            if (!DA.GetData(2, ref write)) return;

            
            if (write || writeBut)
            {
                if (content == null)
                {
                    MessageBox.Show("Error: No content to write!\n\nPlease make sure content input is not empty.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DA.AbortComponentSolution();
                    return;
                }

                if (path == null)
                {
                    MessageBox.Show("Error: No path to write to!\n\nPlease make sure path input is not empty.", "Error",
                                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DA.AbortComponentSolution();
                    return;
                }

                msgs.Clear();
                try
                {
                    File.WriteAllText(path, content);
                    string msg = $"Text has been written to the file '{path}'.";
                    msgs.Add(msg);
                }
                catch (Exception ex)
                {
                    string msg = $"An error occurred: {ex.Message}";
                    msgs.Add(msg);
                }
                writeBut = false;
            }
            DA.SetDataList(0, msgs);

        }

        #region Addition

        List<string> msgs = new List<string>();
        private bool writeBut;

        #endregion


    }
}