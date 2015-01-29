using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void LoadSetting() {
			textVersion.Text = Setting.version;

			if (Directory.Exists(@"X:\Anime")) {
				Status.IsRoot = true;
			}

			if (Migration.CheckMigration()) {
				SaveSetting();
			} else {
				if (!File.Exists(Setting.FileSetting)) {
					SaveSetting();
				}

				using (StreamReader sr = new StreamReader(Setting.FileSetting)) {
					string text = sr.ReadToEnd();

					JsonTextParser parser = new JsonTextParser();
					JsonObjectCollection jsoncollection = (JsonObjectCollection)(parser.Parse(text));

					// Setting

					JsonObjectCollection jsonSetting = (JsonObjectCollection)jsoncollection["Setting"];
					foreach (JsonStringValue value in jsonSetting) {
						switch (value.Name) {
							case "SaveDirectory":
								Setting.SaveDirectory = value.Value;
								break;
							case "Tray":
								Setting.Tray = Convert.ToBoolean(value.Value);
								break;
							case "Notification":
								Setting.Notification = Convert.ToBoolean(value.Value);
								break;
						}
					}

					// Archive

					JsonArrayCollection jsonArchive = (JsonArrayCollection)jsoncollection["Archive"];
					foreach (JsonObjectCollection value in jsonArchive) {
						ArchiveData data = new ArchiveData();

						data.Title = value["Title"].GetValue().ToString();
						data.Episode = Convert.ToInt32(value["Episode"].GetValue());

						if (value["SeasonTitle"] != null) {
							data.SeasonTitle = value["SeasonTitle"].GetValue().ToString();
						}

						Data.DictArchive.Add(data.Title, data);
					}

					// Season

					JsonArrayCollection jsonSeason = (JsonArrayCollection)jsoncollection["Season"];
					foreach (JsonObjectCollection value in jsonSeason) {
						SeasonData data = new SeasonData();

						data.Title = value["Title"].GetValue().ToString();
						data.Week = Convert.ToInt32(value["Week"].GetValue());
						data.TimeString = value["TimeString"].GetValue().ToString();
						data.SearchTag = value["SearchTag"].GetValue().ToString();
						data.ArchiveTitle = value["ArchiveTitle"].GetValue().ToString();

						Data.DictSeason.Add(data.Title, data);
					}
				}
			}

			ApplySettingToControl();
			checkTray.Checked += SettingCheck_Changed;
			checkTray.Unchecked += SettingCheck_Changed;
			checkNoti.Checked += SettingCheck_Changed;
			checkNoti.Unchecked += SettingCheck_Changed;
		}

		private object locker = new object();
		private void SaveSetting() {
			JsonObjectCollection root = new JsonObjectCollection();

			JsonArrayCollection archive = new JsonArrayCollection("Archive");
			foreach (ArchiveData data in Data.DictArchive.Values.ToList()) {
				JsonObjectCollection obj = new JsonObjectCollection();

				obj.Add(new JsonStringValue("Title", data.Title));
				obj.Add(new JsonStringValue("Episode", data.Episode.ToString()));

				if (data.SeasonTitle != null) {
					obj.Add(new JsonStringValue("SeasonTitle", data.SeasonTitle));
				}

				archive.Add(obj);
			}

			JsonArrayCollection season = new JsonArrayCollection("Season");
			foreach (SeasonData data in Data.DictSeason.Values.ToList()) {
				JsonObjectCollection obj = new JsonObjectCollection();

				obj.Add(new JsonStringValue("Title", data.Title));
				obj.Add(new JsonStringValue("Week", data.Week.ToString()));
				obj.Add(new JsonStringValue("TimeString", data.TimeString));
				obj.Add(new JsonStringValue("SearchTag", data.SearchTag));

				if (data.ArchiveTitle != null) {
					obj.Add(new JsonStringValue("ArchiveTitle", data.ArchiveTitle));
				}

				season.Add(obj);
			}

			JsonObjectCollection setting = new JsonObjectCollection("Setting");
			setting.Add(new JsonStringValue("SaveDirectory", Setting.SaveDirectory));
			setting.Add(new JsonStringValue("Tray", Setting.Tray.ToString()));
			setting.Add(new JsonStringValue("Notification", Setting.Notification.ToString()));

			root.Add(archive);
			root.Add(season);
			root.Add(setting);

			lock (locker) {
				using (StreamWriter sw = new StreamWriter(Setting.FileSetting)) {
					sw.Write(root);
				}
			}
		}

		private void SettingCheck_Changed(object sender, RoutedEventArgs e) {
			Setting.Tray = (bool)checkTray.IsChecked;
			Setting.Notification = (bool)checkNoti.IsChecked;

			SaveSetting();
		}

		private void ApplySettingToControl() {
			checkTray.IsChecked = Setting.Tray;
			checkNoti.IsChecked = Setting.Notification;
		}
	}

	class Setting {
		public static string PathFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimpList\";
		public static string FileSetting = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimpList3.txt";

		public static string SaveDirectory = "";
		public static bool Tray = false, Notification = false;

		public static string version = "3.0.0";
	}
}
