using Grasshopper.Kernel;
using MarkerBasedAR.Properties;
using System;
using System.Drawing;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class WebcamCalibration : GH_Component
    {
        public Bitmap image;
        public Bitmap buffer;
        public CalibrationForm iVideoCaptureForm;
        public bool running;
        
        
        
        
        
        
        
        public WebcamCalibration()
          : base("WebcamCalibration", "WebcamCalibration",
            "Use this component to calibrate the camera parameters(Double Click to show the window.)",
            BasicInfo.Category, "Camera")
        {
            image = null;
            running = true;
        }

        
        
        
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        
        
        
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("OutputBitmap", "B", "Output bitmap data", 0);
        }

        
        
        
        
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (iVideoCaptureForm != null && iVideoCaptureForm.currentFrame != null)
            {
                buffer?.Dispose();
                buffer = iVideoCaptureForm.currentFrame;
                try
                {
                    int flags = this.buffer.Flags;
                    image?.Dispose();
                    image = (Bitmap)buffer.Clone();
                }
                catch (Exception) { }
            }
            if (image != null)
                DA.SetData(0, image);
            ExpireSolution(running);
        }

        
        
        
        
        
        
        protected override Bitmap Icon 
        {
            get
            {
                return Resources.WebcamCalibration;
            }
        }

        
        
        
        
        
        public override Guid ComponentGuid => new Guid("b3a81cc1-f1e0-413b-8222-74738b6c23b5");


        //Not default Method below:
        public override GH_Exposure Exposure => GH_Exposure.primary;


        public void ShowWebcamForm()
        {
            iVideoCaptureForm = new CalibrationForm();
            iVideoCaptureForm.Show();
        }
        public override void CreateAttributes() => m_attributes = new WebcamCalibrationAttributes(this);
    }
}