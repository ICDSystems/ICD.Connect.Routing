using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Mock;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	/// <summary>
	/// The MockMidpointDevice does not represent a real hardware device, but allows
	/// us to test systems without hardware connected.
	/// </summary>
	public sealed class MockMidpointDevice : AbstractMockDevice<MockMidpointDeviceSettings>
	{
		#region Methods

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

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(MockMidpointDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new MockRouteMidpointControl(this, 0));
		}

		/// <summary>
		/// Override to add actions on StartSettings
		/// This should be used to start communications with devices and perform initial actions
		/// </summary>
		protected override void StartSettingsFinal()
		{
			base.StartSettingsFinal();

			MockRouteMidpointControl control = Controls.GetControl<MockRouteMidpointControl>();
			if (control == null || control.GetInputs().Count() != 1 || control.GetOutputs().Count() != 1)
				return;

			ConnectorInfo input = control.GetInputs().First();
			ConnectorInfo output = control.GetOutputs().First();
			eConnectionType intersection = EnumUtils.GetFlagsIntersection(input.ConnectionType, output.ConnectionType);

			control.SetInputForOutput(output.Address, input.Address, intersection);
		}

		#region Console

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return
				new GenericConsoleCommand<int>("AddMidpointControl", "AddMidpointControl <ID>", id => AddMidpointControl(id));
			yield return
				new GenericConsoleCommand<int>("RemoveMidpointControl", "RemoveMidpointControl <ID>",
				                               id => RemoveMidpointControl(id));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
