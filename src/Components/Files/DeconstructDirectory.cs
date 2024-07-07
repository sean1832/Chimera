using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

namespace Chimera.Components.Files
{
    public class DeconstructDirectory : GH_Component, IGH_VariableParameterComponent
    {
        #region Metadata

        public DeconstructDirectory()
          : base("Deconstruct Directory", "DeDirectory",
              "Deconstruct a given directory to its elements",
              "Chimera", "File")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "dedir", "deconstructdirectory" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.DeconstructDirectory; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("c29c3d35-26a4-42ec-a607-bb21a27908df");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Directory", "Dir", "Directory to deconstruct.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string directory = "";
            if (!DA.GetData(0, ref directory)) return;
            _pathElements = new List<string>(directory.Split('\\'));
            if (_pathElements.Count == 0) return;
            // remove the drive ":"
            if (_pathElements[0].EndsWith(":")) _pathElements[0] = _pathElements[0].TrimEnd(':');

            // remove any empty elements
            _pathElements.RemoveAll(string.IsNullOrEmpty);


            if (OutputMismatch())
            {
                OnPingDocument().ScheduleSolution(5, doc => AutoCreateOutputs());
            }
            else
            {
                foreach (var element in _pathElements)
                {
                    if (string.IsNullOrEmpty(element)) continue;
                    DA.SetData(element, element);
                }
            }

        }

        #region Additional

        private List<string> _pathElements = new List<string>();

        #endregion

        private bool OutputMismatch()
        {
            var countMatch = _pathElements.Count == Params.Output.Count;
            if (!countMatch) return true;

            foreach (var element in _pathElements)
            {
                if (string.IsNullOrEmpty(element)) continue;
                if (Params.Output.Select(p => p.Name).All(n => n != element))
                {
                    return true;
                }
            }
            return false;
        }

        private void AutoCreateOutputs()
        {
            var tokenCount = _pathElements.Count;
            if (tokenCount == 0) return;

            RecordUndoEvent("output from directory deconstruction");
            if (Params.Output.Count < tokenCount)
            {
                while (Params.Output.Count < tokenCount)
                {
                    var newParam = CreateParameter(GH_ParameterSide.Output, Params.Output.Count);
                    Params.RegisterOutputParam(newParam);
                }
            }
            else if (Params.Output.Count > tokenCount)
            {
                while (Params.Output.Count > tokenCount)
                {
                    Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1]);
                }
            }
            Params.OnParametersChanged();
            VariableParameterMaintenance();
            ExpireSolution(true);
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_String();
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            if (_pathElements == null) return;
            for (int i = 0; i < _pathElements.Count; i++)
            {
                string token = _pathElements[i];
                if (string.IsNullOrEmpty(token)) continue;
                var param = Params.Output[i];
                if (param == null) continue;

                param.Name = token;
                param.NickName = token;
                param.Description = $"Directory element {i}";
                param.MutableNickName = true;
            }
        }

        public void Dispose()
        {
            ClearData();
            foreach (var param in Params)
            {
                param.ClearData();
            }
        }
    }
}