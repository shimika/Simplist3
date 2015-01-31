using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Simplist3 {
	public partial class MainWindow : Window {
		BackgroundWorker BwNoti;
		DispatcherTimer TimerNotify;

		private void InitNotification() {
			UpdateNotification();

			if (TimerNotify == null) {
				TimerNotify = new DispatcherTimer();
				TimerNotify.Interval = TimeSpan.FromMinutes(15);
				TimerNotify.Tick += (o, e) => UpdateNotification();
			}

			TimerNotify.Start();
		}

		private void UpdateNotification() {
			if (BwNoti == null) {
				BwNoti = new BackgroundWorker() {
					WorkerSupportsCancellation = true,
				};
				BwNoti.DoWork += BwNoti_DoWork;
				BwNoti.RunWorkerCompleted += BwNoti_RunWorkerCompleted;
			}

			if (BwNoti.IsBusy) { return; }

			buttonRefresh.StartAnimateImage();
			BwNoti.RunWorkerAsync(WeekDay);
		}

		private void BwNoti_DoWork(object sender, DoWorkEventArgs e) {
			int weekday = (int)e.Argument;

			List<Listdata> listNoti = new List<Listdata>();

			for (int i = -1; i <= 0; i++) {
				int day = (weekday + i + 7) % 7;

				foreach (Listdata data in Parser.Week(day.ToString(), "Noti")) {
					List<Listdata> list = Parser.GetMakerList(data.Url, true);

					if (list.Count > 0) {
						Listdata d = list[0];
						d.Type = data.Title;
						listNoti.Add(d);
					}

					Thread.Sleep(100);
				}
			}

			listNoti.Sort();

			e.Result = listNoti;
		}

		private void BwNoti_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			buttonRefresh.StopAnimateImage();

			stackNotify.Children.Clear();
			foreach (Listdata data in e.Result as List<Listdata>) {
				NotiItem item = new NotiItem(data);
				item.Response += NotifyItem_Response;

				stackNotify.Children.Add(item);
			}
			/*
			using (StreamWriter sw = new StreamWriter("C:\\testtest.txt")) {
				foreach (Listdata data in e.Result as List<Listdata>) {
					sw.WriteLine("{0} : {1} : {2} {3} : {4}", data.Time, data.Type, data.Tag, data.Title, data.Url);
				}
			}
			 */
		}

		private void NotifyItem_Response(object sender, CustomButtonEventArgs e) {
			//MessageBox.Show(e.ActionType + "\n" + e.Main + "\n" + e.Detail);
			ShowNotificationDownloadWindow();
			InitNotify(e.Main, e.Detail);
		}

		private void ShowNotificationDownloadWindow() {
			SetTitlebar(TabMode.Download);
			Mode = ShowMode.Download;

			textNewCaption.Text = "";
			RefreshDownloadControl("Subtitle");

			buttonBack.ViewMode = ImageButton.Mode.Hidden;
			tabTorrent.ViewMode = TabButton.Mode.Hidden;
			
			AnimateDownloadWindow(1, 0);
		}

		private void InitNotify(string title, string url) {
			gridSubtitle.Children.Clear();

			NowSubtitle = title;
			NowCaption = "";
			NowContainer = null;

			if (StackHistory == null) {
				StackHistory = new Stack<Pair>();
			}
			StackHistory.Clear();

			RefreshSubtitle("Anime", new Pair(title, url));
		}
	}
}
