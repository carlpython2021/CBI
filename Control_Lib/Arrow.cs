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
    [ToolboxBitmap(typeof(Arrow), "arrow.ico")]
    public partial class Arrow : UserControl
    {
        Bitmap bmp;
        Point[] point1 = new Point[3];
        Point[] point2 = new Point[3];

        public enum FangXiang
        {
            left,
            right
        }

        FangXiang FX = FangXiang.left;

        [Browsable(true), Category("专用属性")]
        public FangXiang Fangxiang
        {
            get { return FX; }
            set
            {
                FX = value;
                Drawpic(FX);
            }
        }

        public enum color
        {
            红,
            绿,
            灰
        }

        color c = color.灰;

        [Browsable(true), Category("专用属性")]
        public color arrowColor
        {
            get { return c; }
            set
            {
                c = value;
                Drawpic(c);
            }
        }

        public Arrow()
        {
            InitializeComponent();
            Drawpic(FX);
        }
        private void Initial()
        {
            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
            pictureBox1.Location = new Point(0, 0);
        }

        private void Drawpic(color c)
        {
            if (bmp != null)
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            point1[0] = new Point(pictureBox1.Width * 7 / 10, 0);
            point1[1] = new Point(pictureBox1.Width * 7 / 10, pictureBox1.Height);
            point1[2] = new Point(pictureBox1.Width, pictureBox1.Height * 1 / 2);
            point2[0] = new Point(pictureBox1.Width * 3 / 10, 0);
            point2[1] = new Point(pictureBox1.Width * 3 / 10, pictureBox1.Height);
            point2[2] = new Point(0, pictureBox1.Height * 1 / 2);
            System.Drawing.Rectangle rt;
            switch (c)
            {
                case color.红:
                    if (FX == FangXiang.left)
                    {
                        rt = new System.Drawing.Rectangle(new Point(pictureBox1.Width * 3 / 10, pictureBox1.Height * 1 / 3), new Size(pictureBox1.Width * 7 / 10, pictureBox1.Height * 1 / 3));
                        g.FillRectangle(new SolidBrush(Color.Gray), rt);
                        g.FillPolygon(Brushes.Red, point2);
                    }
                    else
                    {
                        rt = new System.Drawing.Rectangle(new Point(0, pictureBox1.Height * 1 / 3), new Size(pictureBox1.Width * 7 / 10, pictureBox1.Height * 1 / 3));
                        g.FillRectangle(new SolidBrush(Color.Red), rt);
                        g.FillPolygon(Brushes.Red, point1);
                    }
                    break;
                case color.绿:
                    if (FX == FangXiang.left)
                    {
                        rt = new System.Drawing.Rectangle(new Point(pictureBox1.Width * 3 / 10, pictureBox1.Height * 1 / 3), new Size(pictureBox1.Width * 7 / 10, pictureBox1.Height * 1 / 3));
                        g.FillRectangle(new SolidBrush(Color.Gray), rt);
                        g.FillPolygon(Brushes.Green, point2);
                    }
                    else
                    {
                        rt = new System.Drawing.Rectangle(new Point(0, pictureBox1.Height * 1 / 3), new Size(pictureBox1.Width * 7 / 10, pictureBox1.Height * 1 / 3));
                        g.FillRectangle(new SolidBrush(Color.Green), rt);
                        g.FillPolygon(Brushes.Green, point1);
                    }
                    break;
                case color.灰:
                    if (FX == FangXiang.left)
                    {
                        rt = new System.Drawing.Rectangle(new Point(pictureBox1.Width * 3 / 10, pictureBox1.Height * 1 / 3), new Size(pictureBox1.Width * 7 / 10, pictureBox1.Height * 1 / 3));
                        g.FillRectangle(new SolidBrush(Color.Gray), rt);
                        g.FillPolygon(Brushes.Gray, point2);
                    }
                    else
                    {
                        rt = new System.Drawing.Rectangle(new Point(0, pictureBox1.Height * 1 / 3), new Size(pictureBox1.Width * 7 / 10, pictureBox1.Height * 1 / 3));
                        g.FillRectangle(new SolidBrush(Color.Gray), rt);
                        g.FillPolygon(Brushes.Gray, point1);
                    }
                    break;
            }
            g.Save();
            pictureBox1.Image = bmp;
            g.Dispose();
        }

        private void Drawpic(FangXiang fx)
        {
            if (bmp != null)
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            point1[0] = new Point(pictureBox1.Width * 7 / 10, 0);
            point1[1] = new Point(pictureBox1.Width * 7 / 10, pictureBox1.Height);
            point1[2] = new Point(pictureBox1.Width, pictureBox1.Height * 1 / 2);
            point2[0] = new Point(pictureBox1.Width * 3 / 10, 0);
            point2[1] = new Point(pictureBox1.Width * 3 / 10, pictureBox1.Height);
            point2[2] = new Point(0, pictureBox1.Height * 1 / 2);
            System.Drawing.Rectangle rt;
            switch (fx)
            {
                case FangXiang.right:
                    rt = new System.Drawing.Rectangle(new Point(0, pictureBox1.Height * 1 / 3), new Size(pictureBox1.Width * 7 / 10, pictureBox1.Height * 1 / 3));
                    g.FillRectangle(new SolidBrush(Color.Gray), rt);
                    g.FillPolygon(Brushes.Gray, point1);
                    break;
                case FangXiang.left:
                    rt = new System.Drawing.Rectangle(new Point(pictureBox1.Width * 3 / 10, pictureBox1.Height * 1 / 3), new Size(pictureBox1.Width * 7 / 10, pictureBox1.Height * 1 / 3));
                    g.FillRectangle(new SolidBrush(Color.Gray), rt);
                    g.FillPolygon(Brushes.Gray, point2);
                    break;
            }
            g.Save();
            pictureBox1.Image = bmp;
            g.Dispose();
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Drawpic(FX);
        }

        private void jiantou_Paint(object sender, PaintEventArgs e)
        {
            Drawpic(FX);
        }

        private void jiantou_Load(object sender, EventArgs e)
        {
            Initial();
            Drawpic(FX);
        }

        //private void pictureBox1_MouseEnter(object sender, EventArgs e)
        //{
        //    this.Cursor = Cursors.Hand;
        //}

        //private void pictureBox1_MouseLeave(object sender, EventArgs e)
        //{
        //    this.Cursor = Cursors.Arrow;
        //}
    }
}
