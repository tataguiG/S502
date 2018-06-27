using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using NLog;

namespace S502
{
    public class Utilities
    {
        public static Logger LoggerInstance = LogManager.GetLogger("file");


        private static string _v1;
        private static string _v2;
        private static string _v3;

        private static string _dataFileFolderPath;
        private static FileStream _fileHandler = null;

        Dictionary<string, string> _usedDevices = new Dictionary<string, string>();

        public static void LoadConfiguration(string configureFile)
        {
            _v1 = ConfigurationManager.AppSettings["v1"];
            _v2 = ConfigurationManager.AppSettings["v2"];
            _v3 = ConfigurationManager.AppSettings["v3"];

            _dataFileFolderPath = ConfigurationManager.AppSettings["FolderPath"];
        }

        public static void SaveConfiguration()
        {
            //System.Configuration config = new Configuration();

        }


        /// <summary>
        /// 存储接收到的刺激或事件类数据
        /// </summary>
        /// <param name="data"></param>
        public static void SaveEventData(byte[] data) { }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="data"></param>
        public static void SaveReceivedData(byte[] data)
        {
            if (!Directory.Exists(_dataFileFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(_dataFileFolderPath);
                }
                catch (DirectoryNotFoundException ex)
                {

                }
                catch (IOException ex)
                {

                }

            }
            try
            {
                Directory.CreateDirectory(_dataFileFolderPath + $@"\{DateTime.Now.ToString("yyyyMMdd")}");
            }
            catch (DirectoryNotFoundException ex)
            {

            }
            catch (IOException ex)
            {

            }

            if (_fileHandler != null)
            {
                if (_fileHandler.Length / 1024 / 1024 >= 10)
                {
                    _fileHandler.Close();
                }
            }
            else
            {
                _fileHandler = new FileStream(
                    _dataFileFolderPath + $@"\{DateTime.Now.ToString("hhmmssfff")}.dat", FileMode.Create);
            }
            
            _fileHandler.Write(data, 0, data.Length);
        }
    }
}
