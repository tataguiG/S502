using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace S502
{
    class SerialPortController : IDataExchange
    {
        private SerialPort _comm;

        public bool Initialized { get; set; }
        public Mutex CloseEvent { get; set; }


        bool IDataExchange.StopRead
        {
            get => _cancellationRead.IsCancellationRequested;
            set { if(value) _cancellationRead.Cancel(); }
        }

        bool IDataExchange.StopWrite
        {
            get => _cancellationWrite.IsCancellationRequested;
            set { if (value) _cancellationWrite.Cancel(); }
        }

        public SerialPortController()
        {
            
        }

        private CancellationTokenSource _cancellationRead = new CancellationTokenSource();
        private CancellationTokenSource _cancellationWrite = new CancellationTokenSource();

        private object _accessLock = new object();
        private readonly  byte[] _inputBufer = new byte[1000];
        private int _readOffset = 0;
        private int _writeOffset = 0;

        public void Initialize(string portName, int baudRate, string parity, int dataBits, int stopBits)
        {
            //
            // 摘要:
            //     使用指定的端口名、波特率、奇偶校验位、数据位和停止位初始化 System.IO.Ports.SerialPort 类的新实例。
            //
            // 参数:
            //   portName:
            //     要使用的端口（例如 COM1）。
            //
            //   baudRate:
            //     波特率。
            //
            //   parity:
            //     System.IO.Ports.SerialPort.Parity 值之一。
            //
            //   dataBits:
            //     数据位值。
            //
            //   stopBits:
            //     System.IO.Ports.SerialPort.StopBits 值之一。
            //
            // 异常:
            //   T:System.IO.IOException:
            //     无法找到或打开指定的端口。
            //public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits);

            // 通过配置文件打开串口
            _comm = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);

            try
            {
                _comm.Open();

                Initialized = true;
            }
            catch (IOException ex)
            {
                Debug.Print("Initialize exception: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Debug.Print(ex.GetType().ToString() + ": " + ex.Message);   
            }
        }

        public async void Work()
        {
            var task = CreateReceiveDataAsyncTask(data =>
                {
                    lock (_accessLock)
                    {
                        _inputBufer[_writeOffset++] = data;
                    }
                    return true;
                }
            );

            task.Start();

            // while ()
        }

        delegate bool ReceiveDataHandler(byte inputData);


        private Task CreateReceiveDataAsyncTask(ReceiveDataHandler handler)
        {
            return new Task(() =>
            {
                while (!_cancellationRead.IsCancellationRequested)
                {
                    byte tmp = (byte)_comm.ReadByte();
                    handler?.Invoke(tmp);
                }

            });
        }

        byte[] IDataExchange.ReadBytes(int targetLength)
        {
            lock (_accessLock)
            {
                if (_writeOffset - _readOffset >= targetLength)
                {
                    var ret = new byte[targetLength];
                    Array.Copy(_inputBufer, _writeOffset, ret, 0, targetLength);
                    return ret;
                }

                return null;
            }
        }

        void IDataExchange.WriteBytes(byte[] message)
        {
            if (message == null || message.Length == 0)
                throw new ArgumentException($"Invalid message!");
            _comm.Write(message, 0, message.Length);
        }
    }
}
