using UnityEngine;
using System.Collections;

public class DevTest : MonoBehaviour {
	
	public Camera cam;
	public OWTerrain terrain;
	public float waveSize = 5;
	public float waveHeight = 5;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		var plane = new Plane(Vector3.up, Vector3.zero);
		
		Ray r = cam.ScreenPointToRay(Input.mousePosition);
		float dis = 0;
		plane.Raycast(r, out dis);
		var point = r.GetPoint(dis);
		var tp = terrain.GetTerrainPoint(point);
		
		Debug.DrawLine(transform.position, point);
		Debug.DrawLine(tp.point, tp.point + Vector3.up * 5, Color.red);
		Debug.DrawLine(tp.point, tp.point + tp.normal * 3, Color.yellow);
		
	
		if(Input.GetKey(KeyCode.Space))
			terrain.SetWave(point,waveHeight, waveSize);
	}
}
