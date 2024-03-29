﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.XSig;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Transmitter
{
    public sealed class Dmps300CTransmitterSourceControl : AbstractRouteSourceControl<Dmps300CTransmitter>
    {
	    private const ushort DIGITAL_HDMI_DETECTED_JOIN = 106;
	    private const ushort DIGITAL_VGA_DETECTED_JOIN = 112;

	    private const ushort ANALOG_DISABLE_FREERUN_JOIN = 48;

	    /// <summary>
	    /// Raised when the device starts/stops actively transmitting on an output.
	    /// </summary>
	    public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

	    private bool m_ActiveTransmissionState;
	    private bool m_HdmiDetected;
	    private bool m_VgaDetected;

	    #region Properties

	    public bool HdmiDetected
	    {
		    get { return m_HdmiDetected; }
		    set
		    {
			    if (value == m_HdmiDetected)
				    return;

			    m_HdmiDetected = value;

			    ActiveTransmissionState = HdmiDetected || VgaDetected;
		    }
	    }

	    public bool VgaDetected
	    {
		    get { return m_VgaDetected; }
		    set
		    {
			    if (value == m_VgaDetected)
				    return;

			    m_VgaDetected = value;

			    ActiveTransmissionState = HdmiDetected || VgaDetected;
			}
	    }

	    public bool ActiveTransmissionState
	    {
		    get { return m_ActiveTransmissionState; }
		    private set
		    {
			    if (value == m_ActiveTransmissionState)
				    return;

			    m_ActiveTransmissionState = value;

			    TransmissionStateEventArgs args =
				    new TransmissionStateEventArgs(1, eConnectionType.Audio | eConnectionType.Video, m_ActiveTransmissionState);

			    OnActiveTransmissionStateChanged.Raise(this, args);
		    }
	    }

	    #endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public Dmps300CTransmitterSourceControl(Dmps300CTransmitter parent)
			: base(parent, 0)
	    {
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnActiveTransmissionStateChanged = null;

			base.DisposeFinal(disposing);
		}

	    #region Methods

	    /// <summary>
	    /// Returns true if the device is actively transmitting on the given output.
	    /// This is NOT the same as sending video, since some devices may send an
	    /// idle signal by default.
	    /// </summary>
	    /// <param name="output"></param>
	    /// <param name="type"></param>
	    /// <returns></returns>
	    public override bool GetActiveTransmissionState(int output, eConnectionType type)
	    {
		    if (EnumUtils.HasMultipleFlags(type))
		    {
			    return
				    EnumUtils.GetFlagsExceptNone(type)
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

	    /// <summary>
	    /// Gets the output at the given address.
	    /// </summary>
	    /// <param name="output"></param>
	    /// <returns></returns>
	    public override ConnectorInfo GetOutput(int output)
	    {
		    if (!ContainsOutput(output))
				throw new ArgumentOutOfRangeException("output");

			return new ConnectorInfo(output, eConnectionType.Audio | eConnectionType.Video);
	    }

	    /// <summary>
	    /// Returns true if the source contains an output at the given address.
	    /// </summary>
	    /// <param name="output"></param>
	    /// <returns></returns>
	    public override bool ContainsOutput(int output)
	    {
		    return output == 1;
	    }

	    public override IEnumerable<ConnectorInfo> GetOutputs()
	    {
		    yield return new ConnectorInfo(1, eConnectionType.Audio | eConnectionType.Video);
	    }

	    #endregion

	    #region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(Dmps300CTransmitter parent)
		{
			base.Subscribe(parent);

			parent.OnSigEvent += ParentOnSigEvent;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(Dmps300CTransmitter parent)
		{
			base.Unsubscribe(parent);

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

			if (xSigEventArgs.Data is AnalogXSig)
				HandleAnalogSigEvent((AnalogXSig)xSigEventArgs.Data);
		}

	    /// <summary>
	    /// Called when we receive an analog sig from the device.
	    /// </summary>
	    /// <param name="data"></param>
		private void HandleAnalogSigEvent(AnalogXSig data)
	    {
		    switch (data.Index)
		    {
			    case ANALOG_DISABLE_FREERUN_JOIN:
				    if (data.Value != 1)
						Parent.SendData(new AnalogXSig(1, ANALOG_DISABLE_FREERUN_JOIN));
				    break;
		    }
	    }

		/// <summary>
		/// Called when we receive a digital sig from the device.
		/// </summary>
		/// <param name="data"></param>
		private void HandleDigitalSigEvent(DigitalXSig data)
		{
			switch (data.Index)
			{
				case DIGITAL_HDMI_DETECTED_JOIN:
					HdmiDetected = data.Value;
					break;
				case DIGITAL_VGA_DETECTED_JOIN:
					VgaDetected = data.Value;
					break;
			}
		}

		#endregion
	}
}
