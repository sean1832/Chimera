using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Monkey
{
    public class MonkeyInfo : GH_AssemblyInfo
    {
        public override string Name => "Monkey";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("69e1ddb9-1ff4-4442-b1b8-9af129e252c6");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}