using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simplist3 {
	public partial class MainWindow : Window {
		int WeekDay = -1;
		private int RefreshWeekHead() {
			if (WeekDay == (int)DateTime.Now.DayOfWeek) { return 0; }

			WeekDay = (int)DateTime.Now.DayOfWeek;
			int height = 0;

			for (int i = 0; i < 7; i++) {
				(FindName(string.Format("containWeek{0}", i)) as Container)
					.SetWeekDay(i== WeekDay);

				if (i <= WeekDay) {
					int value = (FindName(string.Format("containWeek{0}", i)) as Container)
						.GetContainerHeight();
					height += value;

					if (i == WeekDay && value == 0) {
						height = 0;
					}
				}
			}

			return height;
		}

		private void AddSeason(string title, int weekday) {
			string archiveTitle = comboboxSync.SelectedValue as string;
			if (archiveTitle == null) {
				AddArchive(title, false);
				archiveTitle = title;
			}

			SeasonData data = new SeasonData();
			data.SetValue(title, weekday, textboxHour.Text, textboxMinute.Text,
				archiveTitle, textboxKeyword.Text.Trim());

			Data.DictArchive[archiveTitle].SeasonTitle = title;

			Data.DictSeason.Add(data.Title, data);
			(FindName(string.Format("containWeek{0}", weekday)) as Container).Add(true, data);
		}

		List<ComboBoxPairs> listContext;
		private void RefreshSync() {
			listContext = new List<ComboBoxPairs>();
			listContext.Add(new ComboBoxPairs("* 이 이름으로 추가", null));

			foreach (ArchiveData data in Data.DictArchive.Values.OrderBy(kvp => kvp.Title)) {
				if (data.SeasonTitle == null) {
					listContext.Add(new ComboBoxPairs(data.Title, data.Title));
				}
			}

			comboboxSync.DisplayMemberPath = "Key";
			comboboxSync.SelectedValuePath = "Value";
			comboboxSync.ItemsSource = listContext;

			comboboxSync.SelectedIndex = 0;
		}

		private void ModifySeason() {
			SeasonData data = Data.DictSeason[ModifyTag];
			DeleteSeason(ModifyTag);

			string newTitle = textboxTitle.Text.Trim();
			string arcTitle = data.ArchiveTitle;

			data.SetValue(newTitle, comboboxWeekday.SelectedIndex,
				textboxHour.Text, textboxMinute.Text,
				arcTitle, textboxKeyword.Text.Trim());

			Data.DictArchive[arcTitle].SeasonTitle = newTitle;
			Data.DictSeason.Add(data.Title, data);

			(FindName(string.Format("containWeek{0}", data.Week)) as Container).Add(true, data);

			RefreshArchiveEpisode(data.ArchiveTitle, textboxEpisode.Text);
		}

		private void DeleteSeason(string title) {
			SeasonData data = Data.DictSeason[title];

			(FindName(string.Format("containWeek{0}", data.Week)) as Container)
				.Delete(title);
		}

		private bool CheckInput() {
			if (textboxTitle.Text.Trim() == "") {
				Notice("제목을 입력해주세요", true);
				return false;
			}

			if (Tab == TabMode.Season) {
				try {
					switch (AddOpenMode) {
						case OpenMode.SeasonModify:
							if (textboxTitle.Text.Trim() != ModifyTag) {
								if (Data.DictSeason.ContainsKey(textboxTitle.Text.Trim())) {
									throw new Exception("이미 있는 제목입니다");
								}
							}
							break;
						case OpenMode.SeasonAdd:
							if (Data.DictSeason.ContainsKey(textboxTitle.Text.Trim())) {
								throw new Exception("이미 있는 제목입니다");
							}
							if (comboboxSync.SelectedValue == null && Data.DictArchive.ContainsKey(textboxTitle.Text.Trim())) {
								throw new Exception("아카이브에 있는 제목입니다");
							}
							break;
					}

					try {
						int h = Convert.ToInt32(textboxHour.Text);
						int m = Convert.ToInt32(textboxMinute.Text);

						if (h > 99 || m > 99 || h < 0 || m < 0) {
							throw new Exception();
						}
					} catch {
						throw new Exception("시간 형식이 맞지 않습니다");
					}
				} catch (Exception ex) {
					Notice(ex.Message, true);
					return false;
				}
			} else if (Tab == TabMode.Archive && AddOpenMode == OpenMode.ArchiveAdd) {
				if (Data.DictArchive.ContainsKey(textboxTitle.Text.Trim())) {
					Notice("이미 있는 제목입니다", true);
					return false;
				}
			} 
			
			if (AddOpenMode == OpenMode.ArchiveModify || AddOpenMode == OpenMode.SeasonModify) {
				if (gridEpisode.Visibility == Visibility.Collapsed) {
					textboxEpisode.Text = "-1";
					return true;
				}

				try {
					int e = Convert.ToInt32(textboxEpisode.Text);

					if (e < 0) {
						throw new Exception();
					}
				} catch {
					Notice("에피소드 형식이 맞지 않습니다", true);
					return false;
				}
			}

			return true;
		}

		private void textboxTitle_TextChanged(object sender, TextChangedEventArgs e) {
			if (AddOpenMode != OpenMode.SeasonAdd) { return; }

			string title = textboxTitle.Text.Trim().ToLower();
			if (title == "") {
				comboboxSync.SelectedIndex = 0;
				return;
			}

			int focusIndex = 0, maxValue = 0, matchCount, textLength = title.Length;
			string existString;

			for (int i = 1; i < comboboxSync.Items.Count; i++) {
				existString = (comboboxSync.Items[i] as ComboBoxPairs).Key.ToLower();
				int minLength = Math.Min(title.Length, existString.Length);

				matchCount = Function.StringPrefixMatch(title.Substring(0, minLength), existString.Substring(0, minLength));

				if (matchCount > maxValue && (matchCount * 2 >= textLength || matchCount == existString.Length)) {
					maxValue = matchCount;
					focusIndex = i;
				}
			}

			comboboxSync.SelectedIndex = focusIndex;
		}
	}
}
