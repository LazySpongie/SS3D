
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SS3D.Core.Behaviours;

public class SVImageControl : Actor, IDragHandler, IPointerClickHandler
{
    
    [SerializeField] private ColorPickerControl _cc;

    [SerializeField] private RectTransform _picker;

    // private RectTransform _rectTransform;

    // protected override void OnAwake()
    // {
    //     base.OnAwake();

    //     _rectTransform = GetComponent<RectTransform>();
    // }

    private void UpdatePicker(PointerEventData e)
    {
        // get position in local space to rect
        Vector3 pos = RectTransform.InverseTransformPoint(e.position);

        // the size of the rect
        float deltaX = RectTransform.sizeDelta.x;
        float deltaY = RectTransform.sizeDelta.y;

        // clamp the pointer position within the rect
        pos.x = Mathf.Clamp(pos.x, 0, deltaX);
        pos.y = Mathf.Clamp(pos.y, 0, deltaY);

        // float x = pos.x + deltaX;
        // float y = pos.y + deltaY;

        _picker.localPosition = pos;

        // normalise x and y from rect size
        float normX = pos.x / deltaX;
        float normY = pos.y / deltaY;
        _cc.SetSV(normX, normY);
    }

    public void SetPointerPosition(float s, float v)
    {
        _picker.localPosition = new Vector3(s * RectTransform.sizeDelta.x, v * RectTransform.sizeDelta.y, 1);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        UpdatePicker(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdatePicker(eventData);
    }
}
