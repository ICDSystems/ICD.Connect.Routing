using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc100CBase
{
#if SIMPLSHARP
	public abstract class AbstractDmRmc100CBaseAdapter<TReceiver, TSettings> :
		AbstractEndpointReceiverBaseAdapter<TReceiver, TSettings>, IDmRmc100CBaseAdapter
		where TReceiver : Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C
#else
	public abstract class AbstractDmRmc100CBaseAdapter<TSettings> :
		AbstractEndpointReceiverBaseAdapter<TSettings>, IDmRmc100CBaseAdapter
#endif
		where TSettings : IDmRmc100CBaseAdapterSettings, new()
	{
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

#if SIMPLSHARP
		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ComPort GetComPort(int address)
		{
			if (Receiver == null)
				throw new InvalidOperationException("No scaler instantiated");

			if (address == 1)
				return Receiver.ComPorts[1];

			return base.GetComPort(address);
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override IROutputPort GetIrOutputPort(int address)
		{
			if (Receiver == null)
				throw new InvalidOperationException("No scaler instantiated");

			if (address == 1)
				return Receiver.IROutputPorts[1];

			return base.GetIrOutputPort(address);
		}
#endif

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

			if (!ContainsInput(input))
			{
				string message = string.Format("{0} has no {1} input at address {2}", this, type, input);
				throw new IndexOutOfRangeException(message);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					return true;
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

			if (!ContainsOutput(output))
			{
				string message = string.Format("{0} has no {1} output at address {2}", this, type, output);
				throw new IndexOutOfRangeException(message);
			}

			switch (type)
			{
				case eConnectionType.Audio:
					return GetSignalDetectedState(1, eConnectionType.Audio);
				case eConnectionType.Video:
					return GetSignalDetectedState(1, eConnectionType.Video);

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
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			if (!ContainsOutput(output))
				throw new ArgumentException(string.Format("{0} has no output at address {1}", this, output));

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

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			if (!ContainsInput(input))
				throw new ArgumentException(string.Format("{0} has no input at address {1}", this, input));

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
				case eConnectionType.Audio | eConnectionType.Video:
					yield return GetOutput(1);
					yield break;
				default:
					throw new ArgumentException("type");
			}
		}
	}
}
