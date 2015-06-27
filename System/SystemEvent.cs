using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void Window_Activated(object sender, EventArgs e) {
			grideffectShadow.BeginAnimation(
				DropShadowEffect.OpacityProperty, 
				new DoubleAnimation(0.4, TimeSpan.FromMilliseconds(100)));

			RefreshWeekHead();
		}

		private void Window_Deactivated(object sender, EventArgs e) {
			grideffectShadow.BeginAnimation(
				DropShadowEffect.OpacityProperty, 
				new DoubleAnimation(0.1, TimeSpan.FromMilliseconds(100)));
		}

		private void Statusbar_MouseDown(object sender, MouseButtonEventArgs e) {
			try {
				DragMove();
			} catch { }
		}

		public bool ProcessCommandLineArgs(IList<string> args) {
			ActivateMe();
			return true;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			tray.Dispose();
			Setting.SaveSetting();
		}

		private void ImageButton_Response(object sender, CustomButtonEventArgs e) {
			switch (e.Main) {
				case "add":
					switch (Mode) {
						case ShowMode.None:
							ShowAddWindow();
							break;

						case ShowMode.Add:
						case ShowMode.Modify:
							HideAddModifyWindow();
							break;

						case ShowMode.Download:
							StopBackgroundWorker(BwTorrent);
							StopBackgroundWorker(BwSubtitle);

							AnimateDownloadWindow(0, -40);
							SetTitlebar(Tab);
							Mode = ShowMode.None;
							break;
					}
					break;
				case "close":
					if (Setting.Tray) {
						this.Opacity = 0;
						Notice("", false, 0);
						new AltTab().HideAltTab(this);
					} else {
						this.Close();
					}
					break;

				case "disable":
					ToggleAvailable(ModifyTag);
					containArchive.RefreshContainer();
					break;

				case "delete":
					DeleteItem();
					break;

				case "save":
					AddOrModifyItem();
					break;

				case "sort":
					ToggleSelectMode();
					break;

				case "refresh":
					UpdateNotification();
					break;

				case "shot":
					Notice("캡쳐를 저장했습니다");
					string path = Function.SaveScreenShot(stackSeason, 0);

					if (File.Exists(path)) {
						string argument = string.Format("/select, \"{0}\"", path);
						Task.Factory.StartNew(() => Process.Start("explorer.exe", argument));
					}
					break;

				case "arrange":
					ArrangeEpisode();
					break;

				case "update":
					UpdateDownload();
					break;

				case "vercheck":
					//MessageBox.Show(System.Reflection.Assembly.GetExecutingAssembly().Location);
					UpdateCheck();
					break;

				case "openfolder":
					if (Status.Root) {
						Function.ExecuteFile(string.Format(@"X:Anime\{0}", ModifyTag));
					}
					break;

				case "changelog":
					gridChangelog.Visibility = Visibility.Collapsed;
					break;

				default:
					Notice(e.Main);
					break;
			}
		}

		private void Container_Response(object sender, CustomButtonEventArgs e) {
			switch (e.ActionType) {
				case "Click":
					if (e.Detail == "Season") {
						string arcTitle = Data.DictSeason[e.Main].ArchiveTitle;

						if (Data.DictArchive[arcTitle].Episode < 0) {
							RefreshArchiveEpisode(arcTitle, 0);
							containArchive.RefreshContainer();
						} else {
							if (Status.Lite) {
								ShowNotificationDownloadWindow();
								InitSubtitle(e.Main);
							} else {
								ShowDownloadWindow(
									e.Main,
									Data.DictArchive[arcTitle].Episode,
									Data.DictSeason[e.Main].Keyword);
							}
						}
					} else {
						if (Data.DictArchive[e.Main].Episode < 0) {
							RefreshArchiveEpisode(e.Main, 0);
							containArchive.RefreshContainer();
						} else {
							if (!Status.Root) { return; }
							try {
								string d = string.Format(@"X:Anime\{0}", e.Main);
								if (Directory.Exists(d)) {
									Function.ExecuteFile(d);
								}
							} catch { }
						}
					}

					break;

				case "Modify":
					ModifyTag = e.Main;

					if (e.Detail == "Season") {
						AddOpenMode = OpenMode.SeasonModify;
					} else {
						AddOpenMode = OpenMode.ArchiveModify;
					}

					ShowModifyWindow();

					break;
				case "OpenFolder":
					string dir = string.Format(@"X:Anime\{0}", e.Detail);

					try {
						if (!Directory.Exists(dir)) {
							Notice("폴더가 생성되었습니다");
							Directory.CreateDirectory(dir);
						}
						Function.ExecuteFile(dir);
					} catch { }
					break;

				case "CopyClipboard":
					string title = Function.CleanFileName(e.Main);
					string arctitle = e.Main;
					if (e.Detail == "Season") {
						arctitle = Data.DictSeason[e.Main].ArchiveTitle;
					}

					if (Data.DictArchive[arctitle].Episode < 0) {
						Clipboard.SetText(title);
					} else {
						try {
							Clipboard.SetText(string.Format("{0} - {1:D2}",
								title,
								Data.DictArchive[arctitle].Episode));
						}
						catch {
							Notice("클립보드 복사 실패");
							return;
						}
					}
					Notice("클립보드에 복사되었습니다");
					break;

				case "Episode":
					int delta = 0;
					try {
						delta = Convert.ToInt32(e.Detail);
					} catch { return; }

					RefreshArchiveEpisode(e.Main, Data.DictArchive[e.Main].Episode + delta);
					break;
			}
		}

		private void ActivateMe() {
			new AltTab().ShowAltTab(this);
			this.Opacity = 1;
			this.Activate();
		}


		public void Notice(string message, bool alert = false, double duration = 2000) {
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
