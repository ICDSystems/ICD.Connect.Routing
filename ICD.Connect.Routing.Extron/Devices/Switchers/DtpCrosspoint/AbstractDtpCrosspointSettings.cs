using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint
{
	public abstract class AbstractDtpCrosspointSettings : AbstractExtronSwitcherDeviceSettings, IDtpCrosspointSettings
	{
		private const string ELEMENT_PORT = "Port";
		private const string ELEMENT_ADDRESS = "Address";
		private const string ELEMENT_CONFIG = "Config";

		private const string ELEMENT_DTP_INPUT_PORT = "DtpInputPort";
		private const string ELEMENT_DTP_INPUT_PORTS = ELEMENT_DTP_INPUT_PORT + "s";
		private const string ELEMENT_DTP_OUTPUT_PORT = "DtpOutputPort";
		private const string ELEMENT_DTP_OUTPUT_PORTS = ELEMENT_DTP_OUTPUT_PORT + "s";
		private const string ELEMENT_INPUT = "Input";
		private const string ELEMENT_OUTPUT = "Output";

		private readonly Dictionary<int, int> m_DtpInputPorts = new Dictionary<int, int>();

		private readonly Dictionary<int, int> m_DtpOutputPorts = new Dictionary<int, int>();

		#region Properties

		[IpAddressSettingsProperty]
		public string Address { get; set; }

		public string Config { get; set; }

		public IEnumerable<KeyValuePair<int, int>> DtpInputPorts
		{
			get { return m_DtpInputPorts.ToArray(m_DtpInputPorts.Count); }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				m_DtpInputPorts.Clear();

				foreach (KeyValuePair<int, int> item in value)
				{
					if (m_DtpInputPorts.ContainsKey(item.Key))
					{
						Logger.AddEntry(eSeverity.Error, "{0} unable to add port id for duplicate input {1}", GetType().Name,
							item.Key);
						continue;
					}
					if (item.Key <= 0)
					{
						Logger.AddEntry(eSeverity.Error, "{0} - DtpInputPort -> Input value must be greater than 0");
						continue;
					}
					if (item.Value <= 0)
					{
						Logger.AddEntry(eSeverity.Error, "{0} - DtpInputPort -> Port value must be greater than 0");
						continue;
					}

					m_DtpInputPorts.Add(item.Key, item.Value);
				}
			}
		}

		public IEnumerable<KeyValuePair<int, int>> DtpOutputPorts
		{
			get { return m_DtpOutputPorts.ToArray(m_DtpOutputPorts.Count); }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				m_DtpOutputPorts.Clear();

				foreach (KeyValuePair<int, int> item in value)
				{
					if (m_DtpOutputPorts.ContainsKey(item.Key))
					{
						Logger.AddEntry(eSeverity.Error, "{0} unable to add port id for duplicate output {1}", GetType().Name,
							item.Key);
						continue;
					}
					if (item.Key <= 0)
					{
						Logger.AddEntry(eSeverity.Error, "{0} - DtpInputPort -> Input value must be greater than 0");
						continue;
					}
					if (item.Value <= 0)
					{
						Logger.AddEntry(eSeverity.Error, "{0} - DtpInputPort -> Port value must be greater than 0");
						continue;
					}

					m_DtpOutputPorts.Add(item.Key, item.Value);
				}
			}
		}

		#endregion

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ELEMENT_ADDRESS, Address);
			writer.WriteElementString(ELEMENT_CONFIG, Config);

			XmlUtils.WriteDictToXml(writer, DtpInputPorts, ELEMENT_DTP_INPUT_PORTS, ELEMENT_DTP_INPUT_PORT,
				(w, i) => w.WriteElementString(ELEMENT_INPUT, IcdXmlConvert.ToString(i)),
				(w, p) => w.WriteElementString(ELEMENT_PORT, IcdXmlConvert.ToString(p)));
			XmlUtils.WriteDictToXml(writer, DtpOutputPorts, ELEMENT_DTP_OUTPUT_PORTS, ELEMENT_DTP_OUTPUT_PORT,
				(w, i) => w.WriteElementString(ELEMENT_OUTPUT, IcdXmlConvert.ToString(i)),
				(w, p) => w.WriteElementString(ELEMENT_PORT, IcdXmlConvert.ToString(p)));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Address = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_ADDRESS);
			Config = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_CONFIG);

			DtpInputPorts = XmlUtils.ReadDictFromXml(xml, ELEMENT_DTP_INPUT_PORTS, ELEMENT_DTP_INPUT_PORT, ELEMENT_INPUT, ELEMENT_PORT,
					key => XmlUtils.TryReadElementContentAsInt(key) ?? 0,
					value => XmlUtils.TryReadElementContentAsInt(value) ?? 0);

			DtpOutputPorts = XmlUtils.ReadDictFromXml(xml, ELEMENT_DTP_OUTPUT_PORTS, ELEMENT_DTP_OUTPUT_PORT, ELEMENT_OUTPUT, ELEMENT_PORT,
					key => XmlUtils.TryReadElementContentAsInt(key) ?? 0,
					value => XmlUtils.TryReadElementContentAsInt(value) ?? 0);
		}
	}
}