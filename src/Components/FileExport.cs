using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Chimera.IO;
using Chimera.Properties;
using Chimera.UI;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Chimera.Components
{
    public class FileExport : GH_Component
    {
        #region Metadata

        public FileExport()
            : base("File Export", "Export",
                "Export a grasshopper object to a designated path.",
                "Chimera", "File")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { "export", "export object", "file export" };
        protected override Bitmap Icon => Resources.FileExport;
        public override Guid ComponentGuid => new Guid("CF2B164F-C51A-4698-A0C6-D19C6731E2BF");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to export. Any breps or subd will be converted to mesh.",
                GH_ParamAccess.list);
            pManager.AddTextParameter("[]Path", "[]P", "Path to export.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "RUN", "Execute the operation.", GH_ParamAccess.item, false);

            Params.Input[0].Optional = true;
            Params.Input[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "I", "Output information", GH_ParamAccess.list);
        }

        #endregion

        #region Button

        private bool runBut;
        private void Run()
        {
            runBut = true;
            ExpireSolution(true);
        }

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "Export", Run);
        }

        #endregion

        #region Class Variable

        public bool IsGroupExport { get; set; } = false;

        #endregion

        #region (De)Serialization

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // First add our own field.
            writer.SetBoolean("BindMode", IsGroupExport);
            // Then call the base class implementation.
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // First read our own field.
            try
            {
                IsGroupExport = reader.GetBoolean("BindMode");
            }
            catch (Exception e)
            {
                // default value
                IsGroupExport = false;
            }
            
            // Then call the base class implementation.
            return base.Read(reader);
        }

        #endregion

        #region Context Menu

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            var spawn = Menu_AppendItem(menu, "Create Path", Spawn, Properties.Resources.Add_New);
            spawn.ToolTipText = "Create a new path input.";

            ToolStripMenuItem bind = Menu_AppendItem(menu, "Group Mode", ToggleBind, Properties.Resources.Product_Box_03_WF,true, IsGroupExport);
            bind.ToolTipText = "When checked, merge and export all objects as one file.";
        }

        private void ToggleBind(object sender, EventArgs e)
        {
            RecordUndoEvent("ToggleBind");
            IsGroupExport = !IsGroupExport;
            ExpireSolution(true);
        }

        private void Spawn(object sender, EventArgs e)
        {
            RecordUndoEvent("CreatePath");
            SpawnComponent();
            ExpireSolution(true);
        }

        private void SpawnComponent()
        {
            if (this.Params.Input[1].SourceCount != 0) return;

            var input = new ComponentInput(OnPingDocument(), this);
            var filePathComponent = input.CreateCustomComponentAt<FileCreatePath>(1, 0, -40, -70);
            if (filePathComponent is FileCreatePath filePath)
            {
                filePath.ChangeValueList("object");
                filePath.MenuCategory = "object";
                filePath.ExpireSolution(true);
            }
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (IsGroupExport)
            {
                Message = "Group";
            }
            else
            {
                Message = "Unit";
            }

            List<Mesh> meshes = new List<Mesh>();
            string path = null;
            bool run = false;
            if (!DA.GetDataList(0, meshes)) return;
            if (!DA.GetData(1, ref path)) return;
            DA.GetData(2, ref run);

            List<string> info = new List<string>();

            string directory = System.IO.Path.GetDirectoryName(path);
            string filename = System.IO.Path.GetFileNameWithoutExtension(path);
            string extension = System.IO.Path.GetExtension(path);

            if (run || runBut)
            {
                // if path not exist, create it
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                Rhino.RhinoDoc.ActiveDoc.Objects.UnselectAll(); // unselect all
                string filepath;
                var attribute = new Rhino.DocObjects.ObjectAttributes();
                if (!IsGroupExport)
                {
                    try
                    {
                        for (int i = 0; i < meshes.Count; i++)
                        {
                            Mesh mesh = meshes[i];
                            filepath = directory + "\\" + $"{filename}_{i}" + extension;
                            Guid guid = Rhino.RhinoDoc.ActiveDoc.Objects.Add(mesh, attribute);
                            var rhinoObj = Rhino.RhinoDoc.ActiveDoc.Objects.Find(guid);
                            rhinoObj.Select(true);

                            string cmd = "_-Export " + "\"" + filepath + "\"" + " _Enter _Enter";
                            Rhino.RhinoApp.RunScript(cmd, false);
                            Rhino.RhinoDoc.ActiveDoc.Objects.Delete(guid, true);

                            info.Add($"Exported files: [{filename}_{i}{extension}] exported to [{filepath}]");
                        }
                    }
                    catch (Exception e)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error.NotBind:\n{e.Message}");
                        info.Add($"Error.NotBind:\n{e.Message}");
                    }
                }
                else
                {
                    try
                    {
                        List<Guid> guids = new List<Guid>();
                        filepath = directory + "\\" + $"{filename}" + extension;

                        for (int i = 0; i < meshes.Count; i++)
                        {
                            Mesh mesh = meshes[i];
                            Guid guid = Rhino.RhinoDoc.ActiveDoc.Objects.Add(mesh, attribute);
                            guids.Add(guid);
                            var rhinoObj = Rhino.RhinoDoc.ActiveDoc.Objects.Find(guid);
                            rhinoObj.Select(true);
                        }
                        string cmd = "_-Export " + "\"" + filepath + "\"" + " _Enter _Enter";
                        Rhino.RhinoApp.RunScript(cmd, false);

                        foreach (var guid in guids)
                        {
                            Rhino.RhinoDoc.ActiveDoc.Objects.Delete(guid, true);
                        }
                        info.Add($"Export file as one: [{filename}{extension}] to [{filepath}]");
                    }
                    catch (Exception e)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error.Bind:\n{e.Message}");
                        info.Add($"Error.NotBind:\n{e.Message}");
                    }
                }
                Rhino.RhinoDoc.ActiveDoc.Objects.UnselectAll(); // unselect all
                runBut = false;
                DA.SetDataList(0, info);
            }
        }

        #region Additional

       

        #endregion






    }
}