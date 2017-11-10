using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	/// <summary>
	/// The MockMidpointDevice does not represent a real hardware device, but allows
	/// us to test systems without hardware connected.
	/// </summary>
	public sealed class MockMidpointDevice : AbstractDevice<MockMidpointDeviceSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MockMidpointDevice()
		{
			Controls.Add(new MockRouteMidpointControl(this, 0));
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
		/// Adds a midpoint control with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool AddMidpointControl(int id)
		{
			if (Controls.Contains(id))
				return false;

			Controls.Add(new MockRouteMidpointControl(this, id));

			return true;
		}

		/// <summary>
		/// Removes the midpoint control with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool RemoveMidpointControl(int id)
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
				new GenericConsoleCommand<int>("AddMidpointControl", "AddMidpointControl <ID>", id => AddMidpointControl(id));
			yield return
				new GenericConsoleCommand<int>("RemoveMidpointControl", "RemoveMidpointControl <ID>", id => RemoveMidpointControl(id));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
