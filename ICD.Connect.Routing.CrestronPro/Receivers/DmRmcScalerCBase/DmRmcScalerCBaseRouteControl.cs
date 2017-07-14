using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DM.Endpoints;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase
{
	public sealed class DmRmcScalerCBaseRouteControl : AbstractRouteMidpointControl<IDmRmcScalerCAdapter>
	{
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler OnActiveInputsChanged;

		private Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC m_Scaler;
		private bool m_VideoDetected;

		#region Properties

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

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public DmRmcScalerCBaseRouteControl(IDmRmcScalerCAdapter parent)
			: base(parent, 0)
		{
			Subscribe(parent);
			SetScaler(parent.Scaler);
		}

		/// <summary>
		/// Release resources
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveTransmissionStateChanged = null;

			base.DisposeFinal(disposing);

			// Unsbscribe and unregister
			Unsubscribe(Parent);
			SetScaler(null);
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
					return m_Scaler.DmInput.SyncDetectedFeedback.BoolValue;

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
		}

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
				throw new KeyNotFoundException(message);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					return ActiveTransmissionState;

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
		}

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		public override bool GetInputActiveState(int input, eConnectionType type)
		{
			return true;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			yield return new ConnectorInfo(1, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs(int output, eConnectionType type)
		{
			return GetInputs().Where(c => c.ConnectionType.HasFlags(type));
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return new ConnectorInfo(1, eConnectionType.Audio | eConnectionType.Video);
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(IDmRmcScalerCAdapter parent)
		{
			parent.OnScalerChanged += ParentOnScalerChanged;
		}

		/// <summary>
		/// Unsubscribe from parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(IDmRmcScalerCAdapter parent)
		{
			parent.OnScalerChanged -= ParentOnScalerChanged;
		}

		/// <summary>
		/// Called when the parent scaler changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="scaler"></param>
		private void ParentOnScalerChanged(IDmRmcScalerCAdapter sender,
		                                   Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC scaler)
		{
			SetScaler(scaler);
		}

		private void SetScaler(Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC scaler)
		{
			Unsubscribe(m_Scaler);
			m_Scaler = scaler;
			Subscribe(m_Scaler);
		}

		#endregion

		#region Scaler Callbacks

		/// <summary>
		/// Subscribe to the scaler events.
		/// </summary>
		/// <param name="scaler"></param>
		private void Subscribe(Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC scaler)
		{
			if (scaler == null)
				return;

			scaler.DmInput.InputStreamChange += DmInputOnInputStreamChange;
		}

		/// <summary>
		/// Unsubscribes from the scaler events.
		/// </summary>
		/// <param name="scaler"></param>
		private void Unsubscribe(Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC scaler)
		{
			if (scaler == null)
				return;

			scaler.DmInput.InputStreamChange -= DmInputOnInputStreamChange;
		}

		/// <summary>
		/// Called when the input stream changes.
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="args"></param>
		private void DmInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
		{
			if (args.EventId == EndpointInputStreamEventIds.SyncDetectedFeedbackEventId)
				VideoDetected = m_Scaler.DmInput.SyncDetectedFeedback.BoolValue;
		}

		#endregion
	}
}
