using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LANC_v2
{
	internal class MACLookup
	{
		private string DBLocation;

		public Dictionary<string, string> db;

		public MACLookup(string DBLocation)
		{
			this.DBLocation = DBLocation;
			this.db = new Dictionary<string, string>();
		}

		public string createDBFromText(string location)
		{
			string str = "";
			if (File.Exists(location))
			{
				StreamReader streamReader = new StreamReader(location);
				string str1 = "";
				while (true)
				{
					string str2 = streamReader.ReadLine();
					str1 = str2;
					if (str2 == null)
					{
						break;
					}
					if ((new Regex("^(\\w\\w-\\w\\w-\\w\\w)")).Match(str1).Success)
					{
						str1 = str1.Replace('\t', ' ');
						char[] chrArray = new char[] { ' ' };
						List<string> list = (
							from s in str1.Split(chrArray)
							where !string.IsNullOrEmpty(s)
							select s).ToList<string>();
						if (!this.db.ContainsKey(list[0]))
						{
							string str3 = "";
							for (int i = 2; i < list.Count; i++)
							{
								str3 = string.Concat(str3, list[i], " ");
							}
							this.db.Add(list[0], str3);
						}
					}
				}
				streamReader.Close();
			}
			this.writeToFile(this.DBLocation);
			return str;
		}

		public void loadFromFile()
		{
			if (File.Exists(this.DBLocation))
			{
				StreamReader streamReader = new StreamReader(this.DBLocation);
				string str = "";
				while (true)
				{
					string str1 = streamReader.ReadLine();
					str = str1;
					if (str1 == null)
					{
						break;
					}
					string[] strArrays = str.Split(" ".ToCharArray(), 2);
					string str2 = "";
					string str3 = "";
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						if (i == 0)
						{
							str2 = strArrays[0];
						}
						if (i == 1)
						{
							str3 = strArrays[1];
						}
					}
					if (!this.db.ContainsKey(str2))
					{
						this.db.Add(str2, str3);
					}
				}
				streamReader.Close();
			}
		}

		public string lookup(string mac)
		{
			string str = mac.Substring(0, 8);
			str = str.Replace(':', '-');
			return (this.db.ContainsKey(str) ? this.db[str] : "N/A");
		}

		public void writeToFile(string fileLocation)
		{
			if (!File.Exists(fileLocation))
			{
				File.Create(fileLocation);
			}
			StreamWriter streamWriter = new StreamWriter(fileLocation);
			foreach (string key in this.db.Keys)
			{
				streamWriter.WriteLine(string.Concat(key, " ", this.db[key]));
			}
			streamWriter.Close();
		}
	}
}