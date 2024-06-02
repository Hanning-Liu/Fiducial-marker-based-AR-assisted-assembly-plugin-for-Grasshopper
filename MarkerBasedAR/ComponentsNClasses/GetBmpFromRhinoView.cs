using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using MarkerBasedAR.Properties;
using Rhino.Display;
using Rhino.Geometry;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class GetBmpFromRhinoView : GH_Component
    {

        public GetBmpFromRhinoView()
          : base("GetBmpFromRhinoView", "Nickname",
              "Description",
              BasicInfo.Category, "Viewport")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("RhinoView", "RV", "The RhinoView to get the Bitmap.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "B", "The Bitmap.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RhinoView r_view = null;
            if (!DA.GetData(0, ref r_view))
                return;

            Bitmap b = r_view.CaptureToBitmap();
            DA.SetData(0, b);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.GetBmpFromRhinoView;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("277A163B-2F24-46D2-9F25-3E848F0B09E8"); }
        }
    }
}