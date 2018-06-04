using System;
using ICD.Connect.Devices.Simpl;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.SPlus
{
	[KrangSettings("SPlusSwitcher", typeof(SPlusSwitcher))]
	public sealed class SPlusSwitcherSettings : AbstractSimplDeviceSettings
	{
		
	}
}
