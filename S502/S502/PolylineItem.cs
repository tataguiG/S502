using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace S502
{
    public class PolylineItem
    {
        private int _showableBufferLength = 0;
        private int _currentSampleRate = 0;

        public bool Visible { get; set; }

        public Polyline line = null;

        public string LineName { get; set; }

        //public Polyline CreateNewLine()
        //{
        //    Polyline ret = line; 
        //    line = new Polyline() { }
        //}

        // 绘制当前图形所接收到的全部数据点（存在抽样的可能）, 当曲线刷新后（即显示数据点已经被覆盖，对应的数据也应清除）
        // 应该使用时间来作为清除标识
        // 第一维度对应每个可现实的像素点, 第二维度的大小取决于_sampleRate
        private DataPoint[][] _actualReservedDataPointsBuffer = null;

        private object _dataAccessLocker = new object();

        // 待绘制点的数据 
        public DataPoint[] _showableDataPointsBuffer = null;
        public int _inputOffset;
        public int _displayOffset;

        // 本来应该全局保留一份，但对于多条线的情况分别增加数据点，用于移动背景网格线和坐标
        public int _globalOffset;
        /// <summary>
        /// 根据相对坐标获取数据点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DataPoint GetPoint(int index)
        {
            lock (_dataAccessLocker)
            {
                if (index < 0)
                    return null;

                if (index > _showableDataPointsBuffer.Length)
                    index = _showableDataPointsBuffer.Length - 1;

                var point = _showableDataPointsBuffer[index];
                return point;
            }
        }

        public void Initialize(int showableBufferLength, int sampleRate)
        {
            // 控件中可以绘制的点数
            _showableDataPointsBuffer = new DataPoint[showableBufferLength];

            // 设置数据buffer
            _actualReservedDataPointsBuffer = new DataPoint[showableBufferLength][];
            for (int i = 0; i < _actualReservedDataPointsBuffer.Length; ++i)
            {
                _actualReservedDataPointsBuffer[i] = new DataPoint[sampleRate > 0 ? sampleRate : 1];
            }

            _showableBufferLength = showableBufferLength;
            _currentSampleRate = sampleRate;
        }

        // 调整抽样率，决定
        public void AdjustShowRange(int sampleRate)
        {
            lock (_dataAccessLocker)
            {
                _currentSampleRate = sampleRate;
                GenerateShowableDataPointBuffer();
            }
        }

        /// <summary>
        /// X轴显示范围调整时曲线发生变化
        /// </summary>
        private void GenerateShowableDataPointBuffer()
        {

            _showableDataPointsBuffer = new DataPoint[_showableBufferLength];

            _inputOffset = 0;

            DataPoint[] allDataPoints =
                new DataPoint[_actualReservedDataPointsBuffer.Length * _actualReservedDataPointsBuffer[0].Length];

            int pos = 0;
            for (int i = 0; i < _actualReservedDataPointsBuffer.Length; ++i)
            {
                if (allDataPoints.Length - pos >= _actualReservedDataPointsBuffer[i].Length)
                {
                    Array.Copy(_actualReservedDataPointsBuffer[i], 0, allDataPoints, pos,
                        _actualReservedDataPointsBuffer[i].Length);
                    pos += _actualReservedDataPointsBuffer[i].Length;
                }
            }

            int j = 0;
            // 抽样
            while (j < allDataPoints.Length)
            {
                // 提取要显示的数据
                _showableDataPointsBuffer[_inputOffset++] = allDataPoints[j];

                j += (_currentSampleRate == 0 ? 1 : _currentSampleRate);

                // 循环缓冲区 
                if (_inputOffset > 0 && _inputOffset % _showableDataPointsBuffer.Length == 0)
                {
                    _inputOffset = 0;
                }
            }

        }

        public void AddPoints(DataPoint[] data)
        {
            lock (_dataAccessLocker)
            {
                int i = 0;

                int minSampleRate = _currentSampleRate == 0 ? 1 : _currentSampleRate;

                // 抽样
                while (i < data.Length)
                {
                    // 当通过滚动鼠标调整采样率后需要更新存储数据点的数组
                    if (_actualReservedDataPointsBuffer[_inputOffset].Length < minSampleRate)
                        _actualReservedDataPointsBuffer[_inputOffset] = new DataPoint[minSampleRate];

                    // 保存所有待显示数据
                    Array.Copy(data, i, _actualReservedDataPointsBuffer[_inputOffset], 0,
                        (i + minSampleRate) > data.Length ? data.Length - i : minSampleRate);

                    // 提取要显示的数据
                    _showableDataPointsBuffer[_inputOffset++] = data[i];

                    i += minSampleRate;

                    // 循环缓冲区 
                    if (_inputOffset > 0 && _inputOffset % _showableDataPointsBuffer.Length == 0)
                    {
                        _inputOffset = 0;
                    }
                    //_globalOffset++;
                }
            }
        }

        // 滚屏模式下添加数据点，对应位置
        public void AddPointsForAutoMoveScreen(DataPoint[] data)
        {
            var tmpRecvPointsBuffer = new DataPoint[1000][];
            var tmpShowBuffer = new DataPoint[1000];

            int i = 0, actualLength = 0;

            // 抽样
            int minSampleRate = _currentSampleRate == 0 ? 1 : _currentSampleRate;
            while (i < data.Length)
            {
                // 当通过滚动鼠标调整采样率后需要更新存储数据点的数组
                //if (tmpRecvPointsBuffer[actualLength].Length < minSampleRate)
                tmpRecvPointsBuffer[actualLength] = new DataPoint[minSampleRate];

                // 保存所有待显示数据
                Array.Copy(data, i, tmpRecvPointsBuffer[actualLength], 0,
                    (i + minSampleRate) > data.Length ? data.Length - i : minSampleRate);

                // 提取要显示的数据
                tmpShowBuffer[actualLength] = data[i];

                i += minSampleRate;
                actualLength++;

                //// 循环缓冲区 
                //if (_inputOffset > 0 && _inputOffset % _showableDataPointsBuffer.Length == 0)
                //{
                //    _inputOffset = 0;
                //}
                //_globalOffset++;
                _globalOffset++;
            }

            // 移动、拷贝
            lock (_dataAccessLocker)
            {
                if (_inputOffset + actualLength > _actualReservedDataPointsBuffer.Length)
                {
                    ArrayMoveHelper(_actualReservedDataPointsBuffer, actualLength);
                    ArrayMoveHelper(_showableDataPointsBuffer, actualLength);
                    _inputOffset -= actualLength;
                }

                Array.Copy(tmpRecvPointsBuffer, 0, _actualReservedDataPointsBuffer, _inputOffset, actualLength);
                Array.Copy(tmpShowBuffer, 0, _showableDataPointsBuffer, _inputOffset, actualLength);

                _inputOffset += actualLength;
            }
        }

        /// <summary>
        /// 数组内容平移操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="srcArray"></param>
        /// <param name="offset"></param>
        private void ArrayMoveHelper<T>(T[] srcArray, int offset)
        {
            if (offset > srcArray.Length || offset < 0)
                return;

            int targetIndex = 0, srcIndex = offset;
            // 移动数据
            for (; srcIndex < srcArray.Length; ++targetIndex, ++srcIndex)
            {
                // 浅拷贝
                srcArray[targetIndex] = srcArray[srcIndex];
            }
        }  
    }
}
