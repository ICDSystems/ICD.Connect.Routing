namespace ICD.Connect.Routing.Controls
{
	/// <summary>
	/// Describes a route destination where only one input may be active at a given time.
	/// </summary>
	public interface IRouteInputSelectControl : IRouteDestinationControl
	{
		/// <summary>
		/// Gets the current active input.
		/// </summary>
		int? ActiveInput { get; }

		/// <summary>
		/// Sets the current active input.
		/// </summary>
		/// <param name="input"></param>
		void SetActiveInput(int? input);
	}
}
