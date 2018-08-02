using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;

namespace ICD.Connect.Routing.Pathfinding
{
	public sealed class PathBuilder : IPathBuilderStartNew, IPathBuilderFrom, IPathBuilderTo, IPathBuilderOfType
	{
		private readonly List<PathBuilderQuery> m_Queries;

		private PathBuilderQuery m_Current;

		/// <summary>
		/// Constructor.
		/// </summary>
		private PathBuilder()
		{
			m_Queries = new List<PathBuilderQuery>();
		}

		/// <summary>
		/// Starts a new PathBuilder chain.
		/// </summary>
		/// <returns></returns>
		public static IPathBuilderStartNew FindPaths()
		{
			return new PathBuilder();
		}

		/// <summary>
		/// Sets the start point for the path.
		/// </summary>
		/// <param name="sourceEndpoints"></param>
		/// <returns></returns>
		public IPathBuilderFrom From(IEnumerable<EndpointInfo> sourceEndpoints)
		{
			if (m_Current != null)
				throw new InvalidOperationException();

			m_Current = new PathBuilderQuery();
			m_Current.SetStart(sourceEndpoints);

			return this;
		}

		/// <summary>
		/// Sets the start position, end position and type for the path.
		/// </summary>
		/// <param name="operation"></param>
		/// <returns></returns>
		public IPathBuilderOfType ForOperation(RouteOperation operation)
		{
			if (operation == null)
				throw new ArgumentNullException("operation");

			if (m_Current != null)
				throw new InvalidOperationException();

			PathBuilderQuery current = new PathBuilderQuery();

			current.SetStart(new[] {operation.Source});
			current.SetEnd(new[] {operation.Destination});
			current.Type = operation.ConnectionType;

			m_Queries.Add(current);

			return this;
		}

		/// <summary>
		/// Sets the end point for the path.
		/// </summary>
		/// <param name="destinationEndpoints"></param>
		/// <returns></returns>
		public IPathBuilderTo To(IEnumerable<EndpointInfo> destinationEndpoints)
		{
			if (m_Current == null)
				throw new InvalidOperationException();

			m_Current.SetEnd(destinationEndpoints);

			return this;
		}

		/// <summary>
		/// Adds an end point for the path.
		/// </summary>
		/// <param name="destinationEndpoints"></param>
		/// <returns></returns>
		public IPathBuilderTo AndTo(IEnumerable<EndpointInfo> destinationEndpoints)
		{
			if (m_Current == null)
				throw new InvalidOperationException();

			m_Current.AddEnd(destinationEndpoints);

			return this;
		}

		/// <summary>
		/// Sets the connection type flags for the path.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		public IPathBuilderOfType OfType(eConnectionType flags)
		{
			if (m_Current == null)
				throw new InvalidOperationException();

			m_Current.Type = flags;

			return this;
		}

		/// <summary>
		/// Starts describing a new path.
		/// </summary>
		/// <returns></returns>
		public IPathBuilderStartNew AndFindPaths()
		{
			if (m_Current == null)
				throw new InvalidOperationException();

			m_Queries.Add(m_Current);
			m_Current = null;

			return this;
		}

		/// <summary>
		/// Yields the path/s for the defined queries.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ConnectionPath> With(IPathFinder pathFinder)
		{
			if (pathFinder == null)
				throw new ArgumentNullException("pathFinder");

			if (m_Current == null)
				throw new InvalidOperationException();

			m_Queries.Add(m_Current);
			m_Current = null;

			return pathFinder.FindPaths(m_Queries);
		}
	}

	public interface IPathBuilder
	{
	}

	public interface IPathBuilderStartNew : IPathBuilder
	{
		/// <summary>
		/// Sets the start point for the path.
		/// </summary>
		/// <param name="sourceEndpoints"></param>
		/// <returns></returns>
		IPathBuilderFrom From(IEnumerable<EndpointInfo> sourceEndpoints);

		/// <summary>
		/// Sets the start position, end position and type for the path.
		/// </summary>
		/// <param name="operation"></param>
		/// <returns></returns>
		IPathBuilderOfType ForOperation(RouteOperation operation);
	}

	public static class PathBuilderStartNewExtensions
	{
		/// <summary>
		/// Sets the start point for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="sourceEndpoints"></param>
		/// <returns></returns>
		public static IPathBuilderFrom From(this IPathBuilderStartNew extends, params EndpointInfo[] sourceEndpoints)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (sourceEndpoints == null)
				throw new ArgumentNullException("sourceEndpoints");

			return extends.From(sourceEndpoints);
		}

		/// <summary>
		/// Sets the start point for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		public static IPathBuilderFrom From(this IPathBuilderStartNew extends, ISource source)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (source == null)
				throw new ArgumentNullException("source");

			return extends.From(source.GetEndpoints());
		}

		/// <summary>
		/// Sets the start point for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="sourceControl"></param>
		/// <returns></returns>
		public static IPathBuilderFrom From(this IPathBuilderStartNew extends, IRouteSourceControl sourceControl)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			IEnumerable<EndpointInfo> endpoints =
				sourceControl.GetOutputs()
				             .Select(c => sourceControl.GetOutputEndpointInfo(c.Address));

			return extends.From(endpoints);
		}
	}

	public interface IPathBuilderFrom : IPathBuilder
	{
		/// <summary>
		/// Sets the end point for the path.
		/// </summary>
		/// <param name="destinationEndpoints"></param>
		/// <returns></returns>
		IPathBuilderTo To(IEnumerable<EndpointInfo> destinationEndpoints);
	}

	public static class PathBuilderFromExtensions
	{
		/// <summary>
		/// Sets the end point for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="destinationEndpoints"></param>
		/// <returns></returns>
		public static IPathBuilderTo To(this IPathBuilderFrom extends, params EndpointInfo[] destinationEndpoints)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (destinationEndpoints == null)
				throw new ArgumentNullException("sourceEndpoints");

			return extends.To(destinationEndpoints);
		}

		/// <summary>
		/// Sets the end point for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		public static IPathBuilderTo To(this IPathBuilderFrom extends, IDestination destination)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (destination == null)
				throw new ArgumentNullException("destination");

			return extends.To(destination.GetEndpoints());
		}

		/// <summary>
		/// Sets the end point for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="destinationControl"></param>
		/// <returns></returns>
		public static IPathBuilderTo To(this IPathBuilderFrom extends, IRouteDestinationControl destinationControl)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			IEnumerable<EndpointInfo> endpoints =
				destinationControl.GetInputs()
				                  .Select(c => destinationControl.GetInputEndpointInfo(c.Address));

			return extends.To(endpoints);
		}

		/// <summary>
		/// Sets the end points for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="destinations"></param>
		/// <returns></returns>
		public static IPathBuilderTo To<T>(this IPathBuilderFrom extends, IEnumerable<T> destinations)
			where T : IDestination
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			IPathBuilderTo result = null;

			foreach (T destination in destinations)
				result = result == null ? extends.To(destination) : result.AndTo(destination);

			if (result == null)
				throw new InvalidOperationException("Sequence is empty");

			return result;
		}
	}

	public interface IPathBuilderTo : IPathBuilder
	{
		/// <summary>
		/// Adds an end point for the path.
		/// </summary>
		/// <param name="destinationEndpoints"></param>
		/// <returns></returns>
		IPathBuilderTo AndTo(IEnumerable<EndpointInfo> destinationEndpoints);

		/// <summary>
		/// Sets the connection type flags for the path.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		IPathBuilderOfType OfType(eConnectionType flags);
	}

	public static class PathBuilderToExtensions
	{
		/// <summary>
		/// Adds an end point for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="destinationEndpoints"></param>
		/// <returns></returns>
		public static IPathBuilderTo AndTo(this IPathBuilderTo extends, params EndpointInfo[] destinationEndpoints)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (destinationEndpoints == null)
				throw new ArgumentNullException("destinationEndpoints");

			return extends.AndTo(destinationEndpoints);
		}

		/// <summary>
		/// Adds an end point for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		public static IPathBuilderTo AndTo(this IPathBuilderTo extends, IDestination destination)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (destination == null)
				throw new ArgumentNullException("destination");

			return extends.AndTo(destination.GetEndpoints());
		}

		/// <summary>
		/// Adds an end point for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="destinationControl"></param>
		/// <returns></returns>
		public static IPathBuilderTo AndTo(this IPathBuilderTo extends, IRouteDestinationControl destinationControl)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");
			IEnumerable<EndpointInfo> endpoints =
				destinationControl.GetInputs()
				                  .Select(c => destinationControl.GetInputEndpointInfo(c.Address));

			return extends.AndTo(endpoints);
		}

		/// <summary>
		/// Adds an end point for the path.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="destinations"></param>
		/// <returns></returns>
		public static IPathBuilderTo AndTo<T>(this IPathBuilderTo extends, IEnumerable<T> destinations)
			where T : IDestination
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			foreach (T destination in destinations)
				extends = extends.AndTo(destination);

			return extends;
		}
	}

	public interface IPathBuilderOfType : IPathBuilder
	{
		/// <summary>
		/// Starts describing a new path.
		/// </summary>
		/// <returns></returns>
		IPathBuilderStartNew AndFindPaths();

		/// <summary>
		/// Yields the path/s for the defined queries.
		/// </summary>
		/// <param name="pathFinder"></param>
		/// <returns></returns>
		IEnumerable<ConnectionPath> With(IPathFinder pathFinder);
	}
}
