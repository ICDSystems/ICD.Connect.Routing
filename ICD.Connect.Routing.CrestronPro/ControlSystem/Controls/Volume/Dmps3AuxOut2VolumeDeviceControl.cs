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
	public sealed class Dmps3AuxOut2VolumeDeviceControl : AbstractDmps3VolumeDeviceControl
	{

#if SIMPLSHARP
		protected new readonly Card.Dmps3Aux2Output VolumeObject;
#endif
		public Dmps3AuxOut2VolumeDeviceControl(ControlSystemDevice parent, int id, string name, Card.Dmps3Aux2Output output, string xml)
			: base(parent, id, name, output)
		{
			VolumeObject = output;
			SetDefaultOnCrosspointsFromXml(xml);
		}

		#region Methods

		protected override void SetCodec1Level(short gainLevel)
		{
			VolumeObject.Codec1Level.ShortValue = (short)(gainLevel * 10);
		}

		protected override void SetCodec1Mute(bool mute)
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