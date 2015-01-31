﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simplist3 {
	/// <summary>
	/// NotiItem.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class NotiItem : UserControl {
		public NotiItem(Listdata data) {
			InitializeComponent();

			this.Data = data;

			this.textTitle.Text = data.Type;
			this.textEpisode.Text = data.Tag;
			this.textMaker.Text = data.Title;
			//this.textTime.Text = data.Time;

			this.Time = data.Time;
		}

		Point MouseDownPoint;
		Listdata Data { get; set; }

		DateTime UploadTime;
		string time;
		string Time {
			get { return this.time; }
			set {
				this.time = value;
				try {
					this.UploadTime = DateTime.ParseExact(
						this.time,
						"yyyyMMddHHmmss",
						CultureInfo.InvariantCulture);
				} catch {
					this.UploadTime = new DateTime(1900, 1, 1);
				}

				this.UpdateTime();
			}
		}

		public void UpdateTime() {
			if (UploadTime == null) { return; }

			TimeSpan ts = DateTime.Now - this.UploadTime;
			if (this.UploadTime.Year == 1900) {
				this.textTime.Text = "";
			} else if (ts.TotalSeconds < 0) {
				this.textTime.Text = "미래";
			} else if (ts.TotalSeconds < 60) {
				this.textTime.Text = string.Format("{0}초 전", (int)ts.TotalSeconds);
			} else if (ts.TotalMinutes < 60) {
				this.textTime.Text = string.Format("{0}분 전", (int)ts.TotalMinutes);
			} else if (ts.TotalHours < 24) {
				this.textTime.Text = string.Format("{0}시간 전", (int)ts.TotalHours);
			} else {
				if (this.UploadTime.Year != DateTime.Now.Year) {
					this.textTime.Text = string.Format("{0}/{1}/{2} {3}:{4:D2}",
						this.UploadTime.Year, 
						this.UploadTime.Month, 
						this.UploadTime.Day, 
						this.UploadTime.Hour, 
						this.UploadTime.Minute);
				} else {
					this.textTime.Text = string.Format("{0}/{1} {2}:{3:D2}", 
						this.UploadTime.Month, 
						this.UploadTime.Day, 
						this.UploadTime.Hour,
						this.UploadTime.Minute);
				}
			}
		}

		private void GridMouseDown(object sender, MouseButtonEventArgs e) {
			MouseDownPoint = e.GetPosition(this);
		}

		public event EventHandler<CustomButtonEventArgs> Response;
		private void Button_Click(object sender, RoutedEventArgs e) {
			AnimateButton();
			if (Response != null) {
				Response(this, new CustomButtonEventArgs("Click", this.Data.Type, this.Data.Url));
			}
		}

		private void AnimateButton() {
			if (MouseDownPoint == null) { return; }

			Storyboard sb = new Storyboard();

			circle.Margin = new Thickness(MouseDownPoint.X - 150, 0, 0, 0);
			circle.RenderTransformOrigin = new Point(0.5, 0.5);
			circle.RenderTransform = new ScaleTransform(0.5, 1);

			DoubleAnimation opacity = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
			Storyboard.SetTarget(opacity, circle);
			Storyboard.SetTargetProperty(opacity, new PropertyPath(Ellipse.OpacityProperty));

			DoubleAnimation scalex = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(250));
			Storyboard.SetTarget(scalex, circle);
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

			sb.Children.Add(opacity);
			sb.Children.Add(scalex);

			sb.Begin(this);
		}
	}
}