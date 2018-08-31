using System;
using ICD.Connect.Panels;
using ICD.Connect.Panels.Crestron.Controls.TouchScreens;
using ICD.Connect.Panels.CrestronPro.Controls.TouchScreens;
using ICD.Connect.Panels.EventArguments;
using ICD.Connect.Panels.SmartObjectCollections;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls
{
	public abstract class AbstractMPC3BasicTouchScreenControl : AbstractThreeSeriesTouchScreenControl<ControlSystemDevice>,
	                                                            IMPC3BasicTouchScreenControl
	{
		/// <summary>
		/// Raised when the user interacts with the panel.
		/// </summary>
		public override event EventHandler<SigInfoEventArgs> OnAnyOutput;

		/// <summary>
		/// Gets the time that the user last interacted with the panel.
		/// </summary>
		public override DateTime? LastOutput { get { throw new NotImplementedException(); } }

		/// <summary>
		/// Collection containing the loaded SmartObjects of this device.
		/// </summary>
		public override ISmartObjectCollection SmartObjects { get { throw new NotImplementedException(); } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractMPC3BasicTouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Clears the assigned input sig values.
		/// </summary>
		public override void Clear()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends the serial data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="text"></param>
		public override void SendInputSerial(uint number, string text)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends the analog data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="value"></param>
		public override void SendInputAnalog(uint number, ushort value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends the digital data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="value"></param>
		public override void SendInputDigital(uint number, bool value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Unregisters the callback for output sig change events.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="type"></param>
		/// <param name="callback"></param>
		public override void UnregisterOutputSigChangeCallback(uint number, eSigType type, Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Registers the callback for output sig change events.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="type"></param>
		/// <param name="callback"></param>
		public override void RegisterOutputSigChangeCallback(uint number, eSigType type, Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			throw new NotImplementedException();
		}
	}
}