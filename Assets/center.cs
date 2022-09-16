using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class center : MonoBehaviour {

	public Transform A;
	public Transform B;

	// Use this for initialization
	void Start () {
		if(!A || !B){Destroy(this);}
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.Lerp(A.transform.position, B.transform.position,0.5f);
	}
}
