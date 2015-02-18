using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleCamera
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private PXCMSenseManager sm;
		Task _task;
		bool _task_flag = false;

		/// <summary>
		/// 撮影開始
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button1_Click(object sender, EventArgs e)
		{
			sm = PXCMSenseManager.CreateInstance();
			// RGBの場合
			sm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480, 30);
			if (sm.Init() < pxcmStatus.PXCM_STATUS_NO_ERROR)
			{
				// エラー発生
				MessageBox.Show("PXCM_STATUS_NO_ERROR");
				return;
			}
			
			_task = new Task(() =>
			{
				_task_flag = true;
				while (_task_flag)
				{
					if (sm.AcquireFrame(true) < pxcmStatus.PXCM_STATUS_NO_ERROR) 
						return;
					PXCMCapture.Sample sample = sm.QuerySample();
					if (sample.color != null)
					{
						PXCMImage image = sample.color;
						PXCMImage.ImageData data;
						pxcmStatus sts = sample.color.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32, out data);
						if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
						{
							Bitmap bmp = data.ToBitmap(0, image.info.width, image.info.height);
							image.ReleaseAccess(data);
							pictureBox1.Image = bmp;
						}
					}
					sm.ReleaseFrame();
					/// 30fps程度にあわせる
					Thread.Sleep(33);
				}
			});
			_task.Start();
		}

		/// <summary>
		/// 終了
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button2_Click(object sender, EventArgs e)
		{
			_task_flag = false;
		}
	}
}
