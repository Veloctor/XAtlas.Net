using AOT;
using System;
using System.Runtime.InteropServices;

namespace XAtlas.Net.Interop;
public static class PrintFormat
{
	const string msvcrtDll = "msvcrt.dll";

	[MonoPInvokeCallback(typeof(PrintFunc))]
	public unsafe static int Printf(IntPtr PStr_format, byte args)
	{
		int count = _vscprintf(PStr_format, ref args) + 1;
		var buffer = stackalloc byte[count];
		IntPtr pbuffer = (nint)buffer;
		int retcode = vsprintf(pbuffer, PStr_format, ref args);
		Console.Write(Marshal.PtrToStringAnsi(pbuffer));
		return retcode;
	}

	[DllImport(msvcrtDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern int vsprintf(IntPtr buffer, IntPtr PStr_format, ref byte args);

	[DllImport(msvcrtDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern int _vscprintf(IntPtr PStr_format, ref byte args);
}