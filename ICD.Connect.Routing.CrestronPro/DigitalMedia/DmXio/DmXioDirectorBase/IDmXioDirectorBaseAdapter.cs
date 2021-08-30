using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXio.DmXioDirectorBase
{
#if !NETSTANDARD
	public delegate void DmXioDirectorChangeCallback(
		IDmXioDirectorBaseAdapter sender, Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase director);
#endif

	public interface IDmXioDirectorBaseAdapter : IDevice
	{
#if !NETSTANDARD
		/// <summary>
		/// Raised when the wrapped director instance changes.
		/// </summary>
		event DmXioDirectorChangeCallback OnDirectorChanged;

		/// <summary>
		/// Gets the wrapped director instance.
		/// </summary>
		Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase Director { get; }

		/// <summary>
		/// Gets the domain with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain GetDomain(uint id);
#endif
	}
}
