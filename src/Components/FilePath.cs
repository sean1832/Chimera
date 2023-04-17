using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Monkey.src.InputComponents;
using Monkey.src.IO;
using Rhino.Geometry;
using Rhino.Runtime.RhinoAccounts;


namespace Monkey.src.Components
{
    public class FilePath : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FilePath class.
        /// </summary>
        public FilePath()
          : base("Create Path", "Path",
              "Create a file path",
              "Monkey", "File")
        {
        }

        public override IEnumerable<string> Keywords
        {
            get
            {
                return new string[] { "create path", "path" };
            }
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "All", ToggleAll, true).Checked = category == "all";
            Menu_AppendItem(menu, "Object", ToggleObject, true).Checked = category == "object";
            Menu_AppendItem(menu, "Text", ToggleText, true).Checked = category == "text";
        }

        public string category = "all";


        private void ToggleAll(object sender, EventArgs e)
        {
            category = "all";

            ChangeValueList(category);
            
            ExpireSolution(true);
        }

        private void ToggleObject(object sender, EventArgs e)
        {
            category = "object";

            ChangeValueList(category);
            
            ExpireSolution(true);
        }
        private void ToggleText(object sender, EventArgs e)
        {
            category = "text";

            ChangeValueList(category);
            
            ExpireSolution(true);
        }

        public void ChangeValueList(string cat)
        {
            ComponentInput input = new ComponentInput(OnPingDocument(), this);
            (string[] fullnames, string[] extension) = GetListValues(cat);

            // this is the prefer method to find the connected components.
            if (Params.Input[2].SourceCount > 0)
            {
                List<GH_ActiveObject> sourceObjects = new List<GH_ActiveObject>();
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
                    if (ghActiveObject is GH_ValueList)
                    {
                        input.ChangeValueList((GH_ValueList)ghActiveObject, fullnames, extension);
                    }
                }
            }
        }

        private (string[], string[]) GetListValues(string cat)
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

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            if (this.Params.Input[2].SourceCount != 0) return;
            ComponentInput input = new ComponentInput(OnPingDocument(), this);
            int targetCount = 3;
            int targetIndex = targetCount - 1;

            (string[] fullnames, string[] extensions) = GetListValues(category);

            GH_ActiveObject obj = input.CreateValueListAt(targetIndex, fullnames, extensions, false);
            _activeObjects.Add(obj);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Directory", "D", "Directory for the file to write.", GH_ParamAccess.item);
            pManager.AddTextParameter("Filename", "N", "File name for the file to write.", GH_ParamAccess.item);
            pManager.AddTextParameter("File Extension", "E", "Extension of the file to write", GH_ParamAccess.item);

            this.Params.Input[0].Optional = true;
            this.Params.Input[1].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "File path", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string directory = null;
            string filename = null;
            string extension = null;
            DA.GetData(0, ref directory);
            DA.GetData(1, ref filename);
            DA.GetData(2, ref extension);

            string path;

            if (!Util.InputHasData(this, 0))
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

        #endregion

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
            get { return new Guid("3C4B1375-FF6A-4551-A72D-C3D5BDDAD31A"); }
        }
    }
}