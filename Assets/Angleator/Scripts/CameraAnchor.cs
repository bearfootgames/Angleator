using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnchor : MonoBehaviour {

	public GameObject TargetObj;

	public Transform target;

	public enum types {front,left,right,back,top};
	public types type;

	void Start()
	{
		if(!target)
		{
			TargetObj = GameObject.Find("EnfluxHumanoid");
			
		}
		Get_Target();
	}

	public void Get_Target()
	{
		target = TargetObj.transform.Find("EnfluxHumanoid/Armature/hip").GetComponent<Transform>();
	}


	
	// Update is called once per frame
	void LateUpdate () {
		if(type == types.front){transform.LookAt(target.position + target.forward * -3);}
		if(type == types.left){transform.LookAt(target.position + target.right * 3);}
		if(type == types.right){transform.LookAt(target.position + target.right * -3);}
		if(type == types.back){transform.LookAt(target.position + target.forward * 3);}
		if(type == types.top)
		{
			transform.position = new Vector3(target.position.x,transform.position.y,target.position.z);
		}
	}
}
