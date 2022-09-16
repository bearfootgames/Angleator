using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class drag : MonoBehaviour {

	GameObject canvas;
	GameObject guiobj;
	private RectTransform m_DraggingPlane;
	bool dragOnSurfaces = false;

	void Start()
	{
		if(!guiobj){
			guiobj = gameObject;
		}
		if(!canvas){
			if(!GameObject.Find("Canvas")){Debug.Log("'Canvas' is MISSING!");return;}
			canvas = GameObject.Find("Canvas");
		}
		EventTrigger trigger = guiobj.GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.Drag;
		entry.callback.AddListener((data) => { OnBeginDrag((PointerEventData)data); });
		trigger.triggers.Add(entry);
	}
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
}
