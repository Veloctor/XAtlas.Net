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
	unsafe static void Main()
	{
		string path = "E:/Desktop/";
		string inFile = path + "qnb2.obj";
		string outFile = path + "qnb2out.obj";
		//XAtlasAPI.SetPrint(PrintFormatV2.PrintFormat, true); //doesn't work
		using AtlasHandle atlas = new();
		atlas.SetProgressCallback(LogProgress);
		Obj obj = new(inFile);
		Ptr<Vector3> vertsPtr = stackalloc Vector3[obj.VertexCount];
		Ptr<Int3> idxPtr = stackalloc Int3[obj.FaceCount];
		for (int i = 0; i < obj.VertexCount; i++)
			vertsPtr[i] = obj.Verts[i];
		for (int i = 0; i < obj.FaceCount; i++)
			idxPtr[i] = obj.FacesPosIdx[i] - 1;
		MeshDecl meshDecl = new();
		meshDecl.vertexCount = (uint)obj.VertexCount;
		meshDecl.vertexPositionData = vertsPtr;
		meshDecl.indexCount = (uint)obj.FaceCount * 3;
		meshDecl.indexData = idxPtr.CastAs<int>();
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
		ref Atlas  data = ref atlas.Output;
		Vector2 wh = new(data.width, data.height);
		Console.WriteLine($"chart count: {data.chartCount}, width: {wh.X}, height: {wh.Y}, generate time:{sw.Elapsed.TotalSeconds:G4}s");
		var mesh = data.meshes;
		Span<Vertex> vertsout = new(mesh.Target.vertexArray, (int)mesh.Target.vertexCount);
		Span<int> indices = new(mesh.Target.indexArray, (int)mesh.Target.indexCount);
		obj.ClearAll();
		for (int i = 0; i < mesh.Target.vertexCount; i++) {
			Vertex v = vertsout[i];
			Vector3 pos = vertsPtr[(int)v.xref];
			Vector2 uv = v.uv / wh;
			obj.Verts.Add(pos);
			obj.UVs.Add(uv);
		}
		for (int i = 0; i < indices.Length; i += 3) {
			obj.FacesPosIdx.Add(new Int3(indices[i], indices[i + 1], indices[i + 2]) + 1);
			obj.FacesUVIdx.Add(new Int3(indices[i], indices[i + 1], indices[i + 2]) + 1);
		}
		string objStr = obj.ToString();
		File.WriteAllText(outFile, objStr);
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