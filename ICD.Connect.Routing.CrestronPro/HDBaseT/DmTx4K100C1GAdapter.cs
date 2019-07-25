using System;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Settings;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
#endif
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.HDBaseT
{
#if SIMPLSHARP
	public sealed class DmTx4K100C1GAdapter : AbstractDmBasedTEndPointAdapter<DmTx4K100C1G, DmTx4K100C1GAdapterSettings>
#else
	public sealed class DmTx4K100C1GAdapter : AbstractDmBasedTEndPointAdapter<DmTx4K100C1GAdapterSettings>
#endif
	{
#if SIMPLSHARP

		/// <summary>
		/// Constructor.
		/// </summary>
		public DmTx4K100C1GAdapter()
		{
			Controls.Add(new DmTx4K100C1GAdapterRouteMidpointControl(this, 0));
		}

		/// <summary>
		/// Instantiates the device with the given settings.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="deviceFactory"></param>
		/// <returns></returns>
		public override DmTx4K100C1G InstantiateDevice(DmTx4K100C1GAdapterSettings settings, IDeviceFactory deviceFactory)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			if (deviceFactory == null)
				throw new ArgumentNullException("factory");

			if (settings.DmSwitch == null)
				throw new InvalidOperationException("No DM Parent is configured");

			if (settings.DmEndpoint != null)
			{
				IDmEndpoint dmEndpoint = deviceFactory.GetDeviceById((int)settings.DmEndpoint) as IDmEndpoint;
				if (dmEndpoint == null)
					throw new InvalidOperationException(string.Format("Device {0} is not a {1}", settings.DmEndpoint,
																	  typeof(IDmEndpoint).Name));

				return new DmTx4K100C1G(dmEndpoint.Device);
			}

			IDmParent dmParent = deviceFactory.GetDeviceById((int)settings.DmSwitch) as IDmParent;
			if (dmParent == null)
				throw new InvalidOperationException(string.Format("Device {0} is not a {1}", settings.DmSwitch,
																  typeof(IDmParent).Name));

			if (settings.DmInputAddress == null)
				throw new InvalidOperationException("Can't instantiate DM endpoint without an address");

			DMInput input = dmParent.GetDmInput((int)settings.DmInputAddress);

			try
			{
				// DMPS3 4K
				return new DmTx4K100C1G(input);
			}
			catch (ArgumentException)
			{
				if (settings.Ipid == null)
					throw new InvalidOperationException("Can't instantiate DM endpoint without IPID");

				return new DmTx4K100C1G((byte)settings.Ipid, input);
			}
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="io"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		public override Cec GetCecPort(eInputOuptut io, int address)
		{
			if (address != 1)
				throw new ArgumentOutOfRangeException("address");

			if (io != eInputOuptut.Input)
				throw new ArgumentException("io");

			if (Device == null)
				throw new InvalidOperationException("No internal device");

			return Device.StreamCec;
		}
#endif

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(DmTx4K100C1GAdapterSettings settings)
		{
			base.CopySettingsFinal(settings);

#if SIMPLSHARP
			DMInputOutputBase output = Device == null ? null : Device.DMInputOutput;
			settings.DmInputAddress = output == null ? null : (int?)output.Number;
#else
			settings.DmInputAddress = null;
#endif
		}
	}

	[KrangSettings("DmTx4K100C1G", typeof(DmTx4K100C1GAdapter))]
	public sealed class DmTx4K100C1GAdapterSettings : AbstractDmBasedTEndPointAdapterSettings
	{
		private const string DM_INPUT_ELEMENT = "DmInput";

		public int? DmInputAddress { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DM_INPUT_ELEMENT, IcdXmlConvert.ToString(DmInputAddress));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DmInputAddress = XmlUtils.TryReadChildElementContentAsInt(xml, DM_INPUT_ELEMENT);
		}
	}
}
