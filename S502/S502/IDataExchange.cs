using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace S502
{
    public interface IDataExchange
    {
        bool StopRead { get; set; }
        bool StopWrite { get; set; }



        byte[] ReadBytes(int targetLength);
        void WriteBytes(byte[] message);
    }
}
