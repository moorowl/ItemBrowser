using System.Collections.Generic;
using Pug.Properties;
using Unity.Collections;
using Unity.Entities;

namespace ItemBrowser.Utilities.Extensions {
	public static class EcsExtensions {
		public static List<T> ConvertToList<T>(this DynamicBuffer<T> buffer) where T : unmanaged {
			var result = new List<T>(buffer.Length);

			foreach (var item in buffer)
				result.Add(item);
				
			return result;
		}
		
		public static List<T> ConvertToList<T>(this ref BlobArray<T> blobArray) where T : unmanaged {
			var result = new List<T>(blobArray.Length);

			for (var i = 0; i < blobArray.Length; i++)
				result.Add(blobArray[i]);
				
			return result;
		}

		public static List<T> GetManagedList<T>(this ObjectPropertiesCD properties, int propertyId) where T : unmanaged {
			using var array = properties.GetList<T>(propertyId, Allocator.Temp);
			var result = new List<T>(array.Length);

			foreach (var item in array)
				result.Add(item);

			return result;
		}
	}
}