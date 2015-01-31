using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplist3 {
	public class SeasonData : IComparable<SeasonData> {
		public SeasonData() {
			this.ArchiveTitle = null;
		}

		public string Title { get; set; }
		public string ArchiveTitle { get; set; }

		public int Week { get; set; }
		public string TimeString { get; set; }
		public string Keyword { get; set; }

		public void SetValue(string title, int week, string hour, string minute, string arctitle, string keyword) {
			this.Title = title;
			this.Week = week;
			this.TimeString = string.Format("{0}{1}",
				hour.PadLeft(2, '0'),
				minute.PadLeft(2, '0'));
			this.ArchiveTitle = arctitle;
			this.Keyword = keyword;
		}

		public int CompareTo(SeasonData other) {
			int order = this.TimeString.CompareTo(other.TimeString);

			if (order == 0) {
				return this.Title.CompareTo(other.Title);
			}
			return order;
		}
	}

	public class ArchiveData : IComparable<ArchiveData> {
		public ArchiveData() {
			this.SeasonTitle = null;
		}

		public string Title { get; set; }
		public string SeasonTitle { get; set; }

		public int Episode { get; set; }

		public ListItem SeasonItem { get; set; }
		public ListItem ArchiveItem { get; set; }

		public int CompareTo(ArchiveData other) {
			return this.Title.CompareTo(other.Title);
		}
	}

	public class Listdata : IComparable<Listdata> {
		public Listdata() { this.Raw = false; }

		public string Title, Url, Type, Time, Tag, Week;
		public bool Raw;

		public int CompareTo(Listdata other) {
			return this.Time.CompareTo(other.Time) * -1;
		}
	}

	public class Data {
		public static Dictionary<string, SeasonData> DictSeason = new Dictionary<string, SeasonData>();
		public static Dictionary<string, ArchiveData> DictArchive = new Dictionary<string, ArchiveData>();
	}

	public class Status {
		public enum SelectMode { All, Unfinished };
		public static SelectMode NowSelect = SelectMode.Unfinished;

		public static bool IsRoot = false;
	}

	public class Pair {
		public Pair() { }
		public Pair(string first, object second) {
			this.First = first;
			this.Second = second;
		}

		public string First { get; set; }
		public object Second { get; set; }
	}
}
