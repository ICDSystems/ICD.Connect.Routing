using System;
using System.Text.RegularExpressions;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#endif
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.EventArguments;
#if SIMPLSHARP
namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume
{
	public sealed class Dmps3HdmiDmBackEndVolumeDeviceControl : AbstractDmps3VolumeDeviceControl
	{
		private CrestronControlSystem.Dmps3AttachableOutputMixer m_AttachableVolumeOutputMixer;

		public Dmps3HdmiDmBackEndVolumeDeviceControl(ControlSystemDevice parent, int id, string name, uint outputAddress)
			: base(parent, id, name, outputAddress)
		{
			m_AttachableVolumeOutputMixer = ((IOutputMixer)VolumeObject).OutputMixer as CrestronControlSystem.Dmps3AttachableOutputMixer;
		}

		#region Methods

		public override void SetMicrophoneMute(ushort microphone, bool mute)
		{
			if (!MicrophoneSupported(microphone)) return;

			if (m_AttachableVolumeOutputMixer.MicMuteOnFeedback[microphone].BoolValue & !mute)
			{
				m_AttachableVolumeOutputMixer.MicMuteOff(microphone);
			}
			else if (m_AttachableVolumeOutputMixer.MicMuteOffFeedback[microphone].BoolValue & mute)
			{
				m_AttachableVolumeOutputMixer.MicMuteOn(microphone);
			}
		}

		public override void SetMicrophoneLevel(ushort microphone, short gainLevel)
		{
			if (MicrophoneSupported(microphone))
				m_AttachableVolumeOutputMixer.MicLevel[microphone].ShortValue = (short)(gainLevel * 10);
		}

		public override void SetVolumeLevel(float volume)
		{
			m_AttachableVolumeOutputMixer.MasterVolume.ShortValue = (short)(volume * 10);
		}

		public override void SetVolumeMute(bool mute)
		{
			if (mute && !VolumeIsMuted)
			{
				m_AttachableVolumeOutputMixer.MasterMuteOn();
			}
			else if (!mute && VolumeIsMuted)
			{
				m_AttachableVolumeOutputMixer.MasterMuteOff();
			}
		}

		public override void SetMicMasterMute(bool mute)
		{
			if (mute)
			{
				m_AttachableVolumeOutputMixer.MicMasterMuteOn();
			}
			else
			{
				m_AttachableVolumeOutputMixer.MicMasterMuteOff();
			}
				
		}

		public override void SetMicMasterLevel(short gainLevel)
		{
			m_AttachableVolumeOutputMixer.MicMasterLevel.ShortValue = (short)(gainLevel * 10);
		}

		public override void SetSourceLevel(short gainLevel)
		{
			m_AttachableVolumeOutputMixer.SourceLevel.ShortValue = (short)(gainLevel * 10);
		}

		public override void SetSourceMute(bool mute)
		{
			if (mute && m_AttachableVolumeOutputMixer.SourceMuteOffFeedBack.BoolValue)
			{
				m_AttachableVolumeOutputMixer.SourceMuteOn();
			}
			else if (!mute && m_AttachableVolumeOutputMixer.SourceMuteOnFeedBack.BoolValue)
			{
				m_AttachableVolumeOutputMixer.SourceMuteOff();
			}
		}

		#endregion

		#region Private Methods

		private bool MicrophoneSupported(ushort microphone)
		{

			if (m_AttachableVolumeOutputMixer.MicPan.Contains(microphone) && m_AttachableVolumeOutputMixer.MicPan[microphone].Supported)
				return true;

			Log(eSeverity.Warning, "Microphone {0} is not supported", microphone);
			return false;
		}

		#endregion

		protected override void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			if (args.Number != VolumeObject.Number)
				return;

			switch (args.EventId)
			{
				case DMOutputEventIds.AudioOutEventId:
					m_AttachableVolumeOutputMixer = ((IOutputMixer)VolumeObject).OutputMixer as CrestronControlSystem.Dmps3AttachableOutputMixer;
					break;
				default:
					IcdConsole.PrintLine(eConsoleColor.Magenta, "DMOutputEventArgs output={0} eventId={1}", args.Number, args.EventId);
					break;
			}
		}
	}
}
#endif