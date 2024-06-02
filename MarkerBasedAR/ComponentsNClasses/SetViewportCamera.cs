using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using MarkerBasedAR.Properties;
using Rhino.Display;
using Rhino.Geometry;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class SetViewportCamera : GH_Component
    {
        
        
        
        public SetViewportCamera()
          : base("SetViewportCamera", "Nickname",
              "Description",
              BasicInfo.Category, "Viewport")
        {
        }

        
        
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("MainViewport", "M", "The mainviewport to set the camera pose.", GH_ParamAccess.item);
            pManager.AddPointParameter("CameraLocation", "L", "The location point of the camera.", GH_ParamAccess.item);
            pManager.AddPointParameter("CameraTarget", "T", "The target point of the camera.", GH_ParamAccess.item);
            pManager.AddVectorParameter("CameraUP", "Up", "The up direction of the camera.", GH_ParamAccess.item);
            pManager.AddNumberParameter("FocalLength", "F", "The 35mm focal length of the camera.", GH_ParamAccess.item);
        }

        
        
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        
        
        
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RhinoViewport rhinoViewport = null;
            Point3d cameraLocation = Point3d.Unset;
            Point3d cameraTarget = Point3d.Unset;
            Vector3d cameraUp = Vector3d.Unset;
            double camera35mmLensLength = double.NaN;
            if (!DA.GetData(0, ref rhinoViewport) || !DA.GetData(1, ref cameraLocation) || !DA.GetData(2, ref cameraTarget) || !DA.GetData(3, ref cameraUp)||!DA.GetData(4, ref camera35mmLensLength))
                return;

            if (cameraTarget != null)
            {
                rhinoViewport.SetCameraLocations(cameraTarget, cameraLocation);
            }
            if (camera35mmLensLength != double.NaN)
            {
                rhinoViewport.Camera35mmLensLength = camera35mmLensLength;
            }
            rhinoViewport.CameraUp = cameraUp;
        }

        
        
        
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.SetViewportCamera;
            }
        }

        
        
        
        public override Guid ComponentGuid
        {
            get { return new Guid("FD2A14B0-2080-4819-A3D3-FB399C8D7FC6"); }
        }
    }
}