using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Camera_NET;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace MarkerBasedAR.ComponentsNClasses
{
    enum CMode
    {
        DISCONNECTED = -1,
        READY = 0,
        SAVE = 1,
        CALI = 2,
    }
    public partial class CalibrationForm : Form
    {
        // checker pattern
        int NX = 9, NY = 6;
        OpenCvSharp.Size pattern_size = new OpenCvSharp.Size(9, 6);
        float square_size;

        private CameraChoice _CameraChoice = new CameraChoice();
        private CMode mode = CMode.DISCONNECTED;

        Thread playThread;
        private volatile bool shouldRunThread = true;
        Mat imgRead = new Mat();

        bool save = false;
        int numOfSavedImages = 0; // the number of save images

        Thread calibThread;
        private bool isCalibrated = false;

        // camera parameters
        double[,] mtx = new double[3, 3];
        double[] dist = new double[5];
        double[,] new_mtx = new double[3, 3];
        private Mat m_mtx = new Mat(1, 5, MatType.CV_64FC1);
        private Mat m_dist = new Mat(1, 5, MatType.CV_64FC1);
        private Mat m_new_mtx = Mat.Eye(3, 3, MatType.CV_64FC1);
        private Rect roi = new Rect();
        private Mat mapx = new Mat();
        private Mat mapy = new Mat();
        private Vec3d[] rvecs;
        private Vec3d[] tvecs;

        OpenCvSharp.Size img_size;
        private string mtx_fname = "mtxNdist.txt";

        private VideoCapture vc = null;


        public Bitmap currentFrame;
        public bool iftopMost;

        private System.ComponentModel.IContainer components = null;

        #region 
        private GroupBox groupBox_VideoCaptureSettings;
        private ComboBox comboBoxVideoResolution;
        private Label label_Resolution;
        private ComboBox comboBoxVideoSource;
        private Label label_Source;
        private Button buttonStart;
        private Button buttonPause;
        private Button buttonCancel;
        private GroupBox groupBox_SPC;
        private System.Windows.Forms.Timer timer1;
        private Label fpsLabel;
        private CheckBox checkBox_AlwaysOnTop;
        private GroupBox groupBox_CheckerBoard;
        private GroupBox groupBox_ImageSaving;
        private NumericUpDown numericUpDown_NX;
        private TextBox textBox_SquareSize;
        private Label label_SquareSize;
        private Label label_NY;
        private Label label_NX;
        private NumericUpDown numericUpDown_NY;
        private GroupBox groupBox_Calibration;
        private StatusStrip status;
        private Button button_ImageSavingSave;
        private Button button_ImageSavingPath;
        private TextBox textBox_ImageSavingPath;
        private Label label__ImageSavingPath;
        private Label label_EnterPath;
        private Button button_MakeAndSave;
        private Button button_CaliPath;
        private TextBox textBox_Calipath;
        private FolderBrowserDialog fbd;
        private ToolStripStatusLabel lbl_status;
        private Button button_ImgSave;
        private NotifyIcon notifyIcon1;
        private PictureBox pictureBoxCV;
        #endregion

        public CalibrationForm()
        {
            InitializeComponent();
            iftopMost = true;
            lbl_status.ForeColor = Color.Red;
            lbl_status.Text = "Ready";
        }
        private void VideoCaptureForm_Load(object sender, EventArgs e)
        {
            // Fill camera list combobox with available cameras
            FillCameraList();

            // Select the first one
            if (comboBoxVideoSource.Items.Count > 0)
                comboBoxVideoSource.SelectedIndex = 0;

            // Fill camera list combobox with available resolutions
            FillResolutionList();
        }
        private void FillCameraList()
        {
            comboBoxVideoSource.Items.Clear();

            _CameraChoice.UpdateDeviceList();

            foreach (var camera_device in _CameraChoice.Devices)
                comboBoxVideoSource.Items.Add(camera_device.Name);
        }
        private void FillResolutionList()
        {
            comboBoxVideoResolution.Items.Clear();
            ResolutionList resolutions = Camera.GetResolutionList(_CameraChoice.Devices[comboBoxVideoSource.SelectedIndex].Mon);

            if (resolutions == null)
                return;

            for (int index = 0; index < resolutions.Count; index++)
            {
                comboBoxVideoResolution.Items.Add(resolutions[index].ToString());
            }

            // select current resolution
            comboBoxVideoResolution.SelectedIndex = 0;
        }
        private void comboBoxVideoSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxVideoSource.Text != "OBS Virtual Camera")
            {
                FillResolutionList();
            }
            else
            {
                comboBoxVideoResolution.Items.Clear();
                comboBoxVideoResolution.Text = "";
            }
        }
        private void comboBoxVideoResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            //cameraControl.CloseCamera();
        }
        private void buttonStart_Click(object sender, EventArgs e)
        {
            // Set the camera based on the selected source and resolution
            var selectedDevice = _CameraChoice.Devices[comboBoxVideoSource.SelectedIndex];
            if (selectedDevice.Name != "OBS Virtual Camera")
            {
                var selectedResolution = Camera.GetResolutionList(_CameraChoice.Devices[comboBoxVideoSource.SelectedIndex].Mon)[comboBoxVideoResolution.SelectedIndex];
                vc = new VideoCapture(comboBoxVideoSource.SelectedIndex, VideoCaptureAPIs.ANY);
                vc.Set(VideoCaptureProperties.FrameHeight, selectedResolution.Height);
                vc.Set(VideoCaptureProperties.FrameWidth, selectedResolution.Width);
            }
            else
            {
                vc = new VideoCapture(comboBoxVideoSource.SelectedIndex, VideoCaptureAPIs.ANY);
                vc.Set(VideoCaptureProperties.FrameHeight, 768);
                vc.Set(VideoCaptureProperties.FrameWidth, 1024);
            }
            mode = CMode.READY;
            playThread = new Thread(DetectCheckerBoard);
            playThread.IsBackground = true;
            playThread.Start();
            buttonStart.Enabled = false;
            lbl_status.Text = "Camera is activated";
        }
        private void DetectCheckerBoard()
        {
            Mat img;
            while (true)
            {
                vc.Read(imgRead);
                img = imgRead;
                if (mode == CMode.READY || mode == CMode.SAVE)
                {
                    currentFrame = img.ToBitmap();
                    Point2f[] corners;
                    if (Cv2.FindChessboardCorners(img, pattern_size, out corners))
                    {
                        if (mode == CMode.SAVE && save)
                        {
                            saveImage(img);
                            save = false;
                        }
                        Cv2.DrawChessboardCorners(img, pattern_size, corners, true);
                    }

                    int pictureBoxWidth = pictureBoxCV.Width;
                    int pictureBoxHeight = pictureBoxCV.Height;
                    double widthRatio = (double)pictureBoxWidth / imgRead.Width;
                    double heightRatio = (double)pictureBoxHeight / imgRead.Height;
                    double ratio = Math.Min(widthRatio, heightRatio);
                    int newWidth = (int)(imgRead.Width * ratio);
                    int newHeight = (int)(imgRead.Height * ratio);

                    Mat resizedImg = new Mat();
                    Cv2.Resize(img, resizedImg, new OpenCvSharp.Size(newWidth, newHeight));

                    int offsetX = (pictureBoxWidth - newWidth) / 2;
                    int offsetY = (pictureBoxHeight - newHeight) / 2;

                    Mat finalImg = new Mat(new OpenCvSharp.Size(pictureBoxWidth, pictureBoxHeight), MatType.CV_8UC3, Scalar.Black);
                    resizedImg.CopyTo(finalImg[new Rect(offsetX, offsetY, newWidth, newHeight)]);
                    pictureBoxCV.Image = finalImg.ToBitmap();
                }
                Task.Delay(33).Wait();
            }
        }
        private void buttonPause_Click(object sender, EventArgs e)
        {
            shouldRunThread = false; // Signal the thread to stop

            if (playThread != null && playThread.IsAlive) // Check if the thread is running
            {
                playThread.Abort(); // Wait for the thread to complete
            }
            buttonStart.Enabled = true;
            if (vc != null)
            {
                vc.Release();
            }

            pictureBoxCV.Image = null;
            if (currentFrame != null)
            {
                currentFrame.Dispose();
            }

            mode = CMode.DISCONNECTED;
            fpsLabel.Text = "";
            lbl_status.Text = "Camera is paused";
        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            shouldRunThread = false; // Signal the thread to stop

            if (playThread != null && playThread.IsAlive) // Check if the thread is running
            {
                playThread.Abort(); // Wait for the thread to complete
            }
            buttonStart.Enabled = true;
            if (vc != null)
            {
                vc.Release();
            }
            pictureBoxCV.Image = null;
            if (currentFrame != null)
            {
                currentFrame.Dispose();
            }
            mode = CMode.DISCONNECTED;
            fpsLabel.Text = "";
            lbl_status.Text = "Camera is canceled";
            Dispose(true);
            Close();
        }
        private void numericUpDown_NX_ValueChanged(object sender, EventArgs e)
        {
            NX = (int)numericUpDown_NX.Value;
            pattern_size.Width = NX;
        }
        private void numericUpDown_NY_ValueChanged(object sender, EventArgs e)
        {
            NY = (int)numericUpDown_NY.Value;
            pattern_size.Height = NY;
        }
        private void textBox_SquareSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allowing only numbers, decimal point, and control keys (backspace, delete)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true; // Handle the keypress event to prevent non-numeric input
            }

            // Allow only one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true; // Handle the keypress event to prevent multiple decimal points
            }
        }
        private void textBox_SquareSize_TextChanged(object sender, EventArgs e)
        {
            try
            {
                square_size = Convert.ToSingle(textBox_SquareSize.Text);
            }
            catch (FormatException)
            {
                // Handle the case where the input cannot be converted to a double
                // For instance, display an error message or perform any other necessary actions.
            }
        }
        private void VideoCaptureForm_FormClosing(object sender, FormClosingEventArgs e) => exit();
        public void exit()
        {
            shouldRunThread = false; // Signal the thread to stop

            if (playThread != null && playThread.IsAlive) // Check if the thread is running
            {
                playThread.Abort(); // Wait for the thread to complete
            }
            buttonStart.Enabled = true;
            if (!(vc == null))
                vc.Release();
            pictureBoxCV.Image = null;
            Dispose(true);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }
        private void button_ImageSavingPath_Click(object sender, EventArgs e)
        {
            fbd.SelectedPath = Application.StartupPath;
            fbd.ShowDialog();
            textBox_ImageSavingPath.Text = fbd.SelectedPath;
        }
        private void button_ImageSavingSave_Click(object sender, EventArgs e)
        {
            if (mode == CMode.DISCONNECTED)
            {
                lbl_status.Text = "Please start the camera first";
                return;
            }
            if (!Directory.Exists(textBox_ImageSavingPath.Text))
            {
                lbl_status.Text = "Image path is not valid";
                return;
            }
            if (mode == CMode.READY)
            {
                mode = CMode.SAVE;
                button_ImageSavingSave.Text = "stop saving images";
                lbl_status.Text = "Image saving, click 'SaveButton' to save";
                groupBox_Calibration.Enabled = false;
                button_ImgSave.Enabled = true;
            }
            else if (mode == CMode.SAVE)
            {
                mode = CMode.READY;
                button_ImageSavingSave.Text = "start to save images";
                lbl_status.Text = "Image saved";
                groupBox_Calibration.Enabled = true;
                button_ImgSave.Enabled = false;
            }
        }
        private void saveImage(Mat img)
        {
            Cv2.ImWrite(Path.Combine(textBox_ImageSavingPath.Text, string.Format("img_{0:D4}.bmp", ++numOfSavedImages)), img);
            Invoke(new DelSetStatus(setStatus), new object[] {
            string.Format("{0} images are saved",numOfSavedImages) });
        }
        public delegate void DelSetStatus(string s);
        public void setStatus(string s)
        {
            lbl_status.Text = s;
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            iftopMost = checkBox_AlwaysOnTop.Checked;
            TopMost = iftopMost;
        }
        private void VideoCaptureForm_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
                return;
            TopMost = false;
            Hide();
            notifyIcon1.ShowBalloonTip(2000, "MarkerBasedAR_Webcam", "MarkerBasedAR webcam is still running, click the icon in system tray to show webcam again.", ToolTipIcon.Info);
        }
        private void button_CaliPath_Click(object sender, EventArgs e)
        {
            fbd.SelectedPath = Application.StartupPath;
            fbd.ShowDialog();
            textBox_Calipath.Text = fbd.SelectedPath;
        }
        private void button_ImgSave_Click(object sender, EventArgs e)
        {
            save = true;
        }
        private void button_MakeAndSave_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox_ImageSavingPath.Text))
            {
                lbl_status.Text = "Image path is not valid!";
                return;
            }
            if (!Directory.Exists(textBox_Calipath.Text))
            {
                lbl_status.Text = "Matrix path is not valid!";
                return;
            }
            if (square_size == 0)
            {
                lbl_status.Text = "Please imput a number to \"square_size\" !";
                return;
            }
            calibThread = new Thread(new ThreadStart(CalcMatrix));
            calibThread.IsBackground = true;
            calibThread.Start();
        }
        private void CalcMatrix()
        {
            mode = CMode.CALI; // busy!
            TermCriteria criteria = new TermCriteria(CriteriaTypes.MaxIter | CriteriaTypes.Eps, 30, 0.001);

            List<Point3f[]> object_points = new List<Point3f[]>();
            List<Point2f[]> image_points = new List<Point2f[]>();

            Point3f[] obj_standard = new Point3f[NX * NY];
            for (int i = 0; i < NX * NY; i++)
                obj_standard[i] = new Point3f(i % NX * square_size, i / NX * square_size, 0.0f);


            string[] files = { };
            try
            {
                files = Directory.GetFiles(textBox_ImageSavingPath.Text, "*");
            }
            catch
            {
                Invoke(new DelSetStatus(setStatus), new object[] { "Fail to read images" });
                mode = CMode.READY;
                isCalibrated = false;
                return;
            }
            Mat image0_size = Cv2.ImRead(files[0]);
            img_size = new OpenCvSharp.Size(image0_size.Width, image0_size.Height);

            int total = files.Length;
            int progress = 0;
            foreach (string filename in files)
            {
                progress++;
                Mat img = Cv2.ImRead(filename);
                Cv2.CvtColor(img, img, ColorConversionCodes.BGR2GRAY);
                Point2f[] corners;
                bool ret = Cv2.FindChessboardCorners(img, pattern_size, out corners);
                if (ret)
                {
                    object_points.Add(obj_standard);
                    Point2f[] corners2 = Cv2.CornerSubPix(img, corners, new OpenCvSharp.Size(5, 5), new OpenCvSharp.Size(-1, -1), criteria);
                    image_points.Add(corners2);
                }

                Invoke(new DelSetStatus(setStatus), new object[] {
            string.Format("processing {0}% : pattern was found in {1}images", (int)(progress * 100.0/ total ), object_points.Count) });

            }
            Invoke(new DelSetStatus(setStatus), new object[] { "Please wait for calculating matrix. It may take more than 10 minutes." });

            try
            {
                double retval = Cv2.CalibrateCamera(object_points, image_points, img_size, mtx, dist, out rvecs, out tvecs);
                new_mtx = Cv2.GetOptimalNewCameraMatrix(mtx, dist, img_size, 1.0, img_size, out roi);
            }
            catch
            {
                lbl_status.Text = "Failed to calculate matrix. Save more images and retry";
                isCalibrated = false;
                mode = CMode.READY;
                return;
            }

            Invoke(new DelSetStatus(setStatus), new object[] { "Generating matrix is finished. Now, please wait for writing data on file." });

            // write data on file
            StreamWriter sw = new StreamWriter(Path.Combine(textBox_Calipath.Text, mtx_fname));
            for (int i = 0; i < 3; i++) // mtx
            {
                for (int j = 0; j < 3; j++)
                    sw.Write(mtx[i, j] + " ");
                sw.WriteLine();
            }
            foreach (double val in dist) // dist
                sw.Write(val + " ");
            sw.WriteLine();

            for (int i = 0; i < 3; i++) // new mtx
            {
                for (int j = 0; j < 3; j++)
                    sw.Write(new_mtx[i, j] + " ");
                sw.WriteLine();
            }

            sw.Write("{0} {1} {2} {3}", roi.X, roi.Y, roi.Width, roi.Height); // roi
            sw.WriteLine();

            for (int i = 0; i < rvecs.Length; i++) //rvec
            {
                sw.Write(rvecs[i] + " ");
            }
            sw.WriteLine();

            for (int i = 0; i < tvecs.Length; i++) //tvec
            {
                sw.Write(tvecs[i] + " ");
            }
            sw.WriteLine();
            sw.Close();

            // calculating mapping matrix mapx, mapy
            Invoke(new DelSetStatus(setStatus), new object[] { "Please wait for making map-matrix" });
            DoubleArr2Mat();
            Cv2.InitUndistortRectifyMap(m_mtx, m_dist, new Mat(), m_new_mtx, img_size, MatType.CV_32FC1, mapx, mapy);

            isCalibrated = true;
            mode = CMode.READY;

            // Calculate and print mean reprojection error
            double totalError = 0;
            for (int i = 0; i < files.Length; i++)
            {

                InputArray o_points = InputArray.Create(object_points[i]);
                InputArray mtx_i = InputArray.Create(mtx);
                InputArray dist_i = InputArray.Create(dist);
                OutputArray reprojectedPointsArray = new Mat();
                Cv2.ProjectPoints(o_points, rvecs[i], tvecs[i], mtx_i, dist_i, reprojectedPointsArray);

                Point3f[] temp_p3f = object_points[i];
                Point2f[] temp_p2f = new Point2f[temp_p3f.Length];
                for (int j = 0; j < temp_p3f.Length; j++)
                {
                    temp_p2f[j] = new Point2f(temp_p3f[j].X, temp_p3f[j].Y);
                }

                InputArray o = InputArray.Create(temp_p2f);
                InputArray r = reprojectedPointsArray.GetMat();
                double error = Cv2.Norm(o, r, NormTypes.L2) / object_points[i].Length;
                totalError += error;
            }
            double meanError = totalError / files.Length;

            Invoke(new DelSetStatus(setStatus), new object[] { string.Format("Calibration is finished!! Mean Error is {0}", meanError) });
            return;
        }
        public void DoubleArr2Mat()
        {
            m_mtx = new Mat(3, 3, MatType.CV_64FC1, mtx);
            m_dist = new Mat(1, 5, MatType.CV_64FC1, dist);
            m_new_mtx = new Mat(3, 3, MatType.CV_64FC1, new_mtx);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mode == CMode.DISCONNECTED)
                return;

            //else
            //{
            //    const int framesToCapture = 10;
            //    stopWatch = new Stopwatch();
            //    stopWatch.Start();
            //    Mat frame = new Mat();
            //    int frameCount = 0;
            //    if (!(vc == null))
            //    {
            //        while (frameCount < framesToCapture)
            //        {
            //            if (!vc.Read(frame))
            //                break;
            //            frameCount++;
            //        }
            //    }
            //    stopWatch.Stop();
            //    double elapsedMs = stopWatch.ElapsedMilliseconds;
            //    double fps = frameCount / (elapsedMs / 1000.0);
            //    int intpart = (int)Math.Round(fps);
            //    fpsLabel.Text = intpart.ToString() + " fps";
            //}
        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            TopMost = iftopMost;
            WindowState = FormWindowState.Normal;
        }
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
            TopMost = iftopMost;
            WindowState = FormWindowState.Normal;
        }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalibrationFormResource));
            this.groupBox_VideoCaptureSettings = new System.Windows.Forms.GroupBox();
            this.label_Resolution = new System.Windows.Forms.Label();
            this.label_Source = new System.Windows.Forms.Label();
            this.comboBoxVideoSource = new System.Windows.Forms.ComboBox();
            this.comboBoxVideoResolution = new System.Windows.Forms.ComboBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBox_SPC = new System.Windows.Forms.GroupBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.fpsLabel = new System.Windows.Forms.Label();
            this.checkBox_AlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.groupBox_CheckerBoard = new System.Windows.Forms.GroupBox();
            this.textBox_SquareSize = new System.Windows.Forms.TextBox();
            this.label_SquareSize = new System.Windows.Forms.Label();
            this.label_NY = new System.Windows.Forms.Label();
            this.label_NX = new System.Windows.Forms.Label();
            this.numericUpDown_NY = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_NX = new System.Windows.Forms.NumericUpDown();
            this.groupBox_ImageSaving = new System.Windows.Forms.GroupBox();
            this.button_ImageSavingSave = new System.Windows.Forms.Button();
            this.button_ImgSave = new System.Windows.Forms.Button();
            this.button_ImageSavingPath = new System.Windows.Forms.Button();
            this.textBox_ImageSavingPath = new System.Windows.Forms.TextBox();
            this.label__ImageSavingPath = new System.Windows.Forms.Label();
            this.groupBox_Calibration = new System.Windows.Forms.GroupBox();
            this.button_MakeAndSave = new System.Windows.Forms.Button();
            this.label_EnterPath = new System.Windows.Forms.Label();
            this.button_CaliPath = new System.Windows.Forms.Button();
            this.textBox_Calipath = new System.Windows.Forms.TextBox();
            this.status = new System.Windows.Forms.StatusStrip();
            this.lbl_status = new System.Windows.Forms.ToolStripStatusLabel();
            this.pictureBoxCV = new System.Windows.Forms.PictureBox();
            this.fbd = new System.Windows.Forms.FolderBrowserDialog();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.groupBox_VideoCaptureSettings.SuspendLayout();
            this.groupBox_SPC.SuspendLayout();
            this.groupBox_CheckerBoard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_NY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_NX)).BeginInit();
            this.groupBox_ImageSaving.SuspendLayout();
            this.groupBox_Calibration.SuspendLayout();
            this.status.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCV)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox_VideoCaptureSettings
            // 
            this.groupBox_VideoCaptureSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_VideoCaptureSettings.Controls.Add(this.label_Resolution);
            this.groupBox_VideoCaptureSettings.Controls.Add(this.label_Source);
            this.groupBox_VideoCaptureSettings.Controls.Add(this.comboBoxVideoSource);
            this.groupBox_VideoCaptureSettings.Controls.Add(this.comboBoxVideoResolution);
            this.groupBox_VideoCaptureSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox_VideoCaptureSettings.Location = new System.Drawing.Point(454, 162);
            this.groupBox_VideoCaptureSettings.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox_VideoCaptureSettings.Name = "groupBox_VideoCaptureSettings";
            this.groupBox_VideoCaptureSettings.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox_VideoCaptureSettings.Size = new System.Drawing.Size(181, 132);
            this.groupBox_VideoCaptureSettings.TabIndex = 2;
            this.groupBox_VideoCaptureSettings.TabStop = false;
            this.groupBox_VideoCaptureSettings.Text = "VideoCapture Settings";
            // 
            // label_Resolution
            // 
            this.label_Resolution.AutoSize = true;
            this.label_Resolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Resolution.Location = new System.Drawing.Point(3, 74);
            this.label_Resolution.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Resolution.Name = "label_Resolution";
            this.label_Resolution.Size = new System.Drawing.Size(115, 17);
            this.label_Resolution.TabIndex = 2;
            this.label_Resolution.Text = "Video Resolution";
            // 
            // label_Source
            // 
            this.label_Source.AutoSize = true;
            this.label_Source.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Source.Location = new System.Drawing.Point(3, 24);
            this.label_Source.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label_Source.Name = "label_Source";
            this.label_Source.Size = new System.Drawing.Size(93, 17);
            this.label_Source.TabIndex = 0;
            this.label_Source.Text = "Video Source";
            // 
            // comboBoxVideoSource
            // 
            this.comboBoxVideoSource.FormattingEnabled = true;
            this.comboBoxVideoSource.Location = new System.Drawing.Point(6, 48);
            this.comboBoxVideoSource.Margin = new System.Windows.Forms.Padding(1);
            this.comboBoxVideoSource.Name = "comboBoxVideoSource";
            this.comboBoxVideoSource.Size = new System.Drawing.Size(160, 24);
            this.comboBoxVideoSource.TabIndex = 1;
            this.comboBoxVideoSource.SelectedIndexChanged += new System.EventHandler(this.comboBoxVideoSource_SelectedIndexChanged);
            // 
            // comboBoxVideoResolution
            // 
            this.comboBoxVideoResolution.FormattingEnabled = true;
            this.comboBoxVideoResolution.Location = new System.Drawing.Point(6, 98);
            this.comboBoxVideoResolution.Margin = new System.Windows.Forms.Padding(1);
            this.comboBoxVideoResolution.Name = "comboBoxVideoResolution";
            this.comboBoxVideoResolution.Size = new System.Drawing.Size(160, 24);
            this.comboBoxVideoResolution.TabIndex = 3;
            this.comboBoxVideoResolution.SelectedIndexChanged += new System.EventHandler(this.comboBoxVideoResolution_SelectedIndexChanged);
            // 
            // buttonStart
            // 
            this.buttonStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStart.Location = new System.Drawing.Point(5, 21);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(1);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(76, 27);
            this.buttonStart.TabIndex = 3;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonPause
            // 
            this.buttonPause.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPause.Location = new System.Drawing.Point(5, 58);
            this.buttonPause.Margin = new System.Windows.Forms.Padding(1);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(76, 27);
            this.buttonPause.TabIndex = 4;
            this.buttonPause.Text = "Pause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(5, 96);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(1);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(76, 27);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBox_SPC
            // 
            this.groupBox_SPC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_SPC.Controls.Add(this.buttonCancel);
            this.groupBox_SPC.Controls.Add(this.buttonPause);
            this.groupBox_SPC.Controls.Add(this.buttonStart);
            this.groupBox_SPC.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox_SPC.Location = new System.Drawing.Point(547, 354);
            this.groupBox_SPC.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox_SPC.Name = "groupBox_SPC";
            this.groupBox_SPC.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox_SPC.Size = new System.Drawing.Size(88, 132);
            this.groupBox_SPC.TabIndex = 6;
            this.groupBox_SPC.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // fpsLabel
            // 
            this.fpsLabel.AutoSize = true;
            this.fpsLabel.Location = new System.Drawing.Point(456, 464);
            this.fpsLabel.Name = "fpsLabel";
            this.fpsLabel.Size = new System.Drawing.Size(0, 15);
            this.fpsLabel.TabIndex = 8;
            // 
            // checkBox_AlwaysOnTop
            // 
            this.checkBox_AlwaysOnTop.AutoSize = true;
            this.checkBox_AlwaysOnTop.Checked = true;
            this.checkBox_AlwaysOnTop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_AlwaysOnTop.Location = new System.Drawing.Point(518, 322);
            this.checkBox_AlwaysOnTop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_AlwaysOnTop.Name = "checkBox_AlwaysOnTop";
            this.checkBox_AlwaysOnTop.Size = new System.Drawing.Size(117, 19);
            this.checkBox_AlwaysOnTop.TabIndex = 9;
            this.checkBox_AlwaysOnTop.Text = "AlwaysOnTop";
            this.checkBox_AlwaysOnTop.UseVisualStyleBackColor = true;
            this.checkBox_AlwaysOnTop.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // groupBox_CheckerBoard
            // 
            this.groupBox_CheckerBoard.Controls.Add(this.textBox_SquareSize);
            this.groupBox_CheckerBoard.Controls.Add(this.label_SquareSize);
            this.groupBox_CheckerBoard.Controls.Add(this.label_NY);
            this.groupBox_CheckerBoard.Controls.Add(this.label_NX);
            this.groupBox_CheckerBoard.Controls.Add(this.numericUpDown_NY);
            this.groupBox_CheckerBoard.Controls.Add(this.numericUpDown_NX);
            this.groupBox_CheckerBoard.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_CheckerBoard.Location = new System.Drawing.Point(10, 12);
            this.groupBox_CheckerBoard.Name = "groupBox_CheckerBoard";
            this.groupBox_CheckerBoard.Size = new System.Drawing.Size(216, 146);
            this.groupBox_CheckerBoard.TabIndex = 10;
            this.groupBox_CheckerBoard.TabStop = false;
            this.groupBox_CheckerBoard.Text = "Checker Board Settings";
            // 
            // textBox_SquareSize
            // 
            this.textBox_SquareSize.Location = new System.Drawing.Point(142, 108);
            this.textBox_SquareSize.Name = "textBox_SquareSize";
            this.textBox_SquareSize.Size = new System.Drawing.Size(67, 22);
            this.textBox_SquareSize.TabIndex = 2;
            this.textBox_SquareSize.TextChanged += new System.EventHandler(this.textBox_SquareSize_TextChanged);
            this.textBox_SquareSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_SquareSize_KeyPress);
            // 
            // label_SquareSize
            // 
            this.label_SquareSize.AutoSize = true;
            this.label_SquareSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_SquareSize.Location = new System.Drawing.Point(6, 110);
            this.label_SquareSize.Name = "label_SquareSize";
            this.label_SquareSize.Size = new System.Drawing.Size(120, 18);
            this.label_SquareSize.TabIndex = 1;
            this.label_SquareSize.Text = "square size(mm)";
            // 
            // label_NY
            // 
            this.label_NY.AutoSize = true;
            this.label_NY.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_NY.Location = new System.Drawing.Point(6, 68);
            this.label_NY.Name = "label_NY";
            this.label_NY.Size = new System.Drawing.Size(128, 18);
            this.label_NY.TabIndex = 1;
            this.label_NY.Text = "num columns(NY)";
            // 
            // label_NX
            // 
            this.label_NX.AutoSize = true;
            this.label_NX.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_NX.Location = new System.Drawing.Point(6, 30);
            this.label_NX.Name = "label_NX";
            this.label_NX.Size = new System.Drawing.Size(105, 18);
            this.label_NX.TabIndex = 1;
            this.label_NX.Text = "num rows(NX)";
            // 
            // numericUpDown_NY
            // 
            this.numericUpDown_NY.Location = new System.Drawing.Point(142, 68);
            this.numericUpDown_NY.Maximum = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.numericUpDown_NY.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDown_NY.Name = "numericUpDown_NY";
            this.numericUpDown_NY.Size = new System.Drawing.Size(67, 22);
            this.numericUpDown_NY.TabIndex = 0;
            this.numericUpDown_NY.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.numericUpDown_NY.ValueChanged += new System.EventHandler(this.numericUpDown_NY_ValueChanged);
            // 
            // numericUpDown_NX
            // 
            this.numericUpDown_NX.Location = new System.Drawing.Point(142, 28);
            this.numericUpDown_NX.Maximum = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.numericUpDown_NX.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDown_NX.Name = "numericUpDown_NX";
            this.numericUpDown_NX.Size = new System.Drawing.Size(67, 22);
            this.numericUpDown_NX.TabIndex = 0;
            this.numericUpDown_NX.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numericUpDown_NX.ValueChanged += new System.EventHandler(this.numericUpDown_NX_ValueChanged);
            // 
            // groupBox_ImageSaving
            // 
            this.groupBox_ImageSaving.Controls.Add(this.button_ImageSavingSave);
            this.groupBox_ImageSaving.Controls.Add(this.button_ImgSave);
            this.groupBox_ImageSaving.Controls.Add(this.button_ImageSavingPath);
            this.groupBox_ImageSaving.Controls.Add(this.textBox_ImageSavingPath);
            this.groupBox_ImageSaving.Controls.Add(this.label__ImageSavingPath);
            this.groupBox_ImageSaving.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_ImageSaving.Location = new System.Drawing.Point(237, 12);
            this.groupBox_ImageSaving.Name = "groupBox_ImageSaving";
            this.groupBox_ImageSaving.Size = new System.Drawing.Size(398, 146);
            this.groupBox_ImageSaving.TabIndex = 0;
            this.groupBox_ImageSaving.TabStop = false;
            this.groupBox_ImageSaving.Text = "Image Saving";
            // 
            // button_ImageSavingSave
            // 
            this.button_ImageSavingSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_ImageSavingSave.Location = new System.Drawing.Point(6, 96);
            this.button_ImageSavingSave.Name = "button_ImageSavingSave";
            this.button_ImageSavingSave.Size = new System.Drawing.Size(315, 36);
            this.button_ImageSavingSave.TabIndex = 2;
            this.button_ImageSavingSave.Text = "Start to save images";
            this.button_ImageSavingSave.UseVisualStyleBackColor = true;
            this.button_ImageSavingSave.Click += new System.EventHandler(this.button_ImageSavingSave_Click);
            // 
            // button_ImgSave
            // 
            this.button_ImgSave.Enabled = false;
            this.button_ImgSave.Location = new System.Drawing.Point(327, 96);
            this.button_ImgSave.Name = "button_ImgSave";
            this.button_ImgSave.Size = new System.Drawing.Size(56, 36);
            this.button_ImgSave.TabIndex = 1;
            this.button_ImgSave.Text = "Save";
            this.button_ImgSave.UseVisualStyleBackColor = true;
            this.button_ImgSave.Click += new System.EventHandler(this.button_ImgSave_Click);
            // 
            // button_ImageSavingPath
            // 
            this.button_ImageSavingPath.Location = new System.Drawing.Point(348, 52);
            this.button_ImageSavingPath.Name = "button_ImageSavingPath";
            this.button_ImageSavingPath.Size = new System.Drawing.Size(35, 23);
            this.button_ImageSavingPath.TabIndex = 1;
            this.button_ImageSavingPath.Text = "...";
            this.button_ImageSavingPath.UseVisualStyleBackColor = true;
            this.button_ImageSavingPath.Click += new System.EventHandler(this.button_ImageSavingPath_Click);
            // 
            // textBox_ImageSavingPath
            // 
            this.textBox_ImageSavingPath.Location = new System.Drawing.Point(6, 53);
            this.textBox_ImageSavingPath.Name = "textBox_ImageSavingPath";
            this.textBox_ImageSavingPath.Size = new System.Drawing.Size(336, 22);
            this.textBox_ImageSavingPath.TabIndex = 0;
            // 
            // label__ImageSavingPath
            // 
            this.label__ImageSavingPath.AutoSize = true;
            this.label__ImageSavingPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label__ImageSavingPath.Location = new System.Drawing.Point(6, 32);
            this.label__ImageSavingPath.Name = "label__ImageSavingPath";
            this.label__ImageSavingPath.Size = new System.Drawing.Size(181, 18);
            this.label__ImageSavingPath.TabIndex = 1;
            this.label__ImageSavingPath.Text = "Enter the path to the folder";
            // 
            // groupBox_Calibration
            // 
            this.groupBox_Calibration.Controls.Add(this.button_MakeAndSave);
            this.groupBox_Calibration.Controls.Add(this.label_EnterPath);
            this.groupBox_Calibration.Controls.Add(this.button_CaliPath);
            this.groupBox_Calibration.Controls.Add(this.textBox_Calipath);
            this.groupBox_Calibration.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox_Calibration.Location = new System.Drawing.Point(12, 490);
            this.groupBox_Calibration.Name = "groupBox_Calibration";
            this.groupBox_Calibration.Size = new System.Drawing.Size(623, 115);
            this.groupBox_Calibration.TabIndex = 0;
            this.groupBox_Calibration.TabStop = false;
            this.groupBox_Calibration.Text = "Calibration";
            // 
            // button_MakeAndSave
            // 
            this.button_MakeAndSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_MakeAndSave.Location = new System.Drawing.Point(6, 70);
            this.button_MakeAndSave.Name = "button_MakeAndSave";
            this.button_MakeAndSave.Size = new System.Drawing.Size(602, 36);
            this.button_MakeAndSave.TabIndex = 2;
            this.button_MakeAndSave.Text = "make&&save matrixfile";
            this.button_MakeAndSave.UseVisualStyleBackColor = true;
            this.button_MakeAndSave.Click += new System.EventHandler(this.button_MakeAndSave_Click);
            // 
            // label_EnterPath
            // 
            this.label_EnterPath.AutoSize = true;
            this.label_EnterPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_EnterPath.Location = new System.Drawing.Point(6, 18);
            this.label_EnterPath.Name = "label_EnterPath";
            this.label_EnterPath.Size = new System.Drawing.Size(513, 18);
            this.label_EnterPath.TabIndex = 1;
            this.label_EnterPath.Text = "Enter the matrix file path(matrix will be saved  as BELOW_PATH\\mtxNdist.txt)";
            // 
            // button_CaliPath
            // 
            this.button_CaliPath.Location = new System.Drawing.Point(573, 42);
            this.button_CaliPath.Name = "button_CaliPath";
            this.button_CaliPath.Size = new System.Drawing.Size(35, 23);
            this.button_CaliPath.TabIndex = 1;
            this.button_CaliPath.Text = "...";
            this.button_CaliPath.UseVisualStyleBackColor = true;
            this.button_CaliPath.Click += new System.EventHandler(this.button_CaliPath_Click);
            // 
            // textBox_Calipath
            // 
            this.textBox_Calipath.Location = new System.Drawing.Point(7, 42);
            this.textBox_Calipath.Name = "textBox_Calipath";
            this.textBox_Calipath.Size = new System.Drawing.Size(554, 22);
            this.textBox_Calipath.TabIndex = 0;
            // 
            // status
            // 
            this.status.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbl_status});
            this.status.Location = new System.Drawing.Point(0, 604);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(645, 26);
            this.status.TabIndex = 11;
            // 
            // lbl_status
            // 
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(151, 20);
            this.lbl_status.Text = "toolStripStatusLabel1";
            // 
            // pictureBoxCV
            // 
            this.pictureBoxCV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxCV.Location = new System.Drawing.Point(12, 165);
            this.pictureBoxCV.Name = "pictureBoxCV";
            this.pictureBoxCV.Size = new System.Drawing.Size(430, 320);
            this.pictureBoxCV.TabIndex = 13;
            this.pictureBoxCV.TabStop = false;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = CalibrationFormResource.notifyIcon1_Icon;
            this.notifyIcon1.Text = "WebcamCalibration";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // CalibrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(645, 630);
            this.Controls.Add(this.pictureBoxCV);
            this.Controls.Add(this.status);
            this.Controls.Add(this.checkBox_AlwaysOnTop);
            this.Controls.Add(this.fpsLabel);
            this.Controls.Add(this.groupBox_VideoCaptureSettings);
            this.Controls.Add(this.groupBox_SPC);
            this.Controls.Add(this.groupBox_CheckerBoard);
            this.Controls.Add(this.groupBox_ImageSaving);
            this.Controls.Add(this.groupBox_Calibration);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CalibrationForm";
            this.Text = "The Webcam Calibration Form";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VideoCaptureForm_FormClosing);
            this.Load += new System.EventHandler(this.VideoCaptureForm_Load);
            this.Resize += new System.EventHandler(this.VideoCaptureForm_Resize);
            this.groupBox_VideoCaptureSettings.ResumeLayout(false);
            this.groupBox_VideoCaptureSettings.PerformLayout();
            this.groupBox_SPC.ResumeLayout(false);
            this.groupBox_CheckerBoard.ResumeLayout(false);
            this.groupBox_CheckerBoard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_NY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_NX)).EndInit();
            this.groupBox_ImageSaving.ResumeLayout(false);
            this.groupBox_ImageSaving.PerformLayout();
            this.groupBox_Calibration.ResumeLayout(false);
            this.groupBox_Calibration.PerformLayout();
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    }
}
