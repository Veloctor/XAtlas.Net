using System;

namespace Velctor.Utils
{
	public readonly struct Ptr<T> where T : unmanaged
	{
		private readonly unsafe T* addr;
		public readonly unsafe ref T Target => ref *addr;

		public unsafe Ptr(void* ptr) => addr = (T*)ptr;
		public unsafe Ptr(IntPtr ptr) => addr = (T*)ptr;

		/// <summary> attention not ref managed memory(e.g. field of reference type object) </summary>
		public unsafe Ptr(ref T targetObj)
		{
			fixed (T* p = &targetObj) {
				addr = p;
			}
		}

		public static unsafe implicit operator Ptr<T>(T* nativePtr) => new(nativePtr);
		public static unsafe implicit operator Ptr<T>(IntPtr nativePtr) => new(nativePtr);
		public static unsafe implicit operator T*(Ptr<T> nativePtr) => nativePtr.addr;
		public static unsafe explicit operator nint(Ptr<T> v) => (nint)v.addr;

		public unsafe T this[nint indx] {
			get => addr[indx];
			set => addr[indx] = value;
		}

		public unsafe Ptr<U> CastAs<U>() where U : unmanaged => new(addr);
	}

	public static class Ptr
	{
		public static Ptr<T> GetPtr<T>(ref this T target) where T : unmanaged => new(ref target);
	}
}
