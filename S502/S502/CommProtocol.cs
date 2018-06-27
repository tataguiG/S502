using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace S502
{
    public class CommProtocol
    {
        public byte CurrentDeviceId { get; set; }


        private ObservableCollection<string> _runningStatus = new ObservableCollection<string>();

        public ObservableCollection<string> RunningStatus => _runningStatus;
        //{
        //    get { return _runningStatus; }
        //}

        private ObservableCollection<RequestItem> _requests = new ObservableCollection<RequestItem>();
        private int _currentIndex;

        public ObservableCollection<RequestItem> Requests => _requests;
        //{
        //    get { return _requests; }
        //}

        /// <summary>
        /// 为协议指定执行对象
        /// </summary>
        //private PairedBleDevice _device;

        //private NonencryptedBleDevice _device;
        //public void BindPairedDevice(PairedBleDevice device)
        //{
        //    _device = device;
        //}

        //public void BindNonencryptedBleDevice(NonencryptedBleDevice device)
        //{
        //    _device = device;
        //}


        private IDataExchange _device;
        public void BindDevice(IDataExchange device)
        {
            _device = device;
        }

        public void AddRequest(RequestItem request)
        {
            // 对于验证有效的respond默认尝试发送下一条request（如果有的话）
            // 不应该提供默认选项，需要在判断有效后，再决定后面的处理方式
            //request.AfterResponseValidatedHandler += (sender, args) =>
            //{
            //    SendNextRequest();
            //};

            _requests.Add(request);
        }


        /// <summary>
        /// 为request提供默认处理方式
        /// </summary>
        /// <param name="request"></param>
        public void AddRequestWithDefaultOperation(RequestItem request)
        {
            // 对于respond进行检验
            request.ValidateResponseHandler += response =>
            {
                if (response[0] != 0x81)
                {
                    string s = new StringBuilder().AppendFormat(
                        "接收错误数据 {0} : {1}", DateTime.Now.ToString(TimePattern),
                        BytesToHexString(response)).ToString();

                    //App.Current.Dispatcher.Invoke((Action) delegate
                    //{

                    _runningStatus.Add(s);
                    //});

                    return false;
                }

                byte[] CrcTmp = new byte[2];
                if (CalculateCRCHelper(response, response.Length - 2, ref CrcTmp))
                {
                    if (response[response.Length - 2] == CrcTmp[0]
                        && response[response.Length - 1] == CrcTmp[1])
                    {
                        string s = new StringBuilder().AppendFormat(
                            "接收 {0} : {1}", DateTime.Now.ToString(TimePattern),
                            BytesToHexString(response)).ToString();
                        //App.Current.Dispatcher.Invoke((Action) delegate
                        //{
                        _runningStatus.Add(s);
                        //});
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"验证错误 传入的数组为： {BitConverter.ToString(response)}");
                        Debug.WriteLine($"验证错误 计算出的CRC为： {BitConverter.ToString(CrcTmp)}");
                        return false;
                    }
                }
                Debug.WriteLine($"CRC验证错误");
                return false;
            };

            // 对于验证有效的respond默认尝试发送下一条request（如果有的话）
            request.AfterResponseValidatedHandler += async (sender, args) =>
            {
                await Continue();
            };

            _requests.Add(request);
        }

        public void ClearAllRequests()
        {
            _requests.Clear();
            _runningStatus.Clear();
            _currentIndex = 0;
        }

        //public void ResetAllRequests()
        //{
        //    _currentIndex = 0;
        //    _runningStatus.Clear();
        //    foreach (var requestItem in _requests)
        //    {
        //        var response = new BaseResponse(requestItem.Response);
        //        //respond.Data = new byte[] {0x11, 0x22, 0x33};
        //        requestItem.Response = response;
        //    }
        //}

        public async Task Start()
        {
            await SendNextRequest();
        }

        public async Task Continue()
        {
            await SendNextRequest();
        }


        public Action AfterProtocolFinishedHandler;

        public void OnFinish()
        {
            AfterProtocolFinishedHandler?.Invoke();
        }

        public void Finish()
        {
            OnFinish();
        }

        public async Task<BaseResponse> GetFinalResult(CancellationToken cancellationToken)
        {
            while (HasNextRequest())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await SendNextRequestHelper(cancellationToken);
            }
            return _requests.Last().Response;

        }

        public Action AfterProtocolInterruptedHandler;

        public void OnInterrupted()
        {
            AfterProtocolInterruptedHandler?.Invoke();
        }

        public void Interrupted()
        {
            OnInterrupted();
        }

        private RequestItem GetRequest()
        {
            if (_currentIndex == _requests.Count)
                return null;
            return _requests[_currentIndex++];
        }

        private bool HasNextRequest()
        {
            return _currentIndex < _requests.Count;
        }

        private async Task SendNextRequestHelper(CancellationToken token)
        {
            // 获取当前的request并发送
            var request = GetRequest();
            if (request != null)
            {
                // 设置接收
                //_device.ReadBuffer = request.Response;
                //await _device.WriteBytes(request.Data, request.Elapse);

                _device.WriteBytes(request.Data);

                Debug.WriteLine($"send time: {DateTime.Now.Millisecond.ToString()}");
                string s = new StringBuilder().AppendFormat("发送 {0}: {1}",
                    DateTime.Now.ToString(TimePattern),
                    BytesToHexString(request.Data)).ToString();

                // 不使用同步对象 EventWaitHanler
                // 等待成功应答或超时
                while (!request.SuccessResponsed)
                {
                    Thread.Sleep(10);
                    token.ThrowIfCancellationRequested();
                }

            }
        }

        private async Task<bool> SendNextRequest()
        {
            // 获取当前的request并发送
            var request = GetRequest();
            if (request != null)
            {
                // 设置接收
                //_device.ReadBuffer = request.Response;

                _device.WriteBytes(request.Data/*, request.Elapse*/);

                Debug.WriteLine($"send time: {DateTime.Now.Millisecond.ToString()}");
                string s = new StringBuilder().AppendFormat("发送 {0}: {1}",
                    DateTime.Now.ToString(TimePattern),
                    BytesToHexString(request.Data)).ToString();

                //App.Current.Dispatcher.Invoke((Action)delegate
                //{
                _runningStatus.Add(s);
                //});
            }
            return false;
        }

        private static readonly string TimePattern = @"yyyy/M/d hh:mm:ss tt";

        /// <summary> 
        /// 字节数组转16进制字符串 
        /// </summary> 
        /// <param name="bytes"></param> 
        /// <returns></returns> 
        private static string BytesToHexString(byte[] bytes)
        {
            StringBuilder returnStr = new StringBuilder();
            if (bytes != null)
            {
                foreach (var b in bytes)
                {
                    returnStr.AppendFormat("0x{0:X2} ", b);
                }
            }
            return returnStr.ToString();
        }

        // CRC校验
        //计算CRC位
        //byte[]SendData为需要计算CRC的输入数据，len为senddata的长度（不包括2个字节的CRC），输出为计算得到的2字节的CRC校验码byte[]CRC
        public static bool CalculateCRCHelper(byte[] SendData, int nLen, ref byte[] CRC)
        {
            Int32[] tBuf = new Int32[nLen];
            Int32 CRCLo = 0xff;
            Int32 CRCHi = 0xff;
            Int32 CL = 0x01;
            Int32 CH = 0xa0;
            Int32 saveHi = 0;
            Int32 saveLo = 0;

            for (int j = 0; j < nLen; j++)
            {
                tBuf[j] = SendData[j] & 0xFF;
                CRCLo ^= tBuf[j];
                for (int i = 0; i < 8; i++)
                {
                    saveHi = CRCHi;
                    saveLo = CRCLo;
                    CRCHi = (CRCHi / 2);
                    CRCLo = (CRCLo / 2);
                    if ((saveHi & 0x01) == 1)
                        CRCLo |= 0x80;
                    if ((saveLo & 0x01) == 1)
                    {
                        CRCHi ^= CH;
                        CRCLo ^= CL;
                    }
                }
            }
            CRC[0] = (byte)CRCLo;
            CRC[1] = (byte)CRCHi;
            return true;
        }
    }

}
