using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkey.src
{
    internal class GH_Operations
    {
        IGH_Component component;

        public GH_Operations(IGH_Component component)
        {
            this.component = component;
        }


        public List<double> NormalizeList<T>(List<T> list)
        {
            if (!Util.IsNumericType(typeof(T)))
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid input. Normalize List only accept int and double. " + typeof(T).ToString());
            }
            double minValue = list.Min(x => Convert.ToDouble(x));
            double maxValue = list.Max(x => Convert.ToDouble(x));
            double range = maxValue - minValue;

            List<double> normalizedList = new List<double>(list.Count);

            foreach (T value in list)
            {
                double normalizedValue = (Convert.ToDouble(value) - minValue) / range;
                normalizedList.Add(normalizedValue);
            }

            return normalizedList;
        }


        // Cubic Bezier function
        public static double Bezier(double t, double a, double b, double c, double d)
        {
            double s = 1 - t;
            return Math.Pow(s, 3) * a + 3 * Math.Pow(s, 2) * t * b + 3 * s * Math.Pow(t, 2) * c + Math.Pow(t, 3) * d;
        }

        public static double Remap(double value, double sourceMin, double sourceMax, double targetMin, double targetMax)
        {
            return (value - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
        }
        public static List<double> Remap(List<double> value, double sourceMin, double sourceMax, double targetMin, double targetMax)
        {
            List<double> output = new List<double>();
            foreach (double val in value)
            {
                double remapped = (val - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
                output.Add(remapped);
            }
            return output;
        }
    }
}
