using System;
using ICD.Connect.Devices.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
#endif
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.HDBaseT
{
#if SIMPLSHARP
	public sealed class DmRmc4K100C1GAdapter : AbstractDmBasedTEndPointAdapter<DmRmc4K100C1G, DmRmc4K100C1GAdapterSettings>
#else
	public sealed class DmRmc4K100C1GAdapter : AbstractDmBasedTEndPointAdapter<DmRmc4K100C1GAdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmRmc4K100C1GAdapter()
		{
			Controls.Add(new DmRmc4K100C1GAdapterRouteMidpointControl(this, 0));
		}

		/// <summary>
		/// Instantiates the device with the given settings.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="deviceFactory"></param>
		/// <returns></returns>
		public override DmRmc4K100C1G InstantiateDevice(DmRmc4K100C1GAdapterSettings settings, IDeviceFactory deviceFactory)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			if (deviceFactory == null)
				throw new ArgumentNullException("factory");

			if (settings.DmEndpoint != null)
			{
				IDmEndpoint dmEndpoint = deviceFactory.GetDeviceById((int)settings.DmEndpoint) as IDmEndpoint;
				if (dmEndpoint == null)
					throw new InvalidOperationException(string.Format("Device {0} is not a {1}", settings.DmEndpoint,
					                                                  typeof(IDmEndpoint).Name));

				return new DmRmc4K100C1G(dmEndpoint.Device);
			}

			if (settings.DmSwitch == null)
				throw new InvalidOperationException("No DM Parent is configured");

			IDmParent dmParent = deviceFactory.GetDeviceById((int)settings.DmSwitch) as IDmParent;
			if (dmParent == null)
				throw new InvalidOperationException(string.Format("Device {0} is not a {1}", settings.DmSwitch,
																  typeof(IDmParent).Name));

			if (settings.DmOutputAddress == null)
				throw new InvalidOperationException("Can't instantiate DM endpoint without an address");

			DMOutput output = dmParent.GetDmOutput((int)settings.DmOutputAddress);

			try
			{
				// DMPS3 4K & DM-CPU3
				return new DmRmc4K100C1G(output);
			}
			catch (Exception)
			{
				if (settings.Ipid == null)
					throw new InvalidOperationException("Can't instantiate DM endpoint without IPID");

				return new DmRmc4K100C1G((byte)settings.Ipid, output);
			}
		}
#endif

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(DmRmc4K100C1GAdapterSettings settings)
		{
			base.CopySettingsFinal(settings);

#if SIMPLSHARP
			DMInputOutputBase output = Device == null ? null : Device.DMInputOutput;
			settings.DmOutputAddress = output == null ? null : (int?)output.Number;
#else
			settings.DmOutputAddress = null;
#endif
		}
	}

	[KrangSettings("DmRmc4k100C1G", typeof(DmRmc4K100C1GAdapter))]
	public sealed class DmRmc4K100C1GAdapterSettings : AbstractDmBasedTEndPointAdapterSettings
	{
		private const string DM_OUTPUT_ELEMENT = "DmOutput";

		public int? DmOutputAddress { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DM_OUTPUT_ELEMENT, IcdXmlConvert.ToString(DmOutputAddress));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DmOutputAddress = XmlUtils.TryReadChildElementContentAsInt(xml, DM_OUTPUT_ELEMENT);
		}
	}
}
