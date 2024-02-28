using ObjParser;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Velctor.Utils;
using XAtlas.Net;
using XAtlas.Net.Interop;

class Program
{
	private unsafe static void Main()
	{
		string path = "E:/Desktop/";
		string inFile = path + "qnb.obj";
		string outFile = path + "qnbout.obj";
		Atlas atlas = new();
		atlas.SetProgressCallback(LogProgress);
		Obj obj = new(inFile);
		List<Vector3> verts = obj.Verts;
		Ptr<Vector3> posPtr = stackalloc Vector3[verts.Count];
		Span<Vector3> posBuffer = new(posPtr, verts.Count);
		for (int i = 0; i < verts.Count; i++) {
			posPtr[i] = verts[i];
		}
		List<Int3> Faces = obj.FacesPosIdx;
		Ptr<int> idxPtr = stackalloc int[Faces.Count * 3];
		Span<int> idxBuffer = new(idxPtr, Faces.Count * 3);
		for (int i = 0; i < Faces.Count; i++) {
			Int3 face = Faces[i] - 1;
			idxPtr[i * 3 + 0] = face.X;
			idxPtr[i * 3 + 1] = face.Y;
			idxPtr[i * 3 + 2] = face.Z;
		}
		MeshDecl meshDecl = new();
		meshDecl.vertexCount = (uint)posBuffer.Length;
		meshDecl.vertexPositionData = posPtr;
		meshDecl.vertexPositionStride = 12;
		meshDecl.indexCount = (uint)idxBuffer.Length;
		meshDecl.indexData = idxPtr;
		meshDecl.indexFormat = IndexFormat.UInt32;
		AddMeshError addMeshRetCode = atlas.AddMesh(meshDecl, 1);
		if (addMeshRetCode != AddMeshError.Success) {
			Console.WriteLine($"Add Mesh Error:{addMeshRetCode}");
			return;
		}
		Stopwatch sw = Stopwatch.StartNew();
		ChartOptions options = new();
		atlas.Generate(options, new PackOptions(1024));
		sw.Stop();
		var data = atlas.Data;
		Vector2 wh = new(data.Target.width, data.Target.height);
		Console.WriteLine($"chart count: {data.Target.chartCount}, width: {wh.X}, height: {wh.Y}, generate time:{sw.Elapsed.TotalSeconds:G4}s");
		var mesh = data.Target.meshes;
		Span<Vertex> vertsout = new(mesh.Target.vertexArray, (int)mesh.Target.vertexCount);
		Span<int> indices = new(mesh.Target.indexArray, (int)mesh.Target.indexCount);
		obj.ClearAll();
		for (int i = 0; i < mesh.Target.vertexCount; i++) {
			Vertex v = vertsout[i];
			Vector3 pos = posBuffer[(int)v.xref];
			Vector2 uv = v.uv / wh;
			obj.Verts.Add(pos);
			obj.UVs.Add(uv);
		}
		for (int i = 0; i < indices.Length; i += 3) {
			obj.FacesPosIdx.Add(new Int3(indices[i], indices[i + 1], indices[i + 2]) + 1);
		}
		obj.WriteObjFile(outFile, null);
	}

	public static BOOL LogProgress(ProgressCategory category, int progress, IntPtr userData)
	{
		Console.WriteLine($"\t{category}: {progress}%");
		return true;
	}

	public unsafe static StringBuilder GetHex<T>(T obj, StringBuilder sbToAppend = null) where T : unmanaged
	{
		sbToAppend ??= new();
		sbToAppend.Clear();
		int size = Unsafe.SizeOf<T>();
		Ptr<T> ptr = new(ref obj);
		Ptr<byte> dataPtr = ptr.CastAs<byte>();
		for (int i = 0; i < size; i++) {
			byte val = dataPtr[i];
			sbToAppend.Append(HexFrom4bits(val >> 4));
			sbToAppend.Append(HexFrom4bits(val));
		}
		return sbToAppend;
	}

	public static char HexFrom4bits(int val)
	{
		int bin = val & 0xF;
		bin += bin > 9 ? ('A' - 10) : '0';
		return (char)bin;
	}
}