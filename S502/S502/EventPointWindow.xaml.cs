using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace S502
{
    /// <summary>
    /// EventPointWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EventPointWindow : Window
    {
        public EventData CurrentData { get; set; }
        public EventPointWindow()
        {
            InitializeComponent();
        }

        public void LoadEventData(EventData eventData)
        {
            if (eventData == null)
                return;

            CurrentData = eventData;
            Title = $"{eventData.Detail}";

            if (eventData != null)
            {
                Description.Text = $"Value: {eventData.Description}";
                Timestamp.Text = eventData.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss");
                Detail.Text = eventData.Detail;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentData == null)
                CurrentData = new EventData();

            //if (CurrentData.DataTag == null)
            //    CurrentData.DataTag = new Tag();

            CurrentData.Detail = Detail.Text;

            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            //DialogResult = false;
        }
    }
}
