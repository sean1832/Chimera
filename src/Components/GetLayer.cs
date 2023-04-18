using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.DocObjects;

namespace Monkey.src.Components
{
    public class GetLayer : GH_Component
    {
        #region Metadata

        public GetLayer()
          : base("GetLayer", "GetLayer",
              "Get reference object layer name",
              "Monkey", "Utility")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "layername", "getlayername" };
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
            pManager.AddTextParameter("Layer Name", "Name", "The full name of the layer the geometry belongs to.", GH_ParamAccess.item);
            pManager.AddTextParameter("Base Name", "BaseName", "The base name of the layer without its parent.", GH_ParamAccess.item);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase inputGeometry = null;
            if (!DA.GetData(0, ref inputGeometry)) return;

            RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
            string layerName = GetGeometryLayerName(doc, inputGeometry);

            if (!string.IsNullOrEmpty(layerName))
            {
                DA.SetData(0, layerName);
                string ayerName = layerName.Split(new string[] { "::" }, StringSplitOptions.None).Last();
                DA.SetData(1, ayerName);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not find the layer name for the input geometry.\nIs the input geometry baked into rhino?");
                DA.SetData(0, string.Empty);
                DA.SetData(1, string.Empty);
            }
        }

        #region Additional

        private string GetGeometryLayerName(RhinoDoc doc, GeometryBase geometry)
        {
            foreach (RhinoObject obj in doc.Objects)
            {
                if (obj.Geometry.Equals(geometry))
                {
                    Layer layer = doc.Layers.FindIndex(obj.Attributes.LayerIndex);
                    return layer.Name;
                }
            }

            return null;
        }

        #endregion
    }
}