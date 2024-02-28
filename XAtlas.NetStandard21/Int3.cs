namespace XAtlas.Net;
public struct Int3
{
	public int X, Y, Z;

	public Int3(int s) => X = Y = Z = s;

	public Int3(int x, int y, int z) { X = x; Y = y; Z = z; }

	public static Int3 operator +(Int3 a, int b) => new(a.X + b, a.Y + b, a.Z + b);
	public static Int3 operator -(Int3 a, int b) => new(a.X - b, a.Y - b, a.Z - b);
}
