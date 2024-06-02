using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using OpenCvSharp;
using OpenCvSharp.Extensions;
using MarkerBasedAR.Properties;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class MatMovePixels : GH_Component
    {
        
        
        
        public MatMovePixels()
          : base("MatMovePixels", "Nickname",
              "Description",
              BasicInfo.Category, "Bitmap")
        {
        }

        
        
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "B", "The Bitmap to move.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Vertical", "V", "The pixels to move in vertical direction.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Horizontal", "H", "The pixels to move in horizontal direction.", GH_ParamAccess.item);
        }

        
        
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap Output", "O", "The Bitmap after movement.", GH_ParamAccess.item);
        }

        
        
        
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Bitmap bmp = null;
            int verticalShift = 0;
            int horizontalShift = 0;
            if (!DA.GetData(0, ref bmp) || !DA.GetData(1, ref verticalShift) || !DA.GetData(2, ref horizontalShift))
                return;
            Mat originalImage = bmp.ToMat();

            Mat blackBackground = Mat.Zeros((originalImage.Size().Height + 2 * Math.Abs(verticalShift)), (originalImage.Size().Width + 2 * Math.Abs(horizontalShift)), originalImage.Type());

            // Calculate the position to place the smaller image at the center of the larger image
            int x = (blackBackground.Cols - originalImage.Cols) / 2;
            int y = (blackBackground.Rows - originalImage.Rows) / 2;

            // Create a region of interest (ROI) in the larger image
            Rect roi = new Rect(new OpenCvSharp.Point(x, y), new OpenCvSharp.Size(originalImage.Cols, originalImage.Rows));

            // Copy the smaller image to the center of the larger image
            originalImage.CopyTo(blackBackground[roi]);

            // Define the region of interest (ROI) within the original image
            Rect roi_crop = new Rect(
                new OpenCvSharp.Point(Math.Abs(horizontalShift) - horizontalShift, Math.Abs(verticalShift) - verticalShift),
                new OpenCvSharp.Size(originalImage.Cols, originalImage.Rows)
            );

            // Crop the original image to the specified ROI
            Mat croppedImage = blackBackground[roi_crop];
            Bitmap croppedbmp = croppedImage.ToBitmap();
            // Create a black background with the same size as the original image
            DA.SetData(0, croppedbmp);

            // Copy the cropped image to the black background, creating the shifted image


        }

        
        
        
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.MatMovePixels;
            }
        }

        
        
        
        public override Guid ComponentGuid
        {
            get { return new Guid("980733C2-38A3-4323-ADC1-C8726F551516"); }
        }
    }
}