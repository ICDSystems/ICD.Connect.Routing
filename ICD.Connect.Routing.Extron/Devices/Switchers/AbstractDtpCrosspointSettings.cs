using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Extron.Devices.Switchers
{
	public abstract class AbstractDtpCrosspointSettings : AbstractDeviceSettings, IDtpCrosspointSettings
	{
		private const string ELEMENT_PORT = "Port";
		private const string ELEMENT_PASSWORD = "Password";
	    private const string ADDRESS_ELEMENT = "Address";

		private const string ELEMENT_DTP_INPUT_PORT = "DtpInputPort";
		private const string ELEMENT_DTP_INPUT_PORTS = ELEMENT_DTP_INPUT_PORT + "s";
		private const string ELEMENT_DTP_OUTPUT_PORT = "DtpOutputPort";
		private const string ELEMENT_DTP_OUTPUT_PORTS = ELEMENT_DTP_OUTPUT_PORT + "s";
		private const string ELEMENT_INPUT = "Input";
		private const string ELEMENT_OUTPUT = "Output";

		/// <summary>
		/// The port id.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(ISerialPort))]
		public int? Port { get; set; }

		public string Password { get; set; }

		[IpAddressSettingsProperty]
        public string Address { get; set; }

		private Dictionary<int, int> m_DtpInputPorts = new Dictionary<int, int>();
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
					if(item.Key <= 0)
					{
						Logger.AddEntry(eSeverity.Error, "{0} - DtpInputPort -> Input value must be greater than 0");
						continue;
					}
					if(item.Value <= 0)
					{
						Logger.AddEntry(eSeverity.Error, "{0} - DtpInputPort -> Port value must be greater than 0");
						continue;
					}

					m_DtpInputPorts.Add(item.Key, item.Value);
				}
			}
		}

		private Dictionary<int, int> m_DtpOutputPorts = new Dictionary<int, int>();
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
					if(item.Key <= 0)
					{
						Logger.AddEntry(eSeverity.Error, "{0} - DtpInputPort -> Input value must be greater than 0");
						continue;
					}
					if(item.Value <= 0)
					{
						Logger.AddEntry(eSeverity.Error, "{0} - DtpInputPort -> Port value must be greater than 0");
						continue;
					}

					m_DtpOutputPorts.Add(item.Key, item.Value);
				}
			}
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ELEMENT_PORT, IcdXmlConvert.ToString(Port));
			writer.WriteElementString(ELEMENT_PASSWORD, Password);
            writer.WriteElementString(ADDRESS_ELEMENT, Address);

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

			Port = XmlUtils.TryReadChildElementContentAsInt(xml, ELEMENT_PORT);
			Password = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PASSWORD);
		    Address = XmlUtils.TryReadChildElementContentAsString(xml, ADDRESS_ELEMENT);

			DtpInputPorts = XmlUtils.ReadDictFromXml(xml, ELEMENT_DTP_INPUT_PORTS, ELEMENT_DTP_INPUT_PORT, ELEMENT_INPUT, ELEMENT_PORT,
					key => XmlUtils.TryReadElementContentAsInt(key) ?? 0,
					value => XmlUtils.TryReadElementContentAsInt(value) ?? 0);

			DtpOutputPorts = XmlUtils.ReadDictFromXml(xml, ELEMENT_DTP_OUTPUT_PORTS, ELEMENT_DTP_OUTPUT_PORT, ELEMENT_OUTPUT, ELEMENT_PORT,
					key => XmlUtils.TryReadElementContentAsInt(key) ?? 0,
					value => XmlUtils.TryReadElementContentAsInt(value) ?? 0);
		}
	}
}