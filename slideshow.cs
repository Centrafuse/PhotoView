using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.IO;
using centrafuse.Plugins;

namespace photoview
{
    public class slideshow : Form
    {
        public PictureBox picVis;

		private int clickcount = 0;
		private System.Windows.Forms.Timer clickTimer;

#if !WindowsCE
        protected override CreateParams CreateParams
        {
            get
            {
                // Turn on WS_EX_TOOLWINDOW style bit
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }
#endif

        public slideshow()
		{
#if !WindowsCE
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.DoubleBuffer, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.UpdateStyles();

			this.ShowInTaskbar = false;
			this.StartPosition = FormStartPosition.Manual;
#endif
            this.FormBorderStyle = FormBorderStyle.None;

			//this.TopLevel = false;
			this.BackColor = Color.Black;

			picVis = new PictureBox();
			picVis.SizeMode = PictureBoxSizeMode.StretchImage;
			picVis.BackColor = Color.Black;
			picVis.Visible = true;

			clickTimer = new System.Windows.Forms.Timer();
			clickTimer.Interval = SystemInformation.DoubleClickTime;
			clickTimer.Tick += new EventHandler(clickTimer_Tick);
			clickTimer.Enabled = false;

			this.MouseDown += new MouseEventHandler(picviewer_MouseDown);
			picVis.MouseDown += new MouseEventHandler(picVis_MouseDown);

			Controls.Add(picVis);
		}

		public void updateImage(Image newimage)
		{
			try
			{
				double picRatio = ((double)this.Bounds.Width / (double)this.Bounds.Height) / ((double)newimage.Width / (double)newimage.Height);
				int picHeight = this.Bounds.Height;
				int picWidth = this.Bounds.Width;

                CFTools.writeLog("SLIDESHOW", "w = " + picWidth.ToString() + ", h = " + picHeight.ToString() + ", r = " + picRatio.ToString());

				if(picRatio >= 1)
					picWidth = ((int)((double)this.Bounds.Width / picRatio));

                if (newimage.Width < picWidth)
                    picWidth = newimage.Width;
                if (newimage.Height < picHeight)
                    picHeight = newimage.Height;

				picVis.Bounds = new Rectangle(((this.Bounds.Width - picWidth) / 2), ((this.Bounds.Height - picHeight) / 2), picWidth, picHeight);
				picVis.Image = newimage;
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		private void picVis_MouseDown(object sender, MouseEventArgs e)
		{
			clickcount++;
			clickTimer.Enabled = true;
		}

		private void picviewer_MouseDown(object sender, MouseEventArgs e)
		{
			clickcount++;
			clickTimer.Enabled = true;
		}

		private void clickTimer_Tick(object sender, EventArgs e)
		{
            if (clickcount == 1)
            {
                photoview.inslideshow = false;
                this.Visible = false;
            }
            else
            {
                photoview.inslideshow = false;
                this.Visible = false;
            }

			clickcount = 0;
			clickTimer.Enabled = false;
		}

    }
}
