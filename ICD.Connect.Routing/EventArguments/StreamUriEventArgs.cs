using System;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class StreamUriEventArgs : EventArgs
	{
		private readonly eConnectionType m_ConnectionType;
		private readonly int m_Address;
		private readonly Uri m_StreamUri;

		public eConnectionType ConnectionType { get { return m_ConnectionType; } }
		public int Address { get { return m_Address; } }
		public Uri StreamUri { get { return m_StreamUri; } }

		public StreamUriEventArgs(eConnectionType connectionType, int address, Uri streamUri)
		{
			m_ConnectionType = connectionType;
			m_Address = address;
			m_StreamUri = streamUri;
		}
	}
}
