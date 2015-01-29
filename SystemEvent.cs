using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
							AnimateDownloadWindow(0, -40);
							SetTitlebar(Tab);
							Mode = ShowMode.None;
							break;
					}
					break;
				case "close":
					this.Close();
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
							ShowDownloadWindow(e.Main, Data.DictArchive[arcTitle].Episode);
						}
					} else {
						if (Data.DictArchive[e.Main].Episode < 0) {
							RefreshArchiveEpisode(e.Main, 0);
							containArchive.RefreshContainer();
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
					if (Directory.Exists(dir)) {
						Process pro = new Process() {
							StartInfo = new ProcessStartInfo() {
								FileName = dir
							}
						};
						pro.Start();
					}
					break;

				case "CopyClipboard":
					string title = CleanFileName(e.Main);
					string arctitle = e.Main;
					if (e.Detail == "Season") {
						arctitle = Data.DictSeason[e.Main].ArchiveTitle;
					}

					if (Data.DictArchive[arctitle].Episode < 0) {
						Clipboard.SetText(title);
					} else {
						Clipboard.SetText(string.Format("{0} - {1:D2}",
							title,
							Data.DictArchive[arctitle].Episode));
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

		private static string CleanFileName(string fileName) {
			return Path.GetInvalidFileNameChars().Aggregate(
				fileName, 
				(current, c) => current.Replace(c.ToString(), string.Empty));
		}
	}
}
