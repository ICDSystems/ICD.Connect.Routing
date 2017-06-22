using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Mock.Switcher
{
	/// <summary>
	/// The MockSwitcherDevice does not represent a real hardware device, but allows
	/// us to test systems without hardware connected.
	/// </summary>
	public sealed class MockSwitcherDevice : AbstractDevice<MockSwitcherDeviceSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MockSwitcherDevice()
		{
			Controls.Add(new MockRouteSwitcherControl(this, 0));
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
		/// Adds a source control with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool AddSourceControl(int id)
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

		#region Console

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return
				new GenericConsoleCommand<int>("AddSwitcherControl", "AddSwitcherControl <ID>", id => AddSourceControl(id));
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
