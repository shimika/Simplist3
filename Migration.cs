using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplist3 {
	class Migration {
		static string fileOldSeason = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimpListSeason.txt";
		static string fileOldList = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimpList.txt";
		static string fileOldSet = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimpListSet.txt";

		public static bool CheckMigration() {
			if (File.Exists(Setting.FileSetting)) { return false; }

			LoadSetting();
			LoadArchive();

			return true;
		}

		static string Divider = @"\('o')/";
		private static void LoadSetting() {
			if (!File.Exists(fileOldSet)) { return; }

			using (StreamReader sr = new StreamReader(fileOldSet)) {
				Setting.SaveDirectory = sr.ReadLine();
				Setting.Tray = false;
				Setting.Notification = false;

				if (Setting.SaveDirectory == "SaveForm ver.2") {
					string[] split;

					for (int i = 0; ; i++) {
						try {
							split = sr.ReadLine().Split(new string[] { Divider }, StringSplitOptions.RemoveEmptyEntries);
						} catch {
							break;
						}
						switch (split[0]) {
							case "DIR": Setting.SaveDirectory = split[1]; break;
							case "TRAY": Setting.Tray = Convert.ToBoolean(split[1]); break;
							case "NOTI": Setting.Notification = Convert.ToBoolean(split[1]); break;
						}
					}
				}
			}
		}

		public static void LoadArchive() {
			try {
				if (!File.Exists(fileOldList)) { return; }

				string[] strSplitList = null;
				using (StreamReader sr = new StreamReader(fileOldList)) {
					strSplitList = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				}

				foreach (string str in strSplitList) {
					ArchiveData adata = new ArchiveData();

					string[] str2 = str.Split(new string[] { " : ", @" \/ " }, StringSplitOptions.RemoveEmptyEntries);
					str2 = str2[0].Split(new string[] { " - ", @" /\ " }, StringSplitOptions.RemoveEmptyEntries);

					if (str2.Length == 2) {
						string[] str3 = str2[1].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
						if (str3.Length == 1) {
							adata.Episode = Convert.ToInt32(str3[0].Trim());
						} else {
							adata.Episode = Convert.ToInt32(str3[1].Trim());
						}
					} else {
						adata.Episode = -1;
					}

					adata.Title = str2[0].Trim();
					Data.DictArchive.Add(adata.Title, adata);
				}
			} catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}

			LoadSeason();
		}

		public static void LoadSeason() {
			if (!File.Exists(fileOldSet)) { return; }

			string[] strSplitSeason = null;
			using (StreamReader sr = new StreamReader(fileOldSeason)) {
				strSplitSeason = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			}

			for (int i = 0; i < strSplitSeason.Length; i++) {
				string[] strSplit = strSplitSeason[i].Split(new string[] { "	" }, StringSplitOptions.RemoveEmptyEntries);

				SeasonData sdata = new SeasonData();

				sdata.Title = strSplit[1];
				sdata.ArchiveTitle = strSplit[2];
				sdata.SearchTag = strSplit[3].Substring(1);
				sdata.Week = Convert.ToInt32(strSplit[0][0].ToString());
				sdata.TimeString = strSplit[0].Substring(2).Replace(":", "");

				if (!Data.DictArchive.ContainsKey(sdata.ArchiveTitle)) { continue; }
				Data.DictArchive[sdata.ArchiveTitle].SeasonTitle = sdata.Title;

				Data.DictSeason.Add(sdata.Title, sdata);
			}
		}
	}
}
