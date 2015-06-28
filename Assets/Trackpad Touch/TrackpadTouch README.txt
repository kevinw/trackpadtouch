===================
Trackpad Touch OS X
===================

Version 1.0

http://kevinw.github.io/trackpadtouch

by Kevin Watters (kevinwatters@gmail.com)

-----
ABOUT
-----

This plugin reads multitouch input from your trackpad, giving it to your Unity
game with an interface matching Input.touches.

This allows you to test your game's multitouch functionality in the editor,
without having to use Unity Remote or building to the device.

---------------------------------
IMPORTANT - DISABLE OS X GESTURES
---------------------------------

This plugin works best if you disable Mission Control and Notification Center
gestures. Otherwise they take precedence and Unity won't be able to properly
emulate an iPad or mobile device.

To disable the gestures, go to System Preferences -> Trackpad -> More Gestures
and uncheck all the checkboxes.

----------
HOW TO USE
----------

(There is an example scene included on the package showing off multitouch
support.)

Import the Trackpad Touch package. Use TrackpadInput.touches just like you
would use Input.touches:

  using UnityEngine;
  using TrackpadTouch;

  public class TrackpadTest : MonoBehaviour {
      public void Update() {
          foreach (var touch in TrackpadInput.touches) {
              switch (touch.phase) {
                  case TouchPhase.Began:
                      Debug.Log("Finger " + touch.fingerId + " began at " + touch.position);
                      break;
                  case TouchPhase.Moved:
                      Debug.Log("Finger " + touch.fingerId + " moved to " + touch.position);
                      break;
                  case TouchPhase.Ended:
                  case TouchPhase.Canceled:
                      Debug.Log("Finger " + touch.fingerId + " ended at " + touch.position);
                      break;
              }
          }
      }
  }

  See the Touch and TouchPhase classes in the Unity docs for more info on how
  to use the Input.touches interface.
