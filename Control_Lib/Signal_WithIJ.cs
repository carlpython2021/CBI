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
    public partial class Signal_WithIJ : UserControl
    {
        #region 枚举区
        public enum Dengwei
        {
            灯1,
            灯2
        }
        public enum IDWeizhi
        {
            上,
            下
        }
        public enum Xianshi
        {
            红,
            黄,
            绿,
            白
        }
        public enum Fangwei
        {
            右上,
            左上,
            右下,
            左下
        }
        public enum Weizhi
        {
            置顶,
            置底
        }
        public enum Lable
        {
            高柱,
            矮柱
        }
        #endregion

        #region 变量区
        public int X_flag = 5;
        string ID = "XN";
        Bitmap bmp;
        int cuxi = 2;
        public IDWeizhi idweizhi;
        int width, height;
        Pen p_white;
        public IntPtr handle = new IntPtr(0);
        Fangwei fangwei = Fangwei.右上;
        Weizhi weizhi = Weizhi.置底;
        Lable lable = Lable.矮柱;
        #endregion

        #region 属性区
        //属性1：方位
        [Browsable(true), Category("专用属性")]
        public Fangwei 方位
        {
            get { return fangwei; }
            set
            {
                fangwei = value;
                Drawpic(X_flag);
            }
        }
        //属性2：ID位置
        [Browsable(true), Category("专用属性")]
        public IDWeizhi ID位置
        {
            get { return idweizhi; }
            set
            {
                idweizhi = value;
                Drawpic(X_flag);
            }
        }
        //属性3：画笔粗细
        [Browsable(true), Category("专用属性")]
        public int 粗细
        {
            get { return cuxi; }
            set
            {
                cuxi = value;
                p_white = new Pen(new SolidBrush(Color.White), cuxi);
                Drawpic(X_flag);
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
                        this.SendToBack();
                        break;
                    case Weizhi.置顶:
                        this.BringToFront();
                        break;
                }
            }
        }
        //属性5：信号机状态
        [Browsable(true), Category("专用属性")]
        public int 信号灯状态
        {
            get { return X_flag; }
            set
            {
                X_flag = value;
                Drawpic(X_flag);
            }
        }
        //属性6：信号机ID
        [Browsable(true), Category("专用属性")]
        public string ID号
        {
            get { return ID; }
            set
            {
                ID = value;
                Drawpic(X_flag);
            }
        }
        //属性7：信号机类型
        [Browsable(true), Category("专用属性")]
        public Lable 类型
        {
            get { return lable; }
            set
            {
                lable = value;
                Drawpic(X_flag);
            }
        }
        #endregion

        public Signal_WithIJ()
        {
            InitializeComponent();
            Initial();
            p_white = new Pen(new SolidBrush(Color.White));
        }

        public void Initial()
        {
            width = this.Width;
            height = this.Height;
            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
            pictureBox1.Location = new Point(0, 0);
        }

        private void Xinhaoji_2_Load(object sender, EventArgs e)
        {
            Initial();
            Drawpic(5);
        }

        /// <summary>
        /// flag——1 正线停车  2 侧线停车  3 正线通过  4 调车  5 禁止  6绿黄
        /// flag——1 黄        2 双黄      3 绿        4 白    5 红    6绿黄
        /// </summary>
        public void Drawpic(int X_flag)   //graphics 是画板，相当于工具，用于操作图画；bitmap 继承自image，可以理解为画纸；image 是画纸上的内容，就是实际能看到的图像内容，它是个抽象类；
        {
            Initial();
            if (bmp != null)   //画纸内容不空，得清空
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);  //新建画纸
            Graphics g = Graphics.FromImage(bmp);  //获取相应的画板工具，可以修改相应的image类
            g.Clear(Color.Black);

            System.Drawing.Rectangle rt;  //定义一个矩形框
            int Fenshu = 6;  //分割为6份数

            for (int i = 0; i < 3; i = i + 2)
            {
                rt = new System.Drawing.Rectangle(new Point(width * (i + 1) / Fenshu, 0), new Size(width / Fenshu * 2, (height - 2) / 2));
                g.FillEllipse(new SolidBrush(Color.White), rt);
                rt = new System.Drawing.Rectangle(new Point(width * (i + 1) / Fenshu + 1, 1), new Size(width / Fenshu * 2 - 3, (height - 5) / 2));
                g.FillEllipse(new SolidBrush(Color.Black), rt);
            }

            if (X_flag == 1)
            {
                rt = new System.Drawing.Rectangle(new Point(width * 1 / Fenshu+1, 1), new Size(width / Fenshu * 2-2, (height - 4)/2));
                g.FillEllipse(new SolidBrush(Color.Yellow), rt);
            }
            else if (X_flag == 2)
            {
                rt = new System.Drawing.Rectangle(new Point(width * 1 / Fenshu+1, 1), new Size(width / Fenshu * 2-2, (height - 4)/2));
                g.FillEllipse(new SolidBrush(Color.Yellow), rt);
                rt = new System.Drawing.Rectangle(new Point(width * 3 / Fenshu+1, 1), new Size(width / Fenshu * 2-2, (height - 4)/2));
                g.FillEllipse(new SolidBrush(Color.Yellow), rt);
            }
            else if (X_flag == 3)
            {
                rt = new System.Drawing.Rectangle(new Point(width * 1 / Fenshu + 1, 1), new Size(width / Fenshu * 2 - 2, (height - 4) / 2));
                g.FillEllipse(new SolidBrush(Color.Green), rt);
            }
            else if (X_flag == 4)
            {
                rt = new System.Drawing.Rectangle(new Point(width * 3 / Fenshu + 1, 1), new Size(width / Fenshu * 2 - 2, (height - 4) / 2));
                g.FillEllipse(new SolidBrush(Color.White), rt);
            }
            else if (X_flag == 5)
            {
                rt = new System.Drawing.Rectangle(new Point(width * 1 / Fenshu + 1, 1), new Size(width / Fenshu * 2 - 2, (height - 4) / 2));
                g.FillEllipse(new SolidBrush(Color.Red), rt);
            }
            else if (X_flag == 6)
            {
                rt = new System.Drawing.Rectangle(new Point(width * 1 / Fenshu + 1, 1), new Size(width / Fenshu * 2 - 2, (height - 4) / 2));
                g.FillEllipse(new SolidBrush(Color.Yellow), rt);
                rt = new System.Drawing.Rectangle(new Point(width * 3 / Fenshu + 1, 1), new Size(width / Fenshu * 2 - 2, (height - 4) / 2));
                g.FillEllipse(new SolidBrush(Color.Green), rt);
            }
            else
            {
                throw new Exception("输入超出范围");
            }

            if (lable == Lable.矮柱)
            {
                g.DrawLine(p_white, new Point(width / Fenshu * 1, 0), new Point(width / Fenshu * 1, height/2-1));
                g.DrawLine(p_white, new Point(width / Fenshu * 1, height / 2 +2), new Point(width / Fenshu * 1, height));
            }
            else
            {
                g.DrawLine(p_white, new Point(width / Fenshu * 1 / 2, 0), new Point(width / Fenshu * 1 / 2, height/2-1));
                g.DrawLine(p_white, new Point(width / Fenshu * 1 / 2, (height - 2) / 4), new Point(width / Fenshu * 1, (height - 2) / 4));
                g.DrawLine(p_white, new Point(width / Fenshu * 1 / 2, height/2+1), new Point(width / Fenshu * 1 / 2, height));
            }

            switch (fangwei)
            {
                case Fangwei.右上:
                    break;
                case Fangwei.左上:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                    break;
                case Fangwei.右下:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                    break;
                case Fangwei.左下:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                    break;
            }
            g.Save();
            pictureBox1.Image = bmp;
            g.Dispose();
        }

        /// flag——1 正线停车  2 侧线停车  3 正线通过  4 调车  5 禁止  6黄绿
        /// flag——1 黄        2 双黄      3 绿        4 白    5 红    6黄绿
        private void 信号机设为绿灯ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Drawpic(3);
        }

        private void 信号机设为黄灯ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Drawpic(1);
        }

        private void 信号机设为双黄灯ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Drawpic(2);
        }

        private void 信号机设为红灯ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Drawpic(5);
        }

        private void 信号机设为白灯ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Drawpic(4);
        }

        private void 信号机设为绿黄灯ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Drawpic(6);
        }
        
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Drawpic(X_flag);
        }


    }
}
