using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MarkerBasedAR.Properties;
using Rhino;
using Rhino.Geometry;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class Marker2DArrayToHatch : GH_Component
    {
        private static double UnitScalar => RhinoMath.UnitScale(UnitSystem.Millimeters, RhinoDoc.ActiveDoc.ModelUnitSystem);
        public string bitArray;
        public int sidePixels;
        public GH_Number sideLength;
        
        
        
        public Marker2DArrayToHatch()
          : base("Marker2DBitArrayTo2DHatch", "2DHatch",
              "Get the Marker 2DHatch from 2DBitArray.",
              BasicInfo.Category, "Marker")
        {
        }

        
        
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Marker2DArray", "2DArray", "The Marker2DArray.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("SidePixels", "P", "The side length of the Marker 2D Array(Unit in pixels).", GH_ParamAccess.item);
            pManager.AddNumberParameter("SideLength", "L", "The side length of the Marker 2D Array(Unit in Millimeters).", GH_ParamAccess.item);
        }

        
        
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("2D Hatches", "P", "The 2D point array of the marker.", GH_ParamAccess.list);
        }

        
        
        
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref bitArray) || !DA.GetData(1, ref sidePixels) || !DA.GetData(2, ref sideLength))
                return;
            double real_sideLength = sideLength.Value * UnitScalar;
            List<Hatch> hatches = new List<Hatch>();
            string stringWithoutNewlines = bitArray.Replace("\n", "");
            for (int i = 0; i < sidePixels; i++)
            {
                for (int j = 0; j < sidePixels; j++)
                {
                    if (stringWithoutNewlines[i * sidePixels + j] == '0')
                    {
                        double x = j * real_sideLength / sidePixels;
                        double y = (sidePixels - i - 1) * real_sideLength / sidePixels;
                        double z = 0;
                        Point3d point = new Point3d(x, y, z);

                        Plane temp_plane = Plane.WorldXY;
                        temp_plane.Translate(new Vector3d(point - Plane.WorldXY.Origin));
                        Rectangle3d rec = new Rectangle3d(temp_plane, real_sideLength / sidePixels, real_sideLength / sidePixels);
                        Hatch[] hatch = Hatch.Create(rec.ToPolyline().ToPolylineCurve(), 0, 0, 1, 0.001);
                        hatches.Add(hatch[0]);
                    }
                }
            }
            DA.SetDataList(0, hatches);
        }

        
        
        
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.Marker2DArrayToHatch;
            }
        }

        
        
        
        public override Guid ComponentGuid
        {
            get { return new Guid("B336822B-7815-425A-8151-B66D23AF6299"); }
        }
    }
}