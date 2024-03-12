using System;
using AOT;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Velctor.Utils;
using XAtlas.Net.Interop;

namespace XAtlas.Net
{
	public static class XAtlasExample
	{
		private static XAtlasAPI.SetPrintRetCode setPrintRet = XAtlasAPI.SetPrintRetCode.PrintFuncPtrNull;
		public static unsafe Vertex[] GenerateUV(NativeList<Vector3> inVerts, NativeList<int> inoutIndices, ChartOptions chartOptions, PackOptions packOptions)
		{
			if (!inVerts.IsCreated || inVerts.Length <= 0)
				throw new ArgumentNullException(nameof(inVerts));
			if (!inoutIndices.IsCreated || inoutIndices.Length <= 0)
				throw new ArgumentNullException(nameof(inoutIndices));
			MeshDecl meshDecl = new() {
				vertexCount = (uint)inVerts.Length,
				vertexPositionData = new(inVerts.GetUnsafePtr()),
				indexCount = (uint)inoutIndices.Length,
				indexData = new(inoutIndices.GetUnsafePtr()),
				indexFormat = IndexFormat.UInt32,
				vertexPositionStride = 12,
				epsilon = 1.192092896e-07F
			};
			using AtlasHandle atlas = new();
			if (setPrintRet != XAtlasAPI.SetPrintRetCode.Success)
				setPrintRet = XAtlasAPI.SetPrint(OnXAtlasLog, true);
			if (setPrintRet != XAtlasAPI.SetPrintRetCode.Success)
				Debug.Log("XAtlasWrapper SetPrint失败:" + setPrintRet);
			atlas.SetProgressCallback(LogProgress);
			AddMeshError addMeshRetCode = atlas.AddMesh(meshDecl);
			if (addMeshRetCode != AddMeshError.Success) {
				Debug.Log($"Add Mesh Error:{addMeshRetCode}");
				return null;
			}
			Debug.Log($"网格添加完成");
			atlas.Generate(chartOptions, packOptions);
			ref Atlas data = ref atlas.Output;
			Mesh outMesh = data.meshes.Target;
			int outVertCnt = (int)outMesh.vertexCount;
			Debug.Log($"生成完成. 网格数: {data.meshCount}, 岛数量: {data.chartCount}, 宽: {data.width}, 高: {data.height},\n" +
				$"网格0: 顶点数:{outMesh.vertexCount}, 索引数:{outMesh.indexCount}, 岛数量:{outMesh.chartCount}");
			Span<Vertex> vertsout = new(outMesh.vertexArray, outVertCnt);
			var outVerts = new Vertex[outVertCnt];
			float2 wh = new(data.width, data.height);
			for (int i = 0; i < outVertCnt; i++) {
				vertsout[i].uv *= 1 / wh;
				outVerts[i] = vertsout[i];
			}
			inoutIndices.SetCapacity((int)outMesh.indexCount);
			inoutIndices.Clear();
			Ptr<int> outVertBuffer = outMesh.indexArray.CastAs<int>();
			for (int i = 0; i < outMesh.indexCount; i++)
				inoutIndices.Add(outVertBuffer[i]);
			return outVerts;
		}

		[MonoPInvokeCallback(typeof(ProgressFunc))]
		private static Bool LogProgress(ProgressCategory category, int progress, IntPtr userData)
		{
			Debug.Log($"\t{category}: {progress}%");
			return true;
		}

		[MonoPInvokeCallback(typeof(StringFunc))]
		private static void OnXAtlasLog(IntPtr LPStr, int strLen)
		{
			Debug.Log($"XAtlas: {Marshal.PtrToStringAnsi(LPStr, strLen)}");
		}
	}
}
