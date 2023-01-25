using System;
using System.IO;
using System.Text;
using System.Xml;

namespace LANC_v2
{
	internal class IPLocator
	{
		public IPLocator()
		{
		}

        public static string[] IPLocation(string IP)
        {
            System.Net.WebClient WC = new System.Net.WebClient();
            string[] GeoLocationWC = WC.DownloadString("http://ip-api.com/csv/" + IP + "?fields=country,regionName,city").Replace(",", Environment.NewLine).Replace("\"", "").Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] strArrays = new string[] { GeoLocationWC[0], GeoLocationWC[1], GeoLocationWC[2] };
            return strArrays;
        }
	}
}