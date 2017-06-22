using System;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationBase<TSettings> : AbstractOriginator<TSettings>, ISourceDestinationBase
		where TSettings : AbstractSourceDestinationBaseSettings, new()
	{
		#region Properties

		/// <summary>
		/// Contains information about the physical endpoint
		/// </summary>
		public EndpointInfo Endpoint { get; set; }

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
		public bool Disable { get; set; }

		#endregion

		/// <summary>
		/// Override to add additional properties to the ToString representation.
		/// </summary>
		/// <param name="addPropertyAndValue"></param>
		protected override void BuildStringRepresentationProperties(Action<string, object> addPropertyAndValue)
		{
			base.BuildStringRepresentationProperties(addPropertyAndValue);

			addPropertyAndValue("Device", Endpoint.Device);
			addPropertyAndValue("Control", Endpoint.Control);
			addPropertyAndValue("Address", Endpoint.Address);
			addPropertyAndValue("ConnectionType", ConnectionType);
		}

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Endpoint = default(EndpointInfo);
			Order = int.MaxValue;
			Disable = false;
			ConnectionType = default(eConnectionType);
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Device = Endpoint.Device;
			settings.Control = Endpoint.Control;
			settings.Address = Endpoint.Address;
			settings.Order = Order;
			settings.Disable = Disable;
			settings.ConnectionType = ConnectionType;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			Endpoint = new EndpointInfo(settings.Device, settings.Control, settings.Address);
			Order = settings.Order;
			Disable = settings.Disable;
			ConnectionType = settings.ConnectionType;
		}

		#endregion
	}
}
