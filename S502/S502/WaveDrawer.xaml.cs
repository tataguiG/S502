using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace S502
{
    /// <summary>
    /// WaveDrawer.xaml 的交互逻辑
    /// </summary>
    public partial class WaveDrawer : UserControl
    {
        /// <summary>
        /// 定义曲线以及对应控制区域的基本信息，颜色、
        /// </summary>
        internal class LineProperties
        {
            public Color LineColor { get; set; }
            public Color TextColor { get; set; }
            public string LinenName { get; set; }
            public int StrokeThickness { get; set; }
        }

        private Dictionary<string, LineProperties> _linePropertyDictionary = new Dictionary<string, LineProperties>();
        public void LoadLineProperties()
        {
            var lineName = "C+1-";
            _linePropertyDictionary.Add(lineName, new LineProperties()
            {
                LineColor = Color.FromRgb(208, 65, 82),
                TextColor = Color.FromRgb(255, 255, 255),
                LinenName = lineName,
                StrokeThickness = 1
            });

            lineName = "C+2-";
            _linePropertyDictionary.Add(lineName, new LineProperties()
            {
                LineColor = Color.FromRgb(255, 134, 76),
                TextColor = Color.FromRgb(255, 255, 255),
                LinenName = lineName,
                StrokeThickness = 1
            });

            lineName = "C+3-";
            _linePropertyDictionary.Add(lineName, new LineProperties()
            {
                LineColor = Color.FromRgb(253, 190, 62),
                TextColor = Color.FromRgb(255, 255, 255),
                LinenName = lineName,
                StrokeThickness = 1
            });

            lineName = "C+4-";
            _linePropertyDictionary.Add(lineName, new LineProperties()
            {
                LineColor = Color.FromRgb(150, 179, 106),
                TextColor = Color.FromRgb(255, 255, 255),
                LinenName = lineName,
                StrokeThickness = 1
            });

            lineName = "C+5-";
            _linePropertyDictionary.Add(lineName, new LineProperties()
            {
                LineColor = Color.FromRgb(13, 164, 165),
                TextColor = Color.FromRgb(255, 255, 255),
                LinenName = lineName,
                StrokeThickness = 1
            });

            lineName = "C+6-";
            _linePropertyDictionary.Add(lineName, new LineProperties()
            {
                LineColor = Color.FromRgb(74, 144, 226),
                TextColor = Color.FromRgb(255, 255, 255),
                LinenName = lineName,
                StrokeThickness = 1
            });

            lineName = "C+7-";
            _linePropertyDictionary.Add(lineName, new LineProperties()
            {
                LineColor = Color.FromRgb(132, 67, 202),
                TextColor = Color.FromRgb(255, 255, 255),
                LinenName = lineName,
                StrokeThickness = 1
            });

            lineName = "C+8-";
            _linePropertyDictionary.Add(lineName, new LineProperties()
            {
                LineColor = Color.FromRgb(139, 87, 42),
                TextColor = Color.FromRgb(255, 255, 255),
                LinenName = lineName,
                StrokeThickness = 1
            });
        }


        internal class CellInfo
        {
            public int CellWidth { get; set; }
            public Brush CellBrush { get; set; }
        }
        private Window _baseWindow;

        private int _originOffset = 20;

        private Point _originPoint;

        private int _actualDrawWidth;
        private int _actualDrawHeight;

        private bool _isShowed;

        // 记录UI元素
        private List<TextBlock> _axisXUnitTextBlockList = new List<TextBlock>();
        private List<TextBlock> _axisYUnitTextBlockList = new List<TextBlock>();

        private List<Line> _backgroundXLines = new List<Line>();
        private List<Line> _backgroundYLines = new List<Line>();

        private Line _axisXLine = null;
        private Line _axisYLine = null;

        // 扫屏模式下范围起始位置
        private int _currentPosition = 0;

        private Dictionary<string, PolylineItem> _lines = new Dictionary<string, PolylineItem>();

        public void AddPoint(string lineName, DataPoint[] points)
        {
            var line = _lines[lineName];
        }

        public void AddLine(string lineName)
        {
            if (!_lines.ContainsKey(lineName))
            {
                var polylineItem = new PolylineItem()
                {
                    Visible = true,
                    LineName = lineName
                };
                polylineItem.Initialize(_actualDrawWidth, _sampleRate);

                _lines.Add(lineName, polylineItem);
            }
        }


        //// 绘制当前图形所接收到的全部数据点（存在抽样的可能）, 当曲线刷新后（即显示数据点已经被覆盖，对应的数据也应清除）
        //// 应该使用时间来作为清除标识
        //// 第一维度对应每个可现实的像素点, 第二维度的大小取决于_sampleRate
        //private DataPoint[][] _actualReservedDataPointsBuffer = null;

        private int _sampleRate;

        // Y轴表示的范围
        private int _maxValueOfAxisY;
        private int _minValueOfAxisY;
        //private double _ratioOfAxisY;

        // X轴表示的范围
        private int _maxValueOfAxisX;

        private int _minValueOfAxisX;

        private bool _showEventTag = true;
        public bool ShowEventTag
        {
            get { return _showEventTag; }
            set
            {
                foreach (var eventPoint in _eventPoints)
                {

                    eventPoint.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                }
                _showEventTag = value;
            }
        }

        public int DrawerHeight
        {
            get
            {
                //var baseWindow = Window.GetWindow(BasePart);
                return _actualDrawHeight;
            }
            private set { }
        }
        public int DrawerWidth
        {
            get
            {
                //var baseWindow = Window.GetWindow(BasePart);
                return _actualDrawWidth;
            }
            private set { }
        }

        public int DrawerLeft
        {
            get
            {
                //var baseWindow = Window.GetWindow(BasePart);
                return (int)_originPoint.X;
            }
            private set { }
        }


        public Point OriginPoint
        {
            get { return _originPoint; }
            private set { }
        }

        private CellInfo _cellInfo = new CellInfo()
        {
            CellWidth = 30,
            CellBrush = Brushes.Coral
        };

        // X轴表示时间ms为单位
        public int ActualMilliSecondsPerXUnit { get; set; }
        public int XAdjustStep { get; set; }
        // Hz数
        public int ActualReceivedCountPerSecond { get; set; }


        public int YUnitValue { get; set; }
        public int YAdjustStep { get; set; }

        public int CountPerVisualXUnit { get; set; }

        // 点选曲线高亮显示后可以显示具体数值
        private bool _highlightPolyline = false;

        public bool HighlightPolyline
        {
            get { return _highlightPolyline; }
        }

        private PolylineItem _currentSelectPolylineItem = null;

        public WaveDrawer()
        {
            InitializeComponent();

            BasePart.MouseWheel += BasePartOnMouseWheel;
            ShowEventTag = CheckEventTag.IsChecked.GetValueOrDefault(true);

            BasePart.MouseMove += BasePartOnMouseMove;

            LoadLineProperties();
        }

        private void BasePartOnMouseMove(object o, MouseEventArgs mouseEventArgs)
        {
            if (HighlightPolyline)
            {
                var pos = mouseEventArgs.GetPosition(_currentSelectPolylineItem.line);
                int index = (int) (pos.X - _originPoint.X);

                var point = _currentSelectPolylineItem.GetPoint(index);
                if (point == null)
                    return;

                YValue.Content = "" + point.RawData;
                XValue.Content = $"{(double)(index * (_maxValueOfAxisX - _minValueOfAxisX)) / _actualDrawWidth:F2}";

            }
        }

        private void BasePartOnMouseWheel(object o, MouseWheelEventArgs mouseWheelEventArgs)
        {
            if (mouseWheelEventArgs.Delta > 0)
            {
                AdjustX(true);
            }
            else if (mouseWheelEventArgs.Delta < 0)
            {
                AdjustX(false);
            }

            mouseWheelEventArgs.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xUnit"></param>
        /// <param name="actualSampleRate">当前显示的波形的数据点的采样率</param>
        /// <param name="xAdjustStep">X轴调整下单次调整的步长（鼠标滚轮发生调整）</param>
        /// <param name="yAdjustStep">Y轴调整下单次调整的步长</param>
        public void InitializeParameters(int xUnit, int xAdjustStep, int actualSampleRate, int yUnitValue, int yAdjustStep)
        {
            ActualMilliSecondsPerXUnit = xUnit;
            ActualReceivedCountPerSecond = actualSampleRate;
            XAdjustStep = xAdjustStep;

            YUnitValue = yUnitValue;
            YAdjustStep = yAdjustStep;

            CountPerVisualXUnit = _cellInfo.CellWidth;

            _baseWindow = Window.GetWindow(BasePart);
            
            // 实际的显示宽度，或者说是像素点数量
            _actualDrawWidth = (((int)BasePart.ActualWidth - 20) / _cellInfo.CellWidth) * _cellInfo.CellWidth;
            _actualDrawHeight = (((int)BasePart.ActualHeight - 20) / _cellInfo.CellWidth) *_cellInfo.CellWidth;

            // 原点坐标，左下角对应（0，0），则原点设置为（20，20）
            _originPoint.X = _originOffset;
            _originPoint.Y = _actualDrawHeight + _originOffset;

            CaculateSampleRate();

            _isShowed = true;
        }

        public void InitializeParametersWithTimeRange(int totalSeconds, int actualSampleRate, int yUnitValue, int yAdjustStep)
        {
            ActualReceivedCountPerSecond = actualSampleRate;

            YUnitValue = yUnitValue;
            YAdjustStep = yAdjustStep;

            CountPerVisualXUnit = _cellInfo.CellWidth;

            _baseWindow = Window.GetWindow(BasePart);

            //var xUnitCnt = (int) ((BasePart.ActualWidth - 20) / _cellInfo.CellWidth);
            var xUnitCnt = 30;
            _actualDrawWidth = xUnitCnt * _cellInfo.CellWidth;
            

            // 实际的显示宽度，或者说是像素点数量
            //_actualDrawWidth = (((int)BasePart.ActualWidth - 20) / _cellInfo.CellWidth) * _cellInfo.CellWidth;
            _actualDrawHeight = (((int)BasePart.ActualHeight - 20) / _cellInfo.CellWidth) * _cellInfo.CellWidth;

            // 原点坐标，左下角对应（0，0），则原点设置为（20，20）
            _originPoint.X = _originOffset;
            _originPoint.Y = _actualDrawHeight + _originOffset;

            XAdjustStep = totalSeconds / xUnitCnt;
            ActualMilliSecondsPerXUnit = XAdjustStep;

            CaculateSampleRate();

            _isShowed = true;

        }

        public void ClearBaseDraw()
        {
            BasePart.Children.Clear();
        }

        // 调整X轴，同时调整当前显示
        public void AdjustX(bool increase)
        {
            if (increase)
            {
                ActualMilliSecondsPerXUnit += XAdjustStep;
            }
            else
            {
                if (ActualMilliSecondsPerXUnit - XAdjustStep > 0)
                    ActualMilliSecondsPerXUnit -= XAdjustStep;
                else
                {
                    return;
                }
            }

            CaculateSampleRate();
            DrawAxisXUnit(ActualMilliSecondsPerXUnit, ActualMilliSecondsPerXUnit);

            foreach (var polylineItem in _lines)
            {
                polylineItem.Value.AdjustShowRange(_sampleRate);
            }
                //GenerateShowableDataPointBuffer();
            ShowPointsInBuffer();
        }

        public void AdjustY(bool increase)
        {
            if (increase)
            {
                YUnitValue += YAdjustStep;
            }
            else
            {
                if (YUnitValue - YAdjustStep > 0)
                    YUnitValue -= YAdjustStep;
                else
                {
                    return;
                }
            }
            DrawAxisYUnit(YUnitValue, YUnitValue);

            ShowPointsInBuffer();

        }

        // 每单元格（X轴）中能够显示的数据点数量，取决于像素点数
        public int PointCountPerVisualXunit { get; set; }
        // 每单元格

        /// <summary>
        /// 计算采样率
        /// </summary>
        /// <param name="countPerVisualXUnit">可视界面下X单位长度中可绘制的点数（对应像素点）</param>
        /// <param name="actualMilliSecondsPerXUnit">X轴每单元格所表示的实际时长</param>
        /// <param name="actualReceivedCountPerSecond">实际上每秒接收的数据点数</param> 对应Hz值
        private void CaculateSampleRate(/*int countPerVisualXUnit, int actualMilliSecondsPerXUnit, int actualReceivedCountPerSecond*/)
        {
            // 每小格30个点
            // 每小格50ms，实际每秒传输4800个数据（对应9600波特率，每两个字节表示一个数据点)
            // 为了显示，则对应采样率为：
            // 4800 / 20 = 240 -> 240 / 30 = 8, 即每8个点抽一个数据点用于显示

            _sampleRate = (ActualReceivedCountPerSecond * ActualMilliSecondsPerXUnit) / (1000 * CountPerVisualXUnit);
        }

        #region Background
        public void DrawAxises()
        {
            if (!_isShowed)
            {
                throw new Exception("没有初始化");
                //InitializeParameters();    
            }

            // 原点
            int offsetX = (int)_originPoint.X, 
                offsetY = (int)_originPoint.Y;

            // X轴
            _axisXLine = new Line()
            {
                X1 = offsetX,
                Y1 = offsetY,
                X2 = offsetX + DrawerWidth,
                Y2 = offsetY,
                Stroke = _cellInfo.CellBrush,
                StrokeThickness = 1
            };
            BasePart.Children.Add(_axisXLine);

            // Y轴
            _axisYLine = new Line()
            {
                X1 = offsetX,
                Y1 = offsetY,
                X2 = offsetX,
                Y2 = offsetY - DrawerHeight,
                Stroke = _cellInfo.CellBrush,
                StrokeThickness = 1
            };
            BasePart.Children.Add(_axisYLine);

            // 坐标内横线
            
            // 纵向vertical line
            offsetX = (int) _originPoint.X + _cellInfo.CellWidth - GetGlobalOffset(); /*_globalOffset;*/
            offsetY = (int)_originPoint.Y;

            while (offsetX <= DrawerWidth + _originOffset)
            {
                if (offsetX > _originPoint.X)
                {
                    var tmp = new Line()
                    {
                        X1 = offsetX,
                        Y1 = offsetY,
                        X2 = offsetX,
                        Y2 = offsetY - DrawerHeight,
                        Stroke = _cellInfo.CellBrush,
                        StrokeThickness = 0.3
                    };
                    var index = BasePart.Children.Add(tmp);
                    _backgroundYLines.Add(tmp);
                }

                offsetX += _cellInfo.CellWidth;
            }
            DrawAxisXUnit(ActualMilliSecondsPerXUnit, ActualMilliSecondsPerXUnit);


            // 水平horizontal line
            offsetX = (int)_originPoint.X;
            offsetY = (int)_originPoint.Y - _cellInfo.CellWidth;

            while (offsetY - _originOffset >= 0)
            {
                var tmp = new Line()
                {
                    X1 = offsetX,
                    Y1 = offsetY,
                    X2 = offsetX + DrawerWidth,
                    Y2 = offsetY,
                    Stroke = _cellInfo.CellBrush,
                    StrokeThickness = 0.3
                };
                var index = BasePart.Children.Add(tmp);
                //_backgroundXLines.Add(new ComponentItem<Line>(index, tmp));
                _backgroundXLines.Add(tmp);

                offsetY -= _cellInfo.CellWidth;
            }
            DrawAxisYUnit(YUnitValue, YUnitValue);
            
        }


        /// <summary>
        /// 绘制坐标轴单元格坐标
        /// </summary>
        /// <param name="firstUnitValue">第一个坐标的起始位置</param>
        /// <param name="step"></param>
        /// <param name="axisOYffset">纵轴偏移</param>
        public void DrawAxisXUnit(int firstUnitValue, int step/*, int axisYOffset = 0*/)
        {
            _minValueOfAxisX = firstUnitValue - step;

            foreach (var t in _axisXUnitTextBlockList)
            {
                BasePart.Children.Remove(t);
            }
            _axisXUnitTextBlockList.Clear();

            // 绘制时使用
            // offsetX初始为第一条线的X坐标
            int offsetX = (int) _originPoint.X + _cellInfo.CellWidth - GetGlobalOffset();/*_globalOffset;*/
            int offsetY = (int)_originPoint.Y;

            double unitValue = 0.0;

            while (offsetX <= DrawerWidth + _originOffset)
            {
                unitValue = (double)firstUnitValue / 1000;
                // 只显示Y轴右侧区域
                if (offsetX >= (int) _originPoint.X)
                {
                    var unitValueTextBlock = new TextBlock()
                    {
                        Text = "" + unitValue
                    };

                    Canvas.SetLeft(unitValueTextBlock, offsetX - 5);
                    Canvas.SetTop(unitValueTextBlock, offsetY + 10);

                    int index = BasePart.Children.Add(unitValueTextBlock);

                    _axisXUnitTextBlockList.Add(unitValueTextBlock);
                }

                firstUnitValue += step;
                offsetX += _cellInfo.CellWidth;
            }
            _maxValueOfAxisX = (int)unitValue;
        }

        public void DrawAxisYUnit(int firstUnitValue, int step)
        {
            foreach (var t in _axisYUnitTextBlockList)
            {
                //BasePart.Children.RemoveAt(t.Index);
                BasePart.Children.Remove(t);
            }
            _axisYUnitTextBlockList.Clear();

            // 水平horizontal line
           int offsetX = (int) _originPoint.X;
           int offsetY = (int) _originPoint.Y - _cellInfo.CellWidth;

            int unitValue = firstUnitValue;
            while (offsetY - _originOffset >= 0)
            {
                var unitValueTextBlock = new TextBlock()
                {
                    Text = "" + unitValue
                };
                unitValue += step;

                Canvas.SetLeft(unitValueTextBlock, offsetX - 20); // 5, 20为留出显示坐标值的空间
                Canvas.SetTop(unitValueTextBlock, offsetY - 5);

                int index = BasePart.Children.Add(unitValueTextBlock);

                //_axisYUnitTextBlockList.Add(new ComponentItem<TextBlock>(index, unitValueTextBlock)); ;
                _axisYUnitTextBlockList.Add(unitValueTextBlock);

                offsetY -= _cellInfo.CellWidth;
            }

            // 设置显示范围
            SetAxisYRange(firstUnitValue - step, unitValue - step);
        }

        // 调整Y轴显示范围
        private void SetAxisYRange(int min, int max)
        {
            _minValueOfAxisY = min;
            _maxValueOfAxisY = max;

            //_ratioOfAxisY = (double)(max - min) / max;
        }

        #endregion

        // 扫屏线
        //private ComponentItem<Line> _screenLine = null;
        private Line _screenLine = null;


        List<Image> _eventPoints = new List<Image>();

        private void ClearLines()
        {
            foreach (var polylineItem in _lines)
            {
                BasePart.Children.Remove(polylineItem.Value.line);
            }
            //BasePart.Children.Remove(_screenLine);

            foreach (var componentItem in _eventPoints)
            {
                BasePart.Children.Remove(componentItem);
            }
        }

        //滚屏模式
        private bool _autoMoveScreen = false;

        public void AddPointsForAutoMoveScreen(string lineName, DataPoint[] data)
        {
            if (_lines.ContainsKey(lineName))
            {
                _lines[lineName].AddPointsForAutoMoveScreen(data);
            }
        }

        private bool _startRecvData = false;

        public void AddPoints(string lineName, DataPoint[] data)
        {
            if (!_startRecvData)
            {
                RefreshTimeRange();
                _startRecvData = true;
            }
            
            if (_lines.ContainsKey(lineName))
            {
                _lines[lineName].AddPoints(data);
            }
        }

          public void AddAndShowPoints(string lineName, DataPoint[] data)
        {
            if (_autoMoveScreen)
                AddPointsForAutoMoveScreen(lineName, data);
            else
                AddPoints(lineName, data);

            ShowPointsInBuffer();
        }

        public void ShowPointsInBuffer()
        {
            //foreach (var backgroundYLine in _backgroundYLines)
            //{
            //    BasePart.Children.Remove(backgroundYLine);
            //}

            //DrawAxises();
            DrawLine();
            //DrawScreenLine();
        }

        /// <summary>
        /// 绘制数据点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private Point DrawPointHelper(DataPoint x, int position)
        {
            //_currentPosition %= _actualDrawWidth;
            //if (_currentPosition == 0)
            //    _wave.Points = null;

            _currentPosition = position;

            var tmpX = _originPoint.X + _currentPosition;
            _currentPosition++;

            double tmpY = 0.0;

            if (x.RawData - _minValueOfAxisY <= _maxValueOfAxisY)
                tmpY = _originPoint.Y - (double)(x.RawData - _minValueOfAxisY) / _maxValueOfAxisY *
                       _actualDrawHeight;
            // 超过显示范围
            else
                tmpY = _originPoint.Y - _actualDrawHeight;

            Point ret = new Point(tmpX, tmpY);

            //// 绘制事件点的描述信息
            //if (x.DataTag != null && _showEventTag)
            //{
            //    DrawEventPoint(ret, x);
            //}

            return ret;
        }

        // 绘制扫屏线
        //private void DrawScreenLine()
        //{
        //    if (_screenLine != null)
        //        BasePart.Children.Remove(_screenLine);

        //    _screenLine = new Line()
        //    {
        //        X1 = _originPoint.X + _inputOffset,
        //        Y1 = _originPoint.Y - _actualDrawHeight,
        //        X2 = _originPoint.X + _inputOffset,
        //        Y2 = _originPoint.Y,
        //        StrokeThickness = 1,
        //        Stroke = Brushes.Red,
        //    };

        //    var index = BasePart.Children.Add(_screenLine);
        //    //_screenLine = line;
        //}

        /// <summary>
        /// 表示是否冻结显示, true表示冻结，可以点选具体数据点获得峰峰值
        /// </summary>
        public bool Frozen { get; set; }        
        public void Freeze()
        {
            Frozen = true;
        }

        public void Resume()
        {
            Frozen = false;
        }

        // 与坐标原点对应的时间信息
        private DateTime _begDateTime;
        private DateTime _endDateTime;
        private int _millisecondsFromBegToEnd;

        /// <summary>
        /// 设定X轴显示范围
        /// </summary>
        /// <param name="begDateTime"></param>
        /// <param name="totalSeconds"></param>
        public void SetTimeRange(DateTime begDateTime, int totalSeconds)
        {
            _begDateTime = begDateTime;
            _endDateTime = _begDateTime.AddSeconds(totalSeconds);
            _millisecondsFromBegToEnd = totalSeconds * 1000;
        }

        private void RefreshTimeRange()
        {
            _begDateTime = DateTime.Now;
            _millisecondsFromBegToEnd = (_maxValueOfAxisX - _minValueOfAxisX) * 1000;
            _endDateTime = _begDateTime.AddMilliseconds(_millisecondsFromBegToEnd);
        }

        /// <summary>
        /// 根据时标计算对应的位置
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        private int CaculateXPosForEventData(EventData eventData)
        {
            //RefreshTimestamps();
            if (eventData.TimeStamp < _begDateTime || eventData.TimeStamp > _endDateTime)
            {
                return -1;
            }
            else
            {       
                var x = ((eventData.TimeStamp - _begDateTime).TotalMilliseconds * _actualDrawWidth / _millisecondsFromBegToEnd) +
                        _originPoint.X;
                return (int)x;
            }
        }

        /// <summary>
        /// 在当前显示的曲线中选择一条？或者选择某一个
        /// </summary>
        /// <param name="xPos"></param>
        /// <returns></returns>
        private int CaculateAverageYPosForXPos(int xPos)
        {
            var cnt = 0;
            var total = _lines.Sum(line =>
            {
                if (line.Value.Visible)
                {
                    cnt++;

                    // 该事件点所对应的数据点不存在或者已经被“抽样”牺牲掉了，需要在前后范围内找
                    if (line.Value._showableDataPointsBuffer[xPos] == null)
                    {
                        var tmp = xPos;
                        while (line.Value._showableDataPointsBuffer[tmp] == null)
                        {
                            tmp--;
                        }
                        if (tmp > 0)
                            return line.Value._showableDataPointsBuffer[tmp].RawData;
                        else
                        {
                            cnt--;
                            return 0;
                        }
                    }
                    return line.Value._showableDataPointsBuffer[xPos].RawData;
                }
                return 0;
            });
            return (int)(total / (cnt == 0 ? 1 : cnt));
        }

        private List<EventData> _eventDataList = new List<EventData>();

        public void AddEventData(EventData eventData)
        {
            _eventDataList.Add(eventData);
            DrawEventPointHelper(eventData);
        }
        
        
        /// <summary>
        /// 绘制事件点，位置信息来源于计算
        /// </summary>
        ///// <param name="position">事件点显示的位置</param>
        /// <param name="dataPoint">事件内容</param>
        private void DrawEventPointHelper(/*Point position, */EventData eventData)
        {
            //Ellipse eventTag = new Ellipse()
            //{
            //    StrokeThickness = 10,
            //    Stroke = Brushes.DarkSlateGray,
            //    Fill = Brushes.DarkSlateGray,
            //    Height = 10,
            //    Width = 10
            //};

            if (!ShowEventTag)
                return;

            var xPos = CaculateXPosForEventData(eventData);

            if (xPos == -1)
                return;

            var yPos = CaculateAverageYPosForXPos(xPos);

            Image eventTag = new Image();
            eventTag.Source = new BitmapImage(new Uri(@"/Resource/EventPoint.ico", UriKind.Relative));

            eventTag.MouseLeftButtonDown += (sender, args) =>
            {
                EventPointWindow msgWindow = new EventPointWindow();
                msgWindow.Title = "数据点";

                //msgWindow.Description.Text = "Value: " + eventData.RawData;
                msgWindow.Detail.Text = eventData.Description;
                msgWindow.Timestamp.Text = eventData.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss");

                var pos = Mouse.GetPosition(null);
                msgWindow.Left = pos.X;
                msgWindow.Top = pos.Y;
                msgWindow.Show();

                //MessageBox.Show(dataPoint.DataTag.Description);
            };



            Canvas.SetLeft(eventTag, xPos - 5); // 移到线上
            Canvas.SetTop(eventTag, yPos - 5);

            var index = BasePart.Children.Add(eventTag);
            //_eventPoints.Add(new ComponentItem<Ellipse>(index, eventTag));
            _eventPoints.Add(eventTag);
            
        }

        private int GetGlobalOffset()
        {
            if (_lines.Count == 0)
                return 0;
            return _lines.First().Value._globalOffset;
        }

        /// <summary>
        /// 绘制曲线， 采用扫屏的方式
        /// </summary>
        private void DrawLine()
        {
            foreach (var polylineItem in _lines)
            {
                if (polylineItem.Value.Visible)
                {
                    var wave = polylineItem.Value.line;
                    DrawLineHelper(polylineItem.Value);
                }
            }
        }

        private UIElement CreateLineControlBorder(LineProperties prop)
        {
            Border border = new Border()
            {
                CornerRadius = new CornerRadius(5),
                //Name = prop.LinenName,
                Width = double.NaN,
                Height = double.NaN,
                Margin = new Thickness(0, 0, 5, 0),
                Background = new SolidColorBrush(prop.LineColor)
            };

            border.Child = new Label()
            {
                Content = prop.LinenName,
                Foreground = new SolidColorBrush(prop.TextColor),
            };

            return border;
        }

        private void DrawLineHelper(PolylineItem lineItem)
        {
            if (lineItem.line == null)
            {
                var prop = _linePropertyDictionary[lineItem.LineName];
                lineItem.line = new Polyline()
                {
                    Stroke = new SolidColorBrush(prop.LineColor),
                    StrokeThickness = prop.StrokeThickness

                };

                Border lineControl = (Border)CreateLineControlBorder(prop);
                lineControl.MouseLeftButtonDown += (sender, args) =>
                {
                    if (lineItem.Visible)
                    {
                        //var tmp = (Border) lineControl;
                        //var label = (Label) tmp.Child;
                        lineControl.Background = Brushes.DarkGray;
                        lineItem.Visible = false;
                        lineItem.line.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        //var tmp = (Border)lineControl;
                        //var label = (Label)tmp.Child;
                        lineControl.Background = new SolidColorBrush(prop.LineColor);
                        lineItem.Visible = true;
                        lineItem.line.Visibility = Visibility.Visible;
                    }
                };


                LinesStackPanel.Children.Add(lineControl);

                // 判断双击
                int mouseLeftClickCnt = 0;
                DateTime lastClickTime = DateTime.Now;
                // 标记高亮
                //bool highlightPolyline = false;
                lineItem.line.MouseLeftButtonDown += (sender, args) =>
                {
                    if (mouseLeftClickCnt == 0)
                        lastClickTime = DateTime.Now;

                    if (mouseLeftClickCnt > 0 && mouseLeftClickCnt % 2 == 0 && (DateTime.Now - lastClickTime).TotalMilliseconds < 500)
                    {
                        var pos = args.GetPosition(lineItem.line);
                        var point = lineItem._showableDataPointsBuffer[(int)(pos.X - _originPoint.X)];

                        EventPointWindow tmpWindowForPoint = new EventPointWindow();

                        tmpWindowForPoint.LoadEventData(null);

                        // GetPosition参数为null，则默认取全局的绝对坐标（显示屏幕），直接画点即可
                        var tmpPos = Mouse.GetPosition(null);
                        tmpWindowForPoint.Left = tmpPos.X;
                        tmpWindowForPoint.Top = tmpPos.Y;

                        if (tmpWindowForPoint.ShowDialog().Value)
                        {
                            DrawEventPointHelper(tmpWindowForPoint.CurrentData);
                        }
                    }
                    else
                    {
                        lastClickTime = DateTime.Now;

                        if (!_highlightPolyline)
                        {
                            _highlightPolyline = true;
                            lineItem.line.Stroke = Brushes.Aqua;
                            lineItem.line.StrokeThickness = 1.5;

                            var pos = args.GetPosition(lineItem.line);
                            var point = lineItem._showableDataPointsBuffer[(int)pos.X];

                            _currentSelectPolylineItem = lineItem;
                        }
                        else
                        {
                            lineItem.line.Stroke = new SolidColorBrush(prop.LineColor);
                            lineItem.line.StrokeThickness = prop.StrokeThickness;
                        }
                    }

                    mouseLeftClickCnt++;
                };

                ////鼠标碰线
                //_wave.MouseEnter += (sender, args) =>
                //{
                //    _wave.Stroke = Brushes.Aqua;
                //    _wave.StrokeThickness = 1.5;

                //    var pos = args.GetPosition(_wave);
                //    var point = _showableDataPointsBuffer[(int) pos.X];
                //};

                //// 鼠标离开
                //_wave.MouseLeave += (sender, args) =>
                //{
                //    //if (_showPointWindow == false)
                //    //{
                //    _wave.Stroke = Brushes.Black;
                //    _wave.StrokeThickness = 1;

                //    //_tmpWindowForPoint.Hide();
                //    //_tmpWindowForPoint.Visibility = Visibility.Hidden;
                //    //}
                //};



                //_wave.MouseMove += (sender, args) =>
                //{
                //    var pos = args.GetPosition(BasePart);
                //    var point = _showableDataPointsBuffer[(int) (pos.X - _originPoint.X)];

                //    _tmpWindowForPoint.LoadDataPoint(point);
                //};


                var index = BasePart.Children.Add(lineItem.line);
                //_wave = new ComponentItem<Polyline>(index, polyLine);
                //_wave = polyLine;
            }

            // 全屏刷新时每次显示全部的buffer数据点内容
            // 删除事件点
            foreach (var eventPoint in _eventPoints)
            {
                BasePart.Children.Remove(eventPoint);
            }
            _eventPoints.Clear();

            // 画线
            lineItem._displayOffset = 0;
            var tmpPoints = new PointCollection();
            foreach (var dataPoint in lineItem._showableDataPointsBuffer)
            {
                if (dataPoint != null)
                {
                    tmpPoints.Add(DrawPointHelper(dataPoint, lineItem._displayOffset++));
                }

            }
            lineItem.line.Points = tmpPoints;

        }

        // 显示曲线
        private void ShowEventTag_OnChecked(object sender, RoutedEventArgs e)
        {
            ShowEventTag = !ShowEventTag;
        }


        ///// <summary>
        ///// 绘制曲线， 采用扫屏的方式
        ///// </summary>
        //private void DrawLine()
        //{
        //    if (_wave == null)
        //    {
        //        _wave = new Polyline()
        //        {
        //            Stroke = Brushes.Black,
        //            StrokeThickness = 1,

        //        };

        //        // 判断双击
        //        int mouseLeftClickCnt = 0;
        //        DateTime lastClickTime = DateTime.Now;
        //        // 标记高亮
        //        //bool highlightPolyline = false;
        //        _wave.MouseLeftButtonDown += (sender, args) =>
        //        {
        //            if (mouseLeftClickCnt == 0)
        //                lastClickTime = DateTime.Now;

        //            if (mouseLeftClickCnt > 0 && mouseLeftClickCnt % 2 == 0 && (DateTime.Now - lastClickTime).TotalMilliseconds < 500)
        //            {

        //                var pos = args.GetPosition(_wave);
        //                var point = _showableDataPointsBuffer[(int) (pos.X - _originPoint.X)];

        //                EventPointWindow tmpWindowForPoint = new EventPointWindow();

        //                tmpWindowForPoint.LoadDataPoint(point);

        //                // GetPosition参数为null，则默认取全局的绝对坐标（显示屏幕），直接画点即可
        //                var tmpPos = Mouse.GetPosition(null);
        //                tmpWindowForPoint.Left = tmpPos.X;
        //                tmpWindowForPoint.Top = tmpPos.Y;

        //                if (tmpWindowForPoint.ShowDialog().Value)
        //                {
        //                    DrawEventPoint(new Point(pos.X, pos.Y), point);
        //                }
        //            }
        //            else
        //            {
        //                lastClickTime = DateTime.Now;

        //                if (!_highlightPolyline)
        //                {
        //                    _highlightPolyline = true;
        //                    _wave.Stroke = Brushes.Aqua;
        //                    _wave.StrokeThickness = 1.5;

        //                    var pos = args.GetPosition(_wave);
        //                    var point = _showableDataPointsBuffer[(int) pos.X];
        //                }
        //                else
        //                {
        //                    _wave.Stroke = Brushes.Black;
        //                    _wave.StrokeThickness = 1;
        //                }
        //            }

        //            mouseLeftClickCnt++;
        //        };

        //        ////鼠标碰线
        //        //_wave.MouseEnter += (sender, args) =>
        //        //{
        //        //    _wave.Stroke = Brushes.Aqua;
        //        //    _wave.StrokeThickness = 1.5;

        //        //    var pos = args.GetPosition(_wave);
        //        //    var point = _showableDataPointsBuffer[(int) pos.X];
        //        //};

        //        //// 鼠标离开
        //        //_wave.MouseLeave += (sender, args) =>
        //        //{
        //        //    //if (_showPointWindow == false)
        //        //    //{
        //        //    _wave.Stroke = Brushes.Black;
        //        //    _wave.StrokeThickness = 1;

        //        //    //_tmpWindowForPoint.Hide();
        //        //    //_tmpWindowForPoint.Visibility = Visibility.Hidden;
        //        //    //}
        //        //};



        //        //_wave.MouseMove += (sender, args) =>
        //        //{
        //        //    var pos = args.GetPosition(BasePart);
        //        //    var point = _showableDataPointsBuffer[(int) (pos.X - _originPoint.X)];

        //        //    _tmpWindowForPoint.LoadDataPoint(point);
        //        //};


        //        var index = BasePart.Children.Add(_wave);
        //        //_wave = new ComponentItem<Polyline>(index, polyLine);
        //        //_wave = polyLine;
        //    }

        //    // 全屏刷新时每次显示全部的buffer数据点内容
        //    // 删除事件点
        //    foreach (var eventPoint in _eventPoints)
        //    {
        //        BasePart.Children.Remove(eventPoint);
        //    }
        //    _eventPoints.Clear();

        //    // 画线
        //    _displayOffset = 0;
        //    var tmpPoints = new PointCollection();
        //    foreach (var dataPoint in _showableDataPointsBuffer)
        //    {
        //        if (dataPoint != null)
        //        {
        //            tmpPoints.Add(DrawPointHelper(dataPoint, _displayOffset++));
        //        }

        //    }
        //    _wave.Points = tmpPoints;
        //}


        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
