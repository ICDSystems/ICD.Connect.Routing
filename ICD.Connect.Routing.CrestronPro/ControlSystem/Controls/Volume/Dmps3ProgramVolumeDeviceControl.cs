using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
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
	public sealed class Dmps3ProgramVolumeDeviceControl : AbstractDmps3VolumeDeviceControl
	{

#if SIMPLSHARP
		protected new readonly Card.Dmps3ProgramOutput VolumeObject;
#endif
		public Dmps3ProgramVolumeDeviceControl(ControlSystemDevice parent, int id, string name, uint outputAddress)
			: base(parent, id, name, outputAddress)
		{
			VolumeObject = Parent.ControlSystem.SwitcherOutputs[outputAddress] as Card.Dmps3ProgramOutput;
		}

		#region Methods

		public void SetCodec1Level(short gainLevel)
		{
			VolumeObject.Codec1Level.ShortValue = (short)(gainLevel * 10);
		}

		public void SetCodec2Level(short gainLevel)
		{
			VolumeObject.Codec2Level.ShortValue = (short)(gainLevel * 10);
		}

		public void SetCodec1Mute(bool mute)
		{
			if (VolumeObject.CodecMute1OnFeedback.BoolValue & !mute)
			{
				VolumeObject.Codec1MuteOff();
			}
			else if (VolumeObject.CodecMute1OffFeedback.BoolValue & mute)
			{
				VolumeObject.Codec1MuteOn();
			}
		}

		public void SetCodec2Mute(bool mute)
		{
			if (VolumeObject.CodecMute2OnFeedback.BoolValue & !mute)
			{
				VolumeObject.Codec2MuteOff();
			}
			else if (VolumeObject.CodecMute2OffFeedback.BoolValue & mute)
			{
				VolumeObject.Codec2MuteOn();
			}
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<bool>("SetCodec1Mute", "SetCodec1Mute <BOOL>", r => SetCodec1Mute(r));
			yield return new GenericConsoleCommand<short>("SetCodec1Level", "SetCodec1Level <SHORT>", r => SetCodec1Level(r));

			yield return new GenericConsoleCommand<bool>("SetCodec2Mute", "SetCodec2Mute <BOOL>", r => SetCodec2Mute(r));
			yield return new GenericConsoleCommand<short>("SetCodec2Level", "SetCodec2Level <SHORT>", r => SetCodec2Level(r));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion

	}
}
#endif