#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using System;
using ICD.Connect.Panels;
using ICD.Connect.Panels.CrestronPro.Controls.TouchScreens;
using ICD.Connect.Panels.EventArguments;
using ICD.Connect.Panels.SmartObjectCollections;
using eSigType = ICD.Connect.Protocol.Sigs.eSigType;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens
{
#if SIMPLSHARP
	public abstract class AbstractControlSystemTouchScreenControl<TTouchScreen> :
		AbstractThreeSeriesTouchScreenControl<ControlSystemDevice>
		where TTouchScreen : ThreeSeriesTouchScreen
#else
	public abstract class AbstractControlSystemTouchScreenControl : AbstractThreeSeriesTouchScreenControl<ControlSystemDevice>
#endif
	{
		/// <summary>
		/// Raised when the user interacts with the panel.
		/// </summary>
		public override event EventHandler<SigInfoEventArgs> OnAnyOutput;

		#region Panel Properties

		/// <summary>
		/// Gets the time that the user last interacted with the panel.
		/// </summary>
		public override DateTime? LastOutput { get { throw new NotImplementedException(); } }

		/// <summary>
		/// Collection containing the loaded SmartObjects of this device.
		/// </summary>
		public override ISmartObjectCollection SmartObjects { get { throw new NotImplementedException(); } }

#if SIMPLSHARP
		private readonly TTouchScreen m_TouchScreen;

		/// <summary>
		/// Gets the wrapped Crestron TouchScreen instance.
		/// </summary>
		protected TTouchScreen TouchScreen { get { return m_TouchScreen; } }
#endif

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractControlSystemTouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
#if SIMPLSHARP
			m_TouchScreen = parent.ControlSystem.ControllerTouchScreenSlotDevice as TTouchScreen;
			if (m_TouchScreen == null)
				throw new InvalidOperationException();

			m_TouchScreen.Register();
#endif
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnAnyOutput = null;

			base.DisposeFinal(disposing);
		}

		#region Panel Methods

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
		public override void UnregisterOutputSigChangeCallback(uint number, eSigType type,
		                                                       Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Registers the callback for output sig change events.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="type"></param>
		/// <param name="callback"></param>
		public override void RegisterOutputSigChangeCallback(uint number, Protocol.Sigs.eSigType type,
		                                                     Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
