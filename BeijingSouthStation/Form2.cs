using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NanJingNanStation
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string trans = DataConvert.CBIToRBC(textBox1.Text);
            String message = "1000000000000001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001111110010100000001000101110000101100001100000111100000000000000000000000000000000000000000000001111110010100000000000000010000000000000000000000000000000000000000000000000000000100000000000000000000000100000000000000000000000000000000000000000000000000000111111001010000000100010111000010110000110000011110000001111110010100000001000101110000101100001100000111100000000000000000000000000000001000000000000000000000000100000000000000000000000000000000000000000000000000000111111001010000000100010111000010110000110000011110000001111110010100000001000101110000101100001100000111100000000000000000000000000000001100000000000000000000000100000000000000000000000000000000000000000000000000000111111001010000000100010111000010110000110000011110000001111110010100000001000101110000101100001100000111100000000000000000";
            int j = message.Length / 8;
            string MESSAGE = "";
            for (int i = 0; i < j; i++)
            {
                MESSAGE = MESSAGE + string.Format("{0:X}", Convert.ToByte(message.Substring(8 * i, 8), 2)).PadLeft(2, '0');
            }
            message = MESSAGE;
            if (message.Substring(0, 4) == "9201")
            {
                message = DataConvert.RBCToCBI(message);
            }
            else if (message.Substring(0, 4) == "8001")
            {
                message = DataConvert.CTCTosubCTC(message);
            }
            else
            {
                message = null;
            }
            //Encoding.ASCII.GetString(Encoding.UTF8.GetBytes(trans), 0, Encoding.UTF8.GetBytes(trans).Length);
        }
    }
}
