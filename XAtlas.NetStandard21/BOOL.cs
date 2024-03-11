/*by Github-Veloctor*/

namespace XAtlas.Net.Interop
{
	/// <summary>
	/// A blittable type used in P/Invoke as the equivalent of C++ 'bool' (C# 'bool' doesn't support it). 
	/// Note: Different C++ compilers have varying byte sizes for 'bool'. 
	/// If compiling the xatlas library with a compiler other than MSVC-x64 (1 byte bool), adjust this for different blittable type implementations (short-2 bytes, int-4 bytes).
	/// </summary>
	public readonly struct Bool
	{
		private readonly byte value;

		public Bool(bool value) => this.value = (byte)(value ? 1 : 0);

		public static implicit operator Bool(bool value) => new(value);
		public static implicit operator bool(Bool value) => value.value != 0;
	}
}
