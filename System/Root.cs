using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void ArrangeEpisode() {
			int epiCount = 0, smiCount = 0;

			foreach (SeasonData data in Data.DictSeason.Values) {
				string at = data.ArchiveTitle;
				string path = string.Format(@"X:\Anime\{0}", at);

				if (!Directory.Exists(path) || Data.DictArchive[at].Episode < 0) {
					continue;
				}

				int lastSeason = 0;
				for (int i = 1; i<=10; i++) {
					string seasonPath = Path.Combine(path, string.Format("{0}기", i));
					if (Directory.Exists(seasonPath)) {
						lastSeason = i;
					}
				}

				if (lastSeason > 0) {
					path = Path.Combine(path, string.Format("{0}기", lastSeason));
				}

				int episode = Data.DictArchive[at].Episode;

				for (int i = 1; i <= 2500; i++) {
					string file = Function.CleanFileName(string.Format("{0} - {1:D2}", data.Title, i));
					string mp4 = string.Format("{0}.mp4", file);

					if (!File.Exists(Path.Combine(path, mp4))) {
						episode = i;
						break;
					} else {
						string smi = string.Format("{0}.smi", Path.Combine(path, file));

						if (File.Exists(smi)) {
							FileAttributes fileAttributes = File.GetAttributes(smi);

							if (fileAttributes != FileAttributes.Hidden) {
								smiCount++;
								File.SetAttributes(smi, FileAttributes.Hidden);
							}
						}
					}
				}

				if (episode > Data.DictArchive[at].Episode) {
					epiCount++;
					RefreshArchiveEpisode(at, episode);
				}
			}

			Notice(string.Format("Epi:{0}, Sub:{1} updated", epiCount, smiCount));
		}
	}
}
