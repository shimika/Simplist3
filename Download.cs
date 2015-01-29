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
		private void ShowDownloadWindow(string title, int episode) {
			SetTitlebar(TabMode.Download);
			Mode = ShowMode.Download;
			tabTorrent.ViewMode = TabButton.Mode.Focused;

			textNewTitle.Text = string.Format("({0}화) {1}", episode, title);
			buttonBack.ViewMode = ImageButton.Mode.Hidden;

			AnimateDownloadWindow(1, 0);
		}

		private void AnimateDownloadWindow(double opacity, double topMargin) {
			Storyboard sb = new Storyboard();

			gridDown.IsHitTestVisible = opacity == 1 ? true : false;

			DoubleAnimation da = new DoubleAnimation(opacity, TimeSpan.FromMilliseconds(250));
			ThicknessAnimation ta = new ThicknessAnimation(new Thickness(0, topMargin, 0, 0), TimeSpan.FromMilliseconds(350)) {
				EasingFunction = new ExponentialEase() {
					Exponent = 5,
					EasingMode = EasingMode.EaseOut,
				}
			};

			Storyboard.SetTarget(da, gridDown);
			Storyboard.SetTarget(ta, gridDownTab);
			Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
			Storyboard.SetTargetProperty(ta, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(da);
			sb.Children.Add(ta);

			sb.BeginTime = TimeSpan.FromMilliseconds(opacity == 0 ? 0 : 100);
			sb.Begin(this);
		}
	}
}
