using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplist3 {
	public partial class MainWindow : Window {

		int AnitableWeekday = 0;
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

			RefreshAnitable(day);
		}

		BackgroundWorker BwAnitable;
		private void RefreshAnitable(int day) {
			StopBackgroundWorker(BwAnitable);

			BwAnitable = new BackgroundWorker() {
				WorkerSupportsCancellation = true,
			};
			BwAnitable.DoWork += BwAnitable_DoWork;
			BwAnitable.RunWorkerCompleted += BwAnitable_RunWorkerCompleted;

			AnitableWeekday = day;
			BwAnitable.RunWorkerAsync(day);
		}

		private void BwAnitable_DoWork(object sender, DoWorkEventArgs e) {
			e.Result = Parser.Week(e.Argument.ToString(), "Anitable");
			e.Cancel = CheckWorkerCancel(sender);
		}

		private void BwAnitable_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Cancelled) { return; }

			List<Listdata> list = e.Result as List<Listdata>;
			stackAnitable.Children.Clear();

			foreach (Listdata data in list) {
				ListItem item = new ListItem(data);
				item.Response += AnitableItem_Response;
				stackAnitable.Children.Add(item);
			}

			scrollAnitable.ScrollToTop();
		}

		private void AnitableItem_Response(object sender, CustomButtonEventArgs e) {
			if (e.ActionType == "Anitable") {
				this.textboxTitle.Text = e.Main;

				this.comboboxWeekday.SelectedIndex = AnitableWeekday;
				this.textboxHour.Text = e.Detail.Substring(0, 2);
				this.textboxMinute.Text = e.Detail.Substring(2, 2);

				textboxKeyword.Focus();
			}
		}
	}
}
