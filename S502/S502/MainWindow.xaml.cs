using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;


namespace S502
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private string line1 = "C+1-";
        private string line2 = "C+2-";
        private DataDispose dataDispose = new DataDispose();
        private void button_Click(object sender, RoutedEventArgs e)
        {
            //DataDispose a = new DataDispose();
            //var Comm = new SerialPortController();
            //Comm.Initialize("COM1", 115200, "", 8, 1);
            //Comm.Work();
            //dataDispose.StartWork(Comm, null);

            //return;

           // WaveDrawer.InitializeParameters(1000, 1000, 250, 50, 50);
            WaveDrawer.InitializeParametersWithTimeRange(60000, 250, 50, 50);
            //WaveDrawer.AddLine(line1);
            //WaveDrawer.ShowEventTag = ShowEventTag.IsChecked.GetValueOrDefault(true);

            WaveDrawer.DrawAxises();
            //Plotter.Points.

            double[] data = new double[10];
            PointCollection pc = new PointCollection();
            //LineGraph Plotter = new LineGraph();
            //WavePart.Children.Add(Plotter);

            //Point tp = PointToScreen(new Point(Axises.PlotOriginX, Axises.PlotOriginY));

            //Axises.PlotOriginX
            for (int i = 2; i < 10; i++)
            {
                //data[i] = i;
                pc.Add(new Point(i, i));
                //Plotter.Points.Add(new Point(i, i));
            }
            
            //Plotter.Plot(data, data.Select(x => x*2));

            //Plotter.Points = pc;
            //var com = new SerialPortController();
            //com.Initialize("COM1");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WaveDrawer.AddLine(line1);
            Dispatcher x = Dispatcher.CurrentDispatcher;//取得当前工作线程
            //另开线程工作
            System.Threading.ThreadStart start = () =>
            {
                //工作函数
                var data = new DataPoint[250];

                for (int i = 0; i < 250; i++)
                {
                    data[i] = new DataPoint();
                }

                var begTime = DateTime.Now;

                for (int count = 0; count < 60; ++count)
                {
                    
                    for (int i = 0; i < 250; i++)
                    {
                        data[i].RawData = i;
                    }

                    
                    //异步更新界面
                    x.BeginInvoke(new Action(() =>
                    {
                        WaveDrawer.AddAndShowPoints(line1, data);
                    }), DispatcherPriority.Normal);


                    x.BeginInvoke(new Action(() =>
                    {
                        WaveDrawer.AddEventData(new EventData()
                        {
                            TimeStamp = begTime.AddSeconds(count - 0.5),
                            Description = DateTime.Now.ToLongDateString(),
                            Detail = DateTime.Now.Ticks + ""
                        });
                    }), DispatcherPriority.Normal);
                   

                    Thread.Sleep(1000);
                }

            };

            new System.Threading.Thread(start).Start(); //启动线程

            //Task task = Task.Run(() =>
            //{
            //    for (int count = 0; count < 1000; ++count)
            //    {
            //        var data = new DataPoint[200];
            //        for (int i = 0; i < 200; i++)
            //        {
            //            data[i] = new DataPoint()
            //            {
            //                DataTag = null,
            //                RawData = i
            //            };
            //        }

            //        WaveDrawer.AddPoints(data);

            //        Thread.Sleep(100);
            //    }
            //});

            //task.Wait();

            return;

            //var data = new DataPoint[200];
            //for (int i = 0; i < 200; i++)
            //{
            //    data[i] = new DataPoint()
            //    {
            //        DataTag = null,
            //        RawData = i
            //    };
            //}

            //WaveDrawer.AddPoints(data);
            //WaveDrawer.DrawAxisYUnit(10, 5);


        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
            var data = new DataPoint[200];
            for (int i = 199; i >= 0; i--)
            {

                data[i] = new DataPoint()
                {
                    //DataTag = i % 10 == 0 ? new Tag()
                    //{
                    //    Description = "description" + i,
                    //    TimeStamp = DateTime.Now
                    //} : null,
                    RawData = i
                };
            }
            WaveDrawer.AddAndShowPoints(line1, data);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            WaveDrawer.AddLine(line2);

            Dispatcher x = Dispatcher.CurrentDispatcher;//取得当前工作线程
            //另开线程工作
            System.Threading.ThreadStart start = () =>
            {
                //工作函数
                var data = new DataPoint[200];

                for (int i = 0; i < 200; i++)
                {
                    data[i] = new DataPoint();
                }

                for (int count = 0; count < 60; ++count)
                {

                    for (int i = 0; i < 200; i++)
                    {
                        data[i].RawData = i + 50;
                    }

                    //异步更新界面
                    x.BeginInvoke(new Action(() =>
                    {
                        WaveDrawer.AddAndShowPoints(line2, data);
                    }), DispatcherPriority.Normal);

                    Thread.Sleep(100);
                }
            };

            new System.Threading.Thread(start).Start(); //启动线程
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            WaveDrawer.AddEventData(new EventData()
            {
                TimeStamp = DateTime.Now.AddSeconds(-10),
                Description = DateTime.Now.ToLongDateString(),
                Detail = DateTime.Now.Ticks + ""
            });
        }
    }
}
