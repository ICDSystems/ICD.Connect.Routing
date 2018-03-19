using System;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8X2
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class HdMd8X2AdapterSettings : AbstractCrestronSwitchAdapterSettings
	{
		private const string FACTORY_NAME = "HdMd8X2";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(HdMd8X2Adapter); } }
	}
}
