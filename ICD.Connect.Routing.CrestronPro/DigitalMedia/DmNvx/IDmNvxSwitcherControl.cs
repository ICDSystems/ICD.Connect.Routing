using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Routing.Controls;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx
{
	public interface IDmNvxSwitcherControl : IRouteSwitcherControl
	{

		/// <summary>
		/// Raised when the last known multicast address changes.
		/// </summary>
		event EventHandler<StringEventArgs> OnLastKnownMulticastAddressChange;

		/// <summary>
		/// Raised when the last known secondary audio multicast address changes.
		/// </summary>
		event EventHandler<StringEventArgs> OnLastKnownSecondaryAudioMulticastAddressChange;

		/// <summary>
		/// Gets the URL for the stream.
		/// </summary>
		string ServerUrl { get; }

		/// <summary>
		/// Gets the multicast address for the secondary audio stream.
		/// </summary>
		string SecondaryAudioMulticastAddress { get; }

		/// <summary>
		/// Gets the last known multicast address for the stream.
		/// </summary>
		string LastKnownMulticastAddress { get; }

		/// <summary>
		/// Gets the last known multicast address for the secondary audio stream.
		/// </summary>
		string LastKnownSecondaryAudioMulticastAddress { get; }

		/// <summary>
		/// Sets the URL for the stream.
		/// </summary>
		/// <param name="url"></param>
		void SetServerUrl(string url);

		/// <summary>
		/// Sets the multicast address for the stream.
		/// </summary>
		/// <param name="address"></param>
		void SetSecondaryAudioMulticastAddress(string address);
	}
}