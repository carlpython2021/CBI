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
    [ToolboxBitmap(typeof(Ellipse), "rectangle.ico")]
    public partial class Ellipse : UserControl
    {
        #region 变量
        Bitmap bmp;
        #endregion
      
        public Ellipse()
        {
            InitializeComponent();
            Initial();
            Drawpic();
        }
        public void Initial()
        {
            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
            pictureBox1.Location = new Point(0, 0);
        }


        public void Drawpic()
        {
            if (bmp!=null)
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);  //创建bmp对象

            Graphics g = Graphics.FromImage(bmp); //创建Graphics对象，相当于准备画板
            g.Clear(Color.Black); //清空画板并以某一特定颜色填充

            System.Drawing.Rectangle rt;
            rt = new System.Drawing.Rectangle(new Point(0, 0), new Size(pictureBox1.Width, pictureBox1.Height));
            g.FillEllipse(new SolidBrush(Color.White), rt);

            rt = new System.Drawing.Rectangle(new Point(0, 0), new Size(pictureBox1.Width, 4 * pictureBox1.Height / 5));
            g.FillEllipse(new SolidBrush(Color.Gray), rt);

            g.Save();
            pictureBox1.Image = bmp;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Drawpic();
        }

        private void rectangle_Paint(object sender, PaintEventArgs e)
        {
            Initial();
            Drawpic();
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