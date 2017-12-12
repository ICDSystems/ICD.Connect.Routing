using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.XSig;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Receiver
{
    public sealed class Dmps300CReceiverDestinationControl : AbstractRouteMidpointControl<Dmps300CReceiver>
    {
	    private const ushort DIGITAL_SOURCE_DETECTION_JOIN = 6;

	    public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
	    public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
	    public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		private bool m_VideoDetected;

		/// <summary>
		/// Returns true when video is detected.
		/// </summary>
		[PublicAPI]
		public bool VideoDetected
		{
			get { return m_VideoDetected; }
			private set
			{
				if (value == m_VideoDetected)
					return;

				m_VideoDetected = value;

				OnSourceDetectionStateChange.Raise(this,
												   new SourceDetectionStateChangeEventArgs(1,
																						   eConnectionType.Audio |
																						   eConnectionType.Video,
																						   m_VideoDetected));
				OnActiveTransmissionStateChanged.Raise(this,
													   new TransmissionStateEventArgs(1,
																					  eConnectionType.Audio | eConnectionType.Video,
																					  m_VideoDetected));
			}
		}

		/// <summary>
		/// Gets the active transmission state.
		/// </summary>
		[PublicAPI]
		public bool ActiveTransmissionState { get { return VideoDetected; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
	    public Dmps300CReceiverDestinationControl(Dmps300CReceiver parent)
			: base(parent, 0)
	    {
		    Subscribe(parent);
	    }

	    /// <summary>
		/// Release resources.
		/// </summary>
		/// <param name="disposing"></param>
	    protected override void DisposeFinal(bool disposing)
	    {
		    OnSourceDetectionStateChange = null;
		    OnActiveInputsChanged = null;
		    OnActiveTransmissionStateChanged = null;

		    base.DisposeFinal(disposing);

		    Unsubscribe(Parent);
	    }

	    #region Methods

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
				throw new KeyNotFoundException(message);
			}

			switch (type)
			{
				case eConnectionType.Audio:
					return true;
				case eConnectionType.Video:
					return VideoDetected;

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
	    }

	    /// <summary>
	    /// Returns the outputs.
	    /// </summary>
	    /// <returns></returns>
	    public override IEnumerable<ConnectorInfo> GetOutputs()
	    {
			yield return new ConnectorInfo(1, eConnectionType.Audio | eConnectionType.Video);
	    }

	    /// <summary>
	    /// Gets the outputs for the given input.
	    /// </summary>
	    /// <param name="input"></param>
	    /// <param name="type"></param>
	    /// <returns></returns>
	    public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
	    {
			if (input != 1)
				throw new ArgumentException(string.Format("{0} only has 1 input", GetType().Name), "input");

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
				case eConnectionType.Audio | eConnectionType.Video:
					yield return GetOutput(1);
					break;

				default:
					throw new ArgumentException("type");
			}
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
			if (output != 1)
				throw new ArgumentException(string.Format("{0} only has 1 output", GetType().Name), "output");

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
				case eConnectionType.Audio | eConnectionType.Video:
					return GetInput(1);

				default:
					throw new ArgumentException("type");
			}
	    }

	    public override bool GetInputActiveState(int input, eConnectionType type)
	    {
		    return GetSignalDetectedState(input, type);
	    }

	    public override IEnumerable<ConnectorInfo> GetInputs()
	    {
		    yield return new ConnectorInfo(1, eConnectionType.Audio | eConnectionType.Video);
	    }

	    #endregion

	    #region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
	    private void Subscribe(Dmps300CReceiver parent)
	    {
		    parent.OnSigEvent += ParentOnSigEvent;
	    }

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
	    private void Unsubscribe(Dmps300CReceiver parent)
	    {
		    parent.OnSigEvent -= ParentOnSigEvent;
	    }

		/// <summary>
		/// Called when we receive a sig from the device.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="xSigEventArgs"></param>
	    private void ParentOnSigEvent(object sender, XSigEventArgs xSigEventArgs)
		{
			if (xSigEventArgs.Data is DigitalXSig)
				HandleDigitalSigEvent((DigitalXSig)xSigEventArgs.Data);
		}

		/// <summary>
		/// Called when we receive a digital sig from the device.
		/// </summary>
		/// <param name="data"></param>
	    private void HandleDigitalSigEvent(DigitalXSig data)
	    {
		    switch (data.Index)
		    {
			    case DIGITAL_SOURCE_DETECTION_JOIN:
				    VideoDetected = data.Value;
				    break;
		    }
	    }

		#endregion
    }
}
