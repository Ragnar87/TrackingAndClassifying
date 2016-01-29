using Emgu.CV;
//using Emgu.CV.Cuda;
using Emgu.CV.Cvb;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoTest
{

    public partial class SalmonCounter : Form
    {

        //Test video from the SalmonCounter video feed
        const string videoOne = "C:\\Users\\Rsg\\Documents\\KT\\EndaligVerkætlan\\laksar2.avi";
        const string videoTwo = "C:\\Users\\Rsg\\Documents\\video\\laksur2.mp4";
        const string videoThree = "C:\\TinyTake\\Videos\\TinyTake02-09-2015-03-50-28.mp4";
        const string laboratory = "C:\\Users\\Rsg\\Documents\\KT\\EndaligVerkætlan\\laboratory_raw.avi";
        int FPS = 30;
        int frameCount = 0;
        int currentFrame = 0;
        int msec = 0;
        
        Capture _capture;
        Mat  _frame;

        Stopwatch watch;
        TimeSpan time;
        Thread sTrackerThread, backgroundSubtractorThread;

        BlobImage bImage;
        SalmonTracker sTracker;
        ForegroundDetector fgDetector;

        Counter counter;

        public SalmonCounter()
        {
            InitializeComponent();
            _capture = new Capture(videoOne);

            counter = new Counter(_capture.Width);

            bImage = new BlobImage();
            fgDetector = new ForegroundDetector(bImage);
            sTracker = new SalmonTracker(bImage, counter);

            watch = new Stopwatch();
            time = new TimeSpan();

            FPS = (int)_capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
            frameCount = (int)_capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
            pictureBox1.Width = _capture.Width;
            pictureBox1.Height = _capture.Height;

            show.Width = _capture.Width;
            show.Height = _capture.Height;
            
            //msec between frames
            msec = (int)(1000 / FPS);

            //set the event handler
            _capture.ImageGrabbed += grabImage;
            _capture.Start();
            watch.Start();
            _frame = new Mat();

            //Start foregroundSegmenter tread and salmon tracker thread
            backgroundSubtractorThread = new Thread(fgDetector.detect);
            backgroundSubtractorThread.Start();
            sTrackerThread = new Thread(sTracker.updateSalmons);
            sTrackerThread.Start();

        }

        private void grabImage(object sender, EventArgs e)
        {
            if (frameCount > currentFrame++)
            {
                _capture.Retrieve(_frame);
                
                //set image in ForegroundDetector if its not being used
                if (fgDetector.Finished)
                {
                    fgDetector.Image = _frame.Clone();
                    sTracker.FrameImage = _frame.ToImage<Bgr, Byte>();
                }
                
                //draw the tracking bounding boxes
                sTracker.drawBoxes(ref _frame);

                //display frame image
                display(_frame.Bitmap, pictureBox1);

                //display the foreground mask
                if(bImage.getBlobImage()!= null)
                    display(bImage.getBlobImage().Bitmap, show);

                //ensure correct time between frames
                time = watch.Elapsed;
                if(msec - time.Milliseconds > 0)
                    Thread.Sleep(msec - time.Milliseconds);
                watch.Restart();
            }

        }

        //exit threads when application is closed
        protected override void OnClosing(CancelEventArgs e)
        {
            if (_capture != null)
            {
                _capture.Stop();
            }
            if (!sTracker.Stop)
            {
                sTracker.Stop = true;
                sTrackerThread.Abort();
                sTrackerThread.Join();
            }
            if (!fgDetector.Stop)
            {
                fgDetector.Stop = true;
                backgroundSubtractorThread.Abort();
                backgroundSubtractorThread.Join();
            }
        }

        private delegate void displayDelegate(Bitmap image, PictureBox box);

        private void display(Bitmap image, PictureBox box)
        {
            if(box.InvokeRequired)
                try
                {
                    displayDelegate dd = new displayDelegate(display);
                    this.BeginInvoke(dd, new object[] { image, box });
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            else
            {
                box.Image = image;
            }
        }
    }


}
