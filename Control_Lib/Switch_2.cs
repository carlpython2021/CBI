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
    public partial class Switch_2 : UserControl
    {
        #region 枚举区
        public enum STATE
        {
            空闲,
            锁闭,
            占用
        }
        public enum DingFan
        {
            定位,
            反位
        }
        public enum Fangwei
        {
            左,
            右
        }
        public enum Weizhi
        {
            置顶,
            置底
        }
        #endregion

        #region 变量区
        Point[] a = new Point[7];
        Point[] b = new Point[7];
        public IntPtr handle, wparam, pwaram;
        public STATE state_up = STATE.空闲;
        public STATE state_down = STATE.空闲;
        public STATE state = STATE.空闲;
        public string ID_up = "0";
        public string ID_down = "0";
        public string Rlocation = "0,0";
        public Fangwei fangwei = Fangwei.左;
        Bitmap bmp;
        public DingFan DF_flag_up = DingFan.定位;
        public DingFan DF_flag_down = DingFan.定位;
        public DingFan DF_flag = DingFan.定位;
        Pen p_white, p_blue, p_red;
        List<myLine> linea = new List<myLine>();
        List<myLine> lineb = new List<myLine>();
        public int cuxi = 4;
        public Weizhi weizhi;
        public EndPoint Tar_Shapan = new IPEndPoint(IPAddress.Parse("192.168.10.220"), 4001);
        public Socket Ser_toShapan = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public bool Daocha_Flag = false;
        ReadSwitchControlOrder rsco_switch;
        #endregion

        #region 属性区
        //属性1：ID号上
        [Browsable(true), Category("专用属性")]
        public string ID号上
        {
            get { return ID_up; }
            set
            {
                ID_up = value;
            }
        }

        //属性2：实际位置下行
        [Browsable(true), Category("专用属性")]
        public string 实际位置
        {
            get { return Rlocation; }
            set
            {
                Rlocation = value;
            }
        }
        
        //属性4：ID号下
        [Browsable(true), Category("专用属性")]
        public string ID号下
        {
            get { return ID_down; }
            set
            {
                ID_down = value;
            }
        }

        //属性5：Zlocation
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
                        this.SendToBack();
                        break;
                    case Weizhi.置顶:
                        this.BringToFront();
                        break;
                }
            }
        }

        //属性6：方位
        [Browsable(true), Category("专用属性")]
        public Fangwei 方位
        {
            get { return fangwei; }
            set
            {
                fangwei = value;
                Drawpic(DF_flag, state_up, state_down);
            }
        }

        //属性7：粗细
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
                Drawpic(DF_flag, state_up, state_down);
            }
        }

        //属性8：定反位上
        [Browsable(true), Category("专用属性")]
        public DingFan 定反位
        {
            get { return DF_flag; }
            set
            {
                DF_flag = value;
                Daocha_Flag = true;
                Drawpic(DF_flag, state_up, state_down);
            }
        }

        //属性10：锁闭状态上
        [Browsable(true), Category("专用属性")]
        public STATE 锁闭状态上
        {
            get { return state_up; }
            set
            {
                state_up = value;
                Drawpic(DF_flag, state_up, state_down);
            }
        }

        //属性11：锁闭状态下
        [Browsable(true), Category("专用属性")]
        public STATE 锁闭状态下
        {
            get { return state_down; }
            set
            {
                state_down = value;
                Drawpic(DF_flag, state_up, state_down);
            }
        }
        //属性12：反位锁闭状态
        [Browsable(true), Category("专用属性")]
        public STATE 反位锁闭状态
        {
            get { return state; }
            set
            {
                state = value;
                Drawpic1(DF_flag, state);
            }
        }

        //属性12：上行轨道板卡位置
        [Browsable(true), Category("专用属性")]
        public string ch365_s_position;
        public string 上行轨道板卡位置
        {
            get
            { return ch365_s_position; }
            set
            {
                ch365_s_position = value;
            }
        }

        //属性13：下行轨道板卡位置
        [Browsable(true), Category("专用属性")]
        public string ch365_x_position;
        public string 下行轨道板卡位置
        {
            get
            { return ch365_x_position; ; }
            set
            {
                ch365_x_position = value;
            }
        }



        #endregion

        public Switch_2()
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

        private void Daocha_2_Load(object sender, EventArgs e)
        {
            Initial();
            Drawpic(DingFan.定位, STATE.空闲, STATE.空闲);
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

        public void Initial()
        {
            a[1] = new Point(0, this.Height * 9 / 10);
            a[2] = new Point(this.Width * 4 / 16, this.Height * 9 / 10);
            a[3] = new Point(this.Width * 6 / 16, this.Height * 9 / 10);
            a[4] = new Point(this.Width * 16 / 16, this.Height * 9 / 10);
            a[5] = new Point(this.Width * 6 / 16, this.Height * 7 / 10);
            a[6] = new Point(this.Width * 8 / 16, this.Height * 5 / 10);

            b[1] = new Point(this.Width * 16 / 16, this.Height * 1 / 10);
            b[2] = new Point(this.Width * 12 / 16, this.Height * 1 / 10);
            b[3] = new Point(this.Width * 10 / 16, this.Height * 1 / 10);
            b[4] = new Point(this.Width * 0 / 16, this.Height * 1 / 10);
            b[5] = new Point(this.Width * 10 / 16, this.Height * 3 / 10);
            b[6] = new Point(this.Width * 8 / 16, this.Height * 5 / 10);
            linea.Clear();
            lineb.Clear();
            myLine myline = new myLine(a[1], a[2], 1);
            linea.Add(myline);
            myline = new myLine(a[2], a[3], 2);
            linea.Add(myline);
            myline = new myLine(a[3], a[4], 3);
            linea.Add(myline);
            myline = new myLine(a[2], a[5], 4);
            linea.Add(myline);
            myline = new myLine(a[5], a[6], 5);
            linea.Add(myline);

            myline = new myLine(b[1], b[2], 1);
            lineb.Add(myline);
            myline = new myLine(b[2], b[3], 2);
            lineb.Add(myline);
            myline = new myLine(b[3], b[4], 3);
            lineb.Add(myline);
            myline = new myLine(b[2], b[5], 4);
            lineb.Add(myline);
            myline = new myLine(b[5], b[6], 5);
            lineb.Add(myline);

            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
        }

        /// <summary>
        /// key2道岔定位则画蓝线，key4道岔反位则画蓝线
        /// key1、key4、key5在道岔反位情况下，锁闭画白线，占用画红线
        /// </summary>
        /// <param name="g"></param>
        /// <param name="DF_flag"></param>
        /// <param name="state"></param>
        /// <param name="line"></param>
        private void Drawpic_sub(Graphics g, DingFan DF_flag, STATE state, List<myLine> line)
        {
            foreach (myLine m in line)
            {
                if (m.key == 2)
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
                else if (m.key == 4)
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
               
            }
            else
            {
                foreach (myLine m in line)
                {
                    if (m.key == 1 || m.key == 4 || m.key == 5)
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
        }
        /// <summary>
        /// key1、key2、key3在道岔反位情况下，锁闭画白线，占用画红线
        /// key1、key4、key5在道岔反位情况下画蓝线
        /// </summary>
        /// <param name="g"></param>
        /// <param name="DF_flag"></param>
        /// <param name="state"></param>
        /// <param name="line"></param>
        private void Drawpic_sub1(Graphics g, DingFan DF_flag, STATE state, List<myLine> line)
        {
            foreach (myLine m in line)
            {
                if (m.key == 2)
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
                else if (m.key == 4)
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
                    if (m.key == 1 || m.key == 2 || m.key == 3)
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
                    if (m.key == 1 || m.key == 4 || m.key == 5)
                    {
                        m.draw(g, p_blue);
                    }
                }
            }
        }

        /// <summary>
        /// Drawpic使用Drawpic_sub1画线方式
        /// <param name="DF_flag"></param>
        /// <param name="state"></param>
        /// <param name="state2"></param>
        private void Drawpic(DingFan DF_flag, STATE state, STATE state2)
        {
            Initial();
            if (bmp != null)
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);

            Drawpic_sub1(g, DF_flag, state, linea);
            Drawpic_sub1(g, DF_flag, state2, lineb);
            //绝缘节
            // g.DrawLine(p_blue, new Point(a[6].X - 4, a[6].Y - 4), new Point(a[6].X + 4, a[6].Y + 2));
            switch (fangwei)
            {
                case Fangwei.左:
                    break;
                case Fangwei.右:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
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
        /// <summary>
        /// Drawpic1使用Drawpic_sub画线方式
        /// </summary>
        /// <param name="DF_flag"></param>
        /// <param name="state"></param>
        private void Drawpic1(DingFan DF_flag, STATE state)
        {
            Initial();
            if (bmp != null)
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);

            Drawpic_sub(g, DF_flag, state, linea);
            Drawpic_sub(g, DF_flag, state, lineb);

            // g.DrawLine(p_blue, new Point(a[6].X - 4, a[6].Y - 4), new Point(a[6].X + 4, a[6].Y + 2));
            switch (fangwei)
            {
                case Fangwei.左:
                    break;
                case Fangwei.右:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                    break;
            }
            g.Save();
            pictureBox1.Image = bmp;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// contextmenustrip 菜单对应的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 道岔定位ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DF_flag_up = DingFan.定位;
            DF_flag_down = DingFan.定位;
            wparam = new IntPtr(Convert.ToInt32(ID_up + "0" + ID_down));
            pwaram = new IntPtr(1);
            //Send_Message.sendmessage(handle, Send_Message.Message_DC2, wparam, pwaram);
            Drawpic(DF_flag, state_up, state_down);
        }

        private void 道岔反位ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DF_flag_up = DingFan.反位;
            DF_flag_down = DingFan.反位;
            wparam = new IntPtr(Convert.ToInt32(ID_up + "0" + ID_down));
            pwaram = new IntPtr(2);
            //Send_Message.sendmessage(handle, Send_Message.Message_DC2, wparam, pwaram);
            Drawpic(DF_flag, state_up, state_down);
        }

        private void 下行道岔空闲ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state_down = STATE.空闲;
            Drawpic(DF_flag, state_up, state_down);
        }

        private void 下行道岔锁闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state_down = STATE.锁闭;
            Drawpic(DF_flag, state_up, state_down);
        }

        private void 下行道岔占用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state_down = STATE.占用;
            Drawpic(DF_flag, state_up, state_down);
        }

        private void 上行道岔空闲ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state_up = STATE.空闲;
            Drawpic(DF_flag, state_up, state_down);
        }

        private void 上行道岔锁闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state_up = STATE.锁闭;
            Drawpic(DF_flag, state_up, state_down);
        }

        private void 上行道岔占用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state_up = STATE.占用;
            Drawpic(DF_flag, state_up, state_down);
        }

        private void 道岔空闲ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state_down = STATE.空闲;
            state_up = STATE.空闲;
            Drawpic(DF_flag, state_up, state_down);
        }

        private void 道岔锁闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state_down = STATE.锁闭;
            state_up = STATE.锁闭;
            Drawpic(DF_flag, state_up, state_down);
        }

        private void 道岔占用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state_down = STATE.占用;
            state_up = STATE.占用;
            Drawpic(DF_flag, state_up, state_down);
        }

        private void Daocha_2_SizeChanged(object sender, EventArgs e)
        {
            Initial();
            Drawpic(DF_flag, state_up, state_down);
        }
    }
}
