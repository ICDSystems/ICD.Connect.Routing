using System;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.EventArguments;
using ICD.Connect.Devices.Proxies.Controls;
using ICD.Connect.Devices.Proxies.Devices;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy
{
	public sealed class ProxySPlusDestinationPowerControl : AbstractProxyDeviceControl, IPowerDeviceControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ProxySPlusDestinationPowerControl(IProxyDeviceBase parent, int id) : base(parent, id)
		{
		}

		public event EventHandler<PowerDeviceControlPowerStateApiEventArgs> OnIsPoweredChanged;

		private bool m_IsPowered;

		/// <summary>
		/// Gets the powered state of the device.
		/// </summary>
		public bool IsPowered
		{
			get { return m_IsPowered; }
			protected set
			{
				if (value == m_IsPowered)
					return;

				m_IsPowered = value;

				Log(eSeverity.Informational, "IsPowered set to {0}", m_IsPowered);

				OnIsPoweredChanged.Raise(this, new PowerDeviceControlPowerStateApiEventArgs(m_IsPowered));
			}
		}

		/// <summary>
		/// Powers on the device.
		/// </summary>
		public void PowerOn()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Powers on the device.
		/// </summary>
		/// /// <param name="bypassPrePowerOn">If true, skips the pre power on delegate.</param>
		public void PowerOn(bool bypassPrePowerOn)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Powers off the device.
		/// </summary>
		public void PowerOff()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Powers off the device.
		/// </summary>
		/// <param name="bypassPostPowerOff">If true, skips the post power off delegate.</param>
		public void PowerOff(bool bypassPostPowerOff)
		{
			throw new NotImplementedException();
		}
	}
}