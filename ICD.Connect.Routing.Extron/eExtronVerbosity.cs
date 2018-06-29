using System;

namespace ICD.Connect.Routing.Extron
{
	/// <summary>
	/// For initializing the verbose mode of the switcher
	/// </summary>
	[Flags]
	internal enum eExtronVerbosity
	{
		/// <summary>
		/// Default for Telnet connection
		/// </summary>
		None = 0,

		/// <summary>
		/// Default for RS-232 or USB connection
		/// </summary>
		VerboseMode = 1,

		/// <summary>
		/// If tagged responses is enabled, all read commands return the constant string and the value as the set command does.
		/// </summary>
		TaggedResponses = 2,

		/// <summary>
		/// Verbose mode and tagged responses for queries
		/// </summary>
		All = VerboseMode | TaggedResponses
	}
}