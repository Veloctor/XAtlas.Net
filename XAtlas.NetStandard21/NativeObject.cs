using System;

namespace Velctor.Utils;

public abstract class NativeObject : IDisposable
{
	public IntPtr Handle { get; protected set; }
	protected NativeObject() { }
	protected NativeObject(IntPtr handle) => Handle = handle;
	~NativeObject() => Dispose();

	public void Dispose()
	{
		if (Handle != IntPtr.Zero) {
			DestroyNativeObj(Handle);
			Handle = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}
	}

	protected abstract void DestroyNativeObj(IntPtr nativeObjPtr);
}