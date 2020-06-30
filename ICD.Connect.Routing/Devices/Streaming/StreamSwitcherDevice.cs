using System.Linq;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Devices.Streaming
{
	public sealed class StreamSwitcherDevice : AbstractDevice<StreamSwitcherDeviceSettings>
	{
		private StreamSwitcherDeviceRoutingControl RoutingControl
		{
			get
			{
				return Controls.GetControls<StreamSwitcherDeviceRoutingControl>().FirstOrDefault();
			}
		}

		public StreamSwitcherDevice()
		{
			Controls.Add(new StreamSwitcherDeviceRoutingControl(this, 0));
		}

		protected override bool GetIsOnlineStatus()
		{
			return true;
		}

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			var routingControl = RoutingControl;
			if (routingControl != null)
				routingControl.ClearEndpointCache();
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(StreamSwitcherDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			// Load all of the endpoints
			foreach (Connection connection in factory.GetOriginators<Connection>())
			{
				if (connection.Destination.Device == Id)
					factory.LoadOriginator(connection.Source.Device);

				if (connection.Source.Device == Id)
					factory.LoadOriginator(connection.Destination.Device);
			}

			RoutingControl.BuildEndpointCache();
		}

		#endregion
	}
}
