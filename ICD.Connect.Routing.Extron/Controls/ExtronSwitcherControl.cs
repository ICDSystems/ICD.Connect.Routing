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
using ICD.Connect.Routing.Extron.Devices.Switchers;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.Extron.Controls
{
	public sealed class ExtronSwitcherControl : AbstractRouteSwitcherControl<IExtronSwitcherDevice>, IExtronSwitcherControl
	{
		// gets the number of inputs formatted into {0}
		private const string SOURCE_DETECTION_REGEX_FORMAT = "^(?:Frq00 )?([01]){{{0}}}$";
		private const string ROUTE_REGEX_FORMAT = @"^(?:Out(?'output'\d\d?) )?In(?'input'\d\d?) (?'type'All|Aud|Vid)$";

		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Called when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		private readonly SwitcherCache m_Cache;
		private readonly int m_NumberOfInputs;
		private readonly int m_NumberOfOutputs;
		private readonly bool m_Breakaway;

		#region Properties

		private Regex m_SourceDetectionRegex;
		private Regex SourceDetectionRegex
		{
			get
			{
				return m_SourceDetectionRegex ??
					   (m_SourceDetectionRegex = new Regex(string.Format(SOURCE_DETECTION_REGEX_FORMAT, NumberOfInputs)));
			}
		}

		private Regex m_RouteRegex;
		private Regex RouteRegex
		{
			get { return m_RouteRegex ?? (m_RouteRegex = new Regex(ROUTE_REGEX_FORMAT)); }
		}

		/// <summary>
		/// Gets the number of inputs.
		/// </summary>
		public int NumberOfInputs
		{
			get { return m_NumberOfInputs; }
		}

		/// <summary>
		/// Gets the number of outputs.
		/// </summary>
		public int NumberOfOutputs
		{
			get { return m_NumberOfOutputs; }
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="numInputs"></param>
		/// <param name="numOutputs"></param>
		/// <param name="breakaway"></param>
		public ExtronSwitcherControl(IExtronSwitcherDevice parent, int id, int numInputs, int numOutputs, bool breakaway)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();
			m_NumberOfInputs = numInputs;
			m_NumberOfOutputs = numOutputs;
			m_Breakaway = breakaway;

			Subscribe(parent);
			Subscribe(m_Cache);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;
			OnActiveTransmissionStateChanged = null;
			OnRouteChange = null;

			base.DisposeFinal(disposing);

			Unsubscribe(Parent);
			Unsubscribe(m_Cache);
		}

		#region Methods

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_Cache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			return GetInputs().First(i => i.Address == input);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return GetInputs().Any(i => i.Address == input);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			// hdmi/dtp inputs
			foreach (int input in Enumerable.Range(1, NumberOfInputs))
				yield return new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video);

			// analog audio inputs -- disabled for now
			//foreach (var input in Enumerable.Range(1, 6))
			//    yield return new ConnectorInfo(input + 1000, eConnectionType.Audio);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			return GetOutputs().First(o => o.Address == address);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return GetOutputs().Any(o => o.Address == output);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			// hdmi/dtp outputs
			foreach (int output in Enumerable.Range(1, NumberOfOutputs))
				yield return new ConnectorInfo(output, eConnectionType.Audio | eConnectionType.Video);

			// analog audio outputs
			foreach (int output in Enumerable.Range(1, Math.Min(NumberOfOutputs, 4)))
				yield return new ConnectorInfo(output + 1000, eConnectionType.Audio);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return m_Cache.GetOutputsForInput(input, type);
		}

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
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
			int localInput = info.LocalInput; // % 1000;  
			int localOutput = info.LocalOutput % 1000;

			if ((/*info.LocalInput > 1000 ||*/ info.LocalOutput > 1000) && info.ConnectionType != eConnectionType.Audio)
				throw new ArgumentOutOfRangeException("info", "Cannot route anything other than audio to analog audio outputs");

			if (localInput < 1 || localInput > NumberOfInputs)
				throw new ArgumentOutOfRangeException("info", string.Format("Input must be between 1 and {0} (or add 1000 for analog audio inputs)", NumberOfInputs));

			if (localOutput < 1 || localOutput > NumberOfOutputs)
				throw new ArgumentOutOfRangeException("info", string.Format("Output must be between 1 and {0} (or add 1000 for analog audio outputs)", NumberOfOutputs));

			Route(localInput, localOutput, info.ConnectionType);
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
			output = output % 1000;

			char connectionTypeCharacter = m_Breakaway ? GetConnectionTypeCharacter(type) : '!';
			string outputString = m_NumberOfOutputs == 1 ? string.Empty : string.Format("*{0}", output);

			Parent.SendCommand("{0}{1}{2}", input, outputString, connectionTypeCharacter);
		}

		/// <summary>
		/// Removes the given connection type from the output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			output = output % 1000;

			Parent.SendCommand("0*{0}{1}", output, GetConnectionTypeCharacter(type));
			return true;
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

		#region Parent Callbacks

		private void Subscribe(IExtronSwitcherDevice parent)
		{
			parent.OnInitializedChanged += ParentOnOnInitializedChanged;
			parent.OnResponseReceived += ParentOnOnResponseReceived;
		}

		private void Unsubscribe(IExtronSwitcherDevice parent)
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
				string outputString = routeMatch.Groups["output"].Value;

				int output = string.IsNullOrEmpty(outputString) ? 1 : int.Parse(outputString);
				int? input = int.Parse(routeMatch.Groups["input"].Value);
				eConnectionType type = eConnectionType.None;
				switch (routeMatch.Groups["type"].Value)
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
				if (type.HasFlag(eConnectionType.Audio) && output >= 1 && output <= 4)
					m_Cache.SetInputForOutput(output + 1000, input > 0 ? input : null, eConnectionType.Audio);
			}
		}

		private void InitializeCache()
		{
			StringBuilder builder = new StringBuilder();
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
	}
}