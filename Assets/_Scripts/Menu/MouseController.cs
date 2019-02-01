using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public delegate void OnMouseEnterdeleg(GameObject button);
    public delegate void OnMouseExitdeleg(GameObject button);
    public delegate void OnMouseClickdeleg(GameObject button);

    public static event OnMouseClickdeleg onMouseClick;
    public static event OnMouseEnterdeleg onMouseEnter;
    public static event OnMouseExitdeleg onMouseExit;

    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (onMouseEnter != null)
            onMouseEnter(this.gameObject);
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (onMouseExit != null)
            onMouseExit(this.gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {   
        if(onMouseClick != null) {
            onMouseClick(this.gameObject);
        }
    }

}

