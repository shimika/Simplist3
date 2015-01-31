using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private string ModifyTag;
		enum OpenMode { SeasonAdd, ArchiveAdd, SeasonModify, ArchiveModify, Invalid };
		OpenMode AddOpenMode = OpenMode.Invalid;

		private void AddOrModifyItem() {
			if (!CheckInput()) { return; }

			switch (AddOpenMode) {
				case OpenMode.SeasonAdd:
					AddSeason(textboxTitle.Text.Trim(), comboboxWeekday.SelectedIndex);
					break;

				case OpenMode.ArchiveAdd:
					AddArchive(textboxTitle.Text.Trim(), true);
					break;

				case OpenMode.SeasonModify:
					ModifySeason();
					break;

				case OpenMode.ArchiveModify:
					RefreshArchiveEpisode(ModifyTag, textboxEpisode.Text);
					break;
			}

			Setting.SaveSetting();
			HideAddModifyWindow();
		}

		private void DeleteItem() {
			switch (AddOpenMode) {
				case OpenMode.SeasonModify:
					DeleteSeason(ModifyTag);
					break;
				case OpenMode.ArchiveModify:
					string season = Data.DictArchive[ModifyTag].SeasonTitle;
					if (season != null) {
						DeleteSeason(season);
					}
					DeleteArchive(ModifyTag);
					break;
				default:
					return;
			}

			Setting.SaveSetting();
			HideAddModifyWindow();
		}

		private void ShowAddWindow() {
			Mode = ShowMode.Add;

			buttonDisable.ViewMode = ImageButton.Mode.Hidden;
			buttonDelete.ViewMode = ImageButton.Mode.Hidden;
			textCaution.Visibility = Visibility.Collapsed;
			gridEpisode.Visibility = Visibility.Collapsed;
			textboxTitle.IsEnabled = true;

			if (Tab == TabMode.Season) {
				RefreshSync();

				gridModifyExtra.Visibility = Visibility.Visible;
				textSync.Visibility = Visibility.Collapsed;
				comboboxSync.Visibility = Visibility.Visible;
				gridAniTable.Visibility = Visibility.Visible;

				AddOpenMode = OpenMode.SeasonAdd;
			} else {
				gridModifyExtra.Visibility = Visibility.Collapsed;
				gridAniTable.Visibility = Visibility.Collapsed;
				AddOpenMode = OpenMode.ArchiveAdd;
			}

			if (Status.Lite) {
				imageKeyword.Visibility = Visibility.Collapsed;
				textboxKeyword.Visibility = Visibility.Collapsed;
				textKeyword.Visibility = Visibility.Collapsed;
			}

			textboxTitle.Text = "";
			textboxHour.Text = "";
			textboxMinute.Text = "";
			textboxKeyword.Text = "";
			comboboxWeekday.SelectedIndex = 0;

			textboxTitle.Focus();

			SetTitlebar(TabMode.Add);
			gridAddModify.IsHitTestVisible = true;
			AnimateAddModifyWindow(1, 0);
		}

		private void ShowModifyWindow() {
			Mode = ShowMode.Modify;
			SetTitlebar(TabMode.Modify);

			gridAniTable.Visibility = Visibility.Collapsed;

			if (AddOpenMode == OpenMode.SeasonModify) {
				buttonDisable.ViewMode = ImageButton.Mode.Visible;
				buttonDelete.ViewMode = ImageButton.Mode.Visible;

				gridModifyExtra.Visibility = Visibility.Visible;
				textSync.Visibility = Visibility.Visible;
				comboboxSync.Visibility = Visibility.Collapsed;
				textCaution.Visibility = Visibility.Collapsed;
				textboxTitle.IsEnabled = true;

				SeasonData data = Data.DictSeason[ModifyTag];

				textboxTitle.Text = ModifyTag;
				comboboxWeekday.SelectedIndex = data.Week;
				textboxHour.Text = data.TimeString.Substring(0, 2);
				textboxMinute.Text = data.TimeString.Substring(2, 2);
				textSync.Text = data.ArchiveTitle;
				textboxKeyword.Text = data.Keyword;

				RefreshDisableButton(data.ArchiveTitle);

			} else if (AddOpenMode == OpenMode.ArchiveModify) {
				buttonDisable.ViewMode = ImageButton.Mode.Visible;
				buttonDelete.ViewMode = ImageButton.Mode.Visible;
				gridEpisode.Visibility = Visibility.Collapsed;

				if (Data.DictArchive[ModifyTag].SeasonTitle == null) {
					textCaution.Visibility = Visibility.Collapsed;
				} else {
					textCaution.Visibility = Visibility.Visible;
					textCaution.Text = string.Format("이 항목을 삭제하면 시즌 데이터의 항목도 삭제됩니다.\n\n\'{0}\'",
						Data.DictArchive[ModifyTag].SeasonTitle);
				}

				gridModifyExtra.Visibility = Visibility.Collapsed;
				textboxTitle.IsEnabled = false;

				textboxTitle.Text = ModifyTag;

				RefreshDisableButton(ModifyTag);
			} else {
				return;
			}

			textboxTitle.Focus();
			gridAddModify.IsHitTestVisible = true;
			AnimateAddModifyWindow(1, 0);
		}

		private void RefreshDisableButton(string arcTitle) {
			if (Data.DictArchive[arcTitle].Episode >= 0) {
				gridEpisode.Visibility = Visibility.Visible;
				textboxEpisode.Text = Data.DictArchive[arcTitle].Episode.ToString();
				buttonDisable.Source = "Resources/disable.png";
			} else {
				gridEpisode.Visibility = Visibility.Collapsed;
				buttonDisable.Source = "Resources/enable.png";
			}
		}

		private void HideAddModifyWindow() {
			Mode = ShowMode.None;
			AddOpenMode = OpenMode.Invalid;

			buttonAdd.Focus();

			SetTitlebar(Tab);
			gridAddModify.IsHitTestVisible = false;
			AnimateAddModifyWindow(0, -320);
		}

		private void gridAddModify_MouseDown(object sender, MouseButtonEventArgs e) {
			HideAddModifyWindow();
		}

		private void AnimateAddModifyWindow(double opacity, double leftMargin) {
			Storyboard sb = new Storyboard();

			DoubleAnimation da = new DoubleAnimation(opacity, TimeSpan.FromMilliseconds(250));
			ThicknessAnimation ta = new ThicknessAnimation(new Thickness(leftMargin, 0, 0, 0), TimeSpan.FromMilliseconds(350)) {
				EasingFunction = new ExponentialEase() {
					Exponent = 5,
					EasingMode = EasingMode.EaseOut,
				}
			};

			Storyboard.SetTarget(da, gridAddModify);
			Storyboard.SetTarget(ta, stackAddModify);
			Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
			Storyboard.SetTargetProperty(ta,  new PropertyPath(StackPanel.MarginProperty));

			sb.Children.Add(da);
			sb.Children.Add(ta);

			sb.BeginTime = TimeSpan.FromMilliseconds(opacity == 0 ? 0 : 100);
			sb.Begin(this);
		}

		private void button_EnterLeave(object sender, CustomButtonEventArgs e) {
			switch (e.Main) {
				case "disable":
					if (buttonDisable.Source == "Resources/enable.png") {
						textAddScript.Text = "활성화";
					} else {
						textAddScript.Text = "비활성화";
					}
					break;
				case "delete":
					textAddScript.Text = "삭제";
					break;
				case "save":
					textAddScript.Text = "저장";
					break;
				default:
					textAddScript.Text = "";
					return;
			}
		}
	}

	public class ComboBoxPairs {
		public string Key { get; set; }
		public string Value { get; set; }

		public ComboBoxPairs(string _key, string _value) {
			Key = _key;
			Value = _value;
		}
	}
}
