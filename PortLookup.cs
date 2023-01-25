using System;
using System.Collections.Generic;
using System.IO;

namespace LANC_v2
{
	internal class PortLookup
	{
		private string DBLocation;

		private Dictionary<int, ProtocolObject> db;

		public PortLookup(string DBLocation)
		{
			this.DBLocation = DBLocation;
			this.db = new Dictionary<int, ProtocolObject>();
		}

		public void createDBFromText(string location)
		{
			Exception exception;
			if (File.Exists(location))
			{
				StreamReader streamReader = new StreamReader(location);
				string str = "";
				bool flag = false;
				int num = 0;
				int num1 = 0;
				string str1 = "";
				int num2 = 0;
				while (true)
				{
					string str2 = streamReader.ReadLine();
					str = str2;
					if (str2 == null)
					{
						break;
					}
					if (!flag)
					{
						if (str.Contains("<tr"))
						{
							flag = true;
						}
					}
					else if (str == "</tr>")
					{
						if (!this.db.ContainsKey(num))
						{
							ProtocolObject protocolObject = new ProtocolObject(num, num1, str1);
							this.db.Add(num, protocolObject);
						}
						flag = false;
						num2 = 0;
						num = 0;
						num1 = 0;
						str1 = "";
					}
					else if (num2 == 0)
					{
						try
						{
							num = Convert.ToInt32(str.Substring(4, str.IndexOf("</td>") - 4));
						}
						catch (Exception exception1)
						{
							exception = exception1;
						}
						num2++;
					}
					else if (!(num2 == 1 ? false : num2 != 2))
					{
						if (str.IndexOf("</td>") - 4 > 0)
						{
							num1++;
						}
						num2++;
					}
					else if (num2 == 3)
					{
						str1 = str.Substring(4, str.IndexOf("</td>") - 4);
						while (str1.Contains("<"))
						{
							try
							{
								str1 = str1.Replace("&amp;", "&");
								str1 = str1.Remove(str1.IndexOf("<"), str1.IndexOf(">", str1.IndexOf("<")) - str1.IndexOf("<") + 1);
							}
							catch (Exception exception2)
							{
								exception = exception2;
							}
						}
						num2++;
					}
				}
				streamReader.Close();
				this.writeToFile(this.DBLocation);
			}
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
					string[] strArrays = str.Split(" ".ToCharArray(), 3);
					int num = 0;
					int num1 = 0;
					int num2 = 0;
					string str2 = "";
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						if (i == 0)
						{
							int num3 = Convert.ToInt32(strArrays[0]);
							num1 = num3;
							num = num3;
						}
						if (i == 1)
						{
							num2 = Convert.ToInt32(strArrays[1]);
						}
						if (i == 2)
						{
							str2 = strArrays[2];
						}
					}
					if (!this.db.ContainsKey(num))
					{
						ProtocolObject protocolObject = new ProtocolObject(num1, num2, str2);
						this.db.Add(num, protocolObject);
					}
				}
				streamReader.Close();
			}
		}

		public string lookup(string port)
		{
			string item;
			try
			{
				item = this.db[Convert.ToInt32(port)].description;
				return item;
			}
			catch (Exception exception)
			{
			}
			item = "";
			return item;
		}

		public void writeToFile(string fileLocation)
		{
			if (!File.Exists(fileLocation))
			{
				File.Create(fileLocation);
			}
			StreamWriter streamWriter = new StreamWriter(fileLocation);
			foreach (int key in this.db.Keys)
			{
				object[] item = new object[] { key, " ", this.db[key].type, " ", this.db[key].description };
				streamWriter.WriteLine(string.Concat(item));
			}
			streamWriter.Close();
		}
	}
}