using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.Networking
{
    public class BtleAccelerometerReading
    {
        public sbyte RawX { get; set; }
        public sbyte RawY { get; set; }
        public sbyte RawZ { get; set; }

        public double X
        {
            get { return RawX / 64.0; }
        }

        public double Y
        {
            get { return RawY / 64.0; }
        }

        public double Z
        {
            get { return RawZ / -64.0; }
        }

    }
}
