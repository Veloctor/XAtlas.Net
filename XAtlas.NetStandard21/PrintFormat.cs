using System.Reflection;
using System.Runtime.InteropServices;
using System;

// 变参列表
[StructLayout(LayoutKind.Sequential)]
public struct VaList
{
	private IntPtr m_StackPointer;
	public VaList(IntPtr stackPtr)
		=> m_StackPointer = stackPtr;

}

// 保存参数栈帧指针
[StructLayout(LayoutKind.Sequential)]
public struct RawArgumentList
{
	public IntPtr StackPointer;
	public unsafe RawArgumentList(RuntimeArgumentHandle argList)
		=> StackPointer = *(IntPtr*)(&argList); // 强行转为指针
												// 直接访问第 'index' 个参数
	public unsafe IntPtr this[int index] => ((IntPtr*)StackPointer)[index];
	// 获取首个参数为第 index 个参数的变参列表
	public static VaList operator +(RawArgumentList list, int index)
		=> new VaList(list.StackPointer + index * IntPtr.Size);
}

public static class PrintFormatV2
{
	// 由于带了__arglist没法转成delegate，使用反射在初始化时函数指针
	public static IntPtr PrintFormat { get; }
		= typeof(PrintFormatV2).GetMethod("PrintFormatImpl", BindingFlags.NonPublic | BindingFlags.Static).MethodHandle.GetFunctionPointer();
	// (*xatlasPrintFunc)(const char *fmt, ...)
	unsafe static int PrintFormatImpl(__arglist)
	{
		var vaList = new RawArgumentList(__arglist);
		// vaList[0] 取第0个参数fmt的值
		// vaList + 1 获取跳过第0个参数的变参列表
		var count = _vscprintf(vaList[0], vaList + 1) + 1;
		var buffer = stackalloc byte[count];
		int retcode = vsprintf((IntPtr)buffer, vaList[0], vaList + 1);

		Console.Write("xatlas: "+Marshal.PtrToStringAnsi((IntPtr)buffer));
		return retcode;
	}

	[DllImport("msvcrt", CallingConvention = CallingConvention.Cdecl)]
	public static extern int vsprintf(IntPtr buffer, IntPtr PStr_format, VaList args);

	[DllImport("msvcrt", CallingConvention = CallingConvention.Cdecl)]
	public static extern int _vscprintf(IntPtr PStr_format, VaList args);
}