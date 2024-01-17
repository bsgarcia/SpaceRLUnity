using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Selectable))]
public class ButtonSelection : MonoBehaviour, IPointerEnterHandler, IDeselectHandler, IPointerExitHandler
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnPointerEnter(null);
            this.GetComponent<Button>().onClick.Invoke();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    { 
        this.GetComponent<Text>().color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        this.GetComponent<Text>().color = Color.white;

    }

    public void OnDeselect(BaseEventData eventData)
    {
        this.GetComponent<Selectable>().OnPointerExit(null);
    }
}