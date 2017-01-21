using UnityEngine;
//using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TerrainPoint {
	public Vector3 point;
	public Vector3 normal;
	
	public TerrainPoint() {}
	
	public TerrainPoint(Vector3 point, Vector3 normal) {
		this.point = point;
		this.normal = normal;
	}
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class OWTerrain : MonoBehaviour {
	public const int size = 100;
	public const float pointLength = 0.5f;
	
	public static OWTerrain Instance;
	
	public AnimationCurve forceCurve = AnimationCurve.Linear(0,1,1,0);
	public float damp = 10;
	public float fResistance = 0.99f;
	public float hfResistance = 0.99f;
	public float k = 1;
	public float weight = 4;
	public float[,] heightfield;
	public float[,] heightforce;
	
	// private MeshCollider mCollider;
	private Mesh mesh;
	private Vector3[] vertices;
	
	public bool InRange(Vector3 pos) {
		if(pos.x < 0 || pos.x > size * pointLength)
			return false;
		if(pos.z < 0 || pos.z > size * pointLength)
			return false;
		return true;
	}
	
	public TerrainPoint GetTerrainPoint(Vector3 pos) {
		// if(!InRange(pos))
			// return new TerrainPoint(Vector3.zero, Vector3.up)
	
		int x = (int)Mathf.Clamp(pos.x/pointLength,2,size-2);
		int y = (int)Mathf.Clamp(pos.z/pointLength,2,size-2);
		int[] triangles = TestTriangle(pos,x,y) ? GetTriangle0(x,y) : GetTriangle1(x,y);
		
		var point = GetPoints(triangles);
		var plane = new Plane(point[0],point[1],point[2]);
		
		Debug.DrawLine(point[0], point[1]);
		Debug.DrawLine(point[1], point[2]);
		Debug.DrawLine(point[0], point[2]);
		
		return new TerrainPoint(GetPosition(pos,plane), plane.normal);
	}
	
	public bool TestTriangle(Vector3 pos, int x, int y) {
		float xr = pos.x/pointLength - x;
		float yr = pos.z/pointLength - y;
		return xr > yr;
	}
	
	public int[] GetTriangle0(int x, int y) {
		return new int[]{
			y * size + x,
			(y+1) * size + x + 1,
			y * size + x + 1,
		};
	}
	
	public int[] GetTriangle1(int x, int y) {
		return new int[]{
			y * size + x,
			(y+1) * size + x,
			(y+1) * size + x + 1,
		};
	}
	
	public Vector3 GetPosition(Vector3 pos, Plane plane) {
		var p = pos;
		float dis = 0;
		p.y = 100;
		Ray r = new Ray(p,Vector3.up * -1);
		
		if(plane.Raycast(r, out dis)) {
			return r.GetPoint(dis);
		}
		
		return pos;
	}
	
	public Vector3[] GetPoints(int[] triangles) {
		Vector3[] rt = new Vector3[triangles.Length];
		for(int i = 0; i<triangles.Length; i++) {
			rt[i] = vertices[triangles[i]];
		}
		return rt;
	}
	
	public void SetWave(Vector3 pos, float force, float radios) {
		var x = Mathf.RoundToInt(pos.x / pointLength);
		var y = Mathf.RoundToInt(pos.z / pointLength);
		var rad = Mathf.CeilToInt(radios / pointLength);
		var border = size-1;
		
		LoopCell(
			Mathf.Clamp(x-rad,1,border),
			Mathf.Clamp(x+rad,1,border),
			Mathf.Clamp(y-rad,1,border),
			Mathf.Clamp(y+rad,1,border),
			SetHF(pos,force,radios));
	}
	
	private Action<int,int> SetHF(Vector3 pos,float force, float radios) {
		var point = pos;
		point.y = 0;
	
		return (x,y)=>heightfield[x,y] = Mathf.Lerp(
			force, heightfield[x,y],
			Vector3.Distance(point, new Vector3(x*pointLength,0,y*pointLength))/radios);
			// forceCurve.Evaluate(Vector3.Distance(point, new Vector3(x/pointLength,0,y/pointLength))/radios));
	}
	
	void Awake() {
		Instance = this;
		heightfield = new float[size,size];
		heightforce = new float[size,size];
		InitMesh();
		GetComponent<MeshFilter>().mesh = mesh;
		// mCollider = GetComponent<MeshCollider>();
		// mCollider.sharedMesh = mesh;
	}
	
	void Update() {
		FieldUpdate();
	}
	
	private void FieldUpdate() {
		int border = size-1;
		LoopCell(1,border, 1, border, CalcuHeightField);
		mesh.vertices = vertices;
		// mCollider.sharedMesh = mesh;
	}
	
	private void CalcuHeightField(int x, int y) {
		heightforce[x,y] += ((heightfield[x-1,y] + heightfield[x+1,y] + heightfield[x,y-1] + heightfield[x,y+1])/weight - heightfield[x,y]) * k;
		heightforce[x,y] *= fResistance;
		heightfield[x,y] += heightforce[x,y] * damp * Time.deltaTime;
		// heightfield[x,y] += heightforce[x,y] * damp;
		heightfield[x,y] *= hfResistance;
		vertices[x+y*size].y = heightfield[x,y];
		
	}
	
	private void LoopCell(int xBegin, int xEnd, int yBegin, int yEnd, Action<int,int> Process) {
		for(int i = xBegin; i<xEnd; i++) {
			for(int j = yBegin; j<yEnd; j++) {
				Process(i,j);
			}
		}
	}
	
	
	private void InitMesh() {
		mesh = new Mesh();
		mesh.MarkDynamic();
	
		int length = size * size;
		int ground = (size-1) * (size-1);
		vertices = new Vector3[length];
		var uv = new Vector2[length];
		var normals = new Vector3[length];
		var triangles = new int[ground * 3 * 2];
		var count = 0;
		var uvDis = 1f/size;
		
		for(int j = 0; j< size; j++) {
			for(int i=0; i<size; i++) {
				vertices[count] = new Vector3(i*pointLength,0,j*pointLength);
				uv[count] = new Vector2(i*uvDis,j*uvDis);
				normals[count] = Vector3.up;
				
				count++;
			}
		}
		count = 0;
		for(int i = 0; i<size-1; i++) {
			for(int j = 0; j<size-1; j++) {
				var y0 = j*size;
				var y1 = (j+1)*size;
				
				triangles[count + 0] = y0 + i + 0;
				triangles[count + 1] = y1 + i + 1;
				triangles[count + 2] = y0 + i + 1;
				triangles[count + 3] = y0 + i + 0;
				triangles[count + 4] = y1 + i + 0;
				triangles[count + 5] = y1 + i + 1;
				
				count += 6;
			}
		}
		
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.normals = normals;
		mesh.triangles = triangles;
		
		mesh.RecalculateBounds();
		
	}
	
	
	
}
