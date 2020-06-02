using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Mock;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Mock.Destination
{
	public sealed class MockDestinationDevice : AbstractMockDevice<MockDestinationDeviceSettings>
	{
		#region Methods

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

		#region Settings

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(MockDestinationDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new MockRouteDestinationControl(this, 0));
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
