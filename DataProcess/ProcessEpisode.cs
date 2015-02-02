using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void RefreshArchiveEpisode(string title, string strepisode) {
			int episode = 0;
			try {
				episode = Convert.ToInt32(strepisode);
			} catch { return; }

			RefreshArchiveEpisode(title, episode);
		}

		DispatcherTimer TimerEpisode;
		private void RefreshArchiveEpisode(string title, int episode) {
			containArchive.RefreshListEpisode(title, episode);

			if (Data.DictArchive[title].SeasonTitle != null) {
				string sTitle = Data.DictArchive[title].SeasonTitle;
				int week = Data.DictSeason[sTitle].Week;

				(FindName(string.Format("containWeek{0}", week)) as Container)
					.RefreshListEpisode(sTitle, episode);
			}

			if (TimerEpisode == null) {
				TimerEpisode = new DispatcherTimer();
				TimerEpisode.Interval = TimeSpan.FromSeconds(1);
				TimerEpisode.Tick += TimerEpisode_Tick;
			}

			TimerEpisode.Stop();
			TimerEpisode.Start();

			containArchive.RefreshContainer();
		}

		private void TimerEpisode_Tick(object sender, EventArgs e) {
			DispatcherTimer timer = sender as DispatcherTimer;
			timer.Stop();

			Setting.SaveSetting();
		}
	}
}
