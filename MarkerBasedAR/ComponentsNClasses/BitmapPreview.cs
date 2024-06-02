using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using MarkerBasedAR.Properties;
using Rhino.Geometry;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class BitmapPreview : GH_Component
    {
        public Bitmap bmp;
        public Image preview;
        string message;

        public BitmapPreview()
          : base("BitmapPreview", "BMP_Preview",
              "Preview a bitmap image in canvas",
              BasicInfo.Category, "Bitmap")
        {
            preview = Resource_BitmapPreview.BitmapPreview;
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "B", "Bitmap image to preview", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if(!DA.GetData(0, ref bmp)) 
                return;
            preview = bmp;
            message = "(" + bmp.Width.ToString() + "x" + bmp.Height.ToString() + ") " + bmp.PixelFormat.ToString();
            UpdateMessage();
        }

        private void UpdateMessage() => Message = message;

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.BitmapPreview_small;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("669E59F3-62C4-43FD-A081-BABEC9BD1A82"); }
        }
        public override void CreateAttributes()
        {
            m_attributes = new BitmapPreviewAttributes(this);
        }
    }
}