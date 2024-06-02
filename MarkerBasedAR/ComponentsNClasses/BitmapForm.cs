using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Camera_NET;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class BitmapForm : Form
    {
        private CameraChoice _CameraChoice = new CameraChoice();
        Thread playThread;
        Mat mat = new Mat();
        private VideoCapture vc = null;
        public Bitmap currentFrame;
        private PictureBox pictureBoxCV;
        private GroupBox groupBox_VideoCaptureSettings;
        private Label label_Source;
        private ComboBox comboBoxVideoResolution;
        private Label label_Resolution;
        private ComboBox comboBoxVideoSource;
        private GroupBox groupBox_SPC;
        private Button buttonCancel;
        private Button buttonPause;
        private CheckBox checkBox_AlwaysOnTop;
        private Button buttonStart;
        public bool iftopMost;
        private static readonly object lockObject = new object();

        public BitmapForm()
        {
            InitializeComponent();
            iftopMost = true;
        }
        private void InitializeComponent()
        {
            this.pictureBoxCV = new System.Windows.Forms.PictureBox();
            this.groupBox_VideoCaptureSettings = new System.Windows.Forms.GroupBox();
            this.comboBoxVideoResolution = new System.Windows.Forms.ComboBox();
            this.label_Resolution = new System.Windows.Forms.Label();
            this.comboBoxVideoSource = new System.Windows.Forms.ComboBox();
            this.label_Source = new System.Windows.Forms.Label();
            this.groupBox_SPC = new System.Windows.Forms.GroupBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.checkBox_AlwaysOnTop = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCV)).BeginInit();
            this.groupBox_VideoCaptureSettings.SuspendLayout();
            this.groupBox_SPC.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxCV
            // 
            this.pictureBoxCV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxCV.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxCV.Name = "pictureBoxCV";
            this.pictureBoxCV.Size = new System.Drawing.Size(380, 264);
            this.pictureBoxCV.TabIndex = 0;
            this.pictureBoxCV.TabStop = false;
            // 
            // groupBox_VideoCaptureSettings
            // 
            this.groupBox_VideoCaptureSettings.Controls.Add(this.comboBoxVideoResolution);
            this.groupBox_VideoCaptureSettings.Controls.Add(this.label_Resolution);
            this.groupBox_VideoCaptureSettings.Controls.Add(this.comboBoxVideoSource);
            this.groupBox_VideoCaptureSettings.Controls.Add(this.label_Source);
            this.groupBox_VideoCaptureSettings.Location = new System.Drawing.Point(403, 12);
            this.groupBox_VideoCaptureSettings.Name = "groupBox_VideoCaptureSettings";
            this.groupBox_VideoCaptureSettings.Size = new System.Drawing.Size(238, 119);
            this.groupBox_VideoCaptureSettings.TabIndex = 1;
            this.groupBox_VideoCaptureSettings.TabStop = false;
            this.groupBox_VideoCaptureSettings.Text = "VideoCapture Settings";
            // 
            // comboBoxVideoResolution
            // 
            this.comboBoxVideoResolution.FormattingEnabled = true;
            this.comboBoxVideoResolution.Location = new System.Drawing.Point(9, 90);
            this.comboBoxVideoResolution.Name = "comboBoxVideoResolution";
            this.comboBoxVideoResolution.Size = new System.Drawing.Size(223, 23);
            this.comboBoxVideoResolution.TabIndex = 3;
            this.comboBoxVideoResolution.SelectedIndexChanged += new System.EventHandler(this.comboBoxVideoResolution_SelectedIndexChanged);
            // 
            // label_Resolution
            // 
            this.label_Resolution.AutoSize = true;
            this.label_Resolution.Location = new System.Drawing.Point(6, 72);
            this.label_Resolution.Name = "label_Resolution";
            this.label_Resolution.Size = new System.Drawing.Size(135, 15);
            this.label_Resolution.TabIndex = 2;
            this.label_Resolution.Text = "Video Resolution";
            // 
            // comboBoxVideoSource
            // 
            this.comboBoxVideoSource.FormattingEnabled = true;
            this.comboBoxVideoSource.Location = new System.Drawing.Point(9, 39);
            this.comboBoxVideoSource.Name = "comboBoxVideoSource";
            this.comboBoxVideoSource.Size = new System.Drawing.Size(223, 23);
            this.comboBoxVideoSource.TabIndex = 1;
            this.comboBoxVideoSource.SelectedIndexChanged += new System.EventHandler(this.comboBoxVideoSource_SelectedIndexChanged);
            // 
            // label_Source
            // 
            this.label_Source.AutoSize = true;
            this.label_Source.Location = new System.Drawing.Point(6, 21);
            this.label_Source.Name = "label_Source";
            this.label_Source.Size = new System.Drawing.Size(103, 15);
            this.label_Source.TabIndex = 0;
            this.label_Source.Text = "Video Source";
            // 
            // groupBox_SPC
            // 
            this.groupBox_SPC.Controls.Add(this.buttonCancel);
            this.groupBox_SPC.Controls.Add(this.buttonPause);
            this.groupBox_SPC.Controls.Add(this.buttonStart);
            this.groupBox_SPC.Location = new System.Drawing.Point(531, 137);
            this.groupBox_SPC.Name = "groupBox_SPC";
            this.groupBox_SPC.Size = new System.Drawing.Size(110, 139);
            this.groupBox_SPC.TabIndex = 2;
            this.groupBox_SPC.TabStop = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(12, 106);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(83, 27);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonPause
            // 
            this.buttonPause.Location = new System.Drawing.Point(12, 61);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(83, 27);
            this.buttonPause.TabIndex = 1;
            this.buttonPause.Text = "Pause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(12, 17);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(83, 27);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // checkBox_AlwaysOnTop
            // 
            this.checkBox_AlwaysOnTop.AutoSize = true;
            this.checkBox_AlwaysOnTop.Checked = true;
            this.checkBox_AlwaysOnTop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_AlwaysOnTop.Location = new System.Drawing.Point(403, 257);
            this.checkBox_AlwaysOnTop.Name = "checkBox_AlwaysOnTop";
            this.checkBox_AlwaysOnTop.Size = new System.Drawing.Size(117, 19);
            this.checkBox_AlwaysOnTop.TabIndex = 3;
            this.checkBox_AlwaysOnTop.Text = "AlwaysOnTop";
            this.checkBox_AlwaysOnTop.UseVisualStyleBackColor = true;
            this.checkBox_AlwaysOnTop.CheckedChanged += new System.EventHandler(this.checkBoxAlwaysOnTop_CheckedChanged);
            // 
            // BitmapForm
            // 
            this.ClientSize = new System.Drawing.Size(653, 288);
            this.Controls.Add(this.checkBox_AlwaysOnTop);
            this.Controls.Add(this.groupBox_SPC);
            this.Controls.Add(this.groupBox_VideoCaptureSettings);
            this.Controls.Add(this.pictureBoxCV);
            this.Name = "BitmapForm";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BitmapForm_Closing);
            this.Load += new System.EventHandler(this.BitmapForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCV)).EndInit();
            this.groupBox_VideoCaptureSettings.ResumeLayout(false);
            this.groupBox_VideoCaptureSettings.PerformLayout();
            this.groupBox_SPC.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private void BitmapForm_Load(object sender, EventArgs e)
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
            if(comboBoxVideoSource.Text!= "OBS Virtual Camera")
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

        }
        private void buttonStart_Click(object sender, EventArgs e)
        {
            var selectedDevice = _CameraChoice.Devices[comboBoxVideoSource.SelectedIndex];
            if(selectedDevice.Name!= "OBS Virtual Camera")
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
            
            playThread = new Thread(ShowI);
            playThread.IsBackground = true;
            playThread.Start();
            buttonStart.Enabled = false;
        }
        public void ShowI()
        {
            while (true)
            {
                vc.Read(mat);
                currentFrame = mat.ToBitmap();
                int pictureBoxWidth = pictureBoxCV.Width;
                int pictureBoxHeight = pictureBoxCV.Height;
                double widthRatio = (double)pictureBoxWidth / mat.Width;
                double heightRatio = (double)pictureBoxHeight / mat.Height;
                double ratio = Math.Min(widthRatio, heightRatio);
                int newWidth = (int)(mat.Width * ratio);
                int newHeight = (int)(mat.Height * ratio);

                Mat resizedImg = new Mat();
                Cv2.Resize(mat, resizedImg, new OpenCvSharp.Size(newWidth, newHeight));

                int offsetX = (pictureBoxWidth - newWidth) / 2;
                int offsetY = (pictureBoxHeight - newHeight) / 2;

                Mat finalImg = new Mat(new OpenCvSharp.Size(pictureBoxWidth, pictureBoxHeight), MatType.CV_8UC3, Scalar.White);
                resizedImg.CopyTo(finalImg[new Rect(offsetX, offsetY, newWidth, newHeight)]);
                pictureBoxCV.Invoke(new Action(() => {pictureBoxCV.Image = finalImg.ToBitmap();}));
                resizedImg.Dispose();
                finalImg.Dispose();
            }

        }
        private void buttonPause_Click(object sender, EventArgs e)
        {
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
        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
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
            Dispose(true);
            Close();
        }

        private void checkBoxAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            iftopMost = checkBox_AlwaysOnTop.Checked;
            TopMost = iftopMost;
        }
        private void BitmapForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (playThread != null && playThread.IsAlive)
            {
                playThread.Abort();
            }
            if (vc != null)
            {
                vc.Release();
            }
            if (currentFrame != null)
            {
                currentFrame.Dispose();
            }
            Close();
        }
    }
}
