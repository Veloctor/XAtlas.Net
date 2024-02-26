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
			OnDestroyNativeObj(Handle);
			Handle = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}
	}

	protected abstract void OnDestroyNativeObj(IntPtr nativeObjPtr);
}