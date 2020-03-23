using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls.Streaming;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices.Streaming
{
	public sealed class StreamSourceDeviceRoutingControl : AbstractStreamRouteSourceControl<StreamSourceDevice>
	{
		#region Events

		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<StreamUriEventArgs> OnOutputStreamUriChanged;

		#endregion

		private Uri m_StreamUri;
		private bool m_ActiveTransmissionState;

		[PublicAPI]
		public Uri StreamUri
		{
			get { return m_StreamUri; }
			private set
			{
				if (m_StreamUri == value)
					return;

				m_StreamUri = value;
				ActiveTransmissionState = m_StreamUri != null;

				OnOutputStreamUriChanged.Raise(this,
				                         new StreamUriEventArgs(eConnectionType.Audio | eConnectionType.Video, 1,
				                                                m_StreamUri));
			}
		}

		/// <summary>
		/// Returns true when the device is actively transmitting video.
		/// </summary>
		[PublicAPI]
		public bool ActiveTransmissionState
		{
			get { return m_ActiveTransmissionState; }
			private set
			{
				if (value == m_ActiveTransmissionState)
					return;

				m_ActiveTransmissionState = value;

				OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(1, eConnectionType.Audio | eConnectionType.Video, m_ActiveTransmissionState));
			}
		}

		public StreamSourceDeviceRoutingControl(StreamSourceDevice parent, int id)
			: base(parent, id)
		{
			UpdateStreamUri();
		}

		#region Methods

		public override bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => GetActiveTransmissionState(output, f))
				                .Unanimous(false);
			}

			if (output != 1)
			{
				string message = string.Format("{0} has no {1} output at address {2}", this, type, output);
				throw new ArgumentOutOfRangeException("output", message);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					return ActiveTransmissionState;

				default:
					throw new ArgumentOutOfRangeException("type");
			}
		}

		public override ConnectorInfo GetOutput(int output)
		{
			if (output != 1)
				throw new ArgumentOutOfRangeException("No output with address " + output);

			return new ConnectorInfo(output, eConnectionType.Audio | eConnectionType.Video);
		}

		public override bool ContainsOutput(int output)
		{
			return output == 1;
		}

		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return GetOutput(1);
		}

		public override Uri GetStreamForOutput(int output)
		{
			if (output != 1)
				throw new ArgumentOutOfRangeException("No output with address " + output);

			return StreamUri;
		}

		#endregion

		#region Private Methods

		private void UpdateStreamUri()
		{
			StreamUri = Parent.StreamUri;
		}

		#endregion

		#region Parent Callbacks

		protected override void Subscribe(StreamSourceDevice parent)
		{
			base.Subscribe(parent);

			parent.OnStreamUriChanged += ParentOnStreamUriChanged;
		}

		protected override void Unsubscribe(StreamSourceDevice parent)
		{
			base.Unsubscribe(parent);

			parent.OnStreamUriChanged -= ParentOnStreamUriChanged;
		}

		private void ParentOnStreamUriChanged(object sender, GenericEventArgs<Uri> e)
		{
			UpdateStreamUri();
		}

		#endregion
	}
}
