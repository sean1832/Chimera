using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Monkey.src.UI;
using Rhino.Geometry;

namespace Monkey.src.Components
{
    public class MK_Decipher : GH_Component
    {
        private GH_Document GrasshopperDocument;
        private IGH_Component Component;
        /// <summary>
        /// Initializes a new instance of the MK_Decipher class.
        /// </summary>
        public MK_Decipher()
          : base("Decipher", "Decipher",
              "Get the hashcode of a cluster password.\nTO USE: group the target cluster with this component.",
              "Monkey", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Hashcode", "C", "Hashcode of target cluster password", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Component = this;
            GrasshopperDocument = OnPingDocument();

            string password = string.Empty;

            // get all cluster objects that grouped with this component
            List<GH_Cluster> clusters = FindObjectsOfTypeInCurrentGroup<GH_Cluster>();
            if (clusters.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No target cluster detected. Group target cluster for cracking with this component.");
                Message = "No cluster detected!";
                return;
            }

            foreach (GH_Cluster cluster in clusters)
            {
                // use reflection to access its private member fields
                Type type = typeof(GH_Cluster);
                System.Reflection.FieldInfo[] fields = type.GetFields(
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                // get cluster password hashcode
                byte[] passwordBytes = (byte[])fields[0].GetValue(cluster);
                if (passwordBytes == null)
                {
                    Message = "Password is empty!";
                    return;
                }
                password = Convert.ToBase64String(passwordBytes);
                Message = "Deciphered!";
            }


            // output
            DA.SetData(0, password);
        }

        #region Additional

        /// <summary>
        /// Find all objects of type T in any group this current script instance is a member of.
        /// Snipped fetch from ScriptParasite Github Repo:
        /// https://github.com/arendvw/ScriptParasite/blob/master/Component/ScriptParasiteComponent.cs#L563
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A list of objects of type T that belong to group T</returns>
        private List<T> FindObjectsOfTypeInCurrentGroup<T>() where T : IGH_ActiveObject
        {
            // find all groups that this object is in.
            var groups = OnPingDocument()
                .Objects
                .OfType<GH_Group>()
                .Where(gr => gr.ObjectIDs.Contains(InstanceGuid))
                .ToList();

            // find in the groups that this object is in all objects of type T.
            var output = groups.Aggregate(new List<T>(), (list, item) =>
            {
                list.AddRange(
                    OnPingDocument().Objects.OfType<T>()
                        .Where(obj => item.ObjectIDs.Contains(obj.InstanceGuid))
                );
                return list;
            }).Distinct().ToList();

            return output;
        }

        #endregion

        private void Update()
        {
            ExpireSolution(false);
        }

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "Update", Update);
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
                return Properties.Resources.Monkey___Decipher;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("594F37F4-7259-4FEB-8048-B5E2BA17CFC2"); }
        }
    }
}