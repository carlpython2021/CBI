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
    [ToolboxBitmap(typeof(Rectangle), "rectangle.ico")]
    public partial class Rectangle : UserControl
    {
        #region 变量
        Bitmap bmp;
        public enum Xianshi
        {
            白,
            紫,
            黄,
            青
        }
        #endregion

        //属性：框颜色
        [Browsable(true), Category("专用属性")]
        public Xianshi xianshi=Xianshi.白;
        public Xianshi 显示状态
        {
            get { return xianshi; }
            set
            {
                xianshi = value;
                Drawpic( xianshi );
            }
        }
      
        public Rectangle()
        {
            InitializeComponent();
            Initial();
        }
        public void Initial()
        {
            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
            pictureBox1.Location = new Point(0, 0);
        }

        /// <summary>
        /// 绘制红黄绿灯规则
        /// </summary>
        public void Drawpic(Xianshi t)
        {
            if (bmp!=null)
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);  //创建bmp对象

            Graphics g = Graphics.FromImage(bmp); //创建Graphics对象，相当于准备画板
            g.Clear(Color.Black); //清空画板并以某一特定颜色填充

            System.Drawing.Rectangle rt;

            if (t == Xianshi.白)
            {
                rt = new System.Drawing.Rectangle(new Point(0, 0), new Size(pictureBox1.Width, pictureBox1.Height));
                g.FillRectangle(new SolidBrush(Color.White), rt);
                rt = new System.Drawing.Rectangle(new Point(2, 2), new Size(pictureBox1.Width - 4, pictureBox1.Height - 4));
                g.FillRectangle(new SolidBrush(Color.Black), rt);
            }
            else if (t == Xianshi.紫)
            {
                rt = new System.Drawing.Rectangle(new Point(0, 0), new Size(pictureBox1.Width, pictureBox1.Height)); //创建一矩形
                g.FillRectangle(new SolidBrush(Color.Purple), rt);
                rt = new System.Drawing.Rectangle(new Point(2, 2), new Size(pictureBox1.Width - 4, pictureBox1.Height - 4));
                g.FillRectangle(new SolidBrush(Color.Black), rt);
            }
            else if (t == Xianshi.黄)
            {
                rt = new System.Drawing.Rectangle(new Point(0, 0), new Size(pictureBox1.Width, pictureBox1.Height)); //创建一矩形
                g.FillRectangle(new SolidBrush(Color.Yellow), rt);
                rt = new System.Drawing.Rectangle(new Point(2, 2), new Size(pictureBox1.Width - 4, pictureBox1.Height - 4));
                g.FillRectangle(new SolidBrush(Color.Black), rt);
            }
            else if (t == Xianshi.青)
            {
                rt = new System.Drawing.Rectangle(new Point(0, 0), new Size(pictureBox1.Width, pictureBox1.Height)); //创建一矩形
                g.FillRectangle(new SolidBrush(Color.FromArgb(0x7f4474BB)), rt);
                rt = new System.Drawing.Rectangle(new Point(2, 2), new Size(pictureBox1.Width - 4, pictureBox1.Height - 4));
                g.FillRectangle(new SolidBrush(Color.Black), rt);
            }
            g.Save();
            pictureBox1.Image = bmp;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Drawpic(xianshi);
        }

        private void rectangle_Paint(object sender, PaintEventArgs e)
        {
            Initial();
            Drawpic(xianshi);
        }
    }
}