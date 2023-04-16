using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;
using Grasshopper.Kernel;
using Monkey.src.InputComponents;
using Monkey.src.UI;
using Rhino.Geometry;
using Rhino.NodeInCode;


namespace Monkey.src.Components
{
    public class FileExport : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FileExport class.
        /// </summary>
        public FileExport()
          : base("File Export", "Export",
              "Export a grasshopper object to a designated path.",
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
                return new string[] { "export", "export object", "file export" };
            }
        }


        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            ComponentInput input = new ComponentInput(document, this);
            string[] extensions = { "\".fbx\" ", "\".obj\"" };
            string[] extensionsFullname = { "Autodesk FBX", "Wavefront OBJ" };
            input.CreateValueListAt(3, extensionsFullname, extensions, name: "Extension");
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to export. Any breps or subd will be converted to mesh.",
                GH_ParamAccess.list);
            pManager.AddTextParameter("Directory", "D", "Directory for the geometry to export.", GH_ParamAccess.item);
            pManager.AddTextParameter("Filename", "N", "File name for the exported geometry.", GH_ParamAccess.item);
            pManager.AddTextParameter("File Extension", "E", "Extension of the mesh to export\n(.fbx .obj)",
                GH_ParamAccess.item);
            pManager.AddBooleanParameter("Bind", "B", "Export meshes as one file", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Run", "RUN", "Execute the operation.", GH_ParamAccess.item, false);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "I", "Output information", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> meshes = new List<Mesh>();
            string extension = null;
            string filename = null;
            string directory = null;
            bool bind = false;
            bool run = false;
            if (!DA.GetDataList(0, meshes)) return;
            if (!DA.GetData(1, ref directory)) return;
            if (!DA.GetData(2, ref filename)) return;
            if (!DA.GetData(3, ref extension)) return;
            if (!DA.GetData(4, ref bind)) return;
            if (!DA.GetData(5, ref run)) return;
            List<string> info = new List<string>();

            var bounds = this.Attributes.DocObject.Attributes.Bounds;
            DA.SetData(1, bounds.ToString());

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
                if (!bind)
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
                if (showDir)
                {
                    // show dialog ask for open export directory
                    var dialogResult = MessageBox.Show("Exported!\nDo you want to open directory?", "Export Status", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (dialogResult == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(directory);
                    }
                    else
                    {
                        showDir = false;
                    }
                }
            }
        }

        private bool runBut;
        private bool showDir = true;


        private void Run()
        {
            runBut = true;
            ExpireSolution(true);
        }

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "Export", Run);
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
            get { return new Guid("CF2B164F-C51A-4698-A0C6-D19C6731E2BF"); }
        }
    }
}