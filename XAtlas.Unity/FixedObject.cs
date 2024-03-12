using System.Runtime.InteropServices;

namespace XAtlas.Net.Interop
{
	public class FixedObject : UnmanagedResource
	{
		public FixedObject(object toFix) : base(GCHandle.Alloc(toFix, GCHandleType.Pinned).AddrOfPinnedObject())
		{ }
		protected override void ReleaseUnmanagedResource() => 
			GCHandle.FromIntPtr(Handle).Free();
	}
}
