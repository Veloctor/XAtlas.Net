using System;
using System.Runtime.InteropServices;

namespace Velctor.Utils;
public class NativeBuffer<T> : NativeObject where T: unmanaged
{
	public int Count {  get; private set; }
	public unsafe nint ByteLength => Count * sizeof(T);
	public unsafe Span<T> AsSpan=> new(Handle.ToPointer(), Count);
	public Ptr<T> Ptr => new(Handle);
	public bool IsCreated => Handle != IntPtr.Zero;

	public unsafe NativeBuffer(int elementCount) : base(Marshal.AllocHGlobal((nint)elementCount * sizeof(T)))
	{
		Count = elementCount;
	}

	public unsafe void Resize(int newCount)
	{
		Handle = Marshal.ReAllocHGlobal(Handle, (nint)newCount * sizeof(T));
		Count = newCount;
	}

	protected override void DestroyNativeObj(IntPtr nativeObjPtr)
	{
		Marshal.FreeHGlobal(Handle);
	}
}
