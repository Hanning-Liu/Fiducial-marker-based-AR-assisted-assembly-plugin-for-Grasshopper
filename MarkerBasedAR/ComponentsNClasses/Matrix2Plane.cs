using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using MarkerBasedAR.Properties;
using Rhino.Geometry;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class Matrix2Plane : GH_Component
    {
        
        
        
        public Matrix2Plane()
          : base("Matrix2Plane", "M2P",
              "Description",
              BasicInfo.Category, "Math")
        {
        }

        
        
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTransformParameter("Matrix", "M", "The 4*4 Matrix to convert.", GH_ParamAccess.item);
        }

        
        
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "The plane converted from the 4*4 Matrix.", GH_ParamAccess.item);
        }

        
        
        
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Transform m = Transform.Identity;
            if (!DA.GetData(0, ref m))
                return;
            Point3d p_origin = new Point3d(m[0, 3], m[1, 3], m[2, 3]);
            Vector3d v_x = new Vector3d(m[0, 0], m[1, 0], m[2, 0]);
            Vector3d v_y = new Vector3d(m[0, 1], m[1, 1], m[2, 1]);

            Plane p = new Plane(p_origin, v_x, v_y);
            DA.SetData(0, p);
        }

        
        
        
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.Matrix2Plane;
            }
        }

        
        
        
        public override Guid ComponentGuid
        {
            get { return new Guid("A2EEAC7B-D803-493D-8AB6-E7AA2B498419"); }
        }
    }
}