using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Mock;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Mock.Switcher
{
	/// <summary>
	/// The MockSwitcherDevice does not represent a real hardware device, but allows
	/// us to test systems without hardware connected.
	/// </summary>
	public sealed class MockSwitcherDevice : AbstractMockDevice<MockSwitcherDeviceSettings>
	{
		#region Methods

		/// <summary>
		/// Adds a switcher control with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool AddSwitcherControl(int id)
		{
			if (Controls.Contains(id))
				return false;

			Controls.Add(new MockRouteSwitcherControl(this, id));

			return true;
		}

		/// <summary>
		/// Removes the source control with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool RemoveSourceControl(int id)
		{
			return Controls.Remove(id);
		}

		#endregion

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(MockSwitcherDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new MockRouteSwitcherControl(this, 0));
		}

		#region Console

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return
				new GenericConsoleCommand<int>("AddSwitcherControl", "AddSwitcherControl <ID>", id => AddSwitcherControl(id));
			yield return
				new GenericConsoleCommand<int>("RemoveSwitcherControl", "RemoveSourceControl <ID>", id => RemoveSourceControl(id));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
