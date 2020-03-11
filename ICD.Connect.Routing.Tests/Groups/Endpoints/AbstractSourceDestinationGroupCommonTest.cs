using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Groups.Endpoints;
using ICD.Connect.Settings.Tests.Groups;
using NUnit.Framework;

namespace ICD.Connect.Routing.Tests.Groups.Endpoints
{
	public abstract class AbstractSourceDestinationGroupCommonTest<TGroup, TGroupSettings, TOriginator> : AbstractGroupTest<TGroup, TGroupSettings, TOriginator>
		where TGroup : AbstractSourceDestinationGroupCommon<TOriginator, TGroupSettings>
		where TOriginator : class, ISourceDestinationCommon
		where TGroupSettings : ISourceDestinationGroupCommonSettings, new()
	{

		protected override TGroup InstantiateGroup()
		{
			return InstantiateGroup(eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb);
		}

		protected abstract TGroup InstantiateGroup(eConnectionType connectionTypeMask);

		/// <summary>
		/// Method to get an originator of TOriginator with the given id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected override TOriginator GetOriginator(int id)
		{
			return GetOriginator(id, eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb);
		}

		protected abstract TOriginator GetOriginator(int id, eConnectionType connectionType);


		[TestCase(eConnectionType.Video, eConnectionType.Video, eConnectionType.Video)]
		[TestCase(eConnectionType.None, eConnectionType.Video, eConnectionType.Audio)]
		[TestCase(eConnectionType.Video, eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb, eConnectionType.Video)]
		[TestCase(eConnectionType.Video | eConnectionType.Audio, eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb, eConnectionType.Video, eConnectionType.Video | eConnectionType.Audio)]
		[TestCase(eConnectionType.None, eConnectionType.Usb, eConnectionType.Video, eConnectionType.Audio)]
		[TestCase(eConnectionType.Video | eConnectionType.Audio, eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb, eConnectionType.Video | eConnectionType.Audio, eConnectionType.Audio)]
		public void TestConnectionType(eConnectionType expectedResult, eConnectionType groupMask, params eConnectionType[] memberConnectionTypes)
		{
			var group = InstantiateGroup(groupMask);


			int i = 1;
			foreach (eConnectionType connectionType in memberConnectionTypes)
			{
				group.AddItem(GetOriginator(i, connectionType));
				i++;
			}

			Assert.AreEqual(expectedResult, group.ConnectionType);
		}
	}
}
