using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.Extron.Devices.Switchers
{
	public abstract class AbstractDtpCrosspointSwitcherControl<TDevice, TSettings> : AbstractRouteSwitcherControl<TDevice>, IDtpCrosspointSwitcherControl
		where TDevice : AbstractDtpCrosspointDevice<TSettings>
		where TSettings : AbstractDtpCrosspointSettings, new()
	{
		// gets the number of inputs formatted into {0}
		private string SOURCE_DETECTION_REGEX_FORMAT = "^(?:Frq00 )?([01]){{{0}}}$";
		private string ROUTE_REGEX_FORMAT = @"^Out(\d\d?) In(\d\d?) (All|Aud|Vid)$";

		private Regex m_SourceDetectionRegex;
		protected Regex SourceDetectionRegex
		{
			get
			{
				return m_SourceDetectionRegex ??
				       (m_SourceDetectionRegex = new Regex(string.Format(SOURCE_DETECTION_REGEX_FORMAT, NumberOfInputs)));
			}
		}

		private Regex m_RouteRegex;
		protected Regex RouteRegex
		{
			get
			{
				return m_RouteRegex ?? (m_RouteRegex = new Regex(ROUTE_REGEX_FORMAT));
			}
		}

		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		public abstract int NumberOfInputs { get; }
		public abstract int NumberOfOutputs { get; }

		private readonly SwitcherCache m_Cache;

		public AbstractDtpCrosspointSwitcherControl(TDevice parent, int id) : base(parent, id)
		{
			m_Cache = new SwitcherCache();
			Subscribe(parent);
			Subscribe(m_Cache);
		}

		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);
			Unsubscribe(Parent);
		}

		#region Methods

		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_Cache.GetSourceDetectedState(input, type);
		}

		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return Enumerable.Range(1, NumberOfInputs).Select(input => new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video));
		}

		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return Enumerable.Range(1, NumberOfOutputs).Select(input => new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video));
		}

		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return m_Cache.GetOutputsForInput(input, type);
		}

		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			return m_Cache.GetInputConnectorInfoForOutput(output, type);
		}

		/// <summary>
		/// Performs the given RouteOperation on this switcher.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
			if (info.LocalInput < 1 || info.LocalInput > NumberOfInputs)
				throw new ArgumentOutOfRangeException("info", string.Format("Input must be between 1 and {0}", NumberOfInputs));

			if (info.LocalOutput < 1 || info.LocalOutput > NumberOfOutputs)
				throw new ArgumentOutOfRangeException("info", string.Format("Output must be between 1 and {0}", NumberOfOutputs));

			Route(info.LocalInput, info.LocalOutput, info.ConnectionType);
			return true;
		}

		/// <summary>
		/// Routes the input to the output for the given connection type(s)
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		public void Route(int input, int output, eConnectionType type)
		{
			Parent.SendCommand("{0}*{1}{2}", input, output, GetConnectionTypeCharacter(type));
		}

		/// <summary>
		/// Removes the given connection type from the output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			Parent.SendCommand("0*{0}{1}", output, GetConnectionTypeCharacter(type));
			return true;
		}

		#endregion

		#region Parent Callbacks

		private void Subscribe(TDevice parent)
		{
			parent.OnInitializedChanged += ParentOnOnInitializedChanged;
			parent.OnResponseReceived += ParentOnOnResponseReceived;
		}

		private void Unsubscribe(TDevice parent)
		{
			parent.OnInitializedChanged -= ParentOnOnInitializedChanged;
			parent.OnResponseReceived -= ParentOnOnResponseReceived;
		}

		private void ParentOnOnInitializedChanged(object sender, BoolEventArgs args)
		{
			InitializeCache();
		}

		private void ParentOnOnResponseReceived(object sender, StringEventArgs args)
		{
			ParseResponse(args.Data);
		}

		private void ParseResponse(string data)
		{
			if (data == "Qik")
			{
				m_Cache.Clear();
				InitializeCache();
			}

			Match sourceDetectionMatch = SourceDetectionRegex.Match(data);
			if (sourceDetectionMatch.Success)
			{
				for (int i = 0; i < NumberOfInputs; i++)
				{
					bool detected = sourceDetectionMatch.Groups[1].Captures[i].Value != "0";
					m_Cache.SetSourceDetectedState(i + 1, eConnectionType.Audio | eConnectionType.Video, detected);
				}
				return;
			}

			Match routeMatch = RouteRegex.Match(data);
			if (routeMatch.Success)
			{
				int output = int.Parse(routeMatch.Groups[1].Value);
				int? input = int.Parse(routeMatch.Groups[2].Value);
				eConnectionType type = eConnectionType.None;
				switch (routeMatch.Groups[3].Value)
				{
					case "All":
						type = eConnectionType.Audio | eConnectionType.Video;
						break;
					case "Vid":
						type = eConnectionType.Video;
						break;
					case "Aud":
						type = eConnectionType.Audio;
						break;
				}
				m_Cache.SetInputForOutput(output, input > 0 ? input : null, type);
			}
		}

		private void InitializeCache()
		{
			var builder = new StringBuilder();
			for (int i = 1; i <= NumberOfOutputs; i++)
				builder.Append(i + "$" + i + "%");
			Parent.SendCommand(builder.ToString());
			Parent.SendCommand("0LS");
		}

		#endregion

		#region Cache Callbacks

		private void Subscribe(SwitcherCache cache)
		{
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
            cache.OnRouteChange += CacheOnRouteChange;
            cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
            cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
		}

        private void Unsubscribe(SwitcherCache cache)
        {
            cache.OnSourceDetectionStateChange -= CacheOnSourceDetectionStateChange;
            cache.OnRouteChange -= CacheOnRouteChange;
            cache.OnActiveInputsChanged -= CacheOnActiveInputsChanged;
            cache.OnActiveTransmissionStateChanged -= CacheOnActiveTransmissionStateChanged;
        }

        private void CacheOnRouteChange(object sender, RouteChangeEventArgs args)
        {
            OnRouteChange.Raise(this, new RouteChangeEventArgs(args));
        }

        private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));
		}

        private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
        {
            OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
        }

        private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
        {
            OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
        }

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the correct connection type character to send to the Extron switcher
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private char GetConnectionTypeCharacter(eConnectionType type)
		{
			switch (type)
			{
				case eConnectionType.Audio | eConnectionType.Video:
					return '!';
				case eConnectionType.Video:
					return '%';
				case eConnectionType.Audio:
					return '$';
				default:
					throw new NotSupportedException(string.Format("{0} routing not supported", type));
			}
		}

		#endregion
	}
}