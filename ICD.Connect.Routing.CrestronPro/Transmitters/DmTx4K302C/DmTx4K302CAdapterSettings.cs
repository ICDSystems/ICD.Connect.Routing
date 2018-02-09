using System;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4K302C
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class DmTx4K302CAdapterSettings : AbstractEndpointTransmitterBaseAdapterSettings
	{
		private const string FACTORY_NAME = "DmTx4k302C";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmTx4K302CAdapter); } }
	}
}
