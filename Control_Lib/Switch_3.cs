using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ConLib
{
    public partial class Switch_3 : UserControl
    {
        #region 枚举区
        public enum STATE
        {
            空闲,
            占用,
            锁闭
        }
        public enum DingFan
        {
            定位,
            反位
        }
        public enum Fangwei
        {
            左上,
            右上,
            左下,
            右下
        }
        public enum Weizhi
        {
            置顶,
            置底
        }
        #endregion

        #region 变量区
        Point[] a = new Point[8];
        public IntPtr handle = new IntPtr(0);
        public IntPtr wparam = new IntPtr(0);
        public IntPtr pwaram = new IntPtr(0);
        public STATE state = STATE.空闲;
        string ID = "1";
        public Weizhi weizhi;
        public string Rlocation;
        public Fangwei fangwei = Fangwei.左上;
        Bitmap bmp;
        public DingFan DF_flag = DingFan.定位;
        Pen p_white, p_blue, p_red;    //p_jyj_white
        List<myLine> line = new List<myLine>();  //声明一个List数组
        public int cuxi = 4;
        public string ch365_position;
        public EndPoint Tar_Shapan = new IPEndPoint(IPAddress.Parse("192.168.10.220"), 4001);
        public Socket Ser_toShapan = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public bool Daocha_Flag = false;
        ReadSwitchControlOrder rsco_switch;
        #endregion

        #region 属性区
        //属性1：道岔ID号
        [Browsable(true), Category("专用属性")]
        public string 道岔ID号
        {
            get { return ID; }
            set
            {
                ID = value;
                Drawpic(DF_flag, state);
            }
        }

        //属性2：实际位置
         [Browsable(true), Category("专用属性")]
        public string 实际位置
        {
            get { return Rlocation; }
            set
            {
                Rlocation = value;
            }
        }

        //属性3：方位
         [Browsable(true), Category("专用属性")]
        public Fangwei 道岔方位
        {
            get { return fangwei; }
            set
            {
                fangwei = value;
                Drawpic(DF_flag, state);
            }
        }

        //属性4：Zlocation
        [Browsable(true), Category("专用属性")]
        public Weizhi Zlocation
        {
            get { return weizhi; }
            set
            {
                weizhi = value;
                switch (weizhi)
                {
                    case Weizhi.置底:
                        this.SendToBack(); break;
                    case Weizhi.置顶:
                        this.BringToFront(); break;
                }
            }
        }

        //属性5：粗细
         [Browsable(true), Category("专用属性")]
        public int 粗细
        {
            get { return cuxi; }
            set
            {
                cuxi = value;
                p_white = new Pen(new SolidBrush(Color.White), cuxi);
                p_blue = new Pen(new SolidBrush(Color.FromArgb(0x7F6495ED)), cuxi);
                p_red = new Pen(new SolidBrush(Color.Red), cuxi);
                Drawpic(DF_flag, state);
            }
        }

        //属性6：定反位
         [Browsable(true), Category("专用属性")]
        public DingFan 定反位
        {
            get { return DF_flag; }
            set
            {
                DF_flag = value;
                Daocha_Flag = true;
                if (ID != "")
                {
                    wparam = new IntPtr(Convert.ToInt32(ID));
                    if (DF_flag == DingFan.反位)
                        pwaram = new IntPtr(2);
                    else if (DF_flag == DingFan.定位)
                        pwaram = new IntPtr(1);
                }
                Drawpic(DF_flag, state);
            }
        }

        //属性7：锁闭状态
         [Browsable(true), Category("专用属性")]
        public STATE 锁闭状态
        {
            get { return state; }
            set
            {
                state = value;
                Drawpic(DF_flag, state);
            }
        }

        //属性8：道岔板卡位置
         [Browsable(true), Category("专用属性")]
        public string 道岔板卡位置
        {
            get
            { return ch365_position; ; }
            set
            {
                ch365_position = value;
            }
        }

        #endregion

        public Switch_3()
        {
            InitializeComponent();
            try
            {
                rsco_switch = new ReadSwitchControlOrder();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            p_white = new Pen(new SolidBrush(Color.White), cuxi);
            p_blue = new Pen(new SolidBrush(Color.FromArgb(0x7F6495ED)), cuxi);
            p_red = new Pen(new SolidBrush(Color.Red), cuxi);
        }

        public void initial()
        {
            a[0] = new Point(0, this.Height * 9 / 10);
            a[1] = new Point(this.Width * 3 / 13, this.Height * 9 / 10);
            a[2] = new Point(this.Width * 13 / 13, this.Height * 9 / 10);
            a[3] = new Point(this.Width * 2 / 13, this.Height * 7 / 10);
            a[4] = new Point(this.Width * 8 / 13, this.Height * 1 / 10);
            a[5] = new Point(this.Width * 13 / 13, this.Height * 1 / 10);
            
            line.Clear();

            myLine myline = new myLine(a[0], a[1], 1);
            line.Add(myline);
            myline = new myLine(a[1], a[2], 2);
            line.Add(myline);
            myline = new myLine(a[0], a[3], 3);
            line.Add(myline);
            myline = new myLine(a[3], a[4], 4);
            line.Add(myline);
            myline = new myLine(a[4], a[5], 5);
            line.Add(myline);

            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
        }
        public void Switch_Shapan_Control()
        {
            byte[] spData = new byte[1024];
            byte crc = 0;
            UInt16 i = 0;
            //string sig = ((Signal)signals[signal_mz]).信号灯状态.ToString();
            //string sigg = this.信号灯状态.ToString();
            //string key = this.Name + "_" + sigg;
            spData[0] = 0x55;
            spData[1] = 0x02;
            spData[2] = 0xB2;
            if (this.定反位 == DingFan.定位)
            {
                spData[4] = 0x00;
            }
            else
            {
                spData[4] = 0x01;
            }
            string str = this.Name;
            if (str.Contains("Switch_2"))
            {
                string str_send = str + "_sh";
                spData[3] = rsco_switch.switchcontrol_order1[str_send];
                for (i = 0; i < 5; i++)
                {
                    crc += spData[i];
                }
                crc = (byte)(-crc);
                spData[5] = crc;
                Ser_toShapan.SendTo(spData, 6, SocketFlags.None, Tar_Shapan);
                System.Threading.Thread.Sleep(10);
                str_send = str + "_xi";
                spData[3] = rsco_switch.switchcontrol_order1[str_send];
                for (i = 0; i < 5; i++)
                {
                    crc += spData[i];
                }
                crc = (byte)(-crc);
                spData[5] = crc;
                Ser_toShapan.SendTo(spData, 6, SocketFlags.None, Tar_Shapan);
                System.Threading.Thread.Sleep(10);
            }
            else if (str.Contains("Switch_3"))
            {
                spData[3] = rsco_switch.switchcontrol_order1[str];
                for (i = 0; i < 5; i++)
                {
                    crc += spData[i];
                }
                crc = (byte)(-crc);
                spData[5] = crc;
                Ser_toShapan.SendTo(spData, 6, SocketFlags.None, Tar_Shapan);
                System.Threading.Thread.Sleep(10);
            }
        }

        private void Drawpic(DingFan flag, STATE st)
        {
           // Point p1, p2;
            initial();
            if (bmp != null)
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);

            foreach (myLine m in line)  //foreach（数据类型 标识符 in 表达式）{ 循环体 }
            {
                if (m.key == 1)
                {
                    if (DF_flag == DingFan.定位)
                    {
                        m.enable = true;
                    }
                    else if (DF_flag == DingFan.反位)
                    {
                        m.enable = false;
                    }
                }
                else if (m.key == 3)
                {
                    if (DF_flag == DingFan.定位)
                    {
                        m.enable = false;
                    }
                    else if (DF_flag == DingFan.反位)
                    {
                        m.enable = true;
                    }
                }
                m.draw(g, p_blue);
            }

            if (DF_flag == DingFan.定位)
            {
                foreach (myLine m in line)
                {
                    if (m.key == 1 || m.key == 2)
                    {
                        switch (state)
                        {
                            case STATE.锁闭:
                                m.draw(g, p_white);
                                break;
                            case STATE.占用:
                                m.draw(g, p_red);
                                break;
                        }
                    }
                }
            }
            else
            {
                foreach (myLine m in line)
                {
                    if (m.key == 3 || m.key == 4 || m.key == 5)
                    {
                        switch (state)
                        {
                            case STATE.锁闭:
                                m.draw(g, p_white);
                                break;
                            case STATE.占用:
                                m.draw(g, p_red);
                                break;
                        }
                    }
                }
            }

            switch (fangwei)
            {
                case Fangwei.左上:
                    break;
                case Fangwei.左下:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                    break;
                case Fangwei.右上:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                    break;
                case Fangwei.右下:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                    break;
            }
            g.Save();
            pictureBox1.Image = bmp;
            try
            {
                if (Daocha_Flag)
                {
                    Daocha_Flag = false;
                    Switch_Shapan_Control();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Daocha_3_Load(object sender, EventArgs e)
        {
            Drawpic(DF_flag, state);
        }

    }
}
