using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Monkey.src.InputComponents;
using Monkey.src.UI;
using Rhino.Geometry;

namespace Monkey.src.Components
{
    public class FileWrite : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FileWrite class.
        /// </summary>
        public FileWrite()
          : base("File Write", "Write",
              "Write content to a file. Input content must be pre-formatted to correctly export.",
              "Monkey", "File")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public override IEnumerable<string> Keywords
        {
            get
            {
                return new string[] { "write" };
            }
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            if (this.Params.Input[1].SourceCount != 0) return;

            var input = new ComponentInput(document, this);

            var filePathComponent = input.CreateCustomComponentAt<FileCreatePath>(1, 0);
            if (filePathComponent is FileCreatePath filePath)
            {
                filePath.ChangeValueList("text");
                filePath.category = "text";
                filePath.ExpireSolution(true);
            }

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Content", "C", "Content to write to file.", GH_ParamAccess.item);
            pManager.AddTextParameter("[]Path", "[]P", "File path to write.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write", "W", "Write content to file.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "I", "Output information.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string content = null;
            string path = null;
            bool write = false;
            if (!DA.GetData(0, ref content)) return;
            if (!DA.GetData(1, ref path)) return;
            if (!DA.GetData(2, ref write)) return;

            
            if (write || writeBut)
            {
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
        List<string> msgs = new List<string>();
        private bool writeBut;

        private void Write()
        {
            writeBut = true;
            ExpireSolution(true);
        }

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "Write", Write);
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
                return Properties.Resources.Monkey_FileWrite;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("08A598CC-471D-4970-AE2C-0EC50E334EF4"); }
        }
    }
}