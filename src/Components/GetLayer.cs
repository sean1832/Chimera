using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;

namespace Chimera.Components
{
    public class GetLayer : GH_Component
    {
        #region Metadata

        public GetLayer()
          : base("GetLayerName", "GetLayer",
              "Get reference object layer name",
              "Chimera", "Utility")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "layername", "getlayername", "layer name" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Layer_Name; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("0f49af08-6400-4c6b-b123-0a062a94bbe4");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "Geo", "The reference geometry to find its layer.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Layer Path", "Path", "The full name of the layer the geometry belongs to.", GH_ParamAccess.item);
            pManager.AddTextParameter("Layer Name", "Name", "The base name of the layer without its parent.", GH_ParamAccess.item);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_GeometricGoo goo = null;
            if (!DA.GetData(0, ref goo)) return;
            if (goo == null) return;

            if (!goo.IsReferencedGeometry)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Geometry is not referenced and therefore doesn't have attributes.");
                return;
            }

            Guid id = goo.ReferenceID;
            if (id == Guid.Empty)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Reference ID is blank.");
                return;
            }

            ObjRef objRef = new ObjRef(id);
            if (objRef.Object() == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Referenced object no longer exists in the current document.");
                return;
            }

            if (objRef.Object().Document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Referenced object is not associated with a document.");
                return;
            }

            RhinoDoc doc = RhinoDoc.ActiveDoc;
            RhinoObject obj = objRef.Object();
            string fullPath = doc.Layers[obj.Attributes.LayerIndex].FullPath;
            string layerName = doc.Layers[obj.Attributes.LayerIndex].Name;

            DA.SetData(0, fullPath);
            DA.SetData(1, layerName);
        }

        #region Additional

        #endregion
    }
}