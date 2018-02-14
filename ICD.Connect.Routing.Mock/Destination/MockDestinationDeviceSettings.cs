using System;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Mock.Destination
{
	/// <summary>
	/// Settings for the MockDestinationDevice.
	/// </summary>
	[KrangSettings(FACTORY_NAME)]
	public sealed class MockDestinationDeviceSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "MockDestinationDevice";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(MockDestinationDevice); } }
	}
}
