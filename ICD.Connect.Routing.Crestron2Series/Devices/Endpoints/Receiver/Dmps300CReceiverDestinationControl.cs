using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.XSig;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Receiver
{
    public sealed class Dmps300CReceiverDestinationControl : AbstractRouteDestinationControl<Dmps300CReceiver>
    {
	    private const ushort DIGITAL_SOURCE_DETECTION_JOIN = 6;

	    public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
	    public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

	    private readonly SwitcherCache m_Cache;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
	    public Dmps300CReceiverDestinationControl(Dmps300CReceiver parent)
			: base(parent, 0)
	    {
			m_Cache = new SwitcherCache();

			Subscribe(m_Cache);
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

		    base.DisposeFinal(disposing);

			Unsubscribe(m_Cache);
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

		    return m_Cache.GetSourceDetectedState(input, type);
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
				    m_Cache.SetSourceDetectedState(1, eConnectionType.Video, data.Value);
				    break;
		    }
	    }

		#endregion

	    #region Cache Callbacks

	    private void Subscribe(SwitcherCache cache)
	    {
		    cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
	    }

	    private void Unsubscribe(SwitcherCache cache)
	    {
			cache.OnSourceDetectionStateChange -= CacheOnSourceDetectionStateChange;
		    cache.OnActiveInputsChanged -= CacheOnActiveInputsChanged;
		}

	    private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
	    {
		    OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
	    }

	    private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
	    {
		    OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));
	    }

	    #endregion
    }
}
