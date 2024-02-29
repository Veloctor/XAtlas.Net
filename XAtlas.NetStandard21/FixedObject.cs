using System.Runtime.InteropServices;
using Velctor.Utils;

namespace XAtlas.Net.Interop;

public class FixedObject : UnmanagedResource
{
	public FixedObject(object toFix) : base(GCHandle.Alloc(toFix, GCHandleType.Pinned).AddrOfPinnedObject())
	{ }
	protected override void ReleaseUnmanagedResource() => 
		GCHandle.FromIntPtr(Handle).Free();
}
