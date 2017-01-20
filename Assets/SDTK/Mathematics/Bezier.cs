using UnityEngine;
using System;
using System.Collections;

namespace SDTK.Math{
	public static class Bezier {

		public static Vector3 Po2Curve(Vector3 p1, Vector3 p2, Vector3 m0, float t){
			float ot = 1-t;
			float c1 = ot*ot, c2 = 2*t*ot, c3 = t*t;
			
			return c1*p1 + c2*m0 + c3*p2;
		}
		
		public static Vector3 Po3Curve(Vector3 p1, Vector3 p2, Vector3 m0, Vector3 m1, float t){
			float ot = 1-t;
			float c1 =  ot*ot*ot, c2 = 3*t*ot*ot, c3 = 3*t*t*ot, c4 = t*t*t;
			
			return c1*p1+c2*m0+c3*m1+c4*p2;
		}
		
		public static Func<float,Vector3> PoNCurve(Vector3 p1, Vector3 p2, params Vector3[] m){
			if(m == null || m.Length == 0)
				return t => Vector3.Lerp(p1, p2, t);
			else if(m.Length == 1)
				return t => Po2Curve(p1,p2,m[0],t);
			else if(m.Length == 2)
				return t => Po3Curve(p1,p2,m[0],m[1],t);
			
			//TODO: power of n>3
			
			return t => {return Vector3.zero;};
		}
		
	}
}
