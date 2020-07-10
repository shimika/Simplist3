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
using HtmlAgilityPack;

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

		public static List<Listdata> GetTorrentListOhys(string keyword) {
			List<Listdata> list = new List<Listdata>();

			string urlOhys = "https://torrents.ohys.net/download/rss.php?dir=new&q=";

			string[] urls = new string[] { urlOhys };

			foreach (string url in urls) {
				try {
					int count = 0;
					string result = result = Network.GET(string.Format("{0}{1}", url, keyword));

					if (string.IsNullOrEmpty(result)) {
						continue;
					}

					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(result);
					XmlNodeList xmlnode = xmlDoc.SelectNodes("rss/channel/item");

					foreach (XmlNode node in xmlnode) {
						Listdata data = new Listdata() {
							Title = node["title"].InnerText,
							Url = node["link"].InnerText,
							Raw = true,
							Type = "Torrent",
						};

						list.Add(data);
						if (++count >= 30) { break; }
					}
				}
				catch (Exception ex) {
				}
			}

			return list;
		}

		public static List<Listdata> GetTorrentListNyaa(string keyword) {
			List<Listdata> list = new List<Listdata>();

			string url1 = "https://www.nyaa.si/?page=rss&cats=1_0&term=";
			string url2 = "https://www.nyaa.si/?page=rss&cats=1_4&term=";

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
						Raw = node["nyaa:categoryId"].InnerText == "1_4",
						Type = "Torrent",
					};

					list.Add(data);
					if (++count >= 30) { break; }
				}
			} catch (Exception ex) {
				// MessageBox.Show(ex.Message);
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

				if (host.IndexOf("naver.com") >= 0 || host.IndexOf("blog.me") >= 0) {
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

		static string[] naver = new string[] { "naver.com", "blog.me" };
		static string[] other = new string[] { "tistory.com", "egloos.com", "blogspot.kr" };

		static SiteType getSiteType(string url, string result) {
			try {
				if (!url.StartsWith("http")) {
					url = string.Format("http://{0}", url);
				}

				Uri uri = new Uri(url);
				string host = uri.Host;

				if (naver.Where(x => host.IndexOf(x) >= 0).Count() > 0) {
					return SiteType.Naver;
				}
				if (other.Where(x => host.IndexOf(x) >= 0).Count() > 0) {
					return SiteType.Other;
				}
			} catch (Exception ex) {
			}

			Dictionary<SiteType, int> DictCount = new Dictionary<SiteType, int>();
			DictCount[SiteType.Naver] = Regex.Matches(result, "naver", RegexOptions.IgnoreCase).Cast<Match>().Count();
			DictCount[SiteType.Other] = Regex.Matches(result, "tistory", RegexOptions.IgnoreCase).Cast<Match>().Count();
			DictCount[SiteType.Other] += Regex.Matches(result, "egloos", RegexOptions.IgnoreCase).Cast<Match>().Count();

			SiteType sitetype = SiteType.Other;
			if (DictCount[SiteType.Naver] > DictCount[SiteType.Other]) { sitetype = SiteType.Naver; }

			return sitetype;
		}

		enum SiteType { Naver, Other };
		public static List<Listdata> GetFileList(string url, bool expand = true) {
			string result = Network.GET(url);
			bool isHakerano = url.IndexOf("hakerano") >= 0;

			if (result == "") {
				return new List<Listdata>() { new Listdata("블로그로 이동", "Blog", url) };
			}

			SiteType sitetype = getSiteType(url, result);

			if (sitetype == SiteType.Naver) {
				result = Network.GET(url, "UTF-8");

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
							result = Network.GET(url, "UTF-8");
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
				list = NaverParse(Network.GET(url, "UTF-8"));
			} else {
				list = TistoryParse(result);
			}

			list.AddRange(parseGoogleDrive(result));

			if (sitetype == SiteType.Naver && expand) {
				list.Add(new Listdata("이전 자막 보기", "Expand", url));
			}
			list.Add(new Listdata("블로그로 이동", "Blog", url));

			return list;
		}

		private static List<Listdata> parseGoogleDrive(string html) {
			List<Listdata> listData = new List<Listdata>();

			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(html);

			HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//a[contains(@href, 'drive.google.com')]");

			if (nodeList != null) {
				foreach (HtmlNode node in nodeList) {
					try {
						string content = node.InnerText;
						string url = node.GetAttributeValue("href", "");

						if (!url.Contains("folder")) {
							listData.Add(new Listdata() {
								Title = content,
								Url = url,
								Type = "File"
							});
						}
					}
					catch { }
				}
			}

			return listData;
		}

		private static List<Listdata> NaverParse(string html) {
			List<Listdata> listData = new List<Listdata>();
			int sIndex = 0, eIndex = 0;

			try {
				sIndex = html.IndexOf("aPostFiles[", sIndex);
				if (sIndex < 0) { throw new Exception(); }
				sIndex = html.IndexOf("[{", sIndex);
				if (sIndex < 0) { throw new Exception(); }
				eIndex = html.IndexOf("}]", sIndex);
				if (eIndex < 0) { throw new Exception(); }

				string jsonString = html.Substring(sIndex, eIndex - sIndex + 2)
					.Replace("\\'", "\"")
					.Replace("\\\\", "\\");

				MessageBox.Show(jsonString);

				JsonTextParser parser = new JsonTextParser();
				JsonArrayCollection files = (JsonArrayCollection)parser.Parse(jsonString);

				foreach (JsonObjectCollection file in files) {
					listData.Add(new Listdata() {
						Title = Regex.Unescape(file["encodedAttachFileName"].GetValue().ToString()),
						Url = file["encodedAttachFileUrl"].GetValue().ToString(),
						Type = "File"
					});
				}
			}
			catch (Exception ex) {
			}
			return listData;
		}

		private static List<Listdata> TistoryParse(string html) {
			string[] part = new string[] { ".zip", ".rar", ".7z", ".egg", ".smi", "drive.google.com" };
			List<Listdata> listData = new List<Listdata>();
			try {
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(html);

				HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//a");
				foreach (HtmlNode node in nodeList) {
					string href = node.GetAttributeValue("href", "");
					foreach (string str in part) {
						if (href.Contains(str)) {
							string title = node.InnerText;
							int extensionIndex = title.IndexOf(str);

							if (extensionIndex >= 0) {
								title = title.Substring(0, extensionIndex + str.Length);
							}

							listData.Add(new Listdata() {
								Title = title,
								Url = href,
								Type = "File"
							});
						}
					}
				}
			}
			catch { }

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

		public static List<Listdata> getNaverCategoryPostList(string url) {
			List<Listdata> list = new List<Listdata>();

			try {
				string html = Network.GET(url, "UTF-8");

				// Blog id

				Uri uri = new UriBuilder(url).Uri;
				NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
				string blogId = query.Get("blogId");
				string logNo = query.Get("logNo");
				string categoryNo = query.Get("categoryNo");
				string parentCategoryNo = query.Get("parentCategoryNo");

				// etc

				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(html);

				if (logNo == null) {
					string copyLinkIdPrefix = string.Format("url_{0}_", blogId);
					HtmlNode copyLinkNode = doc.DocumentNode.SelectSingleNode(string.Format("//p[contains(@id, '{0}')]", copyLinkIdPrefix));
					if (copyLinkNode != null) {
						logNo = copyLinkNode.Id.Replace(copyLinkIdPrefix, "").Trim();
					}
					else {
						return list;
					}
				}

				if (categoryNo == null) {
					categoryNo = getJsValue(html, "categoryNo");
				}

				if (parentCategoryNo == null) {
					parentCategoryNo = getJsValue(html, "parentCategoryNo");
				}

				Dictionary<string, string> dict = new Dictionary<string, string>() {
					{ "blogId", blogId },
					{ "logNo", logNo },
					{ "viewDate", "" },
					{ "categoryNo", categoryNo == "0" ? "" : categoryNo },
					{ "parentCategoryNo", parentCategoryNo },
					{ "showNextPage", "true" },
					{ "showPreviousPage", "false" },
					{ "sortDateInMilli", "" }
				};

				string data = Function.GetHttpParams(dict);

				JsonTextParser parser = new JsonTextParser();

				for (int i = 0; i < 3; i++) {
					string result = Network.POST("http://blog.naver.com/PostViewBottomTitleListAsync.nhn", data);
					if (result == "") { break; }

					JsonObjectCollection collect = (JsonObjectCollection)parser.Parse(result);
					JsonArrayCollection array = (JsonArrayCollection)collect["postList"];

					foreach (JsonObjectCollection obj in array) {
						Listdata item = new Listdata(
							HttpUtility.HtmlDecode(HttpUtility.UrlDecode(obj["filteredEncodedTitle"].GetValue().ToString())),
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

						data = Function.GetHttpParams(dict);
					}
					else {
						break;
					}
				}
			}
			catch (Exception ex) {
			}
			return list;
		}

		private static string getJsValue(string html, string id) {
			string result = Function.GetSubstring(html, string.Format("var {0}", id), ";");
		
			if (result == null) { return null; }

			string[] split= result.Split(new char[] { '=', ';' });
			if (split.Length > 1) {
				return split[1].Replace("\"", "").Replace("\'", "").Trim();
			}
			return null;
		}
	}
}
