﻿using System;
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
		public string SearchTag { get; set; }

		public void SetValue(string title, int week, string hour, string minute, string arctitle, string search) {
			this.Title = title;
			this.Week = week;
			this.TimeString = string.Format("{0}{1}",
				hour.PadLeft(2, '0'),
				minute.PadLeft(2, '0'));
			this.ArchiveTitle = arctitle;
			this.SearchTag = search;
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

	public class Data {
		public static Dictionary<string, SeasonData> DictSeason = new Dictionary<string, SeasonData>();
		public static Dictionary<string, ArchiveData> DictArchive = new Dictionary<string, ArchiveData>();
	}

	public class Status {
		public enum SelectMode { All, Unfinished };
		public static SelectMode NowSelect = SelectMode.Unfinished;

		public static bool IsRoot = false;
	}
}
