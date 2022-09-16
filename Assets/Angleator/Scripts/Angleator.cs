using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Angleator : MonoBehaviour {

	public GameObject enfluxhumanoid;
	public GameObject angleatorPreFab;
	public Transform transformA;
	public Transform transformB;
	public Transform transformC;
	public GameObject nullpoint;
	public GameObject gui_object;
	public GameObject cam;
	public CameraControl cameracontrol;

	Image colorbutton;
	Text angleText;
	Dropdown dropdown;
	Toggle toggle;
	LineRenderer linerenderer;
	GameObject guiobj;
	GameObject canvas;
	Text repsText;
	//Text targText;
	GameObject np;

	Vector3 pointA;
	Vector3 pointB;
	Vector3 pointC;

	public float angle;
	public bool Use_GUI;

	int Reps = 0;
	int TargetAngle = 90;

	string SelectedLocation;

	void Start()
	{
		gameObject.name = "Angleator";
		//check for stuff
		cameracontrol = GameObject.Find("Angleator Manager").GetComponent<CameraControl>();
		if(!GameObject.Find("EnfluxHumanoid")){
			enfluxhumanoid = GameObject.Find("EnfluxHumanoid_Playback");
		}else{
			enfluxhumanoid = GameObject.Find("EnfluxHumanoid");
		}
			
		if(!GetComponent<LineRenderer>()){Debug.Log("LINE RENDERER is MISSING!");return;}
		linerenderer = GetComponent<LineRenderer>();
		if(!nullpoint){Debug.Log("nullpoint PreFab is MISSING!");return;}
		if(Use_GUI)
		{	
			if(!gui_object){Debug.Log("AngleatorPanel is MISSING!");return;}
			if(!canvas){
				if(!GameObject.Find("Canvas")){Debug.Log("'Canvas' is MISSING!");return;}
				canvas = GameObject.Find("Canvas");
			}
			
			guiobj = Instantiate(gui_object);
			guiobj.transform.SetParent(canvas.transform,false);
			guiobj.transform.name = "AngleatorPanel";
			guiobj.transform.SetSiblingIndex(0);
			
			if(!angleText){angleText = guiobj.transform.Find("Text_Angle").GetComponent<Text>();}
			
			if(!dropdown){
				dropdown = guiobj.transform.Find("Dropdown").GetComponent<Dropdown>();
				dropdown.onValueChanged.AddListener(delegate {
                	dropdown_handle();
            	});
			}
			
			if(!toggle){
				toggle = guiobj.transform.Find("Toggle").GetComponent<Toggle>();
				toggle.onValueChanged.AddListener(delegate {
                	toggle_handle();
            	});
			}
			
			if(!cam){
				if(!GameObject.Find("MainCamera")){Debug.Log("'MainCamera' MISSING!");return;}
				cam = GameObject.Find("MainCamera");
			}
			
			EventTrigger trigger = guiobj.GetComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Drag;
			entry.callback.AddListener((data) => { OnBeginDrag((PointerEventData)data); });
			trigger.triggers.Add(entry);
			
			guiobj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate {Add_Instance();});

			colorbutton = guiobj.transform.Find("Button_Color").GetComponent<Image>();
			guiobj.transform.Find("Button_Color").GetComponent<Button>().onClick.AddListener(delegate {Change_Color();});

			guiobj.transform.Find("Button_Close").GetComponent<Button>().onClick.AddListener(delegate {Close_Angleator();});

			float w = canvas.GetComponent<RectTransform>().rect.x/2 - 200;
			float h = canvas.GetComponent<RectTransform>().rect.y/2 - 90;

			guiobj.GetComponent<RectTransform>().localPosition = new Vector3(Random.Range(-w,w),Random.Range(-h,h),0);

			guiobj.transform.Find("Button_Reset").GetComponent<Button>().onClick.AddListener(delegate {ResetReps();});
			guiobj.transform.Find("InputField").GetComponent<InputField>().onEndEdit.AddListener(delegate {
				ChangeTargetAngle(guiobj.transform.Find("InputField").GetComponent<InputField>());});
			
			repsText = guiobj.transform.Find("Text_Reps").GetComponent<Text>();
			//targText = guiobj.transform.Find("Text_Target").GetComponent<Text>();
		}
		
	}
	

	bool reset = true;
	void Update ()
	{
		if(transformA && transformB && transformC)
		{
			pointA = transformA.position;
			pointB = transformB.position;
			pointC = transformC.position;

			angle = Vector3.Angle(pointA - pointB,pointC - pointB);
			angleText.text = Mathf.Round(angle)+"°";

			//position line for camera
			if(SelectedLocation == "Spine" || SelectedLocation == "Hip" || SelectedLocation == "Lower Spine"){
				if(cameracontrol.i == 0){
					pointA.z -= .2f;
					pointB.z -= .2f;
					pointC.z -= .2f;
				}
				if(cameracontrol.i == 1){
					pointA.x += .2f;
					pointB.x += .2f;
					pointC.x += .2f;
				}
				if(cameracontrol.i == 2){
					pointA.z += .2f;
					pointB.z += .2f;
					pointC.z += .2f;
				}
				if(cameracontrol.i == 3){
					pointA.x -= .2f;
					pointB.x -= .2f;
					pointC.x -= .2f;
				}
			}else{
				if(cameracontrol.i == 0){
					pointA.z -= .12f;
					pointB.z -= .12f;
					pointC.z -= .12f;
				}
				if(cameracontrol.i == 1){
					pointA.x += .12f;
					pointB.x += .12f;
					pointC.x += .12f;
				}
				if(cameracontrol.i == 2){
					pointA.z += .12f;
					pointB.z += .12f;
					pointC.z += .12f;
				}
				if(cameracontrol.i == 3){
					pointA.x -= .12f;
					pointB.x -= .12f;
					pointC.x -= .12f;
				}
			}

			linerenderer.SetPosition(0, pointA);
			linerenderer.SetPosition(1, pointB);
			linerenderer.SetPosition(2, pointC);
			
			if(SelectedLocation == ""){}

		}else{
			if(!Use_GUI){return;}
			angleText.text = "Select A Joint";
		}
		if( (Mathf.Round(angle) < TargetAngle -40 || Mathf.Round(angle) > TargetAngle +40) && !reset){reset = true;}
		if(Vector2Int.Distance(new Vector2Int(Mathf.RoundToInt(angle),0),new Vector2Int(TargetAngle,0)) < 2 && reset){
			Reps += 1;
			repsText.text = "Reps: "+Reps.ToString();
			reset = false;
		}
	}

	public Color[] linecolor;
	int ci = 0;
	void Change_Color()
	{
		ci++;
		if(ci>=linecolor.Length){ci=0;}
		LineRenderer l = GetComponent<LineRenderer>();
		l.material.color = linecolor[ci];
		colorbutton.color = linecolor[ci];
	}

	void Close_Angleator()
	{
		GameObject[] obj = GameObject.FindGameObjectsWithTag("Angleator");
		if(obj.Length > 1)
		{
			Destroy(guiobj);
			Destroy(gameObject);
		}
	}

	void ChangeTargetAngle(InputField targ)
	{
		TargetAngle = int.Parse(targ.text);
		//targText.text = "Target Angle "+TargetAngle;
	}

	void ResetReps()
	{
		Reps = 0;
		repsText.text = "Reps: "+Reps.ToString();
	}

	public void Add_Instance()
	{
		Instantiate(angleatorPreFab);
	}

	private RectTransform m_DraggingPlane;
	bool dragOnSurfaces = false;
	public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null)
            return;


        if (dragOnSurfaces)
            m_DraggingPlane = transform as RectTransform;
        else
            m_DraggingPlane = canvas.transform as RectTransform;

        SetDraggedPosition(eventData);
    }

	void SetDraggedPosition(PointerEventData data)
	{
		//Create a ray going from the camera through the mouse position
       	//Ray ray = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        //Calculate the distance between the Camera and the GameObject, and go this distance along the ray
       	//Vector3 rayPoint = ray.GetPoint(Vector3.Distance(guiobj.transform.position, cam.transform.position));
        //Move the GameObject when you drag it
		//RectTransform rt = guiobj.transform.GetComponent<RectTransform>().localPosition = new Vector3(rayPoint.x,rayPoint.y,0);
		
		if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
            m_DraggingPlane = data.pointerEnter.transform as RectTransform;
		
		RectTransform rt = guiobj.transform.GetComponent<RectTransform>();
		Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
        {
            rt.position = globalMousePos;
            rt.rotation = m_DraggingPlane.rotation;
        }
		
	}

	public void toggle_handle()
	{
		if(toggle.isOn){
			linerenderer.gameObject.SetActive(true);
		}else{
			linerenderer.gameObject.SetActive(false);
		}
	}

	string[] joints = new string[]{"Armature/hip",//0
	"Armature/hip/abdomen/chest/lCollar/lShldr",//1
	"Armature/hip/abdomen/chest/lCollar/lShldr/lForeArm",//2
	"Armature/hip/abdomen/chest/lCollar/lShldr/lForeArm/lHand",//3
	"Armature/hip/abdomen/chest/rCollar/rShldr",//4
	"Armature/hip/abdomen/chest/rCollar/rShldr/rForeArm",//5
	"Armature/hip/abdomen/chest/rCollar/rShldr/rForeArm/rHand",//6
	"Armature/hip/lThigh",//7
	"Armature/hip/lThigh/lShin",//8
	"Armature/hip/lThigh/lShin/lFoot",//9
	"Armature/hip/lThigh/lShin/lFoot/lHeel",//10
	"Armature/hip/lThigh/lShin/lFoot/lToe",//11
	"Armature/hip/lThigh/lShin/lFoot/lToe/lToe_End",//12
	"Armature/hip/rThigh",//13
	"Armature/hip/rThigh/rShin",//14
	"Armature/hip/rThigh/rShin/rFoot",//15
	"Armature/hip/rThigh/rShin/rFoot/rHeel",//16
	"Armature/hip/rThigh/rShin/rFoot/rToe",//17
	"Armature/hip/rThigh/rShin/rFoot/rToe/rToe_End",//18
	"Armature/hip/abdomen",//19
	"Armature/hip/abdomen/chest",//20
	"Armature/hip/abdomen/chest/neck",//21
	"Armature/hip/abdomen/chest/neck/head",//22
	"Armature/hip/abdomen/chest/lCollar",//23
	"Armature/hip/abdomen/chest/rCollar",//24
	"Armature"//25
	};

	public void dropdown_handle()
	{
		if(Use_GUI == false){return;}
		if(dropdown.value == 0){
			angleText.text = "No joint selected";
			Debug.Log("Select a joint "+dropdown.value);
			linerenderer.gameObject.SetActive(false);
			return;
		}

		if(toggle.isOn){linerenderer.gameObject.SetActive(true);}

		GameObject LocationPickerLabel = dropdown.transform.GetChild(0).gameObject;
		
		SelectedLocation = LocationPickerLabel.GetComponent<Text>().text;

		if(np){Destroy(np);}

		//Left Ankle
		if(SelectedLocation == "Left Ankle")
		{
			transformA = enfluxhumanoid.transform.Find(joints[8]);
			transformB = enfluxhumanoid.transform.Find(joints[9]);
			transformC =  enfluxhumanoid.transform.Find(joints[11]);
		}
		//Right Ankle
		if(SelectedLocation == "Right Ankle")
		{
			transformA = enfluxhumanoid.transform.Find(joints[14]);
			transformB = enfluxhumanoid.transform.Find(joints[15]);
			transformC =  enfluxhumanoid.transform.Find(joints[17]);
		}
		//Hip
		if(SelectedLocation == "Hip")
		{
			transformA = enfluxhumanoid.transform.Find(joints[20]);
			transformB = enfluxhumanoid.transform.Find(joints[0]);
			np = Instantiate(nullpoint,Vector3.zero,Quaternion.identity);
			np.GetComponent<center>().A = enfluxhumanoid.transform.Find(joints[14]);
			np.GetComponent<center>().B = enfluxhumanoid.transform.Find(joints[8]);
			transformC = np.transform;
		}
		//Neck
		if(SelectedLocation == "Neck")
		{
			transformA = enfluxhumanoid.transform.Find(joints[22]);
			transformB = enfluxhumanoid.transform.Find(joints[21]);
			transformC =  enfluxhumanoid.transform.Find(joints[20]);
		}
		//Lower Spine
		if(SelectedLocation == "Lower Spine")
		{
			transformA = enfluxhumanoid.transform.Find(joints[20]);
			transformB = enfluxhumanoid.transform.Find(joints[19]);
			transformC =  enfluxhumanoid.transform.Find(joints[0]);
		}
		//Spine
		if(SelectedLocation == "Spine")
		{
			transformA = enfluxhumanoid.transform.Find(joints[21]);
			transformB = enfluxhumanoid.transform.Find(joints[20]);
			transformC =  enfluxhumanoid.transform.Find(joints[0]);
		}
		//L_Knee
		if(SelectedLocation == "Left Knee")
		{
			transformA = enfluxhumanoid.transform.Find(joints[7]);
			transformB = enfluxhumanoid.transform.Find(joints[8]);
			transformC =  enfluxhumanoid.transform.Find(joints[9]);
		}
		//R_Knee
		if(SelectedLocation == "Right Knee")
		{
			transformA = enfluxhumanoid.transform.Find(joints[13]);
			transformB = enfluxhumanoid.transform.Find(joints[14]);
			transformC =  enfluxhumanoid.transform.Find(joints[15]);
		}
		//L_Shoulder
		if(SelectedLocation == "Left Shoulder")
		{
			transformA = enfluxhumanoid.transform.Find(joints[2]);
			transformB = enfluxhumanoid.transform.Find(joints[1]);
			np = Instantiate(nullpoint,Vector3.zero,Quaternion.identity);
			np.transform.SetParent(enfluxhumanoid.transform.Find(joints[19]));
			np.transform.position = new Vector3(transformB.position.x,transformB.position.y-0.5f,transformB.position.z);
			transformC = np.transform;
			TargetAngle = 90;
		}
		//R_Shoulder
		if(SelectedLocation == "Right Shoulder")
		{
			transformA = enfluxhumanoid.transform.Find(joints[5]);
			transformB = enfluxhumanoid.transform.Find(joints[4]);
			np = Instantiate(nullpoint,Vector3.zero,Quaternion.identity);
			np.transform.SetParent(enfluxhumanoid.transform.Find(joints[19]));
			np.transform.position = new Vector3(transformB.position.x,transformB.position.y-0.5f,transformB.position.z);
			transformC = np.transform;
			TargetAngle = 90;
		}
		//L_Elbow
		if(SelectedLocation == "Left Elbow")
		{
			transformA = enfluxhumanoid.transform.Find(joints[1]);
			transformB = enfluxhumanoid.transform.Find(joints[2]);
			transformC =  enfluxhumanoid.transform.Find(joints[3]);
			TargetAngle = 50;
		}
		//R_Elbow
		if(SelectedLocation == "Right Elbow")
		{
			transformA = enfluxhumanoid.transform.Find(joints[4]);
			transformB = enfluxhumanoid.transform.Find(joints[5]);
			transformC =  enfluxhumanoid.transform.Find(joints[6]);
			TargetAngle = 50;
		}
		
	}
}
