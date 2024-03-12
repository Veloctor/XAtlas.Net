using System;
#if !UNITY_5_3_OR_NEWER
namespace Velctor.Utils;
#endif
public abstract class UnmanagedResource : IDisposable
{
	private IntPtr handle;
	public IntPtr Handle { 
		get => handle != IntPtr.Zero ? handle : throw new NullReferenceException("Trying to get null Unmanaged resource handle!");
		protected set => handle = value; }

	protected UnmanagedResource() { }
	protected UnmanagedResource(IntPtr handle) => Handle = handle;
	~UnmanagedResource() => Dispose();

	public void Dispose()
	{
		if (Handle != IntPtr.Zero) {
			ReleaseUnmanagedResource();
			Handle = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>  must release Handle resource in this method </summary>
	protected abstract void ReleaseUnmanagedResource();
}