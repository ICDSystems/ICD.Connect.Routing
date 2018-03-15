using System;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdMNXN;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd8X8
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class DmMd8X8AdapterSettings : AbstractDmMdMNXNAdapterSettings
	{
		private const string FACTORY_NAME = "DmMd8x8";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmMd8X8Adapter); } }
	}
}
