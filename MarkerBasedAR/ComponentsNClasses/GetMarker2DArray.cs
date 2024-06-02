using System;
using System.Collections;
using System.Collections.Generic;

using Grasshopper.Kernel;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using Rhino.Geometry;
using MarkerBasedAR.Properties;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class GetMarker2DArray : GH_Component
    {
        int dic;
        int ID;
        
        public GetMarker2DArray()
          : base("GetMarker2DBitArray", "M_2D",
              "GetMarker2DBitArray by inputting the DictionaryName and the ID(Default Dictionary is Dict4X4_50)",
              BasicInfo.Category, "Marker")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("DictionaryName", "Dic", "The DictionaryName provided by OpenCvSharp.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MarkerID", "ID", "The Marker ID.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Marker2DArray", "M_2D", "The 2D Array of the Marker.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("SidePixels", "P", "The side length of the Marker 2D Array.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref dic) || !DA.GetData(1, ref ID))
                return;
            PredefinedDictionaryName name = (PredefinedDictionaryName)dic;
            Dictionary dictionary = CvAruco.GetPredefinedDictionary(name);
            Mat output = new Mat();
            int sidePixels = 0;
            if (dic >= 0 && dic <= 20)
            {
                switch (dic)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        sidePixels = 6;
                        break;

                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        sidePixels = 7;
                        break;

                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        sidePixels = 8;
                        break;

                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        sidePixels = 9;
                        break;

                    case 16:
                        sidePixels = 7;
                        break;

                    case 17:
                        sidePixels = 6;
                        break;

                    case 18:
                        sidePixels = 7;
                        break;

                    case 19:
                    case 20:
                        sidePixels = 8;
                        break;
                }
            }
            dictionary.GenerateImageMarker(ID, sidePixels, output, 1);

            output.GetArray(out byte[] array);
            byte[,] array_2d = ConvertTo2DArray(array, sidePixels, sidePixels);
            
            if (dic >= 17 && dic <= 20)
            {
                array_2d = Rotate180(array_2d);
            }

            List<string> list = new List<string>();
            for (int i = 0; i < sidePixels; i++)
            {
                for (int j = 0; j < sidePixels; j++)
                {
                    byte b = array_2d[i, j];
                    if (b == 255)
                        b = 1;
                    list.Add(b.ToString());
                }
                list.Add("\n");
            }
            string result = string.Join("", list);
            DA.SetData(0, result);
            DA.SetData(1, sidePixels);
        }
        public static byte[,] ConvertTo2DArray(byte[] source, int rows, int columns)
        {
            if (source.Length != rows * columns)
            {
                throw new ArgumentException("The length of the source array must match the size of the 2D array.");
            }

            byte[,] result = new byte[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    result[i, j] = source[i * columns + j];
                }
            }

            return result;
        }
        public static byte[,] Rotate180(byte[,] original)
        {
            int rows = original.GetLength(0);
            int cols = original.GetLength(1);

            // Create a new array for the rotated elements
            byte[,] rotated = new byte[rows, cols];

            // Populate the rotated array with elements in reverse order
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    rotated[i, j] = original[rows - 1 - i, cols - 1 - j];
                }
            }

            return rotated;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.GetMarker2DArray;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("97338650-B80C-441B-AA50-2BA9CC047BC8"); }
        }
    }
}