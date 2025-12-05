using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace ConLib
{
    public partial class Rectangle_2 : UserControl
    {
        #region 枚举区
        public enum Weizhi
        {
            置顶,
            置底
        }
        public enum STATE
        {
            实线,
            虚线
        }
        #endregion

        #region 变量区
        Point[] a = new Point[4];
        Bitmap bmp;
        List<myLine> line = new List<myLine>();
        public int thickness = 4;
        public Weizhi weizhi;
        public STATE state;
        #endregion

        #region 属性区

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

        //属性6：STATE
        [Browsable(true), Category("专用属性")]
        public STATE Type
        {
            get { return state; }
            set
            {
                state = value;
                switch (state)
                {
                    case STATE.实线:
                        Drawpic(thickness, STATE.实线);
                        break;
                    case STATE.虚线:
                        Drawpic(thickness, STATE.虚线);
                        break;
                }
            }
        }

        //属性7：粗细
        [Browsable(true), Category("专用属性")]
        public int 粗细
        {
            get { return thickness; }
            set
            {
                thickness = value;
                Drawpic(thickness, state);
            }
        }



        #endregion

        public Rectangle_2()
        {
            InitializeComponent();
        }

        private void rectangle_2_Load(object sender, EventArgs e)
        {
            Initial();
            Drawpic(thickness, state);
        }

        public void Initial()
        {

            a[0] = new Point(0, 0);
            a[1] = new Point(this.Width, 0);
            a[2] = new Point(0, this.Height);
            a[3] = new Point(this.Width, this.Height);

            line.Clear();

            myLine myline = new myLine(a[0], a[1], 1);
            line.Add(myline);
            myline = new myLine(a[0], a[2], 2);
            line.Add(myline);
            myline = new myLine(a[3], a[1], 3);
            line.Add(myline);
            myline = new myLine(a[3], a[2], 4);
            line.Add(myline);

            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
        }

        

        private void Drawpic(int thickness, STATE state)
        {
            Initial();
            if (bmp != null)
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            Pen pen = null;
            if (state == STATE.实线) {
                pen = new Pen(Color.FromArgb(0x7F6495ED), 3);
            }else{
                pen = new Pen(Color.White, 2);
                pen.DashStyle = DashStyle.Dot;
                line.Add(new myLine(new Point(0, this.Height / 3), new Point(this.Width, this.Height / 3), 5));
                line.Add(new myLine(new Point(0, this.Height * 2 / 3), new Point(this.Width, this.Height * 2 / 3), 6));
            }
            
            foreach (myLine m in line) {
                m.draw(g, pen);
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

        private void rectangle_2_SizeChanged(object sender, EventArgs e)
        {
            Initial();
            Drawpic(thickness, state);
        }
    }
}
