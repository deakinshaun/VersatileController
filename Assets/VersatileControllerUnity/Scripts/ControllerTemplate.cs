using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// You don't need this. It is included so the values received can 
// be demonstrated by writing them to a text element.
using TMPro;

// This is intended to be a template that you can use to include 
// controller access in your application. You will need:
//
// 1. The VersatileControllerManager: this manages the connection
//    to the controllers. Copy this without any changes required.
//
// 2. A version of this controller template attached to an object
//    in the scene.
//    Modify as indicated below, to select what information you
//    want to receive from the controller. What you do with the 
//    information is specific to the application you are building.
//    This template only goes as far as ensuring you can get the
//    values that you need from the controller.
public class ControllerTemplate : MonoBehaviour
{
  // You don't need this. This is just a text element so you
  // can see what values are received by the controller.
  public TextMeshProUGUI debugText;

  // You don't need this either. This is just a text element so you
  // can monitor the state of buttons reported by the controller..
  public TextMeshProUGUI stateMonitorText;
  
  void Start()
  {
    // You need this. This ensures that when a new controller connects, the addController
    // method is called. You can then subscribe to specific information from that controller
    // in the addController method.
    VersatileControllerVirtual.subscribeNewControllers (addController);
  }
  
  // Called whenever a controller connects. If you use more than one controller, you
  // will need to keep track of each of them. To set their starting position, create
  // a new empty object (in code) and make the controller a child of this. Moving
  // the empty will then determine the position of that particular controller. 
  public void addController (GameObject controller)
  {
    // Subscribe to button events.
    VersatileControllerVirtual ctl = controller.GetComponent <VersatileControllerVirtual> ();
    if (ctl != null)
    {
      // You don't have to subscribe to all buttons. Just select the ones
      // you want, and decide whether you want to know about their up or down
      // events. Custom controller skins may add further buttons.
      
      // You don't have to use only one method for all buttons. In 
      // most cases, it makes sense to have each button trigger a 
      // different method.
      ctl.subscribeButtonDown ("Trigger", clickedDown);
      ctl.subscribeButtonUp ("Trigger", clickedUp);
      ctl.subscribeButtonDown ("Grip", clickedDown);
      ctl.subscribeButtonUp ("Grip", clickedUp);
      ctl.subscribeButtonDown ("A", clickedDown);
      ctl.subscribeButtonUp ("A", clickedUp);
      ctl.subscribeButtonDown ("B", clickedDown);
      ctl.subscribeButtonUp ("B", clickedUp);
      ctl.subscribeButtonDown ("X", clickedDown);
      ctl.subscribeButtonUp ("X", clickedUp);
      ctl.subscribeButtonDown ("Y", clickedDown);
      ctl.subscribeButtonUp ("Y", clickedUp);

      // Select which of these you need to know about. These
      // are specific to the sliders associated with the hand skin.
      // Additional skins may add further sliders.
      ctl.subscribeSlider ("Thumb", sliderChanged);
      ctl.subscribeSlider ("IndexFinger", sliderChanged);
      ctl.subscribeSlider ("MiddleFinger", sliderChanged);
      ctl.subscribeSlider ("RingFinger", sliderChanged);
      ctl.subscribeSlider ("LittleFinger", sliderChanged);
      
      // You can subscribe to all events, by leaving the button/slider name as null.
      ctl.subscribeButtonDown (null, anyButtonDown);

      // Don't do this, unless you want to poll controllers. This means we have to
      // keep a list of known controllers.
      knownControllers.Add (ctl);
    }
  }
  
  // Use a method similar to this, to get informed whenever a button is pressed.
  public void clickedDown (string button, VersatileControllerVirtual ctl)
  {
    // You don't need the code in this method. This just displays information
    // in a text element. Rather do something useful in the context of your
    // application with this information.
    if (debugText != null)
    {
      debugText.text = "Button " + button + " down";
    }
  }
  public void clickedUp (string button, VersatileControllerVirtual ctl)
  {
    // You don't need the code in this method. This just displays information
    // in a text element. Rather do something useful in the context of your
    // application with this information.
    if (debugText != null)
    {
      debugText.text = "Button " + button + " up";
    }
  }  
  public void sliderChanged (string name, float value, VersatileControllerVirtual ctl)
  {
    // You don't need the code in this method. This just displays information
    // in a text element. Rather do something useful in the context of your
    // application with this information.
    if (debugText != null)
    {
      debugText.text = "Slider " + name + " has value " + value;
    }    
  }
  public void anyButtonDown (string button, VersatileControllerVirtual ctl)
  {
    Debug.Log ("Just to show this works, button " + button + " is down");
  }
  
  // Keep track of controllers. Only for polling state. If you use event handlers,
  // the controller details will be provided to you when the event occurs.
  List <VersatileControllerVirtual> knownControllers = new List <VersatileControllerVirtual> ();
  
  // Ideally you don't need to do anything in the Update method. However, if
  // you want to check on the state of particular buttons, you can do this by
  // retrieving state from the controllers. Use of this pattern should be 
  // chosen after consideration: if you're doing this just to determine when
  // a button is pressed, use the event handlers instead. This approach requires
  // that you keep track of which controller you want to query.
  void Update ()
  {
    // Report on the state of some buttons, in each frame.
    stateMonitorText.text = "";
    // flag to check if some controllers have been removed and need to be deleted.
    bool controllersRemoved = false; 
    foreach (VersatileControllerVirtual ctl in knownControllers)
    {
      if (ctl != null)
      {
        stateMonitorText.text += ctl.gameObject.name + ":" + 
                                 (ctl.getButtonState ("A") ? "A+" : "A-") + " " +
                                 (ctl.getButtonState ("B") ? "B+" : "B-") + " " +
                                 (ctl.getButtonState ("X") ? "X+" : "X-") + " " +
                                 (ctl.getButtonState ("Y") ? "Y+" : "Y-") + " " +
                                 "\n";
      }
      else
      {
        controllersRemoved = true;
      }
    }
    if (controllersRemoved) 
    // Safely traverse the controller list and remove controllers that no longer exist.
    {
      knownControllers.RemoveAll (ctl => ctl == null);
    }
  }
}
