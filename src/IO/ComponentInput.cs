﻿using Rhino.NodeInCode;
using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using System.ComponentModel;
using Grasshopper.Kernel.Special;
using Monkey.src.Components;

namespace Monkey.src.InputComponents
{
    internal class ComponentInput
    {
        public GH_Document Document { get; set; }
        public IGH_Component Component { get; set; }

        public ComponentInput(GH_Document document, GH_Component component)
        {
            Document = document;
            Component = component;
        }

        /// <summary>
        /// Create a number slider at the specified index
        /// </summary>
        /// <param name="index">The param index to connect</param>
        /// <param name="defaultVal">Slider default value</param>
        /// <param name="min">Slider minimum value</param>
        /// <param name="max">Slider maximum value</param>
        /// <param name="local">Is the spawn method local? Local mode does not append parent component pivot position.</param>
        /// <returns>Created component object</returns>
        public GH_ActiveObject CreateSliderAt(int index, double defaultVal, double min, double max, bool local = false)
        {
            if (Component.Params.Input[index].Sources.Count != 0) return null;

            var slider = new Grasshopper.Kernel.Special.GH_NumberSlider();
            slider.CreateAttributes();
            slider.Slider.Minimum = (decimal)min;
            slider.Slider.Maximum = (decimal)max;

            var pt = CreateOnCanvasAt(slider, index, local);
            slider.Attributes.Pivot = pt;
            slider.SetSliderValue((decimal)defaultVal);

            Document.AddObject(slider, false);
            Component.Params.Input[index].AddSource(slider);

            return slider;
        }

        /// <summary>
        /// Create a Value List at the specified index
        /// </summary>
        /// <param name="index">The param index to connect</param>
        /// <param name="keys">List of keys</param>
        /// <param name="values">List of values</param>
        /// <param name="local">Is the spawn method local? Local mode does not append parent component pivot position.</param>
        /// <param name="name">Default component name</param>
        /// <returns>Created component object</returns>
        public GH_ActiveObject CreateValueListAt(int index, string[] keys, string[] values, bool local = false, string name = "ValueList")
        {
            if (Component.Params.Input[index].Sources.Count != 0) return null;

            var valueList = new Grasshopper.Kernel.Special.GH_ValueList();
            valueList.CreateAttributes();
            valueList.ListItems.Clear();
            for (int i = 0; i < keys.Length; i++)
            {
                valueList.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem(keys[i], values[i]));
            }
            valueList.SelectItem(0);
            valueList.Name = name;

            var pt = CreateOnCanvasAt(valueList, index, local);
            valueList.Attributes.Pivot = pt;

            
            Document.AddObject(valueList, false);
            Component.Params.Input[index].AddSource(valueList);

            return valueList;
        }

        public GH_ActiveObject ChangeValueList(GH_ValueList valueList, string[] keys, string[] values)
        {
            valueList.ListItems.Clear();
            for (int i = 0; i < keys.Length; i++)
            {
                valueList.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem(keys[i], values[i]));
            }
            valueList.ExpireSolution(true);
            return valueList;
        }

        public GH_ActiveObject CreateCustomComponentAt<T>(int inputIndex, int outputIndex, bool local = false) where T : GH_Component, new()
        {
            var customComponent = new T();
            var pt = CreateOnCanvasAt(customComponent, inputIndex, local);
            customComponent.CreateAttributes();

            customComponent.Attributes.Pivot = pt;

            Document.AddObject(customComponent, false);
            // Connect the first output of the custom component to the input of the target component.
            if (customComponent.Params.Output.Count > 0)
            {
                Component.Params.Input[inputIndex].AddSource(customComponent.Params.Output[outputIndex]);
            }


            return customComponent;
        }


        public List<GH_ActiveObject> GetSourceObjects(int index, Type objectType)
        {
            List<GH_ActiveObject> sourceObjects = new List<GH_ActiveObject>();
            foreach (IGH_Param sourceParam in Component.Params.Input[index].Sources)
            {
                
                GH_ActiveObject sourceObject = sourceParam.Attributes.GetTopLevel.DocObject as GH_ActiveObject;
                if (sourceObject != null && sourceObject.GetType() == objectType || sourceObject.GetType() == typeof(GH_ProxyParameter))
                {
                    sourceObjects.Add(sourceObject);
                }
                else
                {
                    throw new Exception($"Source object is not of type {objectType.Name}");
                }
            }

            return sourceObjects;
        }


        /// <summary>
        /// Remove the source type at the specified index
        /// </summary>
        /// <param name="index">The param index to remove</param>
        /// <param name="sourcesObj">List of objects to remove</param>
        public void RemoveSourceTypeAt(int index, List<GH_ActiveObject> sourcesObj)
        {
            Document.ScheduleSolution(0, (val) =>
            {
                foreach (var source in sourcesObj)
                {
                    Document.RemoveObject(source.Attributes, false);
                }
            });
            
            Component.Params.Input[index].Sources.Clear();
        }

        #region Helper Methods

        /// <summary>
        /// Calculate the position of the object on the canvas
        /// </summary>
        /// <param name="obj">The component object to spawn.</param>
        /// <param name="index">The parameter index of the parent component to connect to.</param>
        /// <param name="local">Is the spawn method local? Local mode does not append parent component pivot position.</param>
        /// <returns>Translated position</returns>
        private PointF CreateOnCanvasAt(GH_ActiveObject obj, int index, bool local)
        {
            float locX;
            float locY;
            if (local)
            {
                locX = Component.Attributes.DocObject.Attributes.Bounds.X;
                locY = Component.Attributes.DocObject.Attributes.Bounds.Y;
                
            }
            else
            {
                var pivot = Component.Attributes.Pivot;
                locX = Component.Attributes.DocObject.Attributes.Bounds.X + pivot.X;
                locY = Component.Attributes.DocObject.Attributes.Bounds.Y + pivot.Y;
            }

            int inputCount = Component.Params.Input.Count;
            locX = locX - obj.Attributes.Bounds.Width;
            locY = locY - Component.Params.Input[index].Attributes.Bounds.Y + inputCount * 15;

            return new PointF(locX, locY);
        }

        #endregion
    }
}
