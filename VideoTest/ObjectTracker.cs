using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//using Emgu.CV.XFeatures2D;
using Emgu.CV.Cvb;
using SVM_BOW_TEST;
//using Emgu.CV.Cuda;

namespace VideoTest
{

    class SalmonTracker
    {
        //directories for classifier training images
        string positives = "C:\\Users\\Rsg\\Documents\\KT\\EmgucvTest\\EmguTestForm\\trening\\pos";
        string negatives = "C:\\Users\\Rsg\\Documents\\KT\\EmgucvTest\\EmguTestForm\\trening\\neg";
        
        Counter _counter;
        List<Salmon> _salmons;
        Classifier _classifier = null;
        ObjectTracker _tracker;
        BlobImage _blobImage;
        bool updated = false;
        int inactiveThresholdMilliseconds = 1000;

        public Image<Bgr, Byte> FrameImage { get; set; }
        public bool Stop { get; set; }

        public SalmonTracker(BlobImage blobImage, Counter c)
        {
            _counter = c;
            List<string> folders = new List<string>();
            folders.Add(positives);
            folders.Add(negatives);
            _classifier = new Classifier(folders);
            _classifier.computeAndExtract();
            _classifier.train();
            _tracker = new ObjectTracker();
            _salmons = new List<Salmon>();
            _blobImage = blobImage;
            FrameImage = null;
            Stop = false;
        }

        //update salmons runs on it's own thread
        public void updateSalmons()
        {
            while (!Stop)
            {
                if (_blobImage.Updated)
                {

                    using (Image<Gray, Byte> blobImage = _blobImage.getBlobImage().Clone())
                    {
                        _tracker.update(blobImage);
                        _blobImage.Updated = false;
                    }
                    //if track intersects with a salmon update the salmon position
                    //if not check if it contains a new salmon
                    foreach (CvTrack track in _tracker.getTracks())
                    {
                        if (checkIfSalmon(track))
                        {
                            bool exists = false;
                            foreach (Salmon salmon in _salmons)
                            {
                                Rectangle rect = track.BoundingBox;
                                Point pos = new Point((int)track.Centroid.X, (int)track.Centroid.Y);
                                if(rect.IntersectsWith(salmon.getBoundingBox()))
                                {
                                    if (rect.Size.Width > salmon.getBoundingBox().Size.Width)
                                        salmon.updateBox(rect);
                                    else
                                        salmon.updatePosition(pos);
                                    updated = true;
                                    exists = true;
                                    break;
                                }
                            }
                            //check if a salmon is inside the track bounding box
                            if (!exists)
                            {
                                if (_classifier.classify(FrameImage.Copy(track.BoundingBox)) == 1)
                                {
                                    _salmons.Add(new Salmon(track.BoundingBox, new Rectangle(new Point(0,0), FrameImage.Size)));
                                    updated = true;
                                }
                            }
                        }
                    }
                    foreach (Salmon s in emptyBoxes())
                    {
                        _counter.updateCounter(s);
                    }
                }
            }
        }

        //check if a track size is between 50 and 5 percent of the frame size
        private bool checkIfSalmon(CvTrack track)
        {
            float frameSize = FrameImage.Width * FrameImage.Height;
            float trackSize = track.BoundingBox.Width * track.BoundingBox.Height;
            int percentage =(int) (trackSize / frameSize * 100);
            if (percentage < 50 && percentage > 5)
                return true;
            return false;
        }

        //draw the bounding boxes
        public void drawBoxes(ref Mat frame)
        {
            if (_salmons != null && updated)
            {
                foreach (Salmon salmon in _salmons)
                {
                    CvInvoke.Rectangle(frame, salmon.getBoundingBox(), new MCvScalar(255, 255, 255));
                }
            }
        }

        //returns the salmons that have been inactive longer than the treshold
        //and are not present in their bounding box
        public List<Salmon> emptyBoxes()
        {
            List<Salmon> inactives = new List<Salmon>();
            int mSeconds = 0;
            foreach (Salmon salmon in _salmons)
            {
                mSeconds = salmon.getInactive().Seconds * 1000 + salmon.getInactive().Milliseconds;
                if (mSeconds > inactiveThresholdMilliseconds)
                    if (_classifier.classify(FrameImage.Copy(salmon.getBoundingBox())) != 1)
                    {
                        inactives.Add(salmon);
                    }
            }
            foreach (Salmon salmon in inactives)
            {
                _salmons.Remove(salmon);
            }
        
        return inactives;
                
        }

        public List<Salmon> getSalmons()
        {
            return _salmons;
        }
    }


    class ObjectTracker
    {
        CvBlobDetector _blobDetector;
        CvTracks _tracker;
        CvBlobs _blobs;

        public ObjectTracker()
        {
            _blobDetector = new CvBlobDetector();
            _tracker = new CvTracks();
            _blobs = new CvBlobs();
        }

        //update the blobs based on the new image
        //and update the tracks in the tracker
        public void update(Image<Gray, Byte> blobImage)
        {
            if (blobImage != null)
            {
                _blobDetector.Detect(blobImage, _blobs);
                _blobs.FilterByArea(3000, int.MaxValue);
                _tracker.Update(_blobs, 150, 150, 200);
            }
        }

        public ICollection<CvTrack> getTracks()
        {
            if (_tracker != null)
                return _tracker.Values;
            return null;
        }
    }

    class Salmon
    {

        private Rectangle _position;
        private Point _center;

        int initialPosition;
        private Rectangle maxSize;

        Stopwatch inactive;
        public Salmon(Rectangle position, Rectangle frameSize)
        {
            maxSize = frameSize;
            inactive = new Stopwatch();
            inactive.Start();
            _position = position;
            initialPosition = position.X;
            int x = position.X + (int)position.Width / 2;
            int y = position.Y + (int)position.Height / 2;
            _center = new Point(x, y);
        }

        public bool checkPoint(Point point)
        {
            if (point.X < _position.X || point.X > (_position.X + _position.Width))
                return false;
            if (point.Y < _position.Y || point.Y > (_position.Y + _position.Height))
                return false;
            return true;
        }

        public void updatePosition(Point center)
        {
            _position.X += center.X - _center.X;
            _position.Y += center.Y - _center.Y;

            //check if box intersects with any frame borders
            if (_position.IntersectsWith(maxSize))
                _position.Intersect(maxSize);

            _center = center;
            inactive.Restart();
        }

        public void updateBox(Rectangle box)
        {
            _position = box;

            //check if the bounding box intersects with the frame borders
            if (_position.IntersectsWith(maxSize))
                _position.Intersect(maxSize);

            int x = _position.X + (int)_position.Width / 2;
            int y = _position.Y + (int)_position.Height / 2;
            _center = new Point(x, y);
            
            //if this method is called the salmon has been active
            inactive.Restart();
        }
        public Rectangle getBoundingBox()
        {
            return _position;
        }

        public Point getCenter()
        {
            return _center;
        }

        public TimeSpan getInactive()
        {
            return inactive.Elapsed;
        }

        public int getInitialPos()
        {
            return initialPosition;
        }
        
    }
}
