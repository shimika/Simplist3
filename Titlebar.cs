using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void SetTitlebar(TabMode mode) {
			SetImageMode(buttonAdd, mode, false, TabMode.Season, TabMode.Archive, TabMode.Download, TabMode.Add, TabMode.Modify);
			SetImageMode(buttonShot, mode, true, TabMode.Season);
			SetImageMode(buttonSort, mode, true, TabMode.Archive);
			SetImageMode(buttonRefresh, mode, true, TabMode.Notification);

			SetTabMode(tabTorrent, mode, TabMode.Download);
			SetTabMode(tabSubtitle, mode, TabMode.Download);

			switch (mode) {
				case TabMode.Add:
				case TabMode.Modify:
				case TabMode.Download:
					buttonAdd.Source = "Resources/back.png";
					break;
				case TabMode.Season:
				case TabMode.Archive:
					buttonAdd.Source = "Resources/plus.png";
					break;
				default:
					buttonAdd.Source = "Resources/favorite.png";
					break;
			}

			ChangeTitle(mode.ToString());
		}

		private void SetImageMode(ImageButton button, TabMode targetMode, bool hidden, params TabMode[] modes) {
			foreach(TabMode mode in modes){
				if (targetMode == mode) {
					button.ViewMode = ImageButton.Mode.Visible;
					return;
				}
			}

			if (hidden) {
				button.ViewMode = ImageButton.Mode.Hidden;
			} else {
				button.ViewMode = ImageButton.Mode.Disable;
			}
		}

		private void SetTabMode(TabButton button, TabMode targetMode, params TabMode[] modes) {
			foreach (TabMode mode in modes) {
				if (targetMode == mode) {
					button.ViewMode = TabButton.Mode.Clickable;
					return;
				}
			}
			button.ViewMode = TabButton.Mode.Hidden;
		}

		private void ChangeTitle(string str) {
			textTitleOld.Text = textTitle.Text;
			textTitle.Text = str;

			Storyboard sb = new Storyboard();

			DoubleAnimation daOld = Animation.GetDoubleAnimation(0, textTitleOld, 350);
			daOld.From = 1;
			DoubleAnimation daNewStart = Animation.GetDoubleAnimation(0, textTitle, 0);
			DoubleAnimation daNew = Animation.GetDoubleAnimation(1, textTitle, 350, 100);
			daNew.From = 0;

			ThicknessAnimation taOld = Animation.GetThicknessAnimation(350, -30, 0, textTitleOld);
			taOld.From = new Thickness(0);
			ThicknessAnimation taNew = Animation.GetThicknessAnimation(350, 0, 0, textTitle, 0, 0, 100);
			taNew.From = new Thickness(60, 0, 0, 0);

			sb.Children.Add(daOld);
			sb.Children.Add(daNewStart);
			sb.Children.Add(daNew);
			sb.Children.Add(taOld);
			sb.Children.Add(taNew);

			sb.Begin(this);
		}
	}
}
