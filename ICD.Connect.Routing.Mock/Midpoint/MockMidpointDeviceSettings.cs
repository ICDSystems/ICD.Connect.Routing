using System;
using ICD.Common.Properties;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	public sealed class MockMidpointDeviceSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "MockMidpointDevice";

		#region Properties

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(MockMidpointDevice); } }

		#endregion

		#region Methods

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static MockMidpointDeviceSettings FromXml(string xml)
		{
			MockMidpointDeviceSettings output = new MockMidpointDeviceSettings();

			ParseXml(output, xml);
			return output;
		}

		#endregion
	}
}
