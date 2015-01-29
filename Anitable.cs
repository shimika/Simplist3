using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void InitBackgroundWorker() {
			bw.WorkerSupportsCancellation = true;
			bw.DoWork += bw_DoWork;
			bw.RunWorkerCompleted += bw_RunWorkerCompleted;
		}

		int NowWeekDay = 0;
		private void Weekday_Click(object sender, CustomButtonEventArgs e) {
			int day = -1;
			try {
				day = Convert.ToInt32(e.Main);
			} catch { return; }

			for (int i = 0; i < 7; i++) {
				(stackTableWeekday.Children[i] as TabButton).ViewMode 
					= TabButton.Mode.Clickable;
			}
			(stackTableWeekday.Children[day] as TabButton).ViewMode
					= TabButton.Mode.Focused;

			RefreshAniTable(day);
		}

		BackgroundWorker bw = new BackgroundWorker();
		private void RefreshAniTable(int day) {
			NowWeekDay = day;
			try {
				bw.CancelAsync();
			} catch { }

			bw.RunWorkerAsync(day);
		}

		private void bw_DoWork(object sender, DoWorkEventArgs e) {
			e.Result = Parser.Week(e.Argument.ToString());
		}

		private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Cancelled) { return; }

			List<ListData> list = e.Result as List<ListData>;
			stackAnitable.Children.Clear();

			foreach (ListData data in list) {
				ListItem item = new ListItem(data, true);
				item.Response += item_Response;
				stackAnitable.Children.Add(item);
			}

			scrollAnitable.ScrollToTop();
		}

		private void item_Response(object sender, CustomButtonEventArgs e) {
			if (e.ActionType == "Click") {
				this.textboxTitle.Text = e.Main;

				this.comboboxWeekday.SelectedIndex = NowWeekDay;
				this.textboxHour.Text = e.Detail.Substring(0, 2);
				this.textboxMinute.Text = e.Detail.Substring(2, 2);

				textboxSearch.Focus();
			}
		}
	}

	public struct ListData {
		public string Title, Url, Memo, Time, ID, Week;
		public bool IsRaw, IsTorrent;
		public DateTime UpdateTime;
	}
}
