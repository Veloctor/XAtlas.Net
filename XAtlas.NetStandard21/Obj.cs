using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using Velctor.Utils;
using XAtlas.Net;
using static System.StringComparison;

namespace ObjParser;

public class Obj
{
	public readonly List<Vector3> Verts = new();
	public readonly List<Vector3> Normals = new();
	public readonly List<Vector2> UVs = new();
	public readonly List<Int3> FacesPosIdx = new();
	public readonly List<Int3> FacesUVIdx = new();
	public readonly List<Int3> FacesNormalIdx = new();
	public int VertCount => Verts.Count;
	public int IndexCount => FacesPosIdx.Count;

	public Obj(string inFile)
	{
		StreamReader sr = new(inFile);
		while (!sr.EndOfStream) {
			AppendLine(sr.ReadLine());
		}
		Debug.Assert(Normals.Count ==VertCount && UVs.Count == VertCount, "Vertex Normal UV count is different");
	}

	public void ClearAll()
	{
		Verts.Clear();
		Normals.Clear();
		UVs.Clear();
		FacesPosIdx.Clear();
		FacesUVIdx.Clear();
		FacesNormalIdx.Clear();
	}

	private void AppendLine(string fullLine)
	{
		ReadOnlySpan<char> objLine = fullLine;
		int cmtIdx = objLine.IndexOf('#');
		if (cmtIdx >= 0)
			objLine = objLine[..cmtIdx];
		if (objLine.IsWhiteSpace()) {
			return;
		}
		Range4 elems = Split4(objLine, separator: ' ');
		ReadOnlySpan<char> Elem(int idx) => new StrSeg(fullLine, elems[idx]).AsSpan;
		float ElemF(int idx) => float.Parse(new StrSeg(fullLine, elems[idx]).AsSpan);

		if (Elem(0).Equals("v", Ordinal)) {
			Verts.Add(new(ElemF(1), ElemF(2), ElemF(3)));
		}
		else if (Elem(0).Equals("vt", Ordinal)) {
			UVs.Add(new(ElemF(1), ElemF(2)));
		}
		else if (Elem(0).Equals("n", Ordinal)) {
			Normals.Add(new(ElemF(1), ElemF(2), ElemF(3)));
		}
		else if (Elem(0).Equals("f", Ordinal)) {
			AppendFaceLine(fullLine, elems);
		}
	}

	private unsafe void AppendFaceLine(string fullLine, Range4 elems)
	{
		ReadOnlySpan<char> Elem(int idx) => new StrSeg(fullLine, elems[idx]).AsSpan;
		int ElemI(int idx) => int.Parse(new StrSeg(fullLine, elems[idx]).AsSpan);
		int SepCnt = PopCnt(Elem(1), '/');
		if (SepCnt > 0) {
			Range4* faceElems = stackalloc Range4[3];
			for (int i = 0; i < 3; i++)
				faceElems[i] = Split4(Elem(i + 1), separator: '/');
			int FElemI(int elemIdx, int subIdx) => int.Parse(new StrSeg(fullLine, faceElems[elemIdx][subIdx]).AsSpan);
			bool TryFElemI(int elemIdx, int subIdx, out int idx)
			{
				idx = default;
				Range elemRag = faceElems[elemIdx][subIdx];
				if(elemRag.le)
				ReadOnlySpan<char> seg = fullLine.AsSpan()[elemRag];
				if (seg.IsEmpty)
					return false;
				return int.TryParse(seg, out idx);
			}

			FacesPosIdx.Add(new(FElemI(0, 0), FElemI(1, 0), FElemI(2, 0)));
			if (TryFElemI(0, 1, out int nx)
				&& TryFElemI(1, 1, out int ny)
				&& TryFElemI(2, 1, out int nz))
				FacesUVIdx.Add(new(nx, ny, nz));
			if (TryFElemI(0, 2, out int uvx)
				&& TryFElemI(1, 2, out int uvy)
				&& TryFElemI(2, 2, out int uvz))
				FacesNormalIdx.Add(new(uvx, uvy, uvz));
		}
		else
			FacesPosIdx.Add(new(ElemI(1), ElemI(2), ElemI(3)));
	}

	private int PopCnt(ReadOnlySpan<char> str, char wantToCount)
	{
		int cnt = 0;
		foreach(char c in str)
			if(c == wantToCount) cnt++;
		return cnt;
	}

	public override string ToString()
	{
		StringBuilder sb = new();
		foreach (var vert in Verts)
			sb.Append("v ").AppendVector3(vert);
		foreach (var norm in Normals)
			sb.Append("n ").AppendVector3(norm);
		foreach (var uv in UVs)
			sb.Append("n ").AppendVector2(uv);
		for (int i = 0;i< Verts.Count; i++) {

		}
		return sb.ToString();

		void AppendFace(Int3 vid, Int3 nid, Int3 vtid)
		{
			sb.Append("f ").Append(vid.X).Append('/');
			if (nid.X > 0)
				sb.Append(nid.X);
			sb.Append('/');
			if (vtid.X > 0)
				sb.Append(vtid.X);
			sb.Append(' ').Append(vid.Y).Append('/');
			if(nid.Y > 0)
				sb.Append(nid.Y);
			sb.Append('/');
			if (vtid.Y > 0)
				sb.Append(vtid.Y);
			sb.Append(' ').Append(vid.Z).Append('/');
			if(nid.Z > 0)
				sb.Append(nid.Z);
			sb.Append("/");
			if(vtid.Z > 0)
				sb.Append(vtid.Z);
		}
	}

	public static Range4 Split4(ReadOnlySpan<char> str, char separator = ' ')
	{
		Range4 ranges = default;
		int lastFound = 0, foundSeps = 0;
		for (int i = 0; i < str.Length; i++) {
			if (str[i] == separator) {
				// 直接使用 lastFound 和 i 来创建范围，以包括空子串
				ranges[foundSeps] = new Range(lastFound, i);
				lastFound = i + 1; // 更新 lastFound 为下一个字符的位置，即分隔符之后
				if (++foundSeps == 4) // 当填充了4个范围后停止
					break;
			}
		}
		if (foundSeps < 4 && lastFound <= str.Length) { // 处理最后一个分隔符之后的字符串
			ranges[foundSeps] = new Range(lastFound, str.Length);
		}
		return ranges;
	}
}

public static class SBExtension
{
	public static StringBuilder AppendVector2(this StringBuilder sb, Vector2 v, char separator = ' ') =>
		sb.Append(v.X).Append(separator).Append(v.Y);

	public static StringBuilder AppendVector3(this StringBuilder sb, Vector3 v, char separator = ' ') =>
		sb.Append(v.X).Append(separator).Append(v.Y).Append(separator).Append(v.Z);

	public static StringBuilder AppendInt3(this StringBuilder sb, Int3 v, char separator = ' ') =>
		sb.Append(v.X).Append(separator).Append(v.Y).Append(separator).Append(v.Z);
}

public struct StrSeg
{
	public string original;
	public Range range;

	public readonly ReadOnlySpan<char> AsSpan => original.AsSpan()[range];

	public StrSeg(StrSeg original, Range range)
	{
		this.original = original.original;
		this.range = range;
	}

	public StrSeg(string original, Range range)
	{
		this.original = original;
		this.range = range;
	}

	public static implicit operator ReadOnlySpan<char>(StrSeg seg) => seg.AsSpan;
	public static implicit operator StrSeg(string fullStr) => new(fullStr, Range.All);
}

public struct Range4
{
	public Range x;
	public Range y;
	public Range z;
	public Range w;

	/// <param name="index">0~3对应x~w</param>
	public Range this[int index] {
		get => x.GetPtr()[index];
		set => x.GetPtr()[index] = value;
	}
}