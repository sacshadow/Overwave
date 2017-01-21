using UnityEngine;
using UnityEngine.Networking;
//using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class OWObject : NetworkBehaviour {
	public const float bumpMax = 10;
	public Rigidbody rBody;
	
	public float slopeForce = 10;
	
	private Vector3 lastPosition;
	
	public void SetPosition(Vector3 point) {
		var position = point;
		position.x = Mathf.Clamp(position.x,0,OWTerrain.size * OWTerrain.pointLength);
		position.z = Mathf.Clamp(position.z,0,OWTerrain.size * OWTerrain.pointLength);
		transform.position = position;
	}
	
	
	// Use this for initialization
	void Start () {
		lastPosition = transform.position;
	}
	
	void Update() {
		
	}
	
	protected virtual void FixedUpdate() {
		if(hasAuthority)
			ObjectFixedUpdate();
	}
	
	
	protected virtual void ObjectFixedUpdate () {
		var tp = OWTerrain.Instance.GetTerrainPoint(transform.position);
		// var yOffset = 0f;
		var velocity = rBody.velocity;
		
		if(transform.position.y < tp.point.y) {
			// yOffset = tp.point.y - transform.position.y;
			SetPosition(tp.point);
			
			var slope = tp.normal;
			slope.y = 0;
			
			velocity = (transform.position - lastPosition) * 50;
			velocity.y = Mathf.Clamp(velocity.y * 0.25f, -bumpMax, bumpMax);
			
			rBody.AddForce(slope * slopeForce, ForceMode.Acceleration);
		}
		else {
			SetPosition(transform.position);
			velocity = (transform.position - lastPosition) * 50;
		}
		// rBody.velocity = (transform.position - lastPosition) / Time.fixedTime;
		
		rBody.velocity = velocity;
		
		lastPosition = transform.position;
	}
}
