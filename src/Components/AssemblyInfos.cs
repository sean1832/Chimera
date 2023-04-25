using System;
using System.Collections.Generic;
using Chimera.UI;
using Grasshopper.Kernel;

namespace Chimera.Components
{
    public class AssemblyInfos : GH_Component
    {
        #region Metadata

        public AssemblyInfos()
          : base("Dependency", "Depends",
              "Checks and outputs information about the plugins used in the current Grasshopper file, their versions, and the assembly info of all plugins available on this computer.",
              "Chimera", "Utility")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { "dependency", "requirements", "assembly", "assemblyinfo", "plugins" };
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Dependency; // Properties.Resources.Icon
        public override Guid ComponentGuid => new Guid("2f210d7b-c155-4598-98b6-b4c557224175");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("PluginDependency", "D", "Checks dependency of current Grasshopper file.", GH_ParamAccess.list);
            pManager.AddTextParameter("AssemblyInfo", "A", "Assembly info of all plugins available on this computer.", GH_ParamAccess.list);
        }

        #endregion

        #region Button

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "Refresh", Refresh);
        }
        private void Refresh()
        {
            ExpireSolution(true);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var dependency = ConvertDictToList(GetDependency());
            var assembly = ConvertDictToList(GetAllAssembly());

            // sort alphabetically
            dependency.Sort();
            assembly.Sort();

            DA.SetDataList(0, dependency);
            DA.SetDataList(1, assembly);
        }

        #region Additional

        private Dictionary<string, string> GetAllAssembly()
        {
            Dictionary<string, string> assemblyDict = new Dictionary<string, string>();

            foreach (var obj in Grasshopper.Instances.ComponentServer.ObjectProxies)
            {
                var assembly = Grasshopper.Instances.ComponentServer.FindAssemblyByObject(obj.Guid);

                if (assembly != null && !assembly.IsCoreLibrary)
                {
                    if (assembly.Name == "" || assembly.Name == null && assembly.Version == "" || assembly.Version == null) continue;
                    assemblyDict[assembly.Name] = assembly.Version;
                }
            }
            return assemblyDict;
        }

        private Dictionary<string, string> GetDependency()
        {
            Dictionary<string, string> dependencyDict = new Dictionary<string, string>();

            foreach (var obj in Grasshopper.Instances.ActiveCanvas.Document.Objects)
            {
                GH_AssemblyInfo assembly = Grasshopper.Instances.ComponentServer.FindAssemblyByObject(obj.ComponentGuid);
                if (assembly != null && !assembly.IsCoreLibrary)
                {
                    dependencyDict[assembly.Name] = assembly.Version;
                }
            }
            return dependencyDict;
        }

        private List<string> ConvertDictToList(Dictionary<string, string> dict)
        {
            List<string> list = new List<string>();
            foreach (var item in dict)
            {
                list.Add(item.Key + " : " + item.Value);
            }
            return list;
        }

        #endregion
    }
}