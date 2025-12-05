namespace ConLib
{
    partial class Rectangle_2
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(80, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseEnter += new System.EventHandler(this.pictureBox1_MouseEnter);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            // 
            // rectangle2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.pictureBox1);
            this.Name = "rectangle2";
            this.Size = new System.Drawing.Size(80, 50);
            this.Load += new System.EventHandler(this.rectangle_2_Load);
            this.SizeChanged += new System.EventHandler(this.rectangle_2_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        //private System.Windows.Forms.ToolStripMenuItem 道岔定位ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 道岔反位ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 下行道岔空闲ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 下行道岔锁闭ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 下行道岔占用ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 上行道岔空闲ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 上行道岔锁闭ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 上行道岔占用ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 道岔空闲ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 道岔锁闭ToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem 道岔占用ToolStripMenuItem;
    }
}
