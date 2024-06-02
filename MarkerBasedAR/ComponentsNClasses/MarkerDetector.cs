using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

using Grasshopper.Kernel;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.Extensions;
using Rhino.Geometry;
using MarkerBasedAR.Properties;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class MarkerDetector : GH_Component
    {
        //Input
        private string path = null;
        private Bitmap bmap = null;
        private int dic = 1;
        private double size = 0.0f;
        //Output
        private List<int> markerIDs = new List<int>();
        private List<Rhino.Geometry.Plane> planes = new List<Rhino.Geometry.Plane>();
        private List<Transform> transforms = new List<Transform>();
        //Others
        double[,] mtx = new double[3, 3];
        double[] dist = new double[5];
        double[,] new_mtx = new double[3, 3];
        private Rect roi = new Rect();
        private Vec3d[] rvecs;
        private Vec3d[] tvecs;

        /// <summary>
        /// Initializes a new instance of the ArucoMarkerDetector class.
        /// </summary>
        public MarkerDetector()
          : base("MarkerDetector", "MD",
              "Use this component to detect the Marker",
              BasicInfo.Category, "Marker")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("CaliFileFolderPath", "P", "The folder path to the mtxNdist.txt file.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Image", "I", "The image to detect the Aruco Marker.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Dictionary", "D", "The Marker Dictionary to detect.(Default=Dict4X4_100)", GH_ParamAccess.item);
            pManager.AddNumberParameter("MarkerSize", "S", "The actual size of the marker(mm).", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("MarkerIndex", "ID", "The index of the detected marker.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "P", "The plane of the detected marker, if you take your camera as standard XY_Plane with Z_axis pointing up.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Image", "I", "The image with annotation of Markers", GH_ParamAccess.item);
            pManager.AddGenericParameter("Marker_Matrix", "M", "The matrix of the marker.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Get all the datas.
            if (!DA.GetData(0, ref path) || !DA.GetData(1, ref bmap) || !DA.GetData(2, ref dic) || !DA.GetData(3, ref size))
                return;

            //Get the calibration file.
            if (!File.Exists(Path.Combine(path, "mtxNdist.txt")))
                return;
            StreamReader sr = new StreamReader(Path.Combine(path, "mtxNdist.txt"));

            //Read the calibration datas.
            string[] tokens;
            for (int i = 0; i < 3; i++)
            {
                tokens = sr.ReadLine().Split(' ');
                for (int j = 0; j < 3; j++)
                {
                    mtx[i, j] = double.Parse(tokens[j]);
                }
            }
            tokens = sr.ReadLine().Split(' ');
            for (int i = 0; i < 5; i++)
                dist[i] = double.Parse(tokens[i]);
            for (int i = 0; i < 3; i++)
            {
                tokens = sr.ReadLine().Split(' ');
                for (int j = 0; j < 3; j++)
                    new_mtx[i, j] = double.Parse(tokens[j]);
            }
            tokens = sr.ReadLine().Split(' ');
            roi = new Rect(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3]));
            sr.Close();

            //Prepare the parameters used for pose estimate.
            //Argument num1
            Mat bmp_Mat = BitmapConverter.ToMat(bmap);
            //Cv2.CvtColor(bmp_Mat, bmp_Mat, ColorConversionCodes.BGR2GRAY);

            //Argument num2
            PredefinedDictionaryName name = (PredefinedDictionaryName)dic;
            var dictionary = CvAruco.GetPredefinedDictionary(name);

            //Argument num5
            var detectorParameters = new DetectorParameters();
            detectorParameters.CornerRefinementMethod = CornerRefineMethod.Subpix;
            detectorParameters.CornerRefinementWinSize = 9;

            CvAruco.DetectMarkers(bmp_Mat, dictionary, out var corners, out var ids, detectorParameters, out var rejectedImgPoints);

            var detectedMarkers = bmp_Mat.Clone();
            CvAruco.DrawDetectedMarkers(detectedMarkers, corners, ids, Scalar.Crimson);

            //Argument num2
            float f_size = (float)size;

            //Argument num3
            InputArray ia_mtx = InputArray.Create(mtx);

            //Argument num4
            InputArray ia_dist = InputArray.Create(dist);

            //Argument num5
            //OutputArray oa_rvecs = OutputArray.Create(rvecs);
            Mat rvecMat = new Mat();

            //Argument num6
            Mat tvecMat = new Mat();
            
            CvAruco.EstimatePoseSingleMarkers(corners, f_size, ia_mtx, ia_dist, rvecMat, tvecMat);


            Vec3d[] test_r = null;
            Vec3d[] test_t = null;
            rvecMat.GetArray(out test_r);
            tvecMat.GetArray(out test_t);
            float halfMarkerSize = f_size / 2.0f;

            Vec3d[] test_FinalR = new Vec3d[ids.Length];
            Vec3d[] test_FinalT = new Vec3d[ids.Length];

            for (int i = 0; i < ids.Length; i++)
            {
                double[,] rr_matrix = new double[3, 3];
                double[,] ttemp_matrix = new double[3, 3];

                Vec3d r = test_r[i];
                double[] r_array = new double[3];
                r_array[0] = r.Item0;
                r_array[1] = r.Item1;
                r_array[2] = r.Item2;

                Cv2.Rodrigues(r_array, out rr_matrix, out ttemp_matrix);

                //Data of rotation matrix
                double data_00 = rr_matrix[0, 0];
                double data_10 = rr_matrix[1, 0];
                double data_20 = rr_matrix[2, 0];

                double data_01 = rr_matrix[0, 1];
                double data_11 = rr_matrix[1, 1];
                double data_21 = rr_matrix[2, 1];

                double data_02 = rr_matrix[0, 2];
                double data_12 = rr_matrix[1, 2];
                double data_22 = rr_matrix[2, 2];
                //Data of translation
                double data_03 = test_t[i].Item0;
                double data_13 = test_t[i].Item1;
                double data_23 = test_t[i].Item2;

                Matrix4x4 matrix_MarkerCam = new Matrix4x4();
                matrix_MarkerCam.M11 = (float)data_00;
                matrix_MarkerCam.M21 = (float)data_10;
                matrix_MarkerCam.M31 = (float)data_20;
                matrix_MarkerCam.M41 = (float)0;

                matrix_MarkerCam.M12 = (float)data_01;
                matrix_MarkerCam.M22 = (float)data_11;
                matrix_MarkerCam.M32 = (float)data_21;
                matrix_MarkerCam.M42 = (float)0;

                matrix_MarkerCam.M13 = (float)data_02;
                matrix_MarkerCam.M23 = (float)data_12;
                matrix_MarkerCam.M33 = (float)data_22;
                matrix_MarkerCam.M43 = (float)0;

                matrix_MarkerCam.M14 = (float)data_03;
                matrix_MarkerCam.M24 = (float)data_13;
                matrix_MarkerCam.M34 = (float)data_23;
                matrix_MarkerCam.M44 = (float)1;

                Matrix4x4 matrix_CornerMarker = new Matrix4x4();
                matrix_CornerMarker.M11 = 1;
                matrix_CornerMarker.M21 = 0;
                matrix_CornerMarker.M31 = 0;
                matrix_CornerMarker.M41 = 0;

                matrix_CornerMarker.M12 = 0;
                matrix_CornerMarker.M22 = 1;
                matrix_CornerMarker.M32 = 0;
                matrix_CornerMarker.M42 = 0;

                matrix_CornerMarker.M13 = 0;
                matrix_CornerMarker.M23 = 0;
                matrix_CornerMarker.M33 = 1;
                matrix_CornerMarker.M43 = 0;

                matrix_CornerMarker.M14 = -halfMarkerSize;
                matrix_CornerMarker.M24 = -halfMarkerSize;
                matrix_CornerMarker.M34 = 0;
                matrix_CornerMarker.M44 = 1;

                Matrix4x4 matrix_CornerMarker_AprilTags = new Matrix4x4();
                matrix_CornerMarker_AprilTags.M11 = -1;
                matrix_CornerMarker_AprilTags.M21 = 0;
                matrix_CornerMarker_AprilTags.M31 = 0;
                matrix_CornerMarker_AprilTags.M41 = 0;

                matrix_CornerMarker_AprilTags.M12 = 0;
                matrix_CornerMarker_AprilTags.M22 = -1;
                matrix_CornerMarker_AprilTags.M32 = 0;
                matrix_CornerMarker_AprilTags.M42 = 0;

                matrix_CornerMarker_AprilTags.M13 = 0;
                matrix_CornerMarker_AprilTags.M23 = 0;
                matrix_CornerMarker_AprilTags.M33 = 1;
                matrix_CornerMarker_AprilTags.M43 = 0;

                matrix_CornerMarker_AprilTags.M14 = halfMarkerSize;
                matrix_CornerMarker_AprilTags.M24 = halfMarkerSize;
                matrix_CornerMarker_AprilTags.M34 = 0;
                matrix_CornerMarker_AprilTags.M44 = 1;

                Matrix4x4 matrix_CornerCam;
                if (dic >= 17 && dic <= 20)
                {
                    matrix_CornerCam = Matrix4x4.Multiply(matrix_MarkerCam, matrix_CornerMarker_AprilTags);
                }
                else
                {
                    matrix_CornerCam = Matrix4x4.Multiply(matrix_MarkerCam, matrix_CornerMarker);
                }
                Mat rr_Mat = new Mat(3, 3, MatType.CV_64F, 
                    new double[] { 
                    matrix_CornerCam.M11, matrix_CornerCam.M12, matrix_CornerCam .M13,
                    matrix_CornerCam.M21, matrix_CornerCam.M22, matrix_CornerCam.M23,
                    matrix_CornerCam.M31, matrix_CornerCam.M32, matrix_CornerCam.M33
                });

                // Convert the rotation matrix to a rotation vector
                Mat rrr_VecMat = new Mat();
                Mat tttemp_matrix = new Mat();
                Cv2.Rodrigues(rr_Mat, rrr_VecMat, tttemp_matrix);

                // Convert "Mat  3*1*CV_64FC1" to "Mat 1*1*CV_64FC3"
                Mat rrrr_VecMat = new Mat(1, 1, MatType.CV_64FC3);
                rrrr_VecMat = rrr_VecMat.Reshape(3, 1);

                Mat ttt_VecMat = new Mat(1,1, MatType.CV_64FC3, 
                    new double[] {
                    matrix_CornerCam.M14,
                    matrix_CornerCam.M24,
                    matrix_CornerCam.M34
                });

                Vec3d[] test_rr = null;
                Vec3d[] test_tt = null;
                
                rrrr_VecMat.GetArray(out test_rr);
                ttt_VecMat.GetArray(out test_tt);

                test_FinalR[i] = test_rr[0];
                test_FinalT[i] = test_tt[0];
            }

            //Draw axes on the detected markerss
            if (rvecMat.Empty() == false)
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    InputArray ia_rvec = InputArray.Create(test_FinalR[i]);
                    InputArray ia_tvec = InputArray.Create(test_FinalT[i]);
                    Cv2.DrawFrameAxes(detectedMarkers, ia_mtx, ia_dist, ia_rvec, ia_tvec, 40, 3);
                }
            }

            if (ids != null)
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    markerIDs.Add(ids[i]);
                }
                DA.SetDataList(0, markerIDs);
                markerIDs.Clear();
            }

            if (test_FinalR != null && test_FinalT != null)
            {
                for (int i = 0; i < test_FinalR.Length; i++)
                {
                    Vec3d r = test_FinalR[i];
                    Vec3d t = test_FinalT[i];

                    Rhino.Geometry.Point3d p_Origin = new Rhino.Geometry.Point3d(t.Item0, t.Item1, t.Item2);
                    double[] r_array = new double[3];
                    r_array[0] = r.Item0;
                    r_array[1] = r.Item1;
                    r_array[2] = r.Item2;
                    double[,] r_matrix = new double[3, 3];
                    double[,] temp_matrix = new double[3, 3];
                    Cv2.Rodrigues(r_array, out r_matrix, out temp_matrix);
                    Vector3d v_X = new Vector3d(r_matrix[0, 0], r_matrix[1, 0], r_matrix[2, 0]);
                    Vector3d v_Y = new Vector3d(r_matrix[0, 1], r_matrix[1, 1], r_matrix[2, 1]);
                    Vector3d v_Z = new Vector3d(r_matrix[0, 2], r_matrix[1, 2], r_matrix[2, 2]);

                    Rhino.Geometry.Plane p = new Rhino.Geometry.Plane(p_Origin, v_X, v_Y);
                    planes.Add(p);

                    Transform transform = new Transform();
                    transform.M00 = r_matrix[0, 0];
                    transform.M10 = r_matrix[1, 0];
                    transform.M20 = r_matrix[2, 0];
                    transform.M30 = 0;

                    transform.M01 = r_matrix[0, 1];
                    transform.M11 = r_matrix[1, 1];
                    transform.M21 = r_matrix[2, 1];
                    transform.M31 = 0;

                    transform.M02 = r_matrix[0, 2];
                    transform.M12 = r_matrix[1, 2];
                    transform.M22 = r_matrix[2, 2];
                    transform.M32 = 0;

                    transform.M03 = t.Item0;
                    transform.M13 = t.Item1;
                    transform.M23 = t.Item2;
                    transform.M33 = 1;

                    transforms.Add(transform);
                }

                DA.SetDataList(1, planes);
                DA.SetDataList(3, transforms);
                planes.Clear();
                transforms.Clear();
            }

            Bitmap b = detectedMarkers.ToBitmap();
            if (detectedMarkers != null)
                DA.SetData(2, b);
            bmp_Mat.Dispose();
            detectedMarkers.Dispose();
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
                return Resources.ArucoMarkerDetector;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6588C096-E98B-4AAA-A39D-24C233325058"); }
        }
        
    }
}