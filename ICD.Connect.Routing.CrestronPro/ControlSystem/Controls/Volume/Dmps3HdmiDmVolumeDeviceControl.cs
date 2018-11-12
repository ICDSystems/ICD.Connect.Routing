using System;
using System.Text.RegularExpressions;
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
	public sealed class Dmps3HdmiDmVolumeDeviceControl : AbstractDmps3VolumeDeviceControl
	{
		public Dmps3HdmiDmVolumeDeviceControl(ControlSystemDevice parent, int id, string name, uint outputAddress)
			: base(parent, id, name, outputAddress)
		{
		}
	}
}
#endif