using System;
using System.Collections.Generic;
using System.IO;

namespace LANC_v2
{
	internal class Database
	{
		public Dictionary<string, List<string>> db;

		public string fileLocation;

		public Database(string fileLocation)
		{
			this.fileLocation = fileLocation;
			this.db = new Dictionary<string, List<string>>();
			this.loadFromFile(fileLocation);
		}

		public bool loadFromFile(string fileLocation)
		{
			List<string> item;
			bool flag;
			if (!File.Exists(fileLocation))
			{
				flag = false;
			}
			else
			{
				StreamReader streamReader = new StreamReader(fileLocation);
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
							str3 = strArrays[0];
						}
						if (i == 1)
						{
							str2 = strArrays[1];
						}
					}
					if (this.db.ContainsKey(str3))
					{
						item = this.db[str3];
						if (!item.Contains(str2))
						{
							item.Add(str2);
						}
						this.db[str3] = item;
					}
					else
					{
						item = new List<string>()
						{
							str2
						};
						this.db.Add(str3, item);
					}
				}
				streamReader.Close();
				flag = true;
			}
			return flag;
		}

		public bool writeToFile(string fileLocation)
		{
			if (!File.Exists(fileLocation))
			{
				File.Create(fileLocation);
			}
			string str = "";
			StreamWriter streamWriter = new StreamWriter(fileLocation);
			foreach (string key in this.db.Keys)
			{
				str = key;
				foreach (string item in this.db[key])
				{
					streamWriter.WriteLine(string.Concat(str, " ", item));
				}
			}
			streamWriter.Close();
			return true;
		}
	}
}