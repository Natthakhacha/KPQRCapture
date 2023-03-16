using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using KPBASE;
using ZXing;

namespace KPQRCapture.V2
{
    public enum emCommand:int
    {
        STOP = 0,
        START = 1
    }
    public partial class QRCapture : uBase
    {
        public event EventHandler OnScan;
        FilterInfoCollection infoCollection;
        VideoCaptureDevice captureDevice;
        public List<string> Device { set; get; } = new List<string>();
        public string QRText { set; get; }
        public QRCapture()
        {
            InitializeComponent();
        }
        private emCommand _cmd = emCommand.STOP;
        public emCommand Command
        {
            set 
            {
                switch (value)
                {
                    case emCommand.STOP:
                        _cmd = Stop();
                        break;
                    case emCommand.START:
                        _cmd = Start();
                        break;
                }
            }
            get
            {
                return _cmd;
            }
        }
        public emCommand Start()
        {
            try
            {
                if (captureDevice != null)
                {
                    captureDevice.Start();
                    QRText = "";
                    captureDevice = null;
                    captureDevice = new VideoCaptureDevice(infoCollection[0].MonikerString);
                    captureDevice.NewFrame += CaptureDevice_NewFrame;
                    timer1.Start();
                }
            }
            catch
            {
            }
            return captureDevice != null ? captureDevice.IsRunning ? emCommand.START : emCommand.STOP : emCommand.STOP;
        }
        public emCommand Stop()
        {
            try
            {
                if (captureDevice != null)
                {
                    timer1.Stop();
                    if (captureDevice.IsRunning)
                    {
                        captureDevice.NewFrame -= CaptureDevice_NewFrame;
                        captureDevice.SignalToStop();
                        captureDevice.WaitForStop();
                        captureDevice.Stop();
                        this.Img = null;
                    }
                }
            }
            catch
            {
            }
            return captureDevice != null ? captureDevice.IsRunning ? emCommand.START : emCommand.STOP : emCommand.STOP;
        }
        public void init()
        {
            infoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterinfo in infoCollection) Device.Add(filterinfo.Name);
            this.QRText = "";
            captureDevice = new VideoCaptureDevice(infoCollection[0].MonikerString);
            captureDevice.NewFrame += CaptureDevice_NewFrame;
            captureDevice.Start();
        }
        private Image Img { set; get; }
        private void CaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            this.Img = (Bitmap)eventArgs.Frame.Clone();
            //this.Img.RotateFlip(RotateFlipType.Rotate180FlipY);
            pictureBox1.Image = this.Img;
            Application.DoEvents();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>{ process(); }));
            }
            else
            {
                process();
            }
        }
        private void process()
        {
            try
            {
                if (pictureBox1.Image != null)
                {
                    BarcodeReader barcoderead = new BarcodeReader();
                    Result result = barcoderead.Decode((Bitmap)this.Img);
                    if (result != null)
                    {
                        //this.QRText = result.ToString();
                        //timer1.Stop();
                        Console.Beep();
                        this.OnScan?.Invoke(result, new EventArgs());
                        //if (captureDevice.IsRunning)
                        //{
                        //    captureDevice.NewFrame -= CaptureDevice_NewFrame;
                        //    captureDevice.SignalToStop();
                        //    captureDevice.WaitForStop();
                        //    captureDevice.Stop();
                        //}
                    }
                }
            }
            catch { }
        }
    }
}
