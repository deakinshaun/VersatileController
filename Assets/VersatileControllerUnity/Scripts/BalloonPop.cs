using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonPop : MonoBehaviour
{
  [Tooltip ("A cylindrical beam object attached to the controller.")]
  public GameObject laserBeam;
  [Tooltip ("An adjustment factor for how fast objects blow up.")]
  public float inflationRate = 1.0f;
  [Tooltip ("A sound effect played during inflation.")]
  public AudioSource hiss;
  [Tooltip ("A sound effect played when the object is destroyed.")]
  public AudioSource pop;
  
  // Balloon dropping code.
  [Tooltip ("An object (with a rigidbody) that will drop from the start point.")]
  public GameObject objectTemplate;
  [Tooltip ("The source point from which objects will be dropped.")]  
  public Vector3 startPoint;
  [Tooltip ("Total number of objects that will be created.")]  
  public int numberOfObjects = 30;
  [Tooltip ("Magnitude of initial velocity (direction is random, so sprays out).")]    
  public float initialSpeed = 1.0f;
  [Tooltip ("Time gap between creating new objects. Recommend enough time for previous objects to fall out of the way.")]  
  public float timeInterval = 0.3f;
  // Tracks time between drops.
  private float currentTime = 0.0f;
  
  // Controller management.  
  // This application associates each controller with a trigger, and a laser beam. This will
  // differ for other applications that use the controller.
  private class ControllerState
  {
    public bool trigger;
    public GameObject laserBeam;
  }
  private Vector3 controllerStartingPoint = new Vector3 (-0.5f, 1.0f, 0.0f);
  private Dictionary <VersatileControllerVirtual, ControllerState> controllers;
  
  void Start ()
  {
    controllers = new Dictionary <VersatileControllerVirtual, ControllerState> ();
    VersatileControllerVirtual.subscribeNewControllers (addController);
  }
  
  public void addController (GameObject controller)
  {
    Debug.Log ("Got controller: " + controller);
    
    // A pattern to add a controller, parented to its absolute position in space (the hand that holds it).
    // In this pattern, multiple controllers are supported by having multiple hands at different positions.
    GameObject hand = new GameObject ();
    hand.name = "Hand";
    hand.transform.position = controllerStartingPoint;
    controller.transform.SetParent (hand.transform);
    controllerStartingPoint += new Vector3 (0.1f, 0.0f, 0.0f);
    
    // Subscribe to button events.
    VersatileControllerVirtual ctl = controller.GetComponent <VersatileControllerVirtual> ();
    if (ctl != null)
    {
      ctl.subscribeButtonDown ("Trigger", clickedDown);
      ctl.subscribeButtonUp ("Trigger", clickedUp);
      ControllerState state = new ControllerState ();
      state.trigger = false;
      state.laserBeam = Instantiate (laserBeam);
      state.laserBeam.transform.SetParent (ctl.gameObject.transform, false);
      controllers[ctl] = state;
    }
  }

  // Update state of trigger, based on information from the controller.
  public void clickedDown (string button, VersatileControllerVirtual ctl)
  {
    Debug.Log ("Trigger down");
    if (controllers.ContainsKey (ctl))
    {
      controllers[ctl].trigger = true;
    }
  }
  public void clickedUp (string button, VersatileControllerVirtual ctl)
  {
    Debug.Log ("Trigger up");
    if (controllers.ContainsKey (ctl))
    {
      controllers[ctl].trigger = false;
    }
  }
  
  void Update()
  {
    // Object dropping.
    currentTime += Time.deltaTime;
    
    if ((currentTime > timeInterval) && (numberOfObjects > 0))
    {
      currentTime = 0.0f;
      numberOfObjects--;
      GameObject g = Instantiate (objectTemplate);
      g.transform.position = startPoint;
      g.GetComponent <Rigidbody> ().velocity = Random.onUnitSphere * initialSpeed;
      g.GetComponent <MeshRenderer> ().material.color = Random.ColorHSV (0, 1, 0.5f, 1, 0.5f, 1);
    }
    
    List <VersatileControllerVirtual> failedControllers = new List <VersatileControllerVirtual> ();
    
    // Check for ballons that are being pointed to, switch laser on and off, and inflate.
    // decay hiss so it stops if no button is pressed.
    hiss.volume *= 0.9f;
    foreach (KeyValuePair <VersatileControllerVirtual, ControllerState> entry in controllers)
    {
      VersatileControllerVirtual ctl = entry.Key;
      
      if (ctl != null) // in case controller has been destroyed
      {
        bool triggerPressed = entry.Value.trigger;
        GameObject laserBeam = entry.Value.laserBeam;
        
        // make the laser beam active if the trigger is pressed.
        laserBeam.SetActive (triggerPressed);
        if (triggerPressed)
        {
          // Raycast, inflate, explode.
          RaycastHit hit;
          if ((Physics.Raycast(ctl.gameObject.transform.position, ctl.gameObject.transform.forward, out hit, Mathf.Infinity)) &&
            (hit.collider.gameObject.tag == "Inflatable"))
          {
            // Inflate the object by manipulating scale.
            hit.collider.gameObject.transform.localScale *= 1.0f + (inflationRate * Time.deltaTime);
            // Play the inflation sound.
            if (hiss != null) { hiss.volume = 1.0f; if (!hiss.isPlaying) hiss.Play (); }
            // Pop the object if it gets too big.
            if (hit.collider.gameObject.transform.localScale.magnitude > 3)
            {
              Destroy (hit.collider.gameObject);
              if (pop != null) pop.Play ();
            }
          }
        }
      }
      else
      {
        // remove controller record.
        failedControllers.Add (ctl);
      }
    }
    
    // Remove any failed controllers.
    foreach (VersatileControllerVirtual ctl in failedControllers)
    {
      controllers.Remove (ctl);
    }
  }
}
