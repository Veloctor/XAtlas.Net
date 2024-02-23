using System;

namespace Velctor.Utils
{
	public readonly struct Ptr<T> where T : unmanaged
	{
		private readonly IntPtr addr;
		public readonly unsafe ref T Target => ref *(T*)addr;

		public unsafe Ptr(void* ptr) => addr = (IntPtr)ptr;
		public Ptr(IntPtr ptr) => addr = ptr;

		/// <summary> attention not ref managed memory(e.g. field of reference type object) </summary>
		public unsafe Ptr(ref T targetObj)
		{
			fixed (T* nativePtr = &targetObj) {
				addr = (IntPtr)nativePtr;
			}
		}

		public unsafe static implicit operator Ptr<T>(T* nativePtr) => new(nativePtr);
		public unsafe static implicit operator T*(Ptr<T> nativePtr) => (T*)nativePtr.addr;

		public unsafe Ptr<U> CastAs<U>() where U : unmanaged => new(addr);
	}

	public static class Ptr
	{
		public static Ptr<T> GetPtr<T>(ref this T target) where T : unmanaged => new(ref target);
	}
}
