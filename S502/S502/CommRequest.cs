using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace S502
{
    public class RequestItem
    {
        // 指令发送时字节之间的发送间隔
        public int Elapse { get; set; }

        public RequestItem(string requestContent, bool withCrc)
        {
            //_data = System.Text.Encoding.Default.GetBytes(requestContent);
            _requestString = requestContent;
            if (withCrc)
            {
                var temp = HexStringToToHexBytes(requestContent);
                _data = new byte[temp.Length + 2];
                temp.CopyTo(_data, 0);

                byte[] crcTmp = new byte[2];
                if (CommProtocol.CalculateCRCHelper(temp, temp.Length, ref crcTmp))
                {
                    _data[temp.Length] = crcTmp[0];
                    _data[temp.Length + 1] = crcTmp[1];
                }
            }
            else
            {
                _data = HexStringToToHexBytes(requestContent);
            }
        }

        public RequestItem(byte[] requestContent, bool withCrc)
        {
            _data = new byte[requestContent.Length + 2];
            requestContent.CopyTo(_data, 0);

            byte[] crcTmp = new byte[2];
            if (CommProtocol.CalculateCRCHelper(requestContent, requestContent.Length, ref crcTmp))
            {
                _data[requestContent.Length] = crcTmp[0];
                _data[requestContent.Length + 1] = crcTmp[1];
            }
        }

        private BaseResponse _response;

        public bool SuccessResponsed { get; set; }

        public BaseResponse Response
        {
            get { return _response; }
            set
            {
                _response = value;

            }
        }

        public void SetTargetResponse(BaseResponse response)
        {
            _response = response;

        }

        private static byte[] HexStringToToHexBytes(string hexString)
        {
            hexString = hexString.Replace(" ", "").Replace("0x", "").Replace("0X", "");

            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }


        private readonly byte[] _data;
        public byte[] Data
        {
            get { return _data; }
        }

        private readonly string _requestString;
        /// <summary>
        /// 命令字符串
        /// </summary>
        public string RequestString { get { return _requestString; } }

        // 用于验证响应的有效性
        public delegate bool ValidateResponse(byte[] response);

        // 提供验证respond有效性的方法
        public event ValidateResponse ValidateResponseHandler;

        // 这里提供一个给外界调用的接口，因为在外部不能通过request.ValidateResponseHandler来直接调用绑定的方法
        /// <summary>
        /// 对接收到的response进行验证
        /// </summary>
        /// <param name="response"></param>
        public bool OnValidateResponse(byte[] response)
        {
            var ret = ValidateResponseHandler?.Invoke(response);
            if (ret.HasValue)
            {
                //OnAfterResponseValidated(null);
                // 结果正确
                SuccessResponsed = true;
                return ret.Value;
            }
            return false;
        }

        // respond有效后进行的工作
        public event EventHandler<EventArgs> AfterResponseValidatedHandler;

        /// <summary>
        /// 当响应有效后进行后续操作
        /// </summary>
        /// <param name="e"></param>
        public void OnAfterResponseValidated(EventArgs e)
        {
            AfterResponseValidatedHandler?.Invoke(this, e);
        }
    }
}
