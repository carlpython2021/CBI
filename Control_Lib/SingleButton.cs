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
    [ToolboxBitmap(typeof(SingleButton), "Danniu.ico")]
    public partial class SingleButton : UserControl
    {
        #region 变量
        Bitmap bmp;
        public enum Xianshi
        {   绿,
            红,
            黄,
            灰,
            默认
        }
        #endregion

        //属性：显示红黄绿
        [Browsable(true), Category("专用属性")]
        public Xianshi xianshi=Xianshi.默认;
        public Xianshi 显示状态
        {
            get { return xianshi; }
            set
            {
                xianshi = value;
                Drawpic( xianshi );
            }
        }
      
        public SingleButton()
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

            if (t == Xianshi.默认)
            {
                rt = new System.Drawing.Rectangle(new Point(0, 0), new Size(pictureBox1.Width, pictureBox1.Height));
                g.FillEllipse(new SolidBrush(Color.White), rt);
                rt = new System.Drawing.Rectangle(new Point(1, 1), new Size(pictureBox1.Width - 2, pictureBox1.Height - 2));
                g.FillEllipse(new SolidBrush(Color.Black), rt);
            }

            else if (t == Xianshi.红)
            {
                rt = new System.Drawing.Rectangle(new Point(1, 1), new Size(pictureBox1.Width - 2, pictureBox1.Height - 2)); //创建一矩形
                g.FillEllipse(new SolidBrush(Color.Red), rt);//指定画刷来填充指定矩形的内切椭圆(圆)
            }

            else if (t == Xianshi.黄)
            {
                rt = new System.Drawing.Rectangle(new Point(1, 1), new Size(pictureBox1.Width - 2, pictureBox1.Height - 2));
                g.FillEllipse(new SolidBrush(Color.Yellow), rt);
            }

            else if (t == Xianshi.绿)
            {
                rt = new System.Drawing.Rectangle(new Point(1, 1), new Size(pictureBox1.Width - 2, pictureBox1.Height - 2));
                g.FillEllipse(new SolidBrush(Color.FromArgb(0x7f11EE11)), rt);
            }
            else if (t == Xianshi.灰)
            {
                rt = new System.Drawing.Rectangle(new Point(1, 1), new Size(pictureBox1.Width - 2, pictureBox1.Height - 2));
                g.FillEllipse(new SolidBrush(Color.Gray), rt);
            }
            g.Save();
             pictureBox1.Image = bmp;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Drawpic(xianshi);
        }

        private void Danniu_Paint(object sender, PaintEventArgs e)
        {
            Initial();
            Drawpic(xianshi);
        }
    }
}