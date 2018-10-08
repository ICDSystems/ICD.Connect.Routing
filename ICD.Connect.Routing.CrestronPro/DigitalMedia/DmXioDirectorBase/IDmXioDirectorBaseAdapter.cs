using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXioDirectorBase
{
#if SIMPLSHARP
	public delegate void DmXioDirectorChangeCallback(
		IDmXioDirectorBaseAdapter sender, Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase director);
#endif

	public interface IDmXioDirectorBaseAdapter : IDevice
	{
#if SIMPLSHARP
		/// <summary>
		/// Raised when the wrapped director instance changes.
		/// </summary>
		event DmXioDirectorChangeCallback OnDirectorChanged;

		/// <summary>
		/// Gets the wrapped director instance.
		/// </summary>
		Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase Director { get; }
#endif
	}
}
