using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void Notice(string message, bool alert = false, double duration = 2000) {
			this.Dispatcher.BeginInvoke(new Action(() => {
				textNotice.Text = message;

				gridAlert.Visibility = alert ? Visibility.Visible : Visibility.Collapsed;

				Storyboard sbNotice = new Storyboard();

				DoubleAnimation noticeOn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(0));
				DoubleAnimation noticeOff = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) {
					BeginTime = TimeSpan.FromMilliseconds(duration)
				};

				Storyboard.SetTarget(noticeOn, gridNotice);
				Storyboard.SetTarget(noticeOff, gridNotice);
				Storyboard.SetTargetProperty(noticeOn, new PropertyPath(Grid.OpacityProperty));
				Storyboard.SetTargetProperty(noticeOff, new PropertyPath(Grid.OpacityProperty));

				sbNotice.Children.Add(noticeOn);
				sbNotice.Children.Add(noticeOff);

				sbNotice.Begin(this);
			}));
		}
	}
}
