﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;


namespace Monkey.Components
{
    public class Attributes_Custom : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        public Attributes_Custom(GH_Component owner) : base(owner) { }

        protected override void Layout()
        {
            base.Layout();

            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(Bounds);
            rec0.Height += 22;

            System.Drawing.Rectangle rec1 = rec0;
            rec1.Y = rec1.Bottom - 22;
            rec1.Height = 22;
            rec1.Inflate(-2, -2);

            Bounds = rec0;
            ButtonBounds = rec1;
        }
        private System.Drawing.Rectangle ButtonBounds { get; set; }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Hack", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
        }
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Drawing.RectangleF rec = ButtonBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    (Owner as MK_Hacker).TriggerHack();
                    return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }

    public class MK_Hacker : GH_Component
    {
        private GH_Document GrasshopperDocument;
        private IGH_Component Component;
        private bool hack;

        public void TriggerHack()
        {
            hack = true;
            ExpireSolution(true);
        }

        /// <summary>
        /// Initializes a new instance of the MK_Hacker class.
        /// </summary>
        public MK_Hacker()
          : base("MK_Hacker", "Hacker",
              "Hack a password protected cluster. Group the target cluster with this component.",
              "Monkey", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Hashcode", "C", "Hash code of the target password to replace.\nDefault: pass",
                GH_ParamAccess.item, "YfhFR2gfV6DIwQxPdvuHeg==");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "I", "output messages", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region Variables

            Component = this;
            GrasshopperDocument = OnPingDocument();
            List<string> infos = new List<string>();
            #endregion

            #region Inputs

            string hashcode = string.Empty;

            if (!DA.GetData(0, ref hashcode)) return;

            #endregion

            // get all cluster objects that grouped with this component
            List<GH_Cluster> clusters = FindObjectsOfTypeInCurrentGroup<GH_Cluster>();
            if (clusters.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No target cluster detected. Group target cluster for cracking with this component.");
                Message = "No cluster detected!";
                return;
            }

            if (hack)
            {
                foreach (GH_Cluster cluster in clusters)
                {
                    // use reflection to access its private member fields
                    Type type = typeof(GH_Cluster);
                    System.Reflection.FieldInfo[] fields = type.GetFields(
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

                    // Replace current password with new hashcode
                    fields[0].SetValue(cluster, Convert.FromBase64String(hashcode));
                }

                if (hashcode == "YfhFR2gfV6DIwQxPdvuHeg==")
                {
                    infos.Add($"Cluster password set to 'pass'.");
                }
                else
                {
                    infos.Add($"Cluster password set to hashcode equivalent {hashcode}.");
                }

                MessageBox.Show("Cluster hacked.", "Hacking status", MessageBoxButtons.OK);
                Message = "Hacked!";
                hack = false;
            }
            

            #region Outputs

            // output
            DA.SetDataList(0, infos);

            #endregion
        }

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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.Monkey___Hacker;
            }
        }

        public override void CreateAttributes()
        {
            m_attributes = new Attributes_Custom(this);
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("281A572A-50AD-4F49-A21E-1C6EBB74F674"); }
        }
    }
}