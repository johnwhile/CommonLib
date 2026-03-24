using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public struct Time
    {
        /// <summary>
        /// A single tick represents 100 nanoseconds.
        /// There are 10,000 ticks in a millisecond and 10 million ticks in a second.
        /// </summary>
        public long Ticks;

        public Time(long ticks)
        {
            Ticks = ticks;
        }
        public double TotMSec
        {
            get => TimerTick.GetMSFromTick(Ticks);
            set { throw new NotImplementedException(); }
        }
        public double TotSec
        {
            get => TotMSec / 1000.0;
            set { throw new NotImplementedException(); }
        }
       
        public double TotMin
        {
            get => TotSec / 60.0;
            set { throw new NotImplementedException(); }
        }
       
        public double TotHour
        {
            get => TotMin / 60.0;
            set { throw new NotImplementedException(); }
        }

        public int Msec
        {
            get
            {
                double sec = TotSec;
                return (int)((sec - (int)sec) * 1000.0);
            }
        }
        public int Sec
        {
            get
            {
                double min = TotMin;
                return (int)((min - (int)min) * 60.0);
            }
        }

        //public void Format(out int h, out int m, out int s, out int ms) { }

    }
}
