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
	public sealed class Dmps3AuxOut1VolumeDeviceControl : AbstractDmps3VolumeDeviceControl
	{

#if SIMPLSHARP
		protected new readonly Card.Dmps3Aux1Output VolumeObject;
#endif
		public Dmps3AuxOut1VolumeDeviceControl(ControlSystemDevice parent, int id, string name, Card.Dmps3Aux1Output output, string xml)
			: base(parent, id, name, output)
		{
			VolumeObject = output;
			SetDefaultOnCrosspointsFromXml(xml);
		}

		#region Methods

		protected override void SetCodec2Level(short gainLevel)
		{
			VolumeObject.Codec2Level.ShortValue = (short)(gainLevel * 10);
		}

		protected override void SetCodec2Mute(bool mute)
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