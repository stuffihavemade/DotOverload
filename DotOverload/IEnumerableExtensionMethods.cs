using System;
using System.Collections.Generic;
namespace DotOverload
{
	internal static class IEnumerableExtensionMethods
	{
		public static IEnumerable<T> Push<T>(this IEnumerable<T> enumerable, T obj)
		{
			yield return obj;
			foreach(var e in enumerable)
				yield return e;
		}
		
		public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T obj)
		{
			foreach(var e in enumerable)
				yield return e;
			yield return obj;
		}
	}
}

