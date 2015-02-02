using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Simplist3 {
	public partial class MainWindow : Window {
		enum TabMode { Season, Archive, Notification, Setting, Add, Modify, Download };
		enum ShowMode { None, Add, Modify, Download };
		TabMode Tab = TabMode.Season;
		ShowMode Mode = ShowMode.None;

		private void TabButton_Response(object sender, CustomButtonEventArgs e) {
			SetProperty(Tab, TabButton.Mode.Clickable);

			SetTabMode(e.Main);
			SetProperty(Tab, TabButton.Mode.Focused);
			SetTitlebar(Tab);
		}

		private void SetTabMode(string tag) {
			switch (tag) {
				case "season":
					Tab = TabMode.Season;
					break;
				case "archive":
					Tab = TabMode.Archive;
					break;
				case "noti":
					Tab = TabMode.Notification;
					UpdateNotifyTime();
					break;
				default:
					Tab = TabMode.Setting;
					break;
			}
		}

		private void SetProperty(TabMode now, TabButton.Mode mode) {
			TabButton button = null;
			FrameworkElement element = null;

			switch (now) {
				case TabMode.Season:
					button = tabSeason;
					element = scrollSeason;
					break;
				case TabMode.Archive:
					button = tabArchive;
					element = scrollArchive;
					break;
				case TabMode.Notification:
					button = tabNotify;
					element = scrollNotify;
					break;
				case TabMode.Setting:
					button = tabSetting;
					element = gridSetting;
					break;
			}

			button.ViewMode = mode;
			element.IsHitTestVisible = 
				mode == TabButton.Mode.Clickable ? false : true;

			Storyboard sb = new Storyboard();
			sb.Children.Add(Animation.GetDoubleAnimation(mode == TabButton.Mode.Clickable ? 0 : 1, element, 0));
			sb.Begin(this);
		}
	}
}
