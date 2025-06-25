using UnityEngine;
using UnityEngine.EventSystems;

public class DesktopTracking : MonoBehaviour
{
    public VersatileControllerPhysical controller;
    
    private float moveSpeed = 1.0f;
    private float turnSpeed = 100.0f;
    
    private Vector3 controllerPosition;
    private Quaternion controllerRotation = Quaternion.identity;
    
    private Vector2 getTouchPosition (BaseEventData data)
    {  
        RectTransform rt = ((PointerEventData) data).pointerDrag.transform.parent.GetComponent <RectTransform> ();
        Rect bounds = rt.rect;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt,
            ((PointerEventData) data).position,
                                                                ((PointerEventData) data).pressEventCamera,
                                                                out localPoint
        );
        string touch = ((PointerEventData) data).pointerDrag.name;
        Vector2 value = new Vector2 (Mathf.InverseLerp(bounds.xMin, bounds.xMax, localPoint.x) * 2.0f - 1.0f,
                                     Mathf.InverseLerp(bounds.yMin, bounds.yMax, localPoint.y) * 2.0f - 1.0f);
        
        return value;
    }
    
    public void updatePosition (BaseEventData data)
    {
        Vector2 pos = getTouchPosition (data);
        
        Vector3 forward = controller.getControllerOrientation () * Vector3.forward;
        Vector3 right = controller.getControllerOrientation () * Vector3.right;
        controllerPosition += Time.deltaTime * moveSpeed * (pos.y * forward + pos.x * right); 
        controller.setOverridePosition (controllerPosition);
    }
    
    public void updateRotation (BaseEventData data)
    {
        Vector2 pos = getTouchPosition (data);
        
        Vector3 right = controller.getControllerOrientation () * Vector3.right;
        controllerRotation = Quaternion.AngleAxis (Time.deltaTime * turnSpeed * pos.y, right) * controllerRotation;
        Vector3 up = controller.getControllerOrientation () * Vector3.up;
        controllerRotation = Quaternion.AngleAxis (Time.deltaTime * turnSpeed * pos.x, up) * controllerRotation;
        controller.setOverrideRotation (controllerRotation);
    }
    
}
