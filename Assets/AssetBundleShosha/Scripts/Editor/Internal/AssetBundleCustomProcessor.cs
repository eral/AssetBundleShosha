// (C) 2018 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Editor.Internal {
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	[System.AttributeUsage(System.AttributeTargets.Method)]
	public abstract class AssetBundleCustomProcessorAttribute : System.Attribute {
		#region Public fields and properties

		/// <summary>
		/// 実行順
		/// </summary>
		public abstract int order {get;}

		#endregion
		#region Public methods

		/// <summary>
		/// カレントドメイン内のカスタムプロセッサを全て取得
		/// </summary>
		/// <typeparam name="T">取得するクラス属性</typeparam>
		/// <param name="bindingAttr">バインディングフラク</param>
		/// <returns>カスタムプロセッサ群</returns>
		public static IEnumerable<MethodInfo> GetAllMethodInfoInCurrentDomain<T>(BindingFlags bindingAttr) where T : AssetBundleCustomProcessorAttribute {
			var result = System.AppDomain.CurrentDomain
										.GetAssemblies()
										.SelectMany(x=>x.GetTypes())
										.SelectMany(x=>x.GetMethods(bindingAttr))
										.SelectMany(x=>System.Attribute.GetCustomAttributes(x, typeof(T))
																		.Select(y=>new{method = x, order = ((T)y).order}))
										.OrderBy(x=>x.order)
										.Select(x=>x.method);
			return result;
		}

		#endregion
	}
}
