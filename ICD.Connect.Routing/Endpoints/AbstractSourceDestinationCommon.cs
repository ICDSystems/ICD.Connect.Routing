using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Cores;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationCommon<TSettings> : AbstractOriginator<TSettings>, ISourceDestinationCommon
		where TSettings : ISourceDestinationCommonSettings, new()
	{
		/// <summary>
		/// Raised when the state of EnableWhenOffline property changes
		/// </summary>
		public event EventHandler<BoolEventArgs> OnEnableWhenOfflineChanged;

		private readonly List<int> m_AddressesOrdered;
		private readonly List<EndpointInfo> m_EndpointsOrdered;

		private readonly IcdHashSet<int> m_Addresses;
		private readonly IcdHashSet<EndpointInfo> m_Endpoints;

		private readonly SafeCriticalSection m_AddressesSection;

		private ICore m_CachedCore;
		private bool m_EnableWhenOffline;

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
		/// Indicates that the UI should enable this source/destination even when offline
		/// </summary>
		public bool EnableWhenOffline
		{
			get { return m_EnableWhenOffline; }
			set
			{
				if (m_EnableWhenOffline == value)
					return;

				m_EnableWhenOffline = value;

				OnEnableWhenOfflineChanged.Raise(this, new BoolEventArgs(value));
			}
		}

		/// <summary>
		/// Gets the parent core instance.
		/// </summary>
		public ICore Core { get { return m_CachedCore = m_CachedCore ?? ServiceProvider.GetService<ICore>(); } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractSourceDestinationCommon()
		{
			m_AddressesSection = new SafeCriticalSection();

			m_Addresses = new IcdHashSet<int>();
			m_Endpoints = new IcdHashSet<EndpointInfo>();

			m_AddressesOrdered = new List<int>();
			m_EndpointsOrdered = new List<EndpointInfo>();
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
		/// Gets the devices for this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IDeviceBase> ISourceDestinationBaseCommon.GetDevices()
		{
			yield return Core.Originators.GetChild<IDeviceBase>(Device);
		}

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
				m_AddressesOrdered.InsertSorted(m_Addresses);

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
			EnableWhenOffline = false;

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
			settings.EnableWhenOffline = EnableWhenOffline;

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
			EnableWhenOffline = settings.EnableWhenOffline;

			SetAddresses(settings.GetAddresses());

			if (!GetAddresses().Any())
				Logger.Log(eSeverity.Warning, "No addresses assigned");
		}

		#endregion
	}
}
