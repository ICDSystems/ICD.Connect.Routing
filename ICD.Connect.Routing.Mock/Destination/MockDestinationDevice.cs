using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Mock.Destination
{
	public sealed class MockDestinationDevice : AbstractDevice<MockDestinationDeviceSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MockDestinationDevice()
		{
			Controls.Add(new MockRouteDestinationControl(this, 0));
		}

		#region Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
		}

		/// <summary>
		/// Adds a destination control with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool AddDestinationControl(int id)
		{
			if (Controls.Contains(id))
				return false;

			Controls.Add(new MockRouteDestinationControl(this, id));

			return true;
		}

		/// <summary>
		/// Removes the destination control with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool RemoveDestinationControl(int id)
		{
			return Controls.Remove(id);
		}

		#endregion

		#region Console

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<int>("AddDestinationControl", "AddDestinationControl <ID>",
			                                            id => AddDestinationControl(id));
			yield return new GenericConsoleCommand<int>("RemoveDestinationControl", "RemoveDestinationControl <ID>",
			                                            id => RemoveDestinationControl(id));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
