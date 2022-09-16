using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enflux.Examples.Components;
using Enflux.SDK.Recording;
public class Manager : MonoBehaviour {

	public GameObject angleator;
	GameObject[] angleators;
	GameObject[] angleatorsGUI;

	public GameObject avatarmenu;

	//public Purchaser purchaser;

	public GameObject connError;
	public GameObject trialError;

	GameObject canvas;
	public GameObject[] characters;
	public character_Skin CharScript;
	GameObject am;
	GameObject human;
	GameObject human_playback;
	public EnfluxFileRecorder enfluxfilerecorder;
	public EnfluxFilePlayer enfluxfileplayer;
	int charnum = 0;

	// Use this for initialization
	void Start () 
	{	
		connError.SetActive(false);
		trialError.SetActive(false);

		SubBought();

		human = GameObject.Find("EnfluxHumanoid");
		human_playback = GameObject.Find("EnfluxHumanoid_Playback");
		human_playback.SetActive(false);
		Instantiate(angleator);
		am = Instantiate(avatarmenu);
		am.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate {GameObject.Find("EnfluxHumanoid").GetComponent<character_Skin>().Change_Skin();});
		am.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate {GameObject.Find("EnfluxHumanoid").GetComponent<character_Skin>().Change_Shirt();});
		am.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate {Open_Avatar();});
		
		if(!canvas){
			canvas = GameObject.Find("Canvas");
		}
		am.transform.SetParent(canvas.transform,false);
		float w = canvas.GetComponent<RectTransform>().rect.x/1.25f;
		//float h = canvas.GetComponent<RectTransform>().rect.y/2;
		am.GetComponent<RectTransform>().localPosition = new Vector3(w,0,0);
		Open_Avatar();
	}

	public void Open_Avatar()
	{
		if(am.activeSelf){
			am.SetActive(false);
		}else{
			am.SetActive(true);
		}
	}

	public void Change_Character()
	{
		if(characters.Length <= 1){return;}
		charnum++;
		if(charnum >= characters.Length){charnum = 0;}
		GameObject g = GameObject.Find("EnfluxHumanoid");
		Instantiate(characters[charnum],Vector3.zero,Quaternion.identity);
		Destroy(g);
	}

	bool isPlaying = false;
	void Update()
	{
		if(enfluxfileplayer.IsFinished && isPlaying)
		{
			isPlaying = false;
			StopPlayback();
		}
		#if UNITY_EDITOR
		#endif
	}

	public void StartPlayBack()
	{
		//Enflux.SDK.Recording.EnfluxFilePlayer.IsLooping = true;
		human.SetActive(false);
		human_playback.SetActive(true);

		GameObject[] angleators = GameObject.FindGameObjectsWithTag("Angleator");
		foreach (GameObject g in angleators){
			g.GetComponent<Angleator>().enfluxhumanoid = human_playback;
			g.GetComponent<Angleator>().dropdown_handle();
		}
		CameraControl C = GetComponent<CameraControl>();
		for (int c = 0; c < C.cameras.Length; c++){
			C.cameras[c].GetComponent<AnchorToTransform>().Anchor = human_playback.transform.Find("Armature/hip");
		}
		isPlaying = true;
	}
	public void StopPlayback()
	{
		human_playback.SetActive(false);
		human.SetActive(true);

		GameObject[] angleators = GameObject.FindGameObjectsWithTag("Angleator");
		foreach (GameObject g in angleators){
			g.GetComponent<Angleator>().enfluxhumanoid = human;
			g.GetComponent<Angleator>().dropdown_handle();
		}
		CameraControl C = GetComponent<CameraControl>();
		for (int c = 0; c < C.cameras.Length; c++){
			C.cameras[c].GetComponent<AnchorToTransform>().Anchor = human_playback.transform.Find("Armature/hip");
		}
		isPlaying = false;
	}

	public void CheckStatus(){
		StartCoroutine(Check_Status());
	}

	IEnumerator Check_Status()
	{
		//no subscription
		bool pur = true;//purchaser.CheckPurchase("monthlysubscribtion");
		if(!pur){
			//check host for ID
			var form = new WWWForm();
 
			form.AddField("id", SystemInfo.deviceUniqueIdentifier);
			
			WWW download = new WWW("http://unalienable-inlet.000webhostapp.com/Angleator/checkid.php", form);
			
			yield return  download;
			
			Debug.Log(download.text);

			if(download.error != null){
				ConnectionError("Connection Error\n"+download.error.ToString());
				yield break;
			}
			if(download.text == "")
			{
				ConnectionError("Return Null");
				yield break;
			}

			if(download.text != "addition")
			{
				string[] dates = download.text.Split(";"[0]);
				string[] date0 = dates[0].Split(" "[0]); 
				string[] date1 = dates[1].Split(" "[0]); 

				System.DateTime departure = new System.DateTime(int.Parse(date0[0]), int.Parse(date0[1]), int.Parse(date0[2]));
				System.DateTime arrival = new System.DateTime(int.Parse(date1[0]), int.Parse(date1[1]), int.Parse(date1[2]));
				System.TimeSpan travelTime = arrival - departure;  
				Debug.Log("travelTime: " + travelTime ); 
				if(travelTime.Days < 7){
					//trial still active
					Debug.Log("Trial Active");
					yield break;
				}else{
					//trial expired
					Debug.Log("Trial Expired");
					trialError.SetActive(true);
					Expired();
					yield break;
				}
			}else{
				Debug.Log("ID added to list");
			}
		}else{
			//ok
		}
	}

	public void ConnectionError(string msg)
	{
		//Expired();
		connError.transform.GetChild(0).GetComponent<Text>().text = msg;
		connError.SetActive(true);
	}

	public void Expired()
	{
		angleators = GameObject.FindGameObjectsWithTag("Angleator");
		angleatorsGUI = GameObject.FindGameObjectsWithTag("AngleatorGUI");
		foreach(GameObject g in angleators){
			g.SetActive(false);
		}
		foreach(GameObject g in angleatorsGUI){
			g.SetActive(false);
		}
	}
	public void SubBought()
	{
		if(angleators!=null){
			foreach(GameObject g in angleators){
				g.SetActive(true);
			}
		}
		if(angleatorsGUI!=null){
			foreach(GameObject g in angleatorsGUI){
				g.SetActive(true);
			}
		}
	}
	public void ReConnect()
	{
		connError.SetActive(false);
		SubBought();
		CheckStatus();
	}

	public void Unlock()
	{
		//purchaser.BuyProductID("monthlysubscribtion");
		trialError.SetActive(false);
	}

}