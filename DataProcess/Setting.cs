using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void LoadSetting() {
			textVersion.Text = Setting.Version;

			if (Directory.Exists(@"X:\Anime")) {
				Status.Root = true;
			}

			ClearFolder();

			if (Migration.CheckMigration()) {
				Setting.SaveSetting();
			} else {
				if (!File.Exists(Setting.FileSetting)) {
					Setting.SaveSetting();
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
						data.Keyword = value["Keyword"].GetValue().ToString();
						data.ArchiveTitle = value["ArchiveTitle"].GetValue().ToString();

						Data.DictSeason.Add(data.Title, data);
					}
				}
			}

			ApplySettingToControl();
			InitTray();

			checkTray.Checked += SettingCheck_Changed;
			checkTray.Unchecked += SettingCheck_Changed;
			checkNotify.Checked += SettingCheck_Changed;
			checkNotify.Unchecked += SettingCheck_Changed;
		}

		private void ClearFolder() {
			if (!Directory.Exists(Setting.PathFolder)) {
				Directory.CreateDirectory(Setting.PathFolder);
			}

			string[] fileNames = Directory.GetFiles(Setting.PathFolder);
			foreach (string fileName in fileNames) {
				try {
					File.Delete(fileName);
				} catch { }
			}
		}

		private void SettingCheck_Changed(object sender, RoutedEventArgs e) {
			Setting.Tray = (bool)checkTray.IsChecked;
			Setting.Notification = (bool)checkNotify.IsChecked;

			Setting.SaveSetting();
		}

		private void ApplySettingToControl() {
			checkTray.IsChecked = Setting.Tray;
			checkNotify.IsChecked = Setting.Notification;
		}

		private void CheckLite() {
			if (!Status.Lite) { return; }

			gridNormalTab.ColumnDefinitions.Clear();

			for (int i = 0; i < 3; i++) {
				gridNormalTab.ColumnDefinitions.Add(
					new ColumnDefinition() {
						Width = new GridLength(1, GridUnitType.Star)
					});
			}

			tabNotify.ViewMode = TabButton.Mode.Hidden;
			Grid.SetColumn(tabSetting, 2);

			checkNotify.Visibility = Visibility.Collapsed;
		}
	}

	class Setting {
		public static string PathFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimpList\";
		public static string FileSetting = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimpList3.txt";

		public static string SaveDirectory = "";
		public static bool Tray = false, Notification = false;

		public static string Version = "3.0.1";

		private static object locker = new object();
		public static void SaveSetting() {
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
				obj.Add(new JsonStringValue("Keyword", data.Keyword));

				if (data.ArchiveTitle != null) {
					obj.Add(new JsonStringValue("ArchiveTitle", data.ArchiveTitle));
				}

				season.Add(obj);
			}

			JsonObjectCollection setting = new JsonObjectCollection("Setting");
			setting.Add(new JsonStringValue("SaveDirectory", SaveDirectory));
			setting.Add(new JsonStringValue("Tray", Tray.ToString()));
			setting.Add(new JsonStringValue("Notification", Notification.ToString()));

			root.Add(archive);
			root.Add(season);
			root.Add(setting);

			lock (locker) {
				using (StreamWriter sw = new StreamWriter(FileSetting)) {
					sw.Write(root);
				}
			}
		}
	}
}
