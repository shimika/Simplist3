using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Json;
using System.Text;
using System.Threading.Tasks;

namespace Simplist3 {
	class Parser {
		public static List<ListData> Week(string weekCode) {
			List<ListData> list = new List<ListData>();
			int week = 0;
			try {
				week = Convert.ToInt32(weekCode);
			} catch { return list; }

			try {
				string result = Network.GetHtml(@"http://www.anissia.net/anitime/list?w=" + weekCode, "UTF-8");

				JsonTextParser parser = new JsonTextParser();
				JsonArrayCollection obj = (JsonArrayCollection)parser.Parse(result);

				foreach (JsonObjectCollection item in obj) {
					ListData sItem = new ListData() {
						Title = item["s"].GetValue().ToString(),
						Url = @"http://www.anissia.net/anitime/cap?i=" + item["i"].GetValue().ToString(),
						Memo = "anime", Time = item["t"].GetValue().ToString(),
						ID = item["i"].GetValue().ToString(),
					};
					list.Add(sItem);
				}
			} catch (Exception ex) {
				list.Clear();
			}

			return list;
		}
	}
}
