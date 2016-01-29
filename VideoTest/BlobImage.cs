using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VideoTest
{
    class BlobImage
    {

        bool locked = false;
        private Image<Gray, Byte> _blobImage;
        public bool Updated {get;set;}

        public BlobImage()
        {
            Updated = false;
        }

        public void setBlobImage(Image<Gray, Byte> blobImage)
        {
            lock(this)
            {
                if (locked)
                {
                    try
                    {
                        Monitor.Wait(this);
                    }
                    catch (SynchronizationLockException e)
                    {
                        Console.WriteLine(e);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Console.WriteLine(e);
                    }
                }
                locked = true;
                _blobImage = blobImage;
                locked = false;
                Updated = true;
                Monitor.Pulse(this);
            }
        }

        public Image<Gray, Byte> getBlobImage()
        {
            Image<Gray, Byte> returnImage;
            if (_blobImage == null)
                return null;
            lock (this)
            {
                if (locked)
                {
                    try
                    {
                        Monitor.Wait(this);
                    }
                    catch (SynchronizationLockException e)
                    {
                        Console.WriteLine(e);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Console.WriteLine(e);
                    }
                }
                locked = true;
                returnImage = _blobImage.Copy();
                locked = false;
                Updated = false;
                Monitor.Pulse(this);
            }
            return returnImage;
        }
    }
}
