using System;
using System.Collections.Generic;
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
	/// <summary> 注意obj的序号是从1开始的，每个减去1才能用作indices buffer </summary>
	public readonly List<Int3> FacesPosIdx = new();
	public readonly List<Int3> FacesUVIdx = new();
	public readonly List<Int3> FacesNormalIdx = new();
	public int VertexCount => Verts.Count;
	public int FaceCount => FacesPosIdx.Count;
	public bool HasUVs => UVs.Count == VertexCount;
	public bool HasNormals => Normals.Count == VertexCount;
	public bool HasUVIdx => FacesUVIdx.Count == FaceCount;
	public bool HasFacesNormalIdx => FacesNormalIdx.Count == FaceCount;

	public Obj(string inFile)
	{
		StreamReader sr = new(inFile);
		while (!sr.EndOfStream) {
			AppendLine(sr.ReadLine());
		}
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
			objLine = objLine[..cmtIdx];//忽略每行第一个#后的内容
		if (objLine.IsWhiteSpace()) {
			return;
		}
		//空格分割当前行的字符串, 返回前四段子串在父串的索引范围(obj四段够用了)
		Range4 elems = Split4(objLine, separator: ' ');
		//根据序号获得子字符串. 例如"v 11.4 51.4 191.9", "v"为0, "11.4"为1,"51.4"为2,"191.9"为3
		ReadOnlySpan<char> Elem(int idx) => new StrSeg(fullLine, elems[idx]).AsSpan;
		float ElemF(int idx) => float.Parse(Elem(idx));//不考虑格式错误的obj, 所以不处理异常了

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
			Span<Range4> faceElemsSpan = new(faceElems, 3);
			for (int i = 0; i < 3; i++)
				faceElemsSpan[i] = Split4(Elem(i + 1), separator: '/').OffsetedWith(elems[i + 1].Start.Value);//i+1跳过f

			//Face Element Int，elemIdx:被空格分割后的子字符串序号(忽略f), subIdx:子字符串继续被/分割后的序号。
			//比如"f 21/32/65 54/23/86 14/25/36"这样的行，FElemI(0, 1)返回的就是32。
			int FElemI(int elemIdx, int subIdx) => int.Parse(new StrSeg(fullLine, faceElems[elemIdx][subIdx]).AsSpan);

			bool TryFElemI(int elemIdx, int subIdx, out int idx)
			{
				idx = default;
				Range elemRag = faceElems[elemIdx][subIdx];
				if (elemRag.Start.Value <= elemRag.End.Value)
					return false;
				ReadOnlySpan<char> seg = fullLine.AsSpan()[elemRag];
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
		foreach (char c in str)
			if (c == wantToCount) cnt++;
		return cnt;
	}

	public override string ToString()
	{
		StringBuilder sb = new();
		foreach (Vector3 vert in Verts)
			sb.Append("v ").AppendVector3(vert).Append('\n');
		if (HasUVs)
			foreach (Vector2 uv in UVs)
				sb.Append("vt ").AppendVector2(uv).Append('\n');
		if (HasNormals)
			foreach (Vector3 norm in Normals)
				sb.Append("n ").AppendVector3(norm).Append('\n');
		for (int i = 0; i < FaceCount; i++) {
			Int3 vIdx = FacesPosIdx[i];
			Int3 uvIdx = HasUVIdx ? FacesUVIdx[i] : 0;//Obj的Index是从1开始的，所以0为无效
			Int3 nIdx = HasNormals ? FacesNormalIdx[i] : 0;
			AppendFace(vIdx, uvIdx, nIdx);
			sb.Append('\n'); ;
		}
		return sb.ToString();

		void AppendFace(Int3 vid, Int3 vtid, Int3 nid)
		{
			//f and first vert
			sb.Append("f ").Append(vid.X);
			if (vtid.X > 0 || nid.X > 0)
				sb.Append('/');
			if (vtid.X > 0)
				sb.Append(vtid.X);
			if (nid.X > 0)
				sb.Append('/').Append(nid.X);
			//vert second
			sb.Append(' ').Append(vid.Y);
			if (vtid.Y > 0 || nid.Y > 0)
				sb.Append('/');
			if (vtid.Y > 0)
				sb.Append(vtid.Y);
			if (nid.Y > 0)
				sb.Append('/').Append(nid.Y);
			//vert third
			sb.Append(' ').Append(vid.Z);
			if (vtid.Z > 0 || nid.Z > 0)
				sb.Append('/');
			if (vtid.Z > 0)
				sb.Append(vtid.Z);
			if (nid.Z > 0)
				sb.Append("/").Append(nid.Z);
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

	public readonly Range4 OffsetedWith(int offset)
	{
		Range4 copy = this;
		Ptr<int> intPtr = copy.GetPtr().CastAs<int>();
		for (int i = 0; i < 8; i++) {
			intPtr[i] += offset;
		}
		return copy;
	}
}