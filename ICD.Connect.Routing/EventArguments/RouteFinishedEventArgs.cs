using System;
using ICD.Common.Properties;

namespace ICD.Connect.Routing.EventArguments
{
	/// <summary>
	/// Used when an output starts routing a different input.
	/// </summary>
	public sealed class RouteFinishedEventArgs : EventArgs
	{
		private readonly RouteOperation m_RouteOperation;
		private readonly bool m_Success;

		/// <summary>
		/// The route operation id.
		/// </summary>
		[PublicAPI]
		public RouteOperation Route { get { return m_RouteOperation; } }

		/// <summary>
		/// Success of the route operation
		/// </summary>
		[PublicAPI]
		public bool Success { get { return m_Success; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="routeOperation"></param>
		/// <param name="success"></param>
		public RouteFinishedEventArgs(RouteOperation routeOperation, bool success)
		{
			m_RouteOperation = routeOperation;
			m_Success = success;
		}
	}
}
