using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S502
{
    public class EventData
    {
        public EventData()
        {
            TimeStamp = DateTime.Now;    
        }

        public DateTime TimeStamp { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
    }
}
