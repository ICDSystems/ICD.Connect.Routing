﻿using System;
using System.Collections.Generic;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;
using NUnit.Framework;

namespace ICD.Connect.Routing.Tests.Utils
{
	[TestFixture]
	public sealed class SwitcherCacheTest
	{
		[Test]
		public void ClearTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetSourceDetectedStateTest()
		{
			SwitcherCache cache = new SwitcherCache();

			Assert.IsFalse(cache.GetSourceDetectedState(1, eConnectionType.Video));

			cache.SetSourceDetectedState(1, eConnectionType.Video, true);
			Assert.IsTrue(cache.GetSourceDetectedState(1, eConnectionType.Video));
			Assert.IsFalse(cache.GetSourceDetectedState(1, eConnectionType.Audio));

			Assert.Throws<ArgumentException>(() => cache.GetSourceDetectedState(1, eConnectionType.Audio | eConnectionType.Video));
		}

		[Test]
		public void SetSourceDetectedStateTest()
		{
			List<SourceDetectionStateChangeEventArgs> eventArgs = new List<SourceDetectionStateChangeEventArgs>();

			SwitcherCache cache = new SwitcherCache();
			cache.OnSourceDetectionStateChange += (s, e) => eventArgs.Add(e);

			Assert.IsFalse(cache.SetSourceDetectedState(1, eConnectionType.Video, false));
			Assert.IsTrue(cache.SetSourceDetectedState(1, eConnectionType.Video, true));
			Assert.IsFalse(cache.SetSourceDetectedState(1, eConnectionType.Video, true));
			Assert.IsTrue(cache.SetSourceDetectedState(1, eConnectionType.Video | eConnectionType.Audio, true));
			Assert.IsTrue(cache.SetSourceDetectedState(1, eConnectionType.Video, false));

			Assert.AreEqual(3, eventArgs.Count);

			Assert.AreEqual(1, eventArgs[0].Input);
			Assert.AreEqual(true, eventArgs[0].State);
			Assert.AreEqual(eConnectionType.Video, eventArgs[0].Type);

			Assert.AreEqual(1, eventArgs[1].Input);
			Assert.AreEqual(true, eventArgs[1].State);
			Assert.AreEqual(eConnectionType.Audio, eventArgs[1].Type);

			Assert.AreEqual(1, eventArgs[2].Input);
			Assert.AreEqual(false, eventArgs[2].State);
			Assert.AreEqual(eConnectionType.Video, eventArgs[2].Type);
		}

		[Test]
		public void SetInputForOutputTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInputForOutputTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetInputConnectorInfoForOutputTest()
		{
			Assert.Inconclusive();
		}
		[Test]
		public void GetOutputsForInputTest()
		{
			Assert.Inconclusive();
		}

	}
}
