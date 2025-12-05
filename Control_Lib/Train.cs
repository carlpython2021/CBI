using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConLib
{
    [ToolboxBitmap(typeof(Train), "Train.ico")]
    public partial class Train : UserControl
    {
        Pen p_white, p_blue, p_red;
        public IntPtr pwaram = new IntPtr(0);
        Bitmap bmp;
        public int cuxi = 2;
        public string RLocation;
        public string checihao = "G000";
        public string EL_Time;
        Font drawfont = new Font("Times New Roman", 9, FontStyle.Bold);
        StringFormat sf = new StringFormat();
        Point[] point1 = new Point[3];
        Point[] point2 = new Point[3];
        public enum Train_state
        {
            early,
            normal,
            late
        }
        public enum FangXiang
        {
            ShangXing,
            XiaXing
        }
        Train_state RState = Train_state.normal;
        FangXiang FX = FangXiang.ShangXing;
        [Browsable(true), Category("专用属性")]
        public Train_state Train_State
        {
            get { return RState; }
            set
            {
                RState = value;
                Drawpic(RState, FX);
            }
        }

        [Browsable(true), Category("专用属性")]
        public FangXiang Fangxiang
        {
            get { return FX; }
            set
            {
                FX = value;
                Drawpic(RState, FX);
            }
        }

        [Browsable(true), Category("专用属性")]
        public string Train_Location
        {
            get { return RLocation; }
            set { RLocation = value; }
        }

        [Browsable(true), Category("专用属性")]
        public string 车次号
        {
            get { return checihao; }
            set
            {
                checihao = value;
                Drawpic(RState, FX);
            }
        }
        [Browsable(true), Category("专用属性")]
        public string 晚点时间
        {
            get { return EL_Time; }
            set
            {
                EL_Time = value;
                Drawpic(RState, FX);
            }
        }
        public enum Weizhi
        {
            置顶,
            置底
        }
        public Weizhi weizhi;
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

        public Train()
        {
            InitializeComponent();
        }
        private void Initial()
        {
            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
            pictureBox1.Location = new Point(0, 0);
            p_white = new Pen(new SolidBrush(Color.White), cuxi);  //画笔类，前参数为颜色，后参数为颜色宽度
            p_blue = new Pen(new SolidBrush(Color.MediumTurquoise), cuxi);
            p_red = new Pen(new SolidBrush(Color.Red), cuxi);
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
        }

        private void Drawpic(Train_state Ts, FangXiang fx)
        {
            if (bmp != null)
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            point1[0] = new Point(pictureBox1.Width / 10, 0);
            point1[1] = new Point(0, pictureBox1.Height / 2);
            point1[2] = new Point(pictureBox1.Width / 10, pictureBox1.Height);
            point2[0] = new Point(pictureBox1.Width * 9 / 10, 0);
            point2[1] = new Point(pictureBox1.Width, pictureBox1.Height / 2);
            point2[2] = new Point(pictureBox1.Width * 9 / 10, pictureBox1.Height);
            System.Drawing.Rectangle rt;
            switch (fx)
            {
                case FangXiang.ShangXing:
                    rt = new System.Drawing.Rectangle(new Point(pictureBox1.Width * 1 / 10, 0), new Size(pictureBox1.Width * 6 / 10, pictureBox1.Height));
                    g.FillRectangle(new SolidBrush(Color.Green), rt);
                    g.DrawString(checihao, drawfont, new SolidBrush(Color.Blue), rt, sf);
                    g.FillPolygon(Brushes.Green, point1);
                    break;
                case FangXiang.XiaXing:
                    rt = new System.Drawing.Rectangle(new Point(pictureBox1.Width * 3 / 10, 0), new Size(pictureBox1.Width * 6 / 10, pictureBox1.Height));
                    g.FillRectangle(new SolidBrush(Color.Green), rt);
                    g.DrawString(checihao, drawfont, new SolidBrush(Color.Blue), rt, sf);
                    g.FillPolygon(Brushes.Green, point2);
                    break;
            }
            switch (Ts)
            {
                case Train_state.early:
                    if (fx == FangXiang.ShangXing)
                    {
                        rt = new System.Drawing.Rectangle(new Point(pictureBox1.Width * 7 / 10, 0), new Size(pictureBox1.Width * 3 / 10, pictureBox1.Height));
                        g.FillRectangle(new SolidBrush(Color.Blue), rt);
                        g.DrawString(EL_Time, drawfont, new SolidBrush(Color.White), rt, sf);
                    }
                    else
                    {
                        rt = new System.Drawing.Rectangle(new Point(pictureBox1.Width * 0 / 10, 0), new Size(pictureBox1.Width * 3 / 10, pictureBox1.Height));
                        g.FillRectangle(new SolidBrush(Color.Blue), rt);
                        g.DrawString(EL_Time, drawfont, new SolidBrush(Color.White), rt, sf);
                    }
                    break;
                case Train_state.late:
                    if (fx == FangXiang.ShangXing)
                    {
                        rt = new System.Drawing.Rectangle(new Point(pictureBox1.Width * 7 / 10, 0), new Size(pictureBox1.Width * 3 / 10, pictureBox1.Height));
                        g.FillRectangle(new SolidBrush(Color.Red), rt);
                        g.DrawString(EL_Time, drawfont, new SolidBrush(Color.White), rt, sf);
                    }
                    else
                    {
                        rt = new System.Drawing.Rectangle(new Point(pictureBox1.Width * 0 / 10, 0), new Size(pictureBox1.Width * 3 / 10, pictureBox1.Height));
                        g.FillRectangle(new SolidBrush(Color.Red), rt);
                        g.DrawString(EL_Time, drawfont, new SolidBrush(Color.White), rt, sf);
                    }
                    break;
            }
            g.Save();
            pictureBox1.Image = bmp;
            g.Dispose();
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Drawpic(RState, FX);
        }

        private void Train_Paint(object sender, PaintEventArgs e)
        {
            Drawpic(RState, FX);
        }

        private void Train_Load(object sender, EventArgs e)
        {
            Initial();
            Drawpic(RState, FX);
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }
    }
}
