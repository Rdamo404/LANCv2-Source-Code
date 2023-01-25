using System;
using System.Collections.Generic;
using System.IO;

namespace LANC_v2
{
	internal class Settings
	{
		public Dictionary<string, string> settings;

		public Settings(string filepath)
		{
			this.settings = new Dictionary<string, string>();
			this.readSettings(filepath);
		}

		private void readSettings(string filepath)
		{
			if (File.Exists(filepath))
			{
				StreamReader streamReader = new StreamReader(filepath);
				string str = "";
				while (true)
				{
					string str1 = streamReader.ReadLine();
					str = str1;
					if (str1 == null)
					{
						break;
					}
					string[] strArrays = str.Split(new char[] { ' ' });
					if ((int)strArrays.Length > 1)
					{
						this.settings.Add(strArrays[0], strArrays[1]);
					}
				}
				streamReader.Close();
			}
		}

		public void writeSettings(string fileLocation)
		{
			if (!File.Exists(fileLocation))
			{
				File.Create(fileLocation);
			}
			StreamWriter streamWriter = new StreamWriter(fileLocation);
			foreach (string key in this.settings.Keys)
			{
				streamWriter.WriteLine(string.Concat(key, " ", this.settings[key]));
			}
			streamWriter.Close();
		}
	}
}