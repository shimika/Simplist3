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
				string safePath = getSafeFileName(at);
				string path = string.Format(@"X:\Anime\{0}", safePath);

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
					string mkv = string.Format("{0}.mkv", file);

					if (!File.Exists(Path.Combine(path, mp4)) && !File.Exists(Path.Combine(path, mkv))) {
						episode = i;
						break;
					} else {
						string[] ext = { ".smi", ".ass" };
						foreach (String e in ext) {
							string smi = string.Format("{0}{1}", Path.Combine(path, file), e);
							if (File.Exists(smi)) {
								FileAttributes fileAttributes = File.GetAttributes(smi);

								if (fileAttributes != FileAttributes.Hidden) {
									smiCount++;
									File.SetAttributes(smi, FileAttributes.Hidden);
								}
								break;
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
