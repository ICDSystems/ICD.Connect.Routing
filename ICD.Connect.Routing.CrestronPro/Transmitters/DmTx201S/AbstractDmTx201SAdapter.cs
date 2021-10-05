using System;
using System.Collections.Generic;
using System.Linq;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
#if !NETSTANDARD
	public abstract class AbstractDmTx201SAdapter<TTransmitter, TSettings> :
		AbstractDmTx200BaseAdapter<TTransmitter, TSettings>, IDmTx201SAdapter
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S
#else
	public abstract class AbstractDmTx201SAdapter<TSettings> :
		AbstractDmTx200BaseAdapter<TSettings>, IDmTx201SAdapter
#endif
		where TSettings : IDmTx201SAdapterSettings, new()
	{
		#region Properties

		private const int OUTPUT_HDMI = 2;

		#endregion

		#region Methods

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int output)
		{
			if (!base.ContainsOutput(output) && output != OUTPUT_HDMI)
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
			return base.ContainsOutput(output) || output == OUTPUT_HDMI;
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
				throw new ArgumentException(string.Format("{0} has no output at address {1}", this, input));

			foreach (ConnectorInfo info in GetBaseOutputs(input, type))
			{
				yield return info;
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
				case eConnectionType.Audio | eConnectionType.Video:
					yield return GetOutput(OUTPUT_HDMI);
					break;

				default:
					throw new ArgumentException("type");
			}
		}

		private IEnumerable<ConnectorInfo> GetBaseOutputs(int input, eConnectionType type)
		{
			return base.GetOutputs(input, type);
		}

		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			foreach (ConnectorInfo info in GetBaseOutputs())
			{
				yield return info;
			}

			yield return new ConnectorInfo(OUTPUT_HDMI, (eConnectionType.Audio | eConnectionType.Video));
		}

		private IEnumerable<ConnectorInfo> GetBaseOutputs()
		{
			return base.GetOutputs();
		}

		public override bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(f => GetActiveTransmissionState(output, f))
								.Unanimous(false);
			}

			if (!ContainsOutput(output))
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
#if !NETSTANDARD

		protected override void TransmitterOnVideoSourceFeedbackEvent(GenericBase device, BaseEventArgs args)
		{
			base.TransmitterOnVideoSourceFeedbackEvent(device, args);

			switch (Transmitter.VideoSourceFeedback)
			{
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Digital:
					SwitcherCache.SetInputForOutput(OUTPUT_HDMI, INPUT_HDMI, eConnectionType.Video);
					break;
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Analog:
					SwitcherCache.SetInputForOutput(OUTPUT_HDMI, INPUT_VGA, eConnectionType.Video);
					break;
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto:
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Disable:
					SwitcherCache.SetInputForOutput(OUTPUT_HDMI, null, eConnectionType.Video);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected override void TransmitterOnAudioSourceFeedbackEvent(GenericBase device, BaseEventArgs args)
		{
			base.TransmitterOnAudioSourceFeedbackEvent(device, args);

			switch (Transmitter.AudioSourceFeedback)
			{
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Digital:
					SwitcherCache.SetInputForOutput(OUTPUT_HDMI, INPUT_HDMI, eConnectionType.Audio);
					break;
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Analog:
					SwitcherCache.SetInputForOutput(OUTPUT_HDMI, INPUT_VGA, eConnectionType.Audio);
					break;
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto:
				case Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Disable:
					SwitcherCache.SetInputForOutput(OUTPUT_HDMI, null, eConnectionType.Audio);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
#endif
		#endregion
	}

	public abstract class AbstractDmTx201SAdapterSettings : AbstractDmTx200BaseAdapterSettings, IDmTx201SAdapterSettings
	{
	}

	public interface IDmTx201SAdapter : IDmTx200BaseAdapter
	{
	}

	public interface IDmTx201SAdapterSettings : IDmTx200BaseAdapterSettings
	{
	}
}
