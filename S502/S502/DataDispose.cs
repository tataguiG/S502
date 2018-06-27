using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace S502
{
    /// <summary>
    /// 核心控制，提供数据接收，整合，存储的操作
    /// </summary>
    public class DataDispose
    {
        //private Thread _recevDataThread;
        //private Thread _recvEventThread;


        private Task _recvDataTask;
        private CancellationTokenSource _cancellationRecvData = new CancellationTokenSource();

        private Task _recvEventTask;
        private CancellationTokenSource _cancellationRecvEvent = new CancellationTokenSource();

        private Thread _mergeDataThread;

        private bool _stopRecvData = false;
        private bool _stopRecvEvent = false;

        private DataPoint[] _mergedDataBuffer = new DataPoint[1000];
        private int _writeOffset = 0;
        private int _readOffset = 0;


        

        public void StartWork(IDataExchange dataSource, IDataExchange eventSource = null)
        {
            if (dataSource == null)
            {
                throw new ArgumentException("dataSupplier is null");
            }

            //_recevDataThread = new Thread(
            //    () =>
            //    {
            //        SerialPortController spRecv = new SerialPortController();
            //        while ()
            //    });

            _recvDataTask = new Task(() =>
            {
                while (!_cancellationRecvData.IsCancellationRequested)
                {
                    byte[] ret = dataSource.ReadBytes(100);


                }
            });

            if (eventSource != null)
            {
                _recvEventTask = new Task(() =>
                    {
                        while (!_cancellationRecvEvent.IsCancellationRequested)
                        {
                            byte[] ret = eventSource.ReadBytes(100);
                        }
                    }

                );
            }
            _recvDataTask.Start();
        }

        public void GetDataFrom(IDataExchange source)
        {
            
        }
    }
}
