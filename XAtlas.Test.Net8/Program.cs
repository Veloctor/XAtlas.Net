using Velctor.Utils;
using XAtlas.Net;
using XAtlas.Net.Interop;

class Program
{
	private static void Main()
	{
		XAtlasAPI.SetPrint(PrintFormat.Printf, true);
		using Atlas atlas = new();
		atlas.SetProgressCallback(LogProgress);
		var obj = new ObjParser.Obj();
		obj.LoadObj("E:/Desktop/qnb2.obj");
		var vtxlist = obj.VertexList;
		int vertCnt = vtxlist.Count;
		//int idxCnt = obj.;
		using NativeBuffer<float> posBuffer = new(vertCnt * 3);
		//using NativeBuffer<int> idxBuffer = new(idxCnt);
		Span<float> posSpan = posBuffer.AsSpan;
		for (int i = 0; i < vertCnt; i++) {
			posSpan[i * 3] = (float)vtxlist[i].X;
			posSpan[i * 3 + 1] = (float)vtxlist[i].Y;
			posSpan[i * 3 + 2] = (float)vtxlist[i].Z;
		}
		//Span<int> idxSpan = idxBuffer.AsSpan;
		//for (int i = 0; i < idxCnt; i++) {
		//	idxSpan[i] = indxRead.ReadInt32();
		//}
		MeshDecl meshDecl = new();
		meshDecl.vertexCount = (uint)vertCnt;
		meshDecl.vertexPositionData = posBuffer.Handle;
		meshDecl.vertexPositionStride = 12;
		//meshDecl.indexCount = (uint)idxCnt;
		//meshDecl.indexData = idxBuffer.Handle;
		meshDecl.indexFormat = IndexFormat.UInt32;
		AddMeshError addMeshRetCode = atlas.AddMesh(meshDecl, 1);
		if (addMeshRetCode != AddMeshError.Success) {
			Console.WriteLine($"Add Mesh Error:{addMeshRetCode}");
			return;
		}
		atlas.Generate(new ChartOptions(), new PackOptions(1024));
		//unsafe {
		//	Atlas.MemLayout* data = atlas.Data;
		//	Mesh* mesh = data->meshes;
		//	Chart* chart = mesh[0].chartArray;
		//}
	}

	public static BOOL LogProgress(ProgressCategory category, int progress, IntPtr userData)
	{
		Console.WriteLine($"\t{category}: {progress}%");
		return true;
	}
}