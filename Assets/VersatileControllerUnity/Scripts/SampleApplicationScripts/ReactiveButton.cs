using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveButton : MonoBehaviour
{
    [Tooltip("The button part, that will change colour and move to respond to hover and click")]
    public GameObject buttonTop;
    [Tooltip("The material for the button when unused")]
    public Material normalMaterial;
    [Tooltip("The material for the button when hovered")]
    public Material hoverMaterial;
    [Tooltip("The material for the button when clicked")]
    public Material clickMaterial;
    [Tooltip("The position of the button when usused or hovered")]
    public Vector3 upPosition;
    [Tooltip("The position of the button when clicked")]
    public Vector3 downPosition;
    [Tooltip("A sound effect played on button down.")]
    public AudioSource clickDown;
    [Tooltip("A sound effect played on button release.")]
    public AudioSource clickUp;


    private List<VersatileControllerVirtual> controllers;

    void Start()
    {
        controllers = new List<VersatileControllerVirtual>();
        VersatileControllerVirtual.subscribeNewControllers(addController);
    }

    public void addController(GameObject controller)
    {
        Debug.Log("Got controller: " + controller);

        // Assumes the default position of the controller is handled elsewhere.

        // Subscribe to button events.
        VersatileControllerVirtual ctl = controller.GetComponent<VersatileControllerVirtual>();
        if (ctl != null)
        {
            ctl.subscribeButtonDown("Trigger", clickedDown);
            ctl.subscribeButtonUp("Trigger", clickedUp);
            controllers.Add(ctl);
        }
    }

    private bool pressState = false; // state of the trigger on the controller.
    private bool buttonClicked = false; // indicates if the button is clicked (i.e., pointed at, and trigger held).
    private bool prevButtonClicked = false; // used to detect change of state.

    // Update state of trigger, based on information from the controller.
    public void clickedDown(string button, VersatileControllerVirtual ctl)
    {
        Debug.Log("Trigger down");
        pressState = true;
    }
    public void clickedUp(string button, VersatileControllerVirtual ctl)
    {
        Debug.Log("Trigger up");
        pressState = false;
    }

    void Update()
    {
        List<VersatileControllerVirtual> failedControllers = new List<VersatileControllerVirtual>();

        buttonTop.GetComponent<MeshRenderer>().material = normalMaterial;
        // Check if the button top is being pointed at.
        foreach (VersatileControllerVirtual ctl in controllers)
        {
            if (ctl != null) // in case controller has been destroyed
            {
                // Raycast
                RaycastHit hit;
                if ((Physics.Raycast(ctl.gameObject.transform.position, ctl.gameObject.transform.forward, out hit, Mathf.Infinity)) &&
                    (hit.collider.gameObject == buttonTop))
                {
                    if (pressState)
                    {
                        buttonClicked = true;
                    }
                    else
                    {
                        buttonTop.GetComponent<MeshRenderer>().material = hoverMaterial;
                    }
                }
            }
            else
            {
                // remove controller record.
                failedControllers.Add(ctl);
            }

            if (pressState && buttonClicked)
            {
                // leave the button clicked, as long as the trigger is pressed, and it was previously clicked.
                buttonTop.transform.localPosition = downPosition;
                buttonTop.GetComponent<MeshRenderer>().material = clickMaterial;
                if (buttonClicked && !prevButtonClicked)
                {
                    onButton(); // signal the button has now been pressed.
                    if (clickDown != null) clickDown.Play();
                }
                prevButtonClicked = buttonClicked;
            }
            else
            {
                prevButtonClicked = buttonClicked;
                if (!pressState)
                {
                    if (buttonClicked && (clickUp != null)) clickUp.Play();
                    buttonClicked = false; // unclick the button when the trigger is released.
                }
                if (buttonClicked)
                {
                    buttonTop.transform.localPosition = downPosition;
                    buttonTop.GetComponent<MeshRenderer>().material = clickMaterial;
                }
                else
                {
                    buttonTop.transform.localPosition = upPosition;
                }
            }
        }

        // Remove any failed controllers.
        foreach (VersatileControllerVirtual ctl in failedControllers)
        {
            controllers.Remove(ctl);
        }
    }

    // Override this to do things when the button is pressed.
    public void onButton()
    {
        Debug.Log("Button press");
    }
}
