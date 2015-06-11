using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.Networking
{
    public class SensorTagServices
    {
        public static Guid Accelerometer { get { return new Guid("F000AA1004514000B000000000000000"); } }
    }

    public class SensorTagCharacteristics
    {
        public static Guid AccelerometerData { get { return new Guid("F000AA1104514000B000000000000000"); } }
        public static Guid AccelerometerConfig { get { return new Guid("F000AA1204514000B000000000000000"); } }
        public static Guid AccelerometerPeriod { get { return new Guid("F000AA1304514000B000000000000000"); } }
    }
}
