using System;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class StreamUriEventArgs : EventArgs
	{
		private readonly eConnectionType m_ConnectionType;
		private readonly int m_Output;
		private readonly Uri m_StreamUri;

		public eConnectionType ConnectionType { get { return m_ConnectionType; } }
		public int Output { get { return m_Output; } }
		public Uri StreamUri { get { return m_StreamUri; } }

		public StreamUriEventArgs(eConnectionType connectionType, int output, Uri streamUri)
		{
			m_ConnectionType = connectionType;
			m_Output = output;
			m_StreamUri = streamUri;
		}
	}
}
