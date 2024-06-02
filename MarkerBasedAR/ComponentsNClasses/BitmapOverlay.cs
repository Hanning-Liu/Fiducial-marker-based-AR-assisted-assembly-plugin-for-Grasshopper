using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Rhino.Geometry;
using MarkerBasedAR.Properties;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class BitmapOverlay : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BitmapOverlay class.
        /// </summary>
        public BitmapOverlay()
          : base("BitmapOverlay", "BmpOverlay",
              "Overlay two bitmaps base on the weight",
              BasicInfo.Category, "Bitmap")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Image_1", "Image_1", "The first image to overlay.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight_1", "Weight_1", "The weight of the first image.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Image_2", "Image_2", "The second image to overlay.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight_2", "Weight_2", "The weight of the second image.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Output", "O", "The overlaid image.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Bitmap image_1 = null;
            Bitmap image_2 = null;
            GH_Number weight_1 = null;
            GH_Number weight_2 = null;
            if (!DA.GetData(0,ref image_1) || !DA.GetData(1, ref weight_1) || !DA.GetData(2,ref image_2) || !DA.GetData(3,ref weight_2))
                return;
            if(image_1.Size != image_2.Size)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The size of the two input images should be same.");
            }
            Mat mat_1 = image_1.ToMat();
            Mat mat_2 = image_2.ToMat();
            Mat output = new Mat();
            if (mat_1.Type() != mat_2.Type())
            {
                //AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Types are different.");

                // Convert mat1 to the desired data type
                mat_1 = mat_1.CvtColor(ColorConversionCodes.BGR2BGRA);

                // Alternatively, convert mat2 to the desired data type
                // mat2 = mat2.CvtColor(desiredMatType);

                GH_Convert.ToDouble(weight_1, out double w_1, GH_Conversion.Primary);
                GH_Convert.ToDouble(weight_2, out double w_2, GH_Conversion.Primary);
                Cv2.AddWeighted(mat_1, w_1, mat_2, w_2, 0, output);
                Bitmap bmp = output.ToBitmap();
                DA.SetData(0, bmp);
                mat_1.Dispose();
                mat_2.Dispose();
                output.Dispose();
            }
            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Resources.BitmapOverlay;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("35C07917-4CCD-478A-B19E-328309C27E78"); }
        }

    }
}