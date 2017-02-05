using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Xml;

namespace Simplist3 {
	class Parser {
		public static List<Listdata> Week(string weekCode, string type) {
			List<Listdata> list = new List<Listdata>();
			int week = 0;
			try {
				week = Convert.ToInt32(weekCode);
			} catch { return list; }

			try {
				string result = Network.GET(@"http://www.anissia.net/anitime/list?w=" + weekCode);

				JsonTextParser parser = new JsonTextParser();
				JsonArrayCollection obj = (JsonArrayCollection)parser.Parse(result);

				foreach (JsonObjectCollection item in obj) {
					Listdata listitem = new Listdata() {
						Title = item["s"].GetValue().ToString(),
						Url = string.Format("{0}{1}", "http://www.anissia.net/anitime/cap?i=", item["i"].GetValue()),
						Type = type, Time = item["t"].GetValue().ToString(),
						Tag = item["i"].GetValue().ToString(),
					};

					list.Add(listitem);
				}
			} catch (Exception ex) {
				list.Clear();
			}

			return list;
		}

		public static List<Listdata> GetTorrentList(string keyword) {
			List<Listdata> list = new List<Listdata>();

			string url1 = "http://www.nyaa.se/?page=rss&cats=1_0&term=";
			string url2 = "http://www.nyaa.se/?page=rss&cats=1_11&term=";

			try {
				int count = 0;
				string result = "";

				if (Setting.ShowRaws) {
					result = Network.GET(string.Format("{0}{1}", url2, keyword));
				}
				else {
					result = Network.GET(string.Format("{0}{1}", url1, keyword));
				}

				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(result);
				XmlNodeList xmlnode = xmlDoc.SelectNodes("rss/channel/item");

				foreach (XmlNode node in xmlnode) {
					Listdata data = new Listdata() {
						Title = node["title"].InnerText, Url = node["link"].InnerText,
						Raw = node["category"].InnerText == "Anime - Raw",
						Type = "Torrent",
					};

					list.Add(data);
					if (++count >= 30) { break; }
				}
			} catch (Exception ex) {
				list.Clear();
			}

			return list;
		}

		public static List<Listdata> GetMakerList(string url, bool noti) {
			List<Listdata> listMaker = new List<Listdata>();

			try {
				string result = Network.GET(url);

				JsonTextParser parser = new JsonTextParser();
				JsonArrayCollection objCollection = (JsonArrayCollection)parser.Parse(result);

				foreach (JsonObjectCollection item in objCollection) {
					Listdata data = GetListData(item, noti);
					if (noti) { data.Url = url; }


					listMaker.Add(data);
				}

				listMaker.Sort();
			} catch (Exception ex) {
				listMaker.Clear();
			}

			return listMaker;
		}

		private static Listdata GetListData(JsonObjectCollection item, bool noti) {
			Listdata data = new Listdata();
			string episode = item["s"].GetValue().ToString();
			bool isnum = false;

			if (episode.Length == 5) {
				isnum = true;
				int epinum = Convert.ToInt32(episode);

				if (epinum % 10 != 0) {
					episode = string.Format("{0:D2}.{1}", epinum / 10, epinum % 10);
				} else {
					episode = string.Format("{0:D2}", epinum / 10);
				}
			}

			data.Tag = episode;
			data.Url = item["a"].GetValue().ToString();
			data.Type = "Maker";

			if (noti) {
				data.Time = item["d"].GetValue().ToString();
				data.Title = item["n"].GetValue().ToString();
			} else {
				data.Title = string.Format("{0}{1} {2}",
					episode,
					isnum ? "화" : "",
					item["n"].GetValue());
				data.Time = string.Format("{0}{1}", item["s"].GetValue().ToString(), item["d"].GetValue().ToString());
			}

			try {
				Uri uri = new UriBuilder(data.Url).Uri;
				string host = uri.Host;

				if (host.IndexOf("naver.com") >= 0 || host.IndexOf("naver.com") >= 0) {
					data.SiteType = 1;
				} else if (host.IndexOf("tistory") >= 0) {
					data.SiteType = 2;
				} else if (host.IndexOf("egloos") >= 0) {
					data.SiteType = 3;
				} else if (host.IndexOf("fc2") >= 0) {
					data.SiteType = 4;
				} else {
					data.SiteType = 0;
				}
			} catch { }

			return data;
		}

		enum SiteType { Naver, Other };
		public static List<Listdata> GetFileList(string url, bool expand = true) {
			string result = Network.GET(url);
			bool isHakerano = url.IndexOf("hakerano") >= 0;

			if (result == "") { return new List<Listdata>(); }

			Dictionary<SiteType, int> DictCount = new Dictionary<SiteType, int>();
			DictCount[SiteType.Naver] = Regex.Matches(result, "naver", RegexOptions.IgnoreCase).Cast<Match>().Count();
			DictCount[SiteType.Other] = Regex.Matches(result, "tistory", RegexOptions.IgnoreCase).Cast<Match>().Count();
			DictCount[SiteType.Other] += Regex.Matches(result, "egloos", RegexOptions.IgnoreCase).Cast<Match>().Count();

			SiteType sitetype = SiteType.Other;
			if (DictCount[SiteType.Naver] > DictCount[SiteType.Other]) { sitetype = SiteType.Naver; }

			if (sitetype == SiteType.Naver) {
				result = Network.GET(url, "EUC-KR");
				string nURL = "";

				if (result.IndexOf("mainFrame") < 0 && result.IndexOf("aPostFiles") < 0) {
					int nIndex = result.IndexOf("screenFrame");

					if (nIndex >= 0) {
						for (int i = result.IndexOf("http://blog.naver.com/", nIndex); ; i++) {
							if (result[i] == '\"') { break; }
							nURL += result[i];
						}
						if (nURL != "") {
							url = nURL;
							result = Network.GET(url, "EUC-KR");
						}
					}
				}

				if (result.IndexOf("mainFrame") >= 0 && result.IndexOf("aPostFiles") < 0) {
					int nIndex = result.IndexOf("mainFrame");
					nIndex = result.IndexOf("src", nIndex);
					bool flag = false;
					nURL = "";
					for (int i = nIndex; ; i++) {
						if (result[i] == '\"') {
							if (flag) {
								break;
							} else {
								flag = true;
								continue;
							}
						}
						if (flag) { nURL += result[i]; }
					}
					if (nURL[0] == '/') { nURL = "http://blog.naver.com" + nURL; }
					if (nURL != "") { url = nURL; }
				}

				result = Network.GET(url);
			}

			List<Listdata> list = null;

			if (isHakerano) {
				list = Fc2Parse(result);
			} else if (sitetype == SiteType.Naver) {
				list = NaverParse(Network.GET(url, "EUC-KR"));
			} else {
				list = TistoryParse(result);
			}

			if (sitetype == SiteType.Naver && expand) {
				list.Add(new Listdata("이전 자막 보기", "Expand", url));
			}
			list.Add(new Listdata("블로그로 이동", "Blog", url));

			return list;
		}

		private static List<Listdata> NaverParse(string html) {
			List<Listdata> listData = new List<Listdata>();
			int sIndex = 0, eIndex = 0;
			string attachString;

			string msg = "";
			for (; ; ) {
				sIndex = html.IndexOf("aPostFiles[", sIndex);
				if (sIndex < 0) { break; }
				sIndex = html.IndexOf("[{", sIndex);
				if (sIndex < 0) { break; }
				eIndex = html.IndexOf("}];", sIndex);
				if (eIndex < 0) { break; }

				attachString = html.Substring(sIndex + 2, eIndex - sIndex).Replace("\"", "\'");

				int flag = 0;
				string sKey = "", sValue = "";
				string fileName = "", fileURL = "";

				for (int i = 0; i < attachString.Length; i++) {
					if (attachString[i] == '\\' && i + 1 != attachString.Length) {
						if (attachString[i + 1] == '\'') {
							if (flag == 1) {
								sKey += '\'';
							} else if (flag == 3) {
								sValue += '\'';
							}
							i++; continue;
						}
					}
					if (attachString[i] == '\'') {
						flag++; continue;
					}

					switch (flag) {
						case 1: sKey += attachString[i]; break;
						case 3: sValue += attachString[i]; break;
						case 4:
							sKey = sKey.Trim(); sValue = sValue.Trim();
							if (sKey == "encodedAttachFileName") { fileName = sValue; }
							if (sKey == "encodedAttachFileUrl") { fileURL = sValue; }

							msg += sKey + " = " + sValue + "\n";
							sKey = sValue = "";
							flag = 0;

							break;
					}
					if (attachString[i] == '}') {
						if (fileName != "" && fileURL != "") {
							listData.Add(new Listdata() { 
								Title = fileName, 
								Url = fileURL, Type = "File" 
							});
							fileName = fileURL = "";
						}
					}
				}

				msg += "\n";
			}
			return listData;
		}
		private static List<Listdata> TistoryParse(string html) {
			List<Listdata> listData = new List<Listdata>();
			int nIndex = 0, lastIndex = 0;
			string[] ext = new string[] { "zip", "rar", "7z", "egg", "smi" };
			List<int> lst = new List<int>();
			string fileName, fileURL;

			try {
				for (; ; ) {
					lst.Clear();
					foreach (string str in ext) {
						nIndex = html.IndexOf(string.Format(".{0}\"", str), lastIndex, StringComparison.OrdinalIgnoreCase);
						if (nIndex < 0) { nIndex = 999999999; }
						lst.Add(nIndex);
					}
					lst.Sort();
					if (lst[0] == 999999999) { break; }
					lastIndex = html.IndexOf("\"", lst[0]);
					fileURL = "";

					for (int i = lastIndex - 1; i >= 0; i--) {
						if (html[i] == '\"') { break; }
						fileURL = html[i] + fileURL;
					}

					fileName = Network.GetFilenameFromURL(fileURL);

					if (fileName != "" && fileURL != "") {
						listData.Add(new Listdata() { 
							Title = fileName, 
							Url = fileURL, 
							Type = "File" 
						});
					}
				}
			} catch (Exception ex) {
			}
			return listData;
		}
		private static List<Listdata> Fc2Parse(string html) {
			List<Listdata> listData = new List<Listdata>();

			int sIndex = 0;
			string str;

			for (; ; ) {
				try {
					sIndex = html.IndexOf("<img", sIndex + 1);
					if (sIndex < 0) { break; }
					int eIndex = html.IndexOf(">", sIndex + 1);
					str = html.Substring(sIndex, eIndex - sIndex + 1);
				} catch (Exception ex) {
					continue;
				}

				if (str.IndexOf("/>") < 0) {
					str = string.Format("{0}</img>", str);
				}

				try {
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(str);
					XmlElement root = xmlDoc.DocumentElement;

					if (!root.HasAttribute("alt") && !root.HasAttribute("src") && !root.HasAttribute("border")) { continue; }
					if (root.HasAttribute("width") || root.HasAttribute("height") || root.HasAttribute("style")) { continue; }

					string alt = root.Attributes["alt"].Value;
					string src = root.Attributes["src"].Value;

					listData.Add(new Listdata() {
						Title = alt,
						Url = src,
						Type = "File"
					});
				} catch (Exception ex) {
					//MessageBox.Show(ex.Message + " : " + "\n" + str);
					continue;
				}
			}

			return listData;
		}

		public static List<Listdata> ParseZip(string path) {
			List<Listdata> listFiles = new List<Listdata>();

			try {
				using (ZipArchive archive = ZipFile.OpenRead(path)) {
					foreach (ZipArchiveEntry entry in archive.Entries) {
						listFiles.Add(new Listdata() {
							Title = entry.FullName, Url = path, Type = "InnerZip",
						});
					}
				}
			} catch { listFiles.Clear(); }

			listFiles.Add(new Listdata() {
				Title = "압축 파일 열기", Url = path,
				Type = "OpenZip",
			});

			return listFiles;
		}

		public static List<Listdata> ExpandNaverBlog(string url) {
			List<Listdata> list = new List<Listdata>();
			try {
				string result = Network.GET(url, "EUC-KR");

				Uri uri = new UriBuilder(url).Uri;
				NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
				string blogId = query.Get("blogId");
				string logNo = query.Get("logNo");

				string extract = Function.GetSubstring(result, "postViewBottomTitleListController.start", "</script>");
				if (extract == null) {
					throw new Exception();
				}

				string[] split = extract.Split(',');
				string sortDateInMilli = "";

				if (split.Length > 7) {
					sortDateInMilli = split[7].Replace("\'", "").Trim();
				}
				else {
					throw new Exception();
				}

				string categoryNo = "";
				string parentCategoryNo = "";

				extract = Function.GetSubstring(result, "var categoryNo", ";");
				categoryNo = extract.Split(new char[] { '=', ';' })[1].Replace("\'", "").Trim();

				extract = Function.GetSubstring(result, "var parentCategoryNo", ";");
				parentCategoryNo = extract.Split(new char[] { '=', ';' })[1].Replace("\'", "").Trim();


				Dictionary<string, string> dict = new Dictionary<string, string>();
				dict.Add("blogId", blogId);
				dict.Add("logNo", logNo);
				dict.Add("viewDate", "");
				dict.Add("categoryNo", categoryNo == "0" ? "" : categoryNo);
				dict.Add("parentCategoryNo", parentCategoryNo);
				dict.Add("showNextPage", "false");
				dict.Add("showPreviousPage", "false");
				dict.Add("sortDateInMilli", sortDateInMilli);

				string data = Function.GetHttpParams(dict);
				JsonTextParser parser = new JsonTextParser();

				List<string> hash = new List<string>();

				for (int i = 0; i < 6; i++) {
					result = Network.POST("http://blog.naver.com/PostViewBottomTitleListAsync.nhn", data);
					result = HttpUtility.UrlDecode(result);

					//MessageBox.Show(String.Format("blogId: {0}\nlogNo: {1}\ncatogoryNo: {2}\nparentCategoryNo: {3}\nsortDateInMilli: {4}\n\n{5}",
						//blogId, logNo, categoryNo, parentCategoryNo, sortDateInMilli, result));

					JsonObjectCollection collect = (JsonObjectCollection)parser.Parse(result);
					JsonArrayCollection array = (JsonArrayCollection)collect["postList"];

					bool lastPage = false;
					foreach (JsonObjectCollection obj in array) {
						string l = obj["logNo"].GetValue().ToString();
						if (hash.Contains(l)) {
							lastPage = true;
							break;
						}
						hash.Add(l);
					}

					if (lastPage) {
						dict["showPreviousPage"] = "false";
						data = Function.GetHttpParams(dict);
						continue;
					}

					foreach (JsonObjectCollection obj in array) {
						Listdata item = new Listdata(
							obj["title"].GetValue().ToString(),
							"Maker2",
							string.Format("http://blog.naver.com/{0}/{1}", blogId, obj["logNo"].GetValue()));

						list.Add(item);
					}

					bool hasNextPage = Convert.ToBoolean(collect["hasNextPage"].GetValue());

					if (hasNextPage) {
						bool hasPreviousPage = Convert.ToBoolean(collect["hasPreviousPage"].GetValue());
						string nextIndexLogNo = collect["nextIndexLogNo"].GetValue().ToString();
						string nextIndexSortDate = collect["nextIndexSortDate"].GetValue().ToString();

						dict["logNo"] = nextIndexLogNo;
						dict["sortDateInMilli"] = nextIndexSortDate;
						dict["showNextPage"] = hasNextPage.ToString().ToLower();
						dict["showPreviousPage"] = hasPreviousPage.ToString().ToLower();

						data = Function.GetHttpParams(dict);
					}
					else {
						break;
					}
				}
			}
			catch { }

			return list;
		}
	}
}
