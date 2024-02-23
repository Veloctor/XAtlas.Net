using Velctor.Utils;
using XAtlas.Net;
using XAtlas.Net.Interop;

class Program
{
	private static void Main()
	{
		using Atlas atlas = new();
		MeshDecl meshDecl = new();

		atlas.AddMesh(meshDecl);
		atlas.Generate(new ChartOptions(), new PackOptions(1024));
		unsafe {
			Atlas.MemLayout* data = atlas.Data;
			Mesh* mesh = data->meshes;
			Chart* chart = mesh[0].chartArray;
		}
	}
}