using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character_Skin : MonoBehaviour {

	public Texture2D[] skins;
	public Texture2D[] shirts;

	public SkinnedMeshRenderer smr;

	void Start ()
	{
		if(!smr){
			smr = gameObject.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();
		}

		gameObject.name = "EnfluxHumanoid";
	}

	int skin = 0;
	public void Change_Skin()
	{
		if(skins.Length <= 1){return;}
		skin++;
		if(skin >= skins.Length){skin = 0;}
		smr.sharedMaterial.SetTexture("_DetailAlbedoMap",skins[skin]);
	}
	
	int shirt = 0;
	public void Change_Shirt()
	{
		if(shirts.Length <= 1){return;}
		shirt++;
		if(shirt >= shirts.Length){shirt = 0;}
		smr.sharedMaterial.SetTexture("_MainTex",shirts[shirt]);
	}
	
}
