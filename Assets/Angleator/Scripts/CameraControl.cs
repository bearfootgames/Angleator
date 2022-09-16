using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public GameObject[] cameras;
	public int i = 0;

	void Start()
	{
		i = -1;
		Iterate_Cameras();
	}

	public void Iterate_Cameras()
	{
		i++;
		if(i == cameras.Length){i=0;}
		cameras[i].SetActive(true);
		foreach(GameObject c in cameras)
		{
			if(c != cameras[i]){
				c.SetActive(false);
			}
		}
	}
}
