# Trackpad Touch

### [See website for a video and more information](https://kev.town/trackpadtouch/)

Reads multitouch input from your trackpad, giving it to your Unity game with an interface matching Input.touches.

This allows you to test your game's multitouch functionality in the editor, without having to use Unity Remote or building to the device. It also works great in standalone builds, so you can ship multitouch support to users.

## Build instructions.

1. Build the XCode project `XCode/TrackPad/TrackPadTouchOSX.xcodeproj`.

2. Place `TrackpadTouchOSX.bundle` into `Assets/Plugins`.
