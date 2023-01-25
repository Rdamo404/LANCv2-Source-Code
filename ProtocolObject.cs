using System;

namespace LANC_v2
{
	internal class ProtocolObject
	{
		public int port;

		public int type;

		public string description;

		public ProtocolObject(int port, int type, string description)
		{
			this.port = port;
			this.type = type;
			this.description = description;
		}
	}
}