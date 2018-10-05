using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationBase<TSettings> : AbstractOriginator<TSettings>, ISourceDestinationBase
		where TSettings : ISourceDestinationBaseSettings, new()
	{
		private readonly List<int> m_AddressesOrdered;
		private readonly List<EndpointInfo> m_EndpointsOrdered;

		private readonly IcdHashSet<int> m_Addresses;
		private readonly IcdHashSet<EndpointInfo> m_Endpoints;

		private readonly SafeCriticalSection m_AddressesSection;

		/// <summary>
		/// Raised when the disable state changes.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnDisableStateChanged;

		private bool m_Disable;

		#region Properties

		/// <summary>
		/// Specifies the device this source/destination is pointing to.
		/// </summary>
		public int Device { get; set; }

		/// <summary>
		/// Specifies the control this source/destination is pointing to.
		/// </summary>
		public int Control { get; set; }

		/// <summary>
		/// Specifies which media types to use for this source.
		/// </summary>
		public eConnectionType ConnectionType { get; set; }

		/// <summary>
		/// Specifies if this instance was discovered via remote broadcast.
		/// </summary>
		public bool Remote { get; set; }

		/// <summary>
		/// Specifies custom ordering of the instance to the end user.
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		/// Shorthand for disabling an instance in the system.
		/// </summary>
		public bool Disable
		{
			get { return m_Disable; }
			set
			{
				if (value == m_Disable)
					return;

				m_Disable = value;

				OnDisableStateChanged.Raise(this, new BoolEventArgs(m_Disable));
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractSourceDestinationBase()
		{
			m_AddressesSection = new SafeCriticalSection();

			m_Addresses = new IcdHashSet<int>();
			m_Endpoints = new IcdHashSet<EndpointInfo>();

			m_AddressesOrdered = new List<int>();
			m_EndpointsOrdered = new List<EndpointInfo>();
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnDisableStateChanged = null;

			base.DisposeFinal(disposing);
		}

		/// <summary>
		/// Override to add additional properties to the ToString representation.
		/// </summary>
		/// <param name="addPropertyAndValue"></param>
		protected override void BuildStringRepresentationProperties(Action<string, object> addPropertyAndValue)
		{
			base.BuildStringRepresentationProperties(addPropertyAndValue);

			addPropertyAndValue("Device", Device);
			addPropertyAndValue("Control", Control);
			addPropertyAndValue("Addresses", StringUtils.ArrayRangeFormat(GetAddresses()));
			addPropertyAndValue("ConnectionType", ConnectionType);
		}

		#region Methods

		/// <summary>
		/// Gets the addresses used by this source/destination.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetAddresses()
		{
			return m_AddressesSection.Execute(() => m_AddressesOrdered.ToArray(m_AddressesOrdered.Count));
		}

		/// <summary>
		/// Gets all of the addresses as endpoint info.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> GetEndpoints()
		{
			return m_AddressesSection.Execute(() => m_EndpointsOrdered.ToArray(m_EndpointsOrdered.Count));
		}

		/// <summary>
		/// Sets the addresses used by this source/destination.
		/// </summary>
		/// <param name="addresses"></param>
		public void SetAddresses(IEnumerable<int> addresses)
		{
			m_AddressesSection.Enter();

			try
			{
				m_Addresses.Clear();
				m_Endpoints.Clear();

				m_AddressesOrdered.Clear();
				m_EndpointsOrdered.Clear();

				m_Addresses.AddRange(addresses);
				m_AddressesOrdered.AddSorted(m_Addresses);

				foreach (EndpointInfo endpoint in m_AddressesOrdered.Select(i => new EndpointInfo(Device, Control, i)))
				{
					m_EndpointsOrdered.Add(endpoint);
					m_Endpoints.Add(endpoint);
				}
			}
			finally
			{
				m_AddressesSection.Leave();
			}
		}

		/// <summary>
		/// Returns true if the source/destination contains the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <returns></returns>
		public bool Contains(EndpointInfo endpoint)
		{
			return m_AddressesSection.Execute(() => m_Endpoints.Contains(endpoint));
		}

		/// <summary>
		/// Filters the endpoints by the endpoints contained in this source/destination.
		/// </summary>
		/// <param name="endpoints"></param>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> FilterEndpoints(IEnumerable<EndpointInfo> endpoints)
		{
			if (endpoints == null)
				throw new ArgumentNullException("endpoints");

			m_AddressesSection.Enter();

			try
			{
				return endpoints.Where(e => m_Endpoints.Contains(e)).ToArray();
			}
			finally
			{
				m_AddressesSection.Leave();
			}
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Device = 0;
			Control = 0;
			Order = 0;
			Disable = false;
			ConnectionType = default(eConnectionType);

			SetAddresses(Enumerable.Empty<int>());
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Device = Device;
			settings.Control = Control;
			settings.Order = Order;
			settings.Disable = Disable;
			settings.ConnectionType = ConnectionType;

			settings.SetAddresses(GetAddresses());
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			factory.LoadOriginator(settings.Device);

			base.ApplySettingsFinal(settings, factory);

			Device = settings.Device;
			Control = settings.Control;
			Order = settings.Order;
			Disable = settings.Disable;
			ConnectionType = settings.ConnectionType;

			SetAddresses(settings.GetAddresses());
		}

		#endregion
	}
}
