using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Panels.SigCollections;
using ICD.Connect.Protocol.Sigs;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Misc.CrestronPro.Sigs;
using ICD.Connect.Panels.CrestronPro.TriListAdapters;
#endif
using System;
using ICD.Connect.Panels;
using ICD.Connect.Panels.CrestronPro.Controls.TouchScreens;
using ICD.Connect.Panels.EventArguments;
using ICD.Connect.Panels.SmartObjectCollections;
using eSigType = ICD.Connect.Protocol.Sigs.eSigType;
using ISmartObject = ICD.Connect.Panels.SmartObjects.ISmartObject;

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

#if SIMPLSHARP
		private readonly DeviceBooleanInputCollectionAdapter m_BooleanInput;
		private readonly DeviceUShortInputCollectionAdapter m_UShortInput;
		private readonly DeviceStringInputCollectionAdapter m_StringInput;
		private readonly SmartObjectCollectionAdapter m_SmartObjects;
#endif

		private readonly SigCallbackManager m_SigCallbacks;

#region Panel Properties

		/// <summary>
		/// Gets the time that the user last interacted with the panel.
		/// </summary>
		public override DateTime? LastOutput
		{
			get
			{ 	
				// Return the most recent output time.
				DateTime? output = m_SigCallbacks.LastOutput;
				DateTime? smartOutput = SmartObjects.Select(p => p.Value)
													.Select(s => s.LastOutput)
													.Where(d => d != null)
													.Max();

				if (output == null)
					return smartOutput;
				if (smartOutput == null)
					return output;

				return output > smartOutput ? output : smartOutput;
			}
		}

		/// <summary>
		/// Collection of Boolean Inputs sent to the panel.
		/// </summary>
		public IDeviceBooleanInputCollection BooleanInput
		{
			get
			{
#if SIMPLSHARP
				return m_BooleanInput;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Collection of Integer Inputs sent to the panel.
		/// </summary>
		public IDeviceUShortInputCollection UShortInput
		{
			get
			{
#if SIMPLSHARP
				return m_UShortInput;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Collection of String Inputs sent to the panel.
		/// </summary>
		public IDeviceStringInputCollection StringInput
		{
			get
			{
#if SIMPLSHARP
				return m_StringInput;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Collection containing the loaded SmartObjects of this panel.
		/// </summary>
		public override ISmartObjectCollection SmartObjects
		{
			get
			{
#if SIMPLSHARP
				return m_SmartObjects;
#else
				throw new NotSupportedException();
#endif
			}
		}

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
			m_SigCallbacks = new SigCallbackManager();
			m_SigCallbacks.OnAnyCallback += SigCallbacksOnAnyCallback;

#if SIMPLSHARP
			m_TouchScreen = parent.ControlSystem.ControllerTouchScreenSlotDevice as TTouchScreen;
			if (m_TouchScreen == null)
				throw new InvalidOperationException();

			Subscribe(m_TouchScreen);

			m_TouchScreen.Register();

			m_SmartObjects = new SmartObjectCollectionAdapter();
			m_BooleanInput = new DeviceBooleanInputCollectionAdapter();
			m_UShortInput = new DeviceUShortInputCollectionAdapter();
			m_StringInput = new DeviceStringInputCollectionAdapter();

			Subscribe(m_SmartObjects);
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

#if SIMPLSHARP
			Unsubscribe(m_TouchScreen);
			Unsubscribe(m_SmartObjects);
#endif
		}

#region Private Methods

		/// <summary>
		/// Raises the callbacks registered with the signature.
		/// </summary>
		/// <param name="sigInfo"></param>
		private void RaiseOutputSigChangeCallback(SigInfo sigInfo)
		{
			m_SigCallbacks.RaiseSigChangeCallback(sigInfo);
		}

		/// <summary>
		/// Called when a sig changes state.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="sigInfoEventArgs"></param>
		private void SigCallbacksOnAnyCallback(object sender, SigInfoEventArgs sigInfoEventArgs)
		{
			RaiseOnAnyOutput(sigInfoEventArgs.Data);
		}

		/// <summary>
		/// Raises the OnAnyOutput event.
		/// </summary>
		private void RaiseOnAnyOutput(SigInfo sigInfo)
		{
			OnAnyOutput.Raise(this, new SigInfoEventArgs(sigInfo));
		}

#endregion

#region Panel Methods

		/// <summary>
		/// Clears the assigned input sig values.
		/// </summary>
		public override void Clear()
		{
			foreach (IBoolInputSig item in BooleanInput)
				SendInputDigital(item.Number, false);

			foreach (IUShortInputSig item in UShortInput)
				SendInputAnalog(item.Number, 0);

			foreach (IStringInputSig item in StringInput)
				SendInputSerial(item.Number, null);

			foreach (ISmartObject so in SmartObjects.Select(kvp => kvp.Value))
				so.Clear();
		}

		/// <summary>
		/// Sends the serial data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="text"></param>
		public override void SendInputSerial(uint number, string text)
		{
			try
			{
				StringInput[number].SetStringValue(text);
			}
			catch (Exception e)
			{
				Log(eSeverity.Error, "Unable to send input serial {0} - {1}", number, e.Message);
			}
		}

		/// <summary>
		/// Sends the analog data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="value"></param>
		public override void SendInputAnalog(uint number, ushort value)
		{
			try
			{
				UShortInput[number].SetUShortValue(value);
			}
			catch (Exception e)
			{
				Log(eSeverity.Error, "Unable to send input analog {0} - {1}", number, e.Message);
			}
		}

		/// <summary>
		/// Sends the digital data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="value"></param>
		public override void SendInputDigital(uint number, bool value)
		{
			try
			{
				BooleanInput[number].SetBoolValue(value);
			}
			catch (Exception e)
			{
				Log(eSeverity.Error, "Unable to send input digital {0} - {1}", number, e.Message);
			}
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
			m_SigCallbacks.UnregisterSigChangeCallback(number, type, callback);
		}

		/// <summary>
		/// Registers the callback for output sig change events.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="type"></param>
		/// <param name="callback"></param>
		public override void RegisterOutputSigChangeCallback(uint number, eSigType type,
		                                                     Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			m_SigCallbacks.RegisterSigChangeCallback(number, type, callback);
		}

#endregion

#region Panel Callbacks

#if SIMPLSHARP
		/// <summary>
		/// Subscribe to the panel events.
		/// </summary>
		/// <param name="panel"></param>
		private void Subscribe(TTouchScreen panel)
		{
			if (panel == null)
				return;

			panel.SigChange += PanelOnSigChange;
		}

		/// <summary>
		/// Subscribe to the TriList events.
		/// </summary>
		/// <param name="panel"></param>
		private void Unsubscribe(TTouchScreen panel)
		{
			if (panel == null)
				return;

			panel.SigChange -= PanelOnSigChange;
		}

		/// <summary>
		/// Called when a sig change comes from the TriList.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void PanelOnSigChange(BasicTriList currentDevice, SigEventArgs args)
		{
			SigInfo sigInfo = args.Sig.ToSigInfo();

			RaiseOutputSigChangeCallback(sigInfo);
		}
#endif
#endregion

#region SmartObject Callbacks

		private void Subscribe(ISmartObjectCollection smartObjects)
		{
			smartObjects.OnSmartObjectSubscribe += SmartObjectsOnSmartObjectSubscribe;
			smartObjects.OnSmartObjectUnsubscribe += SmartObjectsOnSmartObjectUnsubscribe;
		}

		private void Unsubscribe(ISmartObjectCollection smartObjects)
		{
			smartObjects.OnSmartObjectSubscribe -= SmartObjectsOnSmartObjectSubscribe;
			smartObjects.OnSmartObjectUnsubscribe -= SmartObjectsOnSmartObjectUnsubscribe;
		}

		/// <summary>
		/// Subscribes to SmartObject Touch Events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="smartObject"></param>
		private void SmartObjectsOnSmartObjectSubscribe(object sender, ISmartObject smartObject)
		{
			smartObject.OnAnyOutput += SmartObjectOnAnyOutput;
		}

		/// <summary>
		/// Unsubscribes from SmartObject Touch Events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="smartObject"></param>
		private void SmartObjectsOnSmartObjectUnsubscribe(object sender, ISmartObject smartObject)
		{
			smartObject.OnAnyOutput -= SmartObjectOnAnyOutput;
		}

		/// <summary>
		/// Called when the user interacts with a SmartObject.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void SmartObjectOnAnyOutput(object sender, SigInfoEventArgs eventArgs)
		{
			RaiseOnAnyOutput(eventArgs.Data);
		}

#endregion
	}
}
