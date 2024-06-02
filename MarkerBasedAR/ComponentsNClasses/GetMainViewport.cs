using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using MarkerBasedAR.Properties;
using Rhino.Display;
using Rhino.Geometry;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class GetMainViewport : GH_Component
    {

        public GetMainViewport()
          : base("GetMainViewport", "Nickname",
              "Description",
              BasicInfo.Category, "Viewport")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("RhinoView", "RV", "The RhinoView to get the main viewport.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MainViewport", "M", "The MainViewport of the RhinoView.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RhinoView rv = null;
            if (!DA.GetData(0, ref rv))
                return;
            RhinoViewport mainViewport = rv.MainViewport;
            DA.SetData(0, mainViewport);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.GetMainViewport;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("063EB8E4-F269-4FA1-93D6-5BF139AB7D0F"); }
        }
    }
}