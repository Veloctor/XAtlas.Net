using System;
using System.Runtime.CompilerServices;

namespace Velctor.Utils
{
	public readonly struct Ptr<T> where T : unmanaged
	{
		private readonly unsafe T* addr;
		public readonly unsafe ref T Target => ref *addr;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Ptr(void* ptr) => addr = (T*)ptr;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Ptr(IntPtr ptr) => addr = (T*)ptr;

		/// <summary> attention not ref managed memory(e.g. field of reference type object) </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Ptr(ref T targetObj)
		{
			fixed (T* p = &targetObj) {
				addr = p;
			}
		}
		/// <summary> attention not ref managed memory(e.g. field of reference type object) </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Ptr(ref Span<T> targetObj)
		{
			fixed (void* p = &targetObj) {
				addr = (T*)p;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe implicit operator Ptr<T>(T* nativePtr) => new(nativePtr);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe implicit operator Ptr<T>(IntPtr nativePtr) => new(nativePtr);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe implicit operator T*(Ptr<T> nativePtr) => nativePtr.addr;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe explicit operator nint(Ptr<T> v) => (nint)v.addr;

		public unsafe T this[nint indx] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => addr[indx];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => addr[indx] = value;
		}

		public unsafe Ptr<U> CastAs<U>() where U : unmanaged => new(addr);
	}

	public static class Ptr
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Ptr<T> GetPtr<T>(ref this T target) where T : unmanaged => new(ref target);
	}
}
