using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Windows.Forms;
using System.Xml.Linq;
using Chimera.IO;
using Chimera.UI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using Rhino.Runtime;
using Rhino.Runtime.RhinoAccounts;


namespace Chimera.Components
{
    public class FileCreatePath : GH_Component
    {
        #region Metadata
        public FileCreatePath()
            : base("Create Path", "Path",
                "Create a file path",
                "Chimera", "File")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "create path", "path", "constructpath", "createpath", "construct path" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.ConstructPath;
        public override Guid ComponentGuid => new Guid("3C4B1375-FF6A-4551-A72D-C3D5BDDAD31A");

        #endregion

        #region IO
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Directory", "D", "Directory for the file to write.", GH_ParamAccess.item);
            pManager.AddTextParameter("Filename", "N", "File name for the file to write.", GH_ParamAccess.item);
            pManager.AddTextParameter("Extension", "E", "Extension of the file to write", GH_ParamAccess.item);

            this.Params.Input[0].Optional = true;
            this.Params.Input[1].Optional = true;
            this.Params.Input[2].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "File path", GH_ParamAccess.item);
        }

        #endregion

        #region ClassVariables

        private string _menuCategory = "any";
        public string MenuCategory
        {
            get => _menuCategory;
            set
            {
                _menuCategory = value;
                Message = _menuCategory;
            }
        }

        #endregion

        #region (De)Serialization

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // First add our own field.
            writer.SetString("Mode", MenuCategory);
            // Then call the base class implementation.
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // First read our own field.
            try
            {
                MenuCategory = reader.GetString("Mode");
            }
            catch (Exception e)
            {
                MenuCategory = MenuCategory;
            }
            
            // Then call the base class implementation.
            return base.Read(reader);
        }

        #endregion

        #region Context Menu
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Open Directory", OpenDirectory, Properties.Resources.Window_New_Open);
            Menu_AppendItem(menu, "Create Extensions", ToggleSpawn, Properties.Resources.Add_New);
            Menu_AppendSeparator(menu);

            // Create the 'Presets' sub-menu
            ToolStripMenuItem presetsSubMenu = new ToolStripMenuItem("Presets");

            Menu_AppendItem(presetsSubMenu.DropDown, "ANY", ToggleAll, true).Checked = MenuCategory == "any";
            Menu_AppendItem(presetsSubMenu.DropDown, "OBJECT", ToggleObject, true).Checked = MenuCategory == "object";
            Menu_AppendItem(presetsSubMenu.DropDown, "TEXT", ToggleText, true).Checked = MenuCategory == "text";

            // Add the 'Presets' sub-menu to the main context menu
            menu.Items.Add(presetsSubMenu);
        }


        private void ToggleSpawn(object sender, EventArgs e)
        {
            RecordUndoEvent("ToggleSpawn");
            SpawnValueList();
            ExpireSolution(true);
        }

        private void ToggleAll(object sender, EventArgs e)
        {
            RecordUndoEvent("ToggleAll");
            MenuCategory = "any";
            ChangeValueList(MenuCategory);
            ExpireSolution(true);
        }

        private void ToggleObject(object sender, EventArgs e)
        {
            RecordUndoEvent("ToggleObject");
            MenuCategory = "object";
            ChangeValueList(MenuCategory);
            ExpireSolution(true);
        }
        private void ToggleText(object sender, EventArgs e)
        {
            RecordUndoEvent("ToggleText");
            MenuCategory = "text";
            ChangeValueList(MenuCategory);
            ExpireSolution(true);
        }

        private void OpenDirectory(object sender, EventArgs e)
        {
            string _path = Path.GetDirectoryName(path);
            if (path == null)
            {
                MessageBox.Show("Warning: path is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(_path);
            }
            catch (Exception ex)
            { 
                MessageBox.Show($"Unable to open directory!\nIs path {_path} exist?\n\n\n{ex}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ChangeValueList(string cat)
        {
            // this is the prefer method to find the connected components.
            if (Params.Input[2].SourceCount > 0)
            {
                ComponentInput input = new ComponentInput(OnPingDocument(), this);
                (string[] fullnames, string[] extension) = RetreiveListValues(cat);
                List<GH_ActiveObject> sourceObjects;
                try
                {
                    sourceObjects = input.GetSourceObjects(2, typeof(GH_ValueList));
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error: {e}");
                    return;
                }

                foreach (var ghActiveObject in sourceObjects)
                {
                    if (!(ghActiveObject is GH_ValueList list)) continue;

                    input.ChangeValueList(list, fullnames, extension);
                }
            }
        }

        private (string[], string[]) RetreiveListValues(string cat)
        {
            FileTypeManager manager = new FileTypeManager();
            List<FileTypeInfo> fileTypes = manager.GetFileTypesByCategory(cat);

            List<string> fullname = new List<string>();
            List<string> extension = new List<string>();

            if (fileTypes != null)
            {
                foreach (var fileType in fileTypes)
                {
                    fullname.Add(fileType.FullName);
                    extension.Add(fileType.Extension);
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File category not found");
            }
            return (fullname.ToArray(), extension.ToArray());
        }

        #endregion

        #region Button

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "CreatePath", CreatePathNotExist);
        }

        private void CreatePathNotExist()
        {
            if (path == null) return;
            try
            {
                path = Path.GetDirectoryName(path);

                bool created = Util.CreatePathNotExist(path);
                if (!created) return;
                MessageBox.Show("Path Created!", "Created!", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ExpireSolution(true);
            }
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string directory = null;
            string filename = null;
            string extension = null;
            DA.GetData(0, ref directory);
            DA.GetData(1, ref filename);
            DA.GetData(2, ref extension);

            

            if (!Util.InputHasData(this, 0) && !Util.InputHasData(this, 1))
            {
                path = null;
            }
            else if (!Util.InputHasData(this, 0))
            {
                path = filename + extension;
            }
            else if (!Util.InputHasData(this, 1))
            {
                path = directory;
            }
            else
            {
                path = directory + "\\" + filename + extension;
            }

            DA.SetData(0, path);
        }

        #region Additional

        private List<GH_ActiveObject> _activeObjects = new List<GH_ActiveObject>();
        private string path;
        private bool spawned;

        private void SpawnValueList()
        {
            if (this.Params.Input[2].SourceCount != 0) return;
            ComponentInput input = new ComponentInput(OnPingDocument(), this);
            int targetCount = 3;
            int targetIndex = targetCount - 1;

            (string[] fullnames, string[] extensions) = RetreiveListValues(MenuCategory);

            GH_ActiveObject obj = input.CreateValueListAt(targetIndex, fullnames, extensions, false, -150, -65);
            _activeObjects.Add(obj);
        }


        #endregion


    }
}