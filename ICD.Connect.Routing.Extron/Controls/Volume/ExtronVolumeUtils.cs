using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public static class ExtronVolumeUtils
	{
		private static readonly Dictionary<eExtronVolumeType, ExtronVolumeRangeAttribute> s_VolumeRangeCache;
		private static readonly Dictionary<ExtronObjectIdRangeAttribute, eExtronVolumeType> s_ObjectIdCache;

		static ExtronVolumeUtils()
		{
			s_VolumeRangeCache = new Dictionary<eExtronVolumeType, ExtronVolumeRangeAttribute>();
			s_ObjectIdCache = new Dictionary<ExtronObjectIdRangeAttribute, eExtronVolumeType>();
			foreach (var volumeType in EnumUtils.GetValues<eExtronVolumeType>())
				CacheVolumeTypeAttributes(volumeType);
		}

		#region Methods

		public static eExtronVolumeType GetVolumeTypeForObject(eExtronVolumeObject volumeObject)
		{
			int volumeObjectId = (int) volumeObject;
			var mapping = s_ObjectIdCache.SingleOrDefault(kvp => kvp.Key.RangeMin <= volumeObjectId && kvp.Key.RangeMax >= volumeObjectId);

			if (mapping.Key == null)
			{
				string message = string.Format("Could not get volume type for volume object id {0} ({1})", volumeObjectId, volumeObject.ToString());
				throw new ArgumentException(message, "volumeObject");
			}

			return mapping.Value;
		}

		public static float GetMinVolume(eExtronVolumeType volumeType)
		{
			return GetCachedVolumeRangeAttribute(volumeType).VolumeMin;
		}

		public static float GetMaxVolume(eExtronVolumeType volumeType)
		{
			return GetCachedVolumeRangeAttribute(volumeType).VolumeMax;
		}

		#endregion

		#region Private Methods

		private static ExtronVolumeRangeAttribute GetCachedVolumeRangeAttribute(eExtronVolumeType volumeType)
		{
			if (!s_VolumeRangeCache.ContainsKey(volumeType))
			{
				string message = string.Format("Volume Type {0} does not have a volume range attribute", volumeType.ToString());
				throw new ArgumentException(message, "volumeType");
			}

			return s_VolumeRangeCache[volumeType];
		}

		/// <summary>
		/// Called from static constructor to cache the attributes of a volume type enum value
		/// </summary>
		/// <param name="volumeType"></param>
		private static void CacheVolumeTypeAttributes(eExtronVolumeType volumeType)
		{
			var fieldInfo = typeof(eExtronVolumeType)
				.GetField(volumeType.ToString());

			var volumeRangeAttribute = fieldInfo.GetCustomAttribute<ExtronVolumeRangeAttribute>();
			if (volumeRangeAttribute != null)
				s_VolumeRangeCache.Add(volumeType, volumeRangeAttribute);

			var objectIdRangeAttributes = fieldInfo.GetCustomAttributes<ExtronObjectIdRangeAttribute>().ToArray();
			foreach(var attribute in objectIdRangeAttributes)
				s_ObjectIdCache.Add(attribute, volumeType);
		}

		#endregion
	}
}
