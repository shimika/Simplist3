using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void ToggleSelectMode() {
			if (Status.NowSelect == Status.SelectMode.All) {
				Status.NowSelect = Status.SelectMode.Unfinished;
				buttonSort.Source = "Resources/season.png";
			} else {
				Status.NowSelect = Status.SelectMode.All;
				buttonSort.Source = "Resources/archive.png";
			}

			containArchive.RefreshContainer();
		}

		private void AddArchive(string title, bool scroll) {
			ArchiveData data = new ArchiveData();
			data.Title = title;

			Data.DictArchive.Add(data.Title, data);
			int index = containArchive.Add(true, data);

			if (scroll) {
				scrollArchive.ScrollToVerticalOffset(index * 40);
			}
		}

		private void DeleteArchive(string title) {
			containArchive.Delete(title);
		}

		private void ToggleAvailable(string title) {
			if (AddOpenMode == OpenMode.SeasonModify) {
				title = Data.DictSeason[ModifyTag].ArchiveTitle;
			}

			ArchiveData data = Data.DictArchive[title];

			if (data.Episode >= 0) {
				data.Episode = -1;
				textAddScript.Text = "활성화";
			} else {
				data.Episode = 0;
				textAddScript.Text = "비활성화";
			}

			RefreshArchiveEpisode(title, data.Episode);
			RefreshDisableButton(title);
		}
	}
}
