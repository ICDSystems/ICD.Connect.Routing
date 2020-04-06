using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices.Mock;

namespace ICD.Connect.Routing.Mock.Source
{
	/// <summary>
	/// Virtual representation of a source device.
	/// </summary>
	public sealed class MockSourceDevice : AbstractMockDevice<MockSourceDeviceSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MockSourceDevice()
		{
			Controls.Add(new MockRouteSourceControl(this, 0));
		}

		#region Methods

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

			Controls.Add(new MockRouteSourceControl(this, id));

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

			yield return new GenericConsoleCommand<int>("AddSourceControl", "AddSourceControl <ID>", id => AddSourceControl(id));
			yield return
				new GenericConsoleCommand<int>("RemoveSourceControl", "RemoveSourceControl <ID>", id => RemoveSourceControl(id));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
