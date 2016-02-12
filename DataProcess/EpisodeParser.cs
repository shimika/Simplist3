using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplist3 {
	public class EpisodeParse {
		static void pushList(string s, bool isNumber, List<EpisodeData> list) {
			if (s == "") { return; }
			EpisodeData data = new EpisodeData();

			if (isNumber) {
				data.SetNumber(s);
			}
			else {
				data.SetString(s);
			}

			list.Add(data);
		}

		public static int parseEpisode(string a, int episode) {
			List<EpisodeData> list = new List<EpisodeData>();
			string s = "";
			bool isNumber = false;

			for (int i = 0; i < a.Length; i++) {
				if ((Char.IsNumber(a[i]) && !isNumber) || (!Char.IsNumber(a[i]) && isNumber)) {
					pushList(s, isNumber, list);
					s = "";
				}

				s += a[i];
				isNumber = Char.IsNumber(a[i]);
			}
			pushList(s, isNumber, list);

			int min = 30;
			int value = episode;
			int count = 0;

			for (int i = 0; i < list.Count; i++) {
				if (!list[i].IsNumber()) { continue; }
				count++;

				int diff = (int)Math.Abs(list[i].GetNumber() - episode) + 1;
				int cost = diff * diff - count;
				if (min > cost) {
					min = cost;
					value = list[i].GetNumber();
				}
			}

			return value;
		}
	}

	class EpisodeData {
		public bool isNumber;
		string str;
		int num;

		public bool IsNumber() { return this.isNumber; }

		public int GetNumber() { return this.num; }
		public void SetNumber(string s) {
			this.isNumber = true;
			this.num = Convert.ToInt32(s);
		}

		public string GetString() { return this.str; }
		public void SetString(string s) {
			this.isNumber = false;
			this.str = s;
		}
	}
}

