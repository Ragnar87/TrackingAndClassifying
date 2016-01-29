using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTest
{
    class Counter
    {

        int _frameWidth;
        public Counter(int width)
        {
            _frameWidth = width;
        }

        public void updateCounter(Salmon salmon)
        {
            int pos = salmon.getBoundingBox().X + salmon.getBoundingBox().Width;
            int initialPos = salmon.getInitialPos();
            if (initialPos < (int)_frameWidth / 2 && pos > (int)_frameWidth / 2)
            {
                Console.WriteLine("+1");
            }
            else if(initialPos > (int)_frameWidth / 2 && pos < (int)_frameWidth / 2)
            {
                Console.WriteLine("-1");
            }

        }

    }
}
