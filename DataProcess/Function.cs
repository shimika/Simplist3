using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Simplist3 {
	class Function {
		public static int StringPrefixMatch(string s1, string s2) {
			int i, n = Math.Min(s1.Length, s2.Length), m;
			m = n;

			for (i = 0; i < n; i++) {
				if (s1[i] != s2[i]) {
					m = i;
					break;
				}
			}
			return m;
		}

		public static int StringMatching(string s1, string s2) {
			int[,] a = new int[s1.Length + 1, s2.Length + 1];

			for (int i = 0; i <= s1.Length; i++) {
				for (int j = 0; j <= s2.Length; j++) {
					if (i == 0 || j == 0) {
						if (i != 0) {
							a[i, j] = a[i - 1, j] + 1;
						} else if (j != 0) {
							a[i, j] = a[i, j - 1] + 1;
						}
					} else {
						if (s1[i - 1] == s2[j - 1]) {
							a[i, j] = a[i - 1, j - 1];
						} else {
							a[i, j] = Math.Min(a[i - 1, j], a[i, j - 1]) + 1;
						}
					}
				}
			}

			return a[s1.Length, s2.Length];
		}

		public static string GetSubstring(string original, string start, string end) {
			int s = original.IndexOf(start);
			int e = original.IndexOf(end, Math.Max(s, 0));

			if (s < 0 || e < 0) { return null; }

			return original.Substring(s, e - s + end.Length);
		}

		public static string GetHttpParams(Dictionary<string, string> list) {
			StringBuilder builder = new StringBuilder();
			foreach (KeyValuePair<string, string> kvp in list) {
				builder.Append(string.Format("{0}={1}&", kvp.Key, kvp.Value));
			}
			return builder.ToString();
		}

		public static Pair TitleMatching(List<Listdata> list, string subtitle) {
			int max = -1, min = 9999;
			string url1 = null, title1 = null;
			string url2 = null, title2 = null;

			foreach (Listdata data in list) {
				int prefix = Function.StringPrefixMatch(subtitle.Replace(" ", ""), data.Title.Replace(" ", ""));
				if (prefix == subtitle.Length || prefix == data.Title.Length) {
					return new Pair(data.Title, data.Url);
				}

				int match = Function.StringMatching(subtitle, data.Title);

				if (max < prefix) {
					max = prefix;
					title1 = data.Title;
					url1 = data.Url;
				}

				if (min < match) {
					min = match;
					title2 = data.Title;
					url2 = data.Url;
				}
			}

			if (max >= subtitle.Length / 2) {
				return new Pair(title1, url1);
			}
			if (min < subtitle.Length / 3) {
				return new Pair(title2, url2);
			}

			return null;
		}

		public static string GetWeekday(string w) {
			string str = "일월화수목금토";
			try {
				int i = Convert.ToInt32(w);
				if (i < 0 || i >= 7) { throw new Exception(); }

				return string.Format("{0}요일", str[i]);
			} catch {
				return "";
			}
		}

		public static string CleanFileName(string fileName) {
			return Path.GetInvalidFileNameChars().Aggregate(
				fileName,
				(current, c) => current.Replace(c.ToString(), string.Empty));
		}

		public static void ExecuteFile(string path) {
			try {
				Process pro = new Process() {
					StartInfo = new ProcessStartInfo() {
						FileName = path
					}
				};
				pro.Start();
			} catch { }
		}

		public static string SaveFile(string path, string filename, string title) {
			title = Function.CleanFileName(title);
			int num = FindNumberFromString(filename);
			if (num < 0) {
				num = FindNumberFromString(Path.GetFileName(path));
			}

			string ext = Path.GetExtension(path).ToLower();
			string savename = "";

			if (ext == ".smi" || ext == ".ass") {
				savename = string.Format("{0}{1}", title, num, ext);
				if (num >= 0) {
					savename = string.Format("{0} - {1:D2}{2}", title, num, ext);
				}
			}
			else {
				savename = title + ext;
			}

			if (Setting.NoQuestion && Setting.SaveDirectory != "") {
				File.Copy(path, Path.Combine(Setting.SaveDirectory, savename), true);
				return savename;
			}
			else {
				SaveFileDialog saveDialog = new SaveFileDialog();
				saveDialog.InitialDirectory = Setting.SaveDirectory;

				saveDialog.Title = string.Format("원제 : {0}", filename);
				saveDialog.FileName = savename;

				if (saveDialog.ShowDialog() == true) {
					Setting.SaveDirectory = Path.GetDirectoryName(saveDialog.FileName);
					File.Copy(path, saveDialog.FileName, true);

					Setting.SaveSetting();
				}
			}

			return "";
		}

		public static string Unzip(string path, string filename, string title) {
			try {
				using (ZipArchive archive = ZipFile.OpenRead(path)) {
					foreach (ZipArchiveEntry entry in archive.Entries) {
						if (entry.FullName == filename) {
							string[] name = filename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
							string ext = "";

							try {
								ext = name[name.Length - 1];
							}
							catch { return ""; }						

							string extract = string.Format("{0}{1}.{2}", Setting.PathFolder, GetMD5Hash(DateTime.Now + entry.FullName), ext);
							entry.ExtractToFile(extract);
							return SaveFile(extract, filename, title);
						}
					}
				}
			} catch { }

			return "";
		}

		public static string GetMD5Hash(string md5input) {
			md5input = md5input.ToLower();
			MD5CryptoServiceProvider md5x = new MD5CryptoServiceProvider();
			byte[] md5bs = Encoding.UTF8.GetBytes(md5input);
			md5bs = md5x.ComputeHash(md5bs);
			StringBuilder md5s = new StringBuilder();
			foreach (byte md5b in md5bs) { md5s.Append(md5b.ToString("x2").ToLower()); }
			return md5s.ToString();
		}

		private static int FindNumberFromString(string str) {
			str = str.ToLower().Replace("1280x720", "")
				.Replace("x264", "").Replace("1920x1080", "")
				.Replace("720p", "").Replace("1080p", "")
				.Replace("v2", "").Replace("ver1", "").Replace("ver2", "");

			int sIndex = 0, eIndex = -1, value = -1;
			int isInner = 0;
			for (int i = str.Length - 1; i >= 0; i--) {
				if (str[i] == '(' || str[i] == '[') {
					isInner--;
					continue;
				}

				if (str[i] == ')' || str[i] == ']') {
					isInner++;
				}
				if (isInner > 0) { continue; }

				if (eIndex < 0 && isNumber(str[i])) { eIndex = i; }
				if (eIndex >= 0 && !isNumber(str[i])) {
					sIndex = i + 1;
					break;
				}
			}

			if (eIndex < 0) { return -1; }
			string sub = str.Substring(sIndex, eIndex - sIndex + 1);

			try {
				value = Convert.ToInt32(sub);
			} catch (Exception ex) {
				return -1;
			}

			return value;
		}
		private static bool isNumber(char c) {
			try {
				int v = Convert.ToInt32(c.ToString());
			} catch { return false; }
			return true;
		}

		public static string SaveScreenShot(Panel uie, int margin) {
			RenderTargetBitmap renderTarget = new RenderTargetBitmap(
				(int)uie.ActualWidth + margin, (int)uie.ActualHeight, 96, 96, PixelFormats.Pbgra32);

			DrawingVisual dv = new DrawingVisual();
			using (DrawingContext dc = dv.RenderOpen()) {
				VisualBrush vb = new VisualBrush(uie);

				dc.DrawRectangle(vb, null, new Rect(new Point(), new Size((int)uie.ActualWidth + margin, (int)uie.ActualHeight)));
			}
			renderTarget.Render(dv);


			PngBitmapEncoder encoder = new PngBitmapEncoder();
			BitmapFrame outputFrame = BitmapFrame.Create(renderTarget);
			encoder.Frames.Add(outputFrame);

			string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			string path = "";
			for (int i = 1; i <= 1000000; i++) {
				if (!File.Exists(System.IO.Path.Combine(desktop, string.Format("Simplist{0}.png", i)))) {
					path = System.IO.Path.Combine(desktop, string.Format("Simplist{0}.png", i));
					break;
				}
			}

			if (path == "") { return ""; }

			using (var file = File.OpenWrite(path)) {
				encoder.Save(file);
			}

			return path;
		}
	}
}
