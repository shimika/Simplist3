using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void RefreshArchiveEpisode(string title, string strepisode) {
			int episode = 0;
			try {
				episode = Convert.ToInt32(strepisode);
			} catch { return; }

			RefreshArchiveEpisode(title, episode);
		}

		private void RefreshArchiveEpisode(string title, int episode) {
			containArchive.RefreshEpisode(title, episode);

			if (Data.DictArchive[title].SeasonTitle != null) {
				string sTitle = Data.DictArchive[title].SeasonTitle;
				int week = Data.DictSeason[sTitle].Week;

				(FindName(string.Format("containWeek{0}", week)) as Container)
					.RefreshEpisode(sTitle, episode);
			}
		}
	}
}
