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
			SetImageByMode(buttonAdd, mode, false, TabMode.Season, TabMode.Archive, TabMode.Download, TabMode.Add, TabMode.Modify);
			SetImageByMode(buttonShot, mode, true, TabMode.Season);
			SetImageByMode(buttonArrange, mode, true, TabMode.Season);

			SetImageByMode(buttonSort, mode, true, TabMode.Archive);
			SetImageByMode(buttonRefresh, mode, true, TabMode.Notification);
			SetImageByMode(buttonUpdate, mode, true, TabMode.Setting);
			SetImageByMode(buttonUpdateCheck, mode, true, TabMode.Setting);

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

			if (mode == TabMode.Download) {
				ChangeTitle("Torrent");
			} else {
				ChangeTitle(mode.ToString());
			}
		}

		private void SetImageByMode(ImageButton button, TabMode targetMode, bool hidden, params TabMode[] modes) {
			foreach(TabMode mode in modes){
				if (targetMode == mode) {
					if (button.Type == "update" && Version.NowVersion == ver) {
						break;
					}
					if (button.Type == "vercheck" && Version.NowVersion != ver) {
						break;
					}
					if (button.Type == "arrange" && !Status.Root) {
						break;
					}
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
			DoubleAnimation daNew = Animation.GetDoubleAnimation(1, textTitle, 350);
			daNew.From = 0;

			ThicknessAnimation taOld = Animation.GetThicknessAnimation(
				250, -30, 0, textTitleOld, 0, 0, 0, 3);
			ThicknessAnimation taNew = Animation.GetThicknessAnimation(
				250, 0, 0, textTitle, 0, 0, 0, 3);

			taOld.From = new Thickness(0);
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
