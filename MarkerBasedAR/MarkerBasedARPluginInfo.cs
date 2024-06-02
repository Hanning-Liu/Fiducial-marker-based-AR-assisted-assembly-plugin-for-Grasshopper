using Grasshopper;
using Grasshopper.Kernel;
using MarkerBasedAR.Properties;
using System;
using System.Drawing;

namespace MarkerBasedAR
{
    public class MarkerBasedARPluginInfo : GH_AssemblyInfo
    {
        public override string Name => "MarkerBasedAR";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Resources.appIcon;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "MarkerBasedAR";

        public override Guid Id => new Guid("bdc665ff-4703-444e-aaa3-997374a0c6dd");

        //Return a string identifying you or your company.
        public override string AuthorName => "HanningLiu";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "liuhanning@tongji.edu.cn";
    }
}