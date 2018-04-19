using System;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd6XN;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd6X1
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class DmMd6X1AdapterSettings : AbstractDmMd6XNAdapterSettings
	{
		private const string FACTORY_NAME = "DmMd6X1";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { throw new NotImplementedException(); } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmMd6X1Adapter); } }
	}
}