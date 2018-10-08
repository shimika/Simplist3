using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Simplist3 {
	class Network {
		public static string GET(string url, string encoding = "UTF-8") {
			for (int i = 1; i <= 4; i++) {
				try {
					HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new UriBuilder(url).Uri);
					httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
					httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
					httpWebRequest.Method = "GET";
					httpWebRequest.UserAgent =
						"Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
						"AppleWebKit/537.36 (KHTML, like Gecko) " +
						"Chrome/58.0.3029.110 Safari/537.36";
					if (i % 2 == 0) {
						httpWebRequest.Referer = "google.com";
					}
					if (i > 2) {
						httpWebRequest.ContentLength = 0;
					}
					httpWebRequest.Proxy = null;

					HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
					StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding(encoding));

					return streamReader.ReadToEnd();
				} catch (Exception ex) {
					// MessageBox.Show(url + "\n" + ex.Message + "\n" + ex.StackTrace);
				}
			}

			return "";
		}

		public static string POST(string url, string data) {
			try {
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new UriBuilder(url).Uri);
				httpWebRequest.Accept = "*/*";
				httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				httpWebRequest.Method = "POST";
				httpWebRequest.Referer = "google.com";
				httpWebRequest.UserAgent =
					"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
					"Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
					".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
					"InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";
				httpWebRequest.Proxy = null;

				using (StreamWriter sw = new StreamWriter(httpWebRequest.GetRequestStream())) {
					sw.Write(data);
				}

				HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);

				return streamReader.ReadToEnd();
			} catch (Exception ex) {
				//MessageBox.Show(ex.Message);
			}

			return "";
		}

		public static string GetFilenameFromURL(string url) {
			using (WebClient client = new WebClient() { Proxy = null }) {
				using (Stream rawStream = client.OpenRead(url)) {
					string fileName = string.Empty;
					string contentDisposition = client.ResponseHeaders["content-disposition"];
					string realName = "";
					if (!string.IsNullOrEmpty(contentDisposition)) {
						return new ContentDisposition(contentDisposition).FileName;
					} else {
						string[] strSplit = url.Split('/');
						realName = strSplit[strSplit.Length - 1];
					}
					rawStream.Close();
					if (realName[realName.Length - 1] == '\"') {
						realName = realName.Substring(0, realName.Length - 1);
						if (realName[0] == '\"') {
							realName = realName.Substring(1);
						}
					}
					return realName;
				}
			}
		}

		public static string DownloadFile(string url, string caption) {
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new UriBuilder(url).Uri);

			if (url.Contains("drive.google.com")) {
				return downloadGoogleDriveFile(url, caption);
			}

			if (Path.GetExtension(caption) == "") { caption += ".zip"; }
			string path = string.Format("{0}{1:MM-dd HH_mm_ss}{2}_{3}", 
				Setting.PathFolder, 
				DateTime.Now, 
				new Random().Next(),
				Function.CleanFileName(caption));

			httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
			httpWebRequest.Method = "GET";
			httpWebRequest.Referer = "www.google.com";
			httpWebRequest.UserAgent =
				"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
				"Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
				".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
				"InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";
			httpWebRequest.ContentLength = 0;
			httpWebRequest.Credentials = CredentialCache.DefaultCredentials;

			try {
				httpWebRequest.Proxy = null;

				HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				using (var stream = httpWebResponse.GetResponseStream()) {
					using (FileStream fstream = new FileStream(path, FileMode.Create)) {
						var buffer = new byte[8192];
						var maxCount = buffer.Length;
						int count;
						while ((count = stream.Read(buffer, 0, maxCount)) > 0)
							fstream.Write(buffer, 0, count);
					}
				}
			} catch (Exception ex) {
				return null;
			}

			return path;
		}

		private static string downloadGoogleDriveFile(string url, string caption) {
			string path = string.Format(
				"{0}{1:MM-dd HH_mm_ss}{2}_{3}_",
				Setting.PathFolder,
				DateTime.Now,
				new Random().Next(),
				Function.CleanFileName(caption));

			try {
				string newUrl = GoogleDriveDownloader.GetGoogleDriveDownloadLinkFromUrl(url);
				path += Function.CleanFileName(GetFilenameFromURL(newUrl));
				FileInfo fileInfo = GoogleDriveDownloader.DownloadFileFromURLToPath(url, path);

				if (fileInfo != null) {
					return path;
				}
			}
			catch (Exception ex) { }

			return null;
		}
	}
}
