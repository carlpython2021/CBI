using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConLib
{
    class ReadSignalControlOrder
    {
        public Dictionary<string, byte> signalcontrol_order1 = new Dictionary<string, byte>();
        public ReadSignalControlOrder()
        {
            readSignalControl();
        }
        public void readSignalControl()
        {
            string key;
            byte value;
            string str;

            StreamReader sr = null;
            signalcontrol_order1.Clear();
            try
            {
                //string test = this.Name;
                sr = new StreamReader(@"..\..\signalcontrol.txt");
                while (!sr.EndOfStream)
                {
                    key = sr.ReadLine();
                    str = sr.ReadLine();
                    value = Convert.ToByte(Convert.ToInt16(str));
                    signalcontrol_order1.Add(key, value);
                }
            }
            catch (System.Exception ex)
            {
                Console.Write("读取文件出错");
            }
            finally
            {
                sr.Close();
            }
        }
    }
}
