using System;

namespace AOT
{
#pragma warning disable IDE0060 // 删除未使用的参数
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class MonoPInvokeCallbackAttribute : Attribute
	{
		public MonoPInvokeCallbackAttribute(Type type)
		{
		}
	}
#pragma warning restore IDE0060 // 删除未使用的参数
}
