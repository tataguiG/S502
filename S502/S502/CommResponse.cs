using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S502
{
    public class ResponseReceivedEventArgs : EventArgs
    {
        public byte[] ReceivedData { get; set; }
    }

    //public class ResponseItem
    //{
    //    public ResponseItem(ResponseItem rh)
    //    {
    //        Length = rh.Length;
    //        AfterResponseReceivedHandler = rh.AfterResponseReceivedHandler;
    //    }

    //    public ResponseItem() { }

    //    private byte[] _data;
    //    private int _length; // 初始设置的目标长度，用来判断是否接收完整数据
    //    private int _offset;

    //    public int Length
    //    {
    //        get { return _length; }
    //        set
    //        {
    //            if (_length < value && _length > 0)
    //            {
    //                var temp = new byte[value];
    //                _data.CopyTo(temp, 0);
    //                _data = temp;
    //                _length = value;
    //            }
    //            else
    //            {
    //                _length = value;
    //            }
    //        }
    //    }

    //    public byte[] Data
    //    {
    //        get { return _data; }
    //        set
    //        {
    //            if (_data == null)
    //            {
    //                _data = new byte[Length];
    //            }
    //            value.CopyTo(_data, _offset);
    //            _offset += value.Length;
    //            Debug.WriteLine($"{_offset} receive time: {DateTime.Now.Millisecond.ToString()} data: {BitConverter.ToString(value)}");
    //            if (_offset == Length)
    //            {

    //                OnResponseReceived(new ResponseReceivedEventArgs()
    //                {
    //                    ReceivedData = _data
    //                });
    //                Debug.WriteLine($"{_offset} == {Length} complete time: {DateTime.Now.Millisecond.ToString()}");
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 指定长度的响应接收到后所需进行的处理
    //    /// </summary>
    //    public event EventHandler<ResponseReceivedEventArgs> AfterResponseReceivedHandler;

    //    public void OnResponseReceived(ResponseReceivedEventArgs e)
    //    {
    //        AfterResponseReceivedHandler?.Invoke(this, e);
    //    }
    //}

    /// <summary>
    /// 应答消息结构
    /// </summary>
    public abstract class BaseResponse
    {
        public enum RecieveStatus
        {
            Nonstarted,
            Working,
            Finished,
        }
        public RecieveStatus CurrentStatus { get; set; } = RecieveStatus.Nonstarted;


        protected BaseResponse() { }

        //protected BaseResponse(BaseResponse rh)
        //{
        //    Length = rh.Length;
        //    AfterCompleteResponseReceivedHandler = rh.AfterCompleteResponseReceivedHandler;
        //    DataReceiveHandler = rh.DataReceiveHandler;
        //}

        protected byte[] _data;
        protected int _length; // 初始设置的目标长度，用来判断是否接收完整数据
        protected int _offset;


        public int CurrentLength => _data.Length;

        public int Length { get; set; }
        //{
        //    get { return _length; }
        //    set
        //    {
        //        // 长度
        //        if (_length < value && _length > 0)
        //        {
        //            var temp = new byte[value];
        //            _data.CopyTo(temp, 0);
        //            _data = temp;
        //            _length = value;
        //        }
        //        else
        //        {
        //            _length = value;
        //        }
        //    }
        //}


        //public byte[] GetData()
        //{
        //    return _data;
        //}

        public byte[] Data
        {
            get { return _data; }
            set
            {
                _data = new byte[value.Length];
                value.CopyTo(_data, 0);
            }
        }

        public abstract void PushData(byte[] data);

        // 单次收到数据进行处理
        public event EventHandler<byte[]> DataReceiveHandler;
        // 子类不能调用父类事件，只能通过方法来间接调用
        // 父类采用template模式，定义接口
        public virtual void OnDataReceived(byte[] data)
        {
            // 当接收到指定数据后当前Response已经完成使命，不再接收数据
            if (CurrentStatus != RecieveStatus.Finished)
            {
                HandleTimeOut();
                DataReceiveHandler?.Invoke(this, data);
            }               
        }


        public void EnableHandleTimeout(int timeInterval)
        {
            _enableTimeout = true;
            _timeoutInterval = timeInterval;
        }

        private bool _enableTimeout = false;
        private int _timeoutInterval = 0;
        private DateTime _start = DateTime.Now, _end = DateTime.Now;
        private bool _isStart = false;
        private void HandleTimeOut()
        {
            if (!_enableTimeout)
                return;

            if (!_isStart)
            {
                _start = DateTime.Now;
                _isStart = true;
            }
            else
            {
                _end = DateTime.Now;
                if ((_end - _start).TotalMilliseconds > _timeoutInterval)
                {
                    // 接收超时
                    throw new TimeoutException("接收数据超时");
                }
            }
        }

        /// <summary>
        /// 完整应答收到后进行处理（这里的完整指固定长度）指定长度的响应接收到后所需进行的处理
        /// </summary>
        public event EventHandler<ResponseReceivedEventArgs> AfterCompleteResponseReceivedHandler;

        public void OnCompleteResponseReceived(ResponseReceivedEventArgs e)
        {
            AfterCompleteResponseReceivedHandler?.Invoke(this, e);
        }
    }

    /// <summary>
    /// 固定长度应答
    /// </summary>
    public class FixedLengthResponse : BaseResponse
    {
        public FixedLengthResponse(int length)
        {

            Length = length;
        }

        public override void PushData(byte[] recvData)
        {
            OnDataReceived(recvData);

            //recvData.CopyTo(_data, _offset);
            //_offset += recvData.Length;
            //Debug.WriteLine($"{_offset} receive time: {DateTime.Now.Millisecond.ToString()} data: {BitConverter.ToString(recvData)}");
            //if (_offset == Length)
            //{

            //    OnCompleteResponseReceived(new ResponseReceivedEventArgs()
            //    {
            //        ReceivedData = _data
            //    });
            //    Debug.WriteLine($"{_offset} == {Length} complete time: {DateTime.Now.Millisecond.ToString()}");
            //}
        }
    }

    /// <summary>
    /// 非固定长度应答, 需要
    /// </summary>
    public class UnfixedLengthResponse : BaseResponse
    {
        public UnfixedLengthResponse() { }

        public override void PushData(byte[] recvData)
        {
            OnDataReceived(recvData);
        }
    }


}
