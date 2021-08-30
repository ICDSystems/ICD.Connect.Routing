#if !NETSTANDARD
using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM.Endpoints;
using ICD.Connect.Routing.Controls;

namespace ICD.Connect.Routing.CrestronPro.HDBaseT
{
	public abstract class AbstractDmBasedTEndPointAdapterRouteMidpointControl<TParent, TDevice> : AbstractRouteMidpointControl<TParent>
		where TParent : IDmBasedTEndPointAdapter
		where TDevice : DmHDBasedTEndPoint
	{
		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Raised when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		private readonly SwitcherCache m_SwitcherCache;

		protected TDevice Device { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractDmBasedTEndPointAdapterRouteMidpointControl(TParent parent, int id)
			: base(parent, id)
		{
			m_SwitcherCache = new SwitcherCache();
			Subscribe(m_SwitcherCache);

			SetDevice(parent.Device as TDevice);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;
			OnRouteChange = null;
			OnActiveTransmissionStateChanged = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_SwitcherCache);

			SetDevice(null);
		}

		private void SetDevice(TDevice device)
		{
			if (device == Device)
				return;

			Unsubscribe(Device);
			Device = device;
			Subscribe(Device);

			UpdateCache(m_SwitcherCache);
		}

		protected virtual void UpdateCache(SwitcherCache cache)
		{
			if (Device == null)
				cache.Clear();
			else
				cache.SetInputForOutput(1, 1, eConnectionType.Audio | eConnectionType.Video);
		}

		#region Methods

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			if (!ContainsInput(input))
				throw new ArgumentOutOfRangeException("input");

			ConnectorInfo connector = GetInput(input);
			if (!connector.ConnectionType.HasFlags(type))
				throw new ArgumentOutOfRangeException("type");

			return m_SwitcherCache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input == 1;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			yield return GetInput(1);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return output == 1;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return GetOutput(1);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			if (!ContainsInput(input))
				throw new ArgumentOutOfRangeException("input");

			if (type == eConnectionType.None)
				yield break;

			ConnectorInfo connector = GetInput(input);
			if (!connector.ConnectionType.HasFlags(type))
				throw new ArgumentOutOfRangeException("type");

			if (type.HasFlag(eConnectionType.Usb))
				yield break;

			yield return GetOutput(1);
		}

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			if (!ContainsOutput(output))
				throw new ArgumentOutOfRangeException("output");

			if (type == eConnectionType.None)
				return null;

			ConnectorInfo connector = GetOutput(output);
			if (!connector.ConnectionType.HasFlags(type))
				throw new ArgumentOutOfRangeException("type");

			if (type.HasFlag(eConnectionType.Usb))
				return null;

			return GetInput(1);
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(TParent parent)
		{
			base.Subscribe(parent);

			parent.OnDeviceChanged += ParentOnDeviceChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(TParent parent)
		{
			base.Unsubscribe(parent);

			parent.OnDeviceChanged -= ParentOnDeviceChanged;
		}

		/// <summary>
		/// Called when the parent wrapped device changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="device"></param>
		private void ParentOnDeviceChanged(IHdBaseTBaseAdapter sender, HDBaseTBase device)
		{
			SetDevice(device as TDevice);
		}

		#endregion

		#region Device Callbacks

		private void Subscribe(TDevice device)
		{
			if (device == null)
				return;

			device.BaseEvent += DeviceOnBaseEvent;
		}

		private void Unsubscribe(TDevice device)
		{
			if (device == null)
				return;

			device.BaseEvent -= DeviceOnBaseEvent;
		}

		private void DeviceOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			UpdateCache(m_SwitcherCache);
		}

		#endregion

		#region SwitcherCache Callbacks

		private void Subscribe(SwitcherCache switcherCache)
		{
			switcherCache.OnActiveInputsChanged += SwitcherCacheOnActiveInputsChanged;
			switcherCache.OnActiveTransmissionStateChanged += SwitcherCacheOnActiveTransmissionStateChanged;
			switcherCache.OnRouteChange += SwitcherCacheOnRouteChange;
			switcherCache.OnSourceDetectionStateChange += SwitcherCacheOnSourceDetectionStateChange;
		}

		private void Unsubscribe(SwitcherCache switcherCache)
		{
			switcherCache.OnActiveInputsChanged -= SwitcherCacheOnActiveInputsChanged;
			switcherCache.OnActiveTransmissionStateChanged -= SwitcherCacheOnActiveTransmissionStateChanged;
			switcherCache.OnRouteChange -= SwitcherCacheOnRouteChange;
			switcherCache.OnSourceDetectionStateChange -= SwitcherCacheOnSourceDetectionStateChange;
		}

		private void SwitcherCacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs eventArgs)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(eventArgs));
		}

		private void SwitcherCacheOnRouteChange(object sender, RouteChangeEventArgs eventArgs)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(eventArgs));
		}

		private void SwitcherCacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs eventArgs)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(eventArgs));
		}

		private void SwitcherCacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs eventArgs)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(eventArgs));
		}

		#endregion
	}
}
#endif