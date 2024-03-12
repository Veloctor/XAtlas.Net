using System;
using UnityEngine;
using System.Runtime.InteropServices;
using Velctor.Utils;
using static System.Runtime.InteropServices.CallingConvention;
using UnityEngine.Rendering;

namespace XAtlas.Net.Interop
{
	public enum AddMeshError
	{
		Success, // No error.
		Error, // Unspecified error.
		IndexOutOfRange, // An index is >= MeshDecl vertexCount.
		InvalidFaceVertexCount, // Must be >= 3.
		InvalidIndexCount // Not evenly divisible by 3 - expecting triangles.
	};

	/// <summary>
	/// remeber to set count and stride(byte size of vertex element) when set buffer! buffer must be accessable during native process!!!
	/// </summary>

	[Serializable]
	public struct MeshDecl
	{
		public static MeshDecl Empty = new() {
			vertexPositionStride = 12,
			epsilon = 1.192092896e-07F,
		};

		public Ptr<Vector3> vertexPositionData;
		/// <summary>  Optional </summary>
		public Ptr<Vector3> vertexNormalData;
		/// <summary> optional. The input UVs are provided as a hint to the chart generator. </summary>
		public Ptr<Vector2> vertexUvData;
		/// <summary>  Optional </summary>
		public Ptr<int> indexData;
		/// <summary>
		/// Optional. Must be faceCount in length.
		/// Don't atlas faces set to true. Ignored faces still exist in the output meshes, Vertex uv is set to (0, 0) and Vertex atlasIndex to -1.
		/// </summary>
		public Ptr<Bool> faceIgnoreData;
		/// <summary>
		/// Optional. Must be faceCount in length.
		/// Only faces with the same material will be assigned to the same chart.
		/// </summary>
		public Ptr<uint> faceMaterialData;
		/// <summary>
		/// Optional. Must be faceCount in length.
		/// Polygon / n-gon support. Faces are assumed to be triangles if this is null.
		/// </summary>
		public Ptr<byte> faceVertexCount;
		public uint vertexCount;
		public uint vertexPositionStride;
		/// <summary> Optional </summary>
		public uint vertexNormalStride;
		/// <summary> Optional </summary>
		public uint vertexUvStride;
		public uint indexCount;
		/// <summary> Optional. Add this offset to all indices. </summary>
		public int indexOffset;
		/// <summary> Optional if faceVertexCount is null. Otherwise assumed to be indexCount / 3.</summary>
		public uint faceCount;
		public IndexFormat indexFormat;
		/// <summary>  Vertex positions within epsilon distance of each other are considered colocal.  </summary>
		public float epsilon;
	}

	[Serializable]
	public struct UvMeshDecl
	{
		public static UvMeshDecl Empty = new() {
			vertexStride = 12,
		};

		public Ptr<Vector2> vertexUvData;
		public Ptr<uint> indexData;
		public Ptr<uint> faceMaterialData;
		public uint vertexCount;
		public uint vertexStride;
		public uint indexCount;
		/// <summary> optional. Add this offset to all indices. </summary>
		public int indexOffset;
		public IndexFormat indexFormat;
	}

	[UnmanagedFunctionPointer(Cdecl)]
	public delegate void ParameterizeFunc(Ptr<float> positions, Ptr<float> texcoords, uint vertexCount, Ptr<uint> indices, uint indexCount);

	[Serializable]
	public struct ChartOptions
	{
		public static ChartOptions Default => new() {
			normalDeviationWeight = 2,
			roundnessWeight = 0.01f,
			straightnessWeight = 6,
			normalSeamWeight = 4,
			textureSeamWeight = .5f,
			maxCost = 2,
			maxIterations = 1,
		};

		/// <summary> <see cref="Marshal.GetFunctionPointerForDelegate{ParameterizeFunc}"/> TDelegate = ParameterizeFunc </summary>
		public IntPtr paramFunc;
		public float maxChartArea;
		public float maxBoundaryLength;
		public float normalDeviationWeight;
		public float roundnessWeight;
		public float straightnessWeight;
		public float normalSeamWeight;
		public float textureSeamWeight;
		public float maxCost;
		public uint maxIterations;
		public Bool useInputMeshUvs;
		public Bool fixWinding;
	}

	[Serializable]
	public struct PackOptions
	{
		public PackOptions(uint resolution)
		{
			this = Default;
			this.resolution = resolution;
		}

		public static PackOptions Default => new() {
			padding = 1,
			bilinear = true,
			rotateChartsToAxis = true,
			rotateCharts = true,
		};

		/// <summary>
		/// Charts larger than this will be scaled down. 0 means no limit.
		/// </summary>
		public uint maxChartSize;

		/// <summary>
		/// Number of pixels to pad charts with.
		/// </summary>
		public uint padding;

		/// <summary>
		/// Unit to texel scale. e.g. a 1x1 quad with texelsPerUnit of 32 will take up approximately 32x32 texels in the atlas.
		/// If 0, an estimated value will be calculated to approximately match the given resolution.
		/// If resolution is also 0, the estimated value will approximately match a 1024x1024 atlas.
		/// </summary>
		public float texelsPerUnit;

		/// <summary>
		/// If 0, generate a single atlas with texelsPerUnit determining the final resolution.
		/// If not 0, and texelsPerUnit is not 0, generate one or more atlases with that exact resolution.
		/// If not 0, and texelsPerUnit is 0, texelsPerUnit is estimated to approximately match the resolution.
		/// </summary>
		public uint resolution;

		/// <summary>
		/// Leave space around charts for texels that would be sampled by bilinear filtering.
		/// </summary>
		public Bool bilinear;

		/// <summary>
		/// Align charts to 4x4 blocks. Also improves packing speed, since there are fewer possible chart locations to consider.
		/// </summary>
		public Bool blockAlign;

		/// <summary>
		/// Slower, but gives the best result. If false, use random chart placement.
		/// </summary>
		public Bool bruteForce;

		/// <summary>
		/// Create AtlasHandle::image
		/// </summary>
		public Bool createImage;

		/// <summary>
		/// Rotate charts to the axis of their convex hull.
		/// </summary>
		public Bool rotateChartsToAxis;

		/// <summary>
		/// Rotate charts to improve packing.
		/// </summary>
		public Bool rotateCharts;
	}

	/// <summary> Progress tracking.  </summary>
	public enum ProgressCategory
	{
		AddMesh,
		ComputeCharts,
		PackCharts,
		BuildOutputMeshes
	}


	[UnmanagedFunctionPointer(Cdecl)]
	public delegate Bool ProgressFunc(ProgressCategory category, int progress, IntPtr userData);

	[UnmanagedFunctionPointer(Cdecl)]
	public delegate IntPtr ReallocFunc(IntPtr addr, IntPtr size);

	[UnmanagedFunctionPointer(Cdecl)]
	public delegate void FreeFunc(IntPtr addr);

	//typedef void (*StringFunc)(const char* formattedContent, int strLen);
	[UnmanagedFunctionPointer(Cdecl)]
	public delegate void StringFunc(IntPtr LPStr, int strLen);

	public static class XAtlasAPI
	{
		const string DLL = "xatlas";

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasCreate")]
		internal static extern IntPtr Create();

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasDestroy")]
		internal static extern void Destroy(IntPtr atlas);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasAddMesh")]
		internal static extern AddMeshError AddMesh(IntPtr atlas, in MeshDecl meshDecl, uint meshCountHint);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasAddMeshJoin")]
		internal static extern void AddMeshJoin(IntPtr atlas);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasAddUvMesh")]
		internal static extern AddMeshError AddUvMesh(IntPtr atlas, in UvMeshDecl decl);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasComputeCharts")]
		internal static extern void ComputeCharts(IntPtr atlas, in ChartOptions chartOptions);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasPackCharts")]
		internal static extern void PackCharts(IntPtr atlas, in PackOptions packOptions);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasGenerate")]
		internal static extern void Generate(IntPtr atlas, in ChartOptions chartOptions, in PackOptions packOptions);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasSetProgressCallback")]
		internal static extern void SetProgressCallback(IntPtr atlas, ProgressFunc progressFunc, IntPtr progressUserData);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasSetAlloc")]
		public static extern void SetAlloc(ReallocFunc reallocFunc, FreeFunc freeFunc);

		/// <summary>
		/// this maybe can only be called after any other xatlas.dll func called(for dllimport set dll search path to plugins)
		/// </summary>
		[DllImport("LogWrapper", CallingConvention = Cdecl, EntryPoint = "SetPrintFormatted")]
		public static extern SetPrintRetCode SetPrint(StringFunc printFuncPtr, Bool verbose);//int SetPrintFormatted(StringFunc func, bool verbose);

		public enum SetPrintRetCode
		{
			Success = 0,
			PrintFuncPtrNull = -1,
			LoadXAtlasDllFail = -2,
			LoadxatlasSetPrintFail = -3
		}

		[DllImport(DLL, CallingConvention = Cdecl, CharSet = CharSet.Ansi, EntryPoint = "xatlasAddMeshErrorString")]
		public static extern string AddMeshErrorString(AddMeshError error);

		[DllImport(DLL, CallingConvention = Cdecl, CharSet = CharSet.Ansi, EntryPoint = "xatlasProgressCategoryString")]
		public static extern string ProgressCategoryString(ProgressCategory category);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasMeshDeclInit")]
		public static extern void MeshDeclInit(ref MeshDecl meshDecl);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasUvMeshDeclInit")]
		public static extern void UvMeshDeclInit(ref UvMeshDecl uvMeshDecl);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasChartOptionsInit")]
		public static extern void ChartOptionsInit(ref ChartOptions chartOptions);

		[DllImport(DLL, CallingConvention = Cdecl, EntryPoint = "xatlasPackOptionsInit")]
		public static extern void PackOptionsInit(ref PackOptions packOptions);
	}
}