using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoTest
{
    class ForegroundDetector
    {
        private BackgroundSubtractor _foregroundDetector;
        private BlobImage _bImage;

        private Mat _image;
        public Mat Image
        {
            get
            {
                return _image;
            }
            set 
            { 
                _image = value;
                updated = true;
            } 
        }


        public bool Finished
        { get; set; }

        public bool Stop
        { get; set; }

        bool updated = false;


        public ForegroundDetector(BlobImage image)
        {
            Finished = true;
            Stop = false;
            _foregroundDetector = new BackgroundSubtractorMOG2(200, 400, false);
            _bImage = image;
        }

        //detect method runs on it's own thread
        public void detect()
        {
            while (!Stop)
            {
                //if the frame image is updated
                if (updated)
                {
                    //not safe to set the frame image
                    Finished = false;
                    using (Mat bgImage = _image.Clone())
                    using (Mat mask = new Mat())
                    {
                        Finished = true;
                        updated = false;
                        //get mask
                        _foregroundDetector.Apply(bgImage, mask, -1);
                        //set blob image
                        _bImage.setBlobImage(mask.ToImage<Gray, Byte>().Clone());

                    }
                }
            }
        }
    }
}
