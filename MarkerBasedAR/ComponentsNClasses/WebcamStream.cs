using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using MarkerBasedAR.Properties;
using Rhino.Geometry;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class WebcamStream : GH_Component
    {
        public bool running;
        public Bitmap image = null;
        public Bitmap buffer = null;
        public BitmapForm iBitmapForm;
        
        
        
        public WebcamStream()
          : base("WebcamStream", "WebcamStream",
              "Get camera frames.(Double Click to show the window.)",
              BasicInfo.Category, "Camera")
        {
            image = (Bitmap)null;
            running = true;
        }

        
        
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        
        
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("OutputBitmap", "B", "Output bitmap data", 0);
        }

        
        
        
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (iBitmapForm != null && iBitmapForm.currentFrame != null)
            {
                buffer?.Dispose();
                buffer = iBitmapForm.currentFrame;
                try
                {
                    int flags = this.buffer.Flags;
                    image?.Dispose();
                    image = (Bitmap)buffer.Clone();
                }
                catch  { }
            }
            if (image != null)
                DA.SetData(0, image);
            ExpireSolution(running);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        
        
        
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.WebcamStream;
            }
        }

        
        
        
        public override Guid ComponentGuid
        {
            get { return new Guid("CD2E744D-01C0-4139-B095-33570A44D93E"); }
        }
        public void ShowWebcamForm()
        {
            iBitmapForm = new BitmapForm();
            iBitmapForm.Show();
        }
        public override void CreateAttributes() => m_attributes = new WebcamStreamAttributes(this);
    }
}