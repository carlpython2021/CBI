using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConLib
{
    class ReadSwitchControlOrder
    {
        public Dictionary<string, byte> switchcontrol_order1 = new Dictionary<string, byte>();
        public ReadSwitchControlOrder()
        {
            readSwitchControl();
        }
        public void readSwitchControl()
        {
            string key;
            byte value;
            string str;
            StreamReader sr = null;
            switchcontrol_order1.Clear();
            try
            {
                //string test = this.Name;
                sr = new StreamReader(@"..\..\switchcontrol.txt");
                while (!sr.EndOfStream)
                {
                    key = sr.ReadLine();
                    str = sr.ReadLine();
                    value = Convert.ToByte(Convert.ToInt16(str));
                    switchcontrol_order1.Add(key, value);
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
