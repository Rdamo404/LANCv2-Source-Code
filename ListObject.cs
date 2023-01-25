using System;
using System.Runtime.CompilerServices;

namespace LANC_v2
{
	internal class ListObject
	{
		public int packetDelt;

        public string label
        {
            get;
            set;
        }
        
        public string GeoLocation
		{
			get;
			set;
		}

        public string ipDisplay
        {
            get;
            set;
        }

        public int packetCount
        {
            get;
            set;
        }

        public string ipSource
        {
            get;
            set;
        }

        public string portSource
        {
            get;
            set;
        }

        public string ipDest
		{
			get;
			set;
		}

        public string portDest
        {
            get;
            set;
        }

		public string macAddress
		{
			get;
			set;
		}

        public string protocol
        {
            get;
            set;
        }
        
        public string macVendor
		{
			get;
			set;
		}

		public ListObject()
		{
		}
	}
}