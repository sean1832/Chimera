using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Reflection;

namespace Monkey.src
{
    public class MonkeyInfo : GH_AssemblyInfo
    {
        public override string Name => GetInfo<AssemblyProductAttribute>().Product;
        public override string Version => GetInfo<AssemblyInformationalVersionAttribute>().InformationalVersion;
        public override Bitmap Icon => null;
        public override string Description => GetInfo<AssemblyDescriptionAttribute>().Description;
        public override Guid Id => new Guid("69e1ddb9-1ff4-4442-b1b8-9af129e252c6");
        public override string AuthorName => GetInfo<AssemblyCompanyAttribute>().Company;
        public override string AuthorContact => "https://github.com/sean1832";


        T GetInfo<T>() where T : Attribute
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetCustomAttribute<T>();
        }
    }
}