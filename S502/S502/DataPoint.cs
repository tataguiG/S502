using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S502
{
    //public class Tag
    //{
    //    public DateTime TimeStamp { get; set; }
    //    public string Description { get; set; }

    //    public string Detail { get; set; }
    //}

    public class DataPoint
    {
        public int RawData { get; set; }
        //public Tag DataTag { get; set; }

        public static DataPoint BuildDataPoint(byte[] data)
        {
            return null;
        }
    }

}
