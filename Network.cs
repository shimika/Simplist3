using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplist3 {
	class Network {
		public static string GetHtml(string url, string encoding) {
			try {
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new UriBuilder(url).Uri);
				httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
				httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
				httpWebRequest.Method = "GET";
				httpWebRequest.Referer = "google.com";
				httpWebRequest.UserAgent =
					"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
					"Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
					".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
					"InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";
				httpWebRequest.ContentLength = 0;

				string rtHTML = "";

				httpWebRequest.Proxy = null;

				HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding(encoding));
				rtHTML = streamReader.ReadToEnd();

				return rtHTML;
			} catch (Exception ex) { }

			return "";
		}
	}
}
