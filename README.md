# Trackpad Touch

<img src="https://kev.town/trackpadtouch/tt_logo-horizontal-700px.png">

## What is this?

Reads multitouch input from your trackpad, giving it to your Unity game with an interface matching `Input.touches`.

This allows you to test your game's multitouch functionality in the editor, without having to use Unity Remote or building to the device. It also works great in standalone builds, so you can ship multitouch support to users.


An [explanation video](https://www.youtube.com/watch?v=DT7tO7-4VnM):

[![YouTube video describing the plugin](https://img.youtube.com/vi/DT7tO7-4VnM/0.jpg)](https://www.youtube.com/watch?v=DT7tO7-4VnM)

An editor screenshot:

<img src="https://kev.town/trackpadtouch/TrackpadTouchEditorScreenshot700.jpg">

## Disabling OS X Gestures

This plugin cannot prevent OS X from catching Mission Control and Notification Center gestures. They take precedence and Unity won't be able to properly emulate an iPad or mobile device.

If you want to disable the OS gestures, go to System Preferences -> Trackpad -> More Gestures and uncheck all the checkboxes.

## Build instructions

1. Build the XCode project `XCode/TrackPad/TrackPadTouchOSX.xcodeproj`.

2. Place `TrackpadTouchOSX.bundle` into your Unity project's `Assets/Plugins` folder.

## How to Use

**NOTE: There is an example scene included in the package showing off multitouch support.**

Import the `TrackpadTouch` package.

Use `TrackpadInput.touches` just like you would use `Input.touches`:

```C#
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
          Debug.Log("Finger " + touch.fingerId + " ended");
          break;
      }
    }
  }
}
```

See the [`Touch`](https://docs.unity3d.com/ScriptReference/Touch.html) and [`TouchPhase`](http://docs.unity3d.com/ScriptReference/TouchPhase.html) classes in the Unity docs for more info on how to use the `Input.touches` interface.

## How does it work?

See the [plugin code](XCode/TrackPad/TrackPad/Plugin.m) for details.
