using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace ConLib
{
    public partial class Switch_1 : UserControl
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
        public string Rlocation = "0,0";
        public Fangwei fangwei = Fangwei.左上;
        Bitmap bmp;
        public DingFan DF_flag = DingFan.定位;
        Pen p_white, p_blue, p_red, p_jyj_white;
        List<myLine> line = new List<myLine>();  //声明一个List数组
        public int cuxi = 4;
        public string ch365_position;
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
                        this.SendToBack();break;
                    case Weizhi.置顶:
                        this.BringToFront();break;
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
                p_blue = new Pen(new SolidBrush(Color.MediumTurquoise), cuxi);
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
                if (ID != "")
                {
                    wparam = new IntPtr(Convert.ToInt32(ID));
                    if (DF_flag == DingFan.反位)
                        pwaram = new IntPtr(2);
                    else if (DF_flag == DingFan.定位)
                        pwaram = new IntPtr(1);
                    //Send_Message.sendmessage(handle, Send_Message.Message_DC1, wparam, pwaram);
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

        public Switch_1()
        {
            InitializeComponent();
            p_white = new Pen(new SolidBrush(Color.White), cuxi);
            p_blue = new Pen(new SolidBrush(Color.FromArgb(0x7F6495ED)), cuxi);
            p_red = new Pen(new SolidBrush(Color.Red), cuxi);
            p_jyj_white = new Pen(new SolidBrush(Color.White), cuxi/2);
        }

        public void initial()
        {
            a[0] = new Point(cuxi, this.Height * 9 / 10);
            a[1] = new Point(this.Width * 3 / 16,  this.Height * 9 / 10);
            a[2] = new Point(this.Width * 6 / 16,  this.Height * 9 / 10);
            a[3] = new Point(this.Width * 16 / 16, this.Height * 9 / 10);
            a[4] = new Point(this.Width * 5 / 16, this.Height * 7 / 10);
            a[5] = new Point(this.Width * 11 / 16, this.Height * 1 / 10);
            a[6] = new Point(this.Width * 16 / 16, this.Height * 1 / 10);
            line.Clear();

            myLine myline = new myLine(a[0], a[1], 1);
            line.Add(myline);
            myline = new myLine(a[1], a[2], 2);
            line.Add(myline);
            myline = new myLine(a[2], a[3], 3);
            line.Add(myline);
            myline = new myLine(a[1], a[4], 4);
            line.Add(myline);
            myline = new myLine(a[4], a[5], 5);
            line.Add(myline);
            myline = new myLine(a[5], a[6], 6);
            line.Add(myline);

            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
        }

        private void Drawpic(DingFan flag, STATE st)
        {
            Point p1, p2;
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
                    if (m.key == 1 || m.key == 4 || m.key == 5 || m.key == 6)
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
                    p1 = new Point(0, pictureBox1.Height - 12);
                    p2 = new Point(0, pictureBox1.Height);
                    g.DrawLine(p_jyj_white, p1, p2);
                    break;
                case Fangwei.左下:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                    p1 = new Point(0, 0);
                    p2 = new Point(0, 12);
                    g.DrawLine(p_jyj_white, p1, p2);
                    break;
                case Fangwei.右上:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                    p1 = new Point(pictureBox1.Width-1, pictureBox1.Height - 12);
                    p2 = new Point(pictureBox1.Width-1, pictureBox1.Height);
                    g.DrawLine(p_jyj_white, p1, p2);
                    break;
                case Fangwei.右下:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                    p1 = new Point(pictureBox1.Width-1, 0);
                    p2 = new Point(pictureBox1.Width-1, 12);
                    g.DrawLine(p_jyj_white, p1, p2);
                    break;
            }
            g.Save();
            pictureBox1.Image = bmp;
        }

        private void Daocha_1_Load(object sender, EventArgs e)
        {
            Drawpic(DF_flag, state);
        }

        private void 道岔定位ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DF_flag = DingFan.定位;
            wparam = new IntPtr(Convert.ToInt32(ID));
            pwaram = new IntPtr(1);
            //Send_Message.sendmessage(handle, Send_Message.Message_DC1, wparam, pwaram);
            Drawpic(DF_flag, state);
        }

        private void 道岔反位ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DF_flag = DingFan.反位;
            wparam = new IntPtr(Convert.ToInt32(ID));
            pwaram = new IntPtr(2);
            //Send_Message.sendmessage(handle, Send_Message.Message_DC1, wparam, pwaram);
            Drawpic(DF_flag, state);
        }

        private void 道岔空闲ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = STATE.空闲;
            Drawpic(DF_flag, state);
        }

        private void 道岔锁闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = STATE.锁闭;
            Drawpic(DF_flag, state);
        }

        private void 道岔占用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state = STATE.占用;
            Drawpic(DF_flag, state);
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void Daocha_1_SizeChanged(object sender, EventArgs e)
        {
            Drawpic(DF_flag,state);
        }
    }
    /// <summary>
    /// 两点间画线函数
    /// </summary>
    public class myLine
    {
        Point p1, p2;
        public int key;
        public bool enable;
        public myLine(Point t1, Point t2, int i)
        {
            p1 = t1;
            p2 = t2;
            key = i;
            enable = true;
        }
        public void draw(Graphics g, Pen p)
        {
            if (enable)
            {
                g.DrawLine(p, p1, p2);
            }
        }
    }
}
