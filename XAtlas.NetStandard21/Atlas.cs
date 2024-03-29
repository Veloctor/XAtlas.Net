﻿using System;
using System.Numerics;
using Velctor.Utils;
using XAtlas.Net.Interop;

namespace XAtlas.Net
{
	public enum ChartType
	{
		Planar,
		Ortho,
		LSCM,
		Piecewise,
		Invalid
	}

	// A group of connected faces, belonging to a single atlas.
	public struct Chart
	{
		public Ptr<uint> faceArray;
		public uint atlasIndex; // Sub-atlas index.
		public uint faceCount;
		public ChartType type;
		public uint material;
	};
	// Output vertex.
	public struct Vertex
	{
		public int atlasIndex; // Sub-atlas index. -1 if the vertex doesn't exist in any atlas.
		public int chartIndex; // -1 if the vertex doesn't exist in any chart.
		public Vector2 uv; // Not normalized - values are in AtlasHandle width and height range.
		public uint xref; // Index of input vertex from which this output vertex originated.
	};
	// Output mesh.
	public struct Mesh
	{
		public Ptr<Chart> chartArray;
		public Ptr<uint> indexArray;
		public Ptr<Vertex> vertexArray;
		public uint chartCount;
		public uint indexCount;
		public uint vertexCount;
	};

	//if you want generate atlas, use AtlasHandle class
	public struct Atlas
	{
		public Ptr<uint> image;
		public Ptr<Mesh> meshes; // The output meshes, corresponding to each AddMesh call.
		public Ptr<float> utilization; // Normalized atlas texel utilization array. E.g. a value of 0.8 means 20% empty space. atlasCount in length.
		public uint width; // Atlas width in texels.
		public uint height; // Atlas height in texels.
		public uint atlasCount; // Number of sub-atlases. Equal to 0 unless PackOptions resolution is changed from default (0).
		public uint chartCount; // Total number of charts in all meshes.
		public uint meshCount; // Number of output meshes. Equal to the number of times AddMesh was called.
		public float texelsPerUnit; // Equal to PackOptions texelsPerUnit if texelsPerUnit > 0, otherwise an estimated value to match PackOptions resolution.
	}

	public class AtlasHandle : UnmanagedResource
	{
		public AtlasHandle() : base(XAtlasAPI.Create()) { }

		/// <summary> You should only read it unless you know what are you doing </summary>
		public ref Atlas Output => ref new Ptr<Atlas>(Handle).Target;

		/// <summary>
		/// Add a mesh to the atlas. MeshDecl 
		/// </summary>
		/// <param name="meshDecl">data is copied, so it can be freed after AddMesh returns.</param>
		public AddMeshError AddMesh(MeshDecl meshDecl, uint meshCountHint = 0) =>
			XAtlasAPI.AddMesh(Handle, meshDecl, meshCountHint);

		/// <summary>
		/// Wait for AddMesh async processing to finish. ComputeCharts / Generate call this internally.
		/// </summary>
		public void AddMeshJoin() =>
			XAtlasAPI.AddMeshJoin(Handle);

		public AddMeshError AddUvMesh(UvMeshDecl decl) =>
			XAtlasAPI.AddUvMesh(Handle, decl);

		/// <summary>
		/// Call after all AddMesh calls. Can be called multiple times to recompute charts with different options.
		/// </summary>
		public void ComputeCharts(ChartOptions chartOptions) =>
			XAtlasAPI.ComputeCharts(Handle, chartOptions);

		/// <summary> Call after ComputeCharts. Can be called multiple times to re-pack charts with different options. </summary>
		public void PackCharts(PackOptions packOptions) =>
			XAtlasAPI.PackCharts(Handle, packOptions);

		/// <summary>
		/// Equivalent to calling ComputeCharts and PackCharts in sequence. Can be called multiple times to regenerate with different options.
		/// </summary>
		public void Generate(ChartOptions chartOptions, PackOptions packOptions) =>
			XAtlasAPI.Generate(Handle, chartOptions, packOptions);

		/// <summary>
		/// May be called from any thread. Return false to cancel.
		/// </summary>
		public void SetProgressCallback(ProgressFunc progressFunc, IntPtr progressUserData = default) =>
			XAtlasAPI.SetProgressCallback(Handle, progressFunc, progressUserData);

		protected override void ReleaseUnmanagedResource() => XAtlasAPI.Destroy(Handle);
	}
}
