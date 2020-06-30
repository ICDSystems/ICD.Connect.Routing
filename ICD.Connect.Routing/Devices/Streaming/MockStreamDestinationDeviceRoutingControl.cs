using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls.Streaming;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices.Streaming
{
	public sealed class MockStreamDestinationDeviceRoutingControl : AbstractStreamRouteDestinationControl<MockStreamDestinationDevice>
	{
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public override event EventHandler<StreamUriEventArgs> OnInputStreamUriChanged;

		private Uri m_StreamUri;
		private bool m_SourceDetectedState;

		[PublicAPI]
		public Uri StreamUri
		{
			get { return m_StreamUri; }
			private set
			{
				if (m_StreamUri == value)
					return;

				m_StreamUri = value;
				SourceDetectedState = m_StreamUri != null;

				OnInputStreamUriChanged.Raise(this,
				                              new StreamUriEventArgs(eConnectionType.Audio | eConnectionType.Video, 1,
				                                                     m_StreamUri));
			}
		}

		/// <summary>
		/// Returns true when the device is actively receiving video.
		/// </summary>
		[PublicAPI]
		public bool SourceDetectedState
		{
			get { return m_SourceDetectedState; }
			private set
			{
				if (value == m_SourceDetectedState)
					return;

				m_SourceDetectedState = value;

				OnSourceDetectionStateChange.Raise(this,
				                                   new SourceDetectionStateChangeEventArgs(1,
				                                                                           eConnectionType.Audio |
				                                                                           eConnectionType.Video,
				                                                                           m_SourceDetectedState));
			}
		}

		public MockStreamDestinationDeviceRoutingControl(MockStreamDestinationDevice parent, int id) 
			: base(parent, id)
		{
		}

		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => GetSignalDetectedState(input, f))
				                .Unanimous(false);
			}

			if (input != 1)
			{
				string message = string.Format("{0} has no {1} input at address {2}", this, type, input);
				throw new ArgumentOutOfRangeException("input", message);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					return SourceDetectedState;

				default:
					throw new ArgumentOutOfRangeException("type");
			}
		}

		public override bool GetInputActiveState(int input, eConnectionType type)
		{
			return GetSignalDetectedState(input, type);
		}

		public override ConnectorInfo GetInput(int input)
		{
			if (input != 1)
				throw new ArgumentOutOfRangeException("No inputs with address " + input);

			return new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video);
		}

		public override bool ContainsInput(int input)
		{
			return input == 1;
		}

		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			yield return GetInput(1);
		}

		public override bool SetStreamForInput(int input, Uri stream)
		{
			if (input != 1)
				throw new ArgumentOutOfRangeException("No input with address " + input);

			StreamUri = stream;
			return true;
		}

		public override Uri GetStreamForInput(int input)
		{
			if (input != 1)
				throw new ArgumentOutOfRangeException("No input with address " + input);

			return StreamUri;
		}

		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("StreamUri", StreamUri);
		}

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand baseConsoleCommand in GetBaseConsoleCommands())
			{
				yield return baseConsoleCommand;
			}

			yield return new GenericConsoleCommand<string>("SetStreamUri", "Set the destination stream uri",
			                                               s => StreamUri = new Uri(s));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}
	}
}
