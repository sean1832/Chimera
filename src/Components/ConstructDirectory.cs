using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel.Parameters;

namespace Chimera.src.Components
{
    public class ConstructDirectory : GH_Component, IGH_VariableParameterComponent
    {
        #region Metadata

        public ConstructDirectory()
          : base("ConstructDirectory", "Directory",
              "Construct Directory from a list of elements",
              "Chimera", "File")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "directory", "consdir", "dir" };
        protected override System.Drawing.Bitmap Icon => null; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("f3ecb43b-d861-483f-a6a0-f4892f8d4c0b");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Drive", "Dv", "Drive of the directory", GH_ParamAccess.item, "c");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Directory", "Dir", "Directory", GH_ParamAccess.item);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string drive = "";
            if (!DA.GetData(0, ref drive)) return;
            string directory = drive + ":\\";
            for (int i = 0; i < Params.Input.Count; i++)
            {
                if (Params.Input[i].Name == "Drive") continue;
                string path = "";
                DA.GetData(i, ref path);
                if (string.IsNullOrEmpty(path)) continue;

                directory += path + "\\";
            }
            DA.SetData(0, directory);

        }

        #region Additional



        #endregion


        #region Create Variable Parameters

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (Params.Input.Count > 1)
            {
                if (Params.Input[index].Name == "Drive")
                {
                    return false;
                }
                return side == GH_ParameterSide.Input;
            }
            else
            {
                return false;
            }
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var param = new Param_String
            {
                NickName = "-"
            };
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            for (int i = 0; i < Params.Input.Count; i++)
            {
                var param = Params.Input[i];
                if (param.NickName == "-")
                {
                    param.Name = $"Path {i}";
                    param.NickName = $"P{i}";
                    param.Description = $"Input path {i}";
                    param.Optional = true;
                    param.MutableNickName = true;
                }
            }
        }

        #endregion
    }
}