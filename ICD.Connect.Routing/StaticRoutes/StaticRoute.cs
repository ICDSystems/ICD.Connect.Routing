using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.StaticRoutes
{
	public sealed class StaticRoute : AbstractOriginator<StaticRouteSettings>
	{
		private readonly IcdHashSet<int> m_Connections;
		private readonly SafeCriticalSection m_ConnectionsSection;

		public eConnectionType ConnectionType { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StaticRoute()
		{
			m_Connections = new IcdHashSet<int>();
			m_ConnectionsSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Sets the connections for this static route.
		/// </summary>
		/// <param name="connections"></param>
		[PublicAPI]
		public void SetConnections(IEnumerable<int> connections)
		{
			m_ConnectionsSection.Enter();

			try
			{
				m_Connections.Clear();
				m_Connections.AddRange(connections);
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the connections for this static route.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<int> GetConnections()
		{
			return m_ConnectionsSection.Execute(() => m_Connections.ToArray());
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(StaticRouteSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.ConnectionType = ConnectionType;
			settings.SetConnections(GetConnections());
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			ConnectionType = default(eConnectionType);
			SetConnections(Enumerable.Empty<int>());
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(StaticRouteSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			ConnectionType = settings.ConnectionType;
			SetConnections(settings.GetConnections());
		}

		#endregion
	}
}
