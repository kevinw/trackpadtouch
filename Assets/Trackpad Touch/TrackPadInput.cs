using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class TrackPadInput : MonoBehaviour {

	#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	const string TrackPadTouchLibrary = "TrackPadTouchOSX";
	#endif

	[DllImport (TrackPadTouchLibrary)]
	public static extern void InitPlugin(string assemblyPath);

	[DllImport (TrackPadTouchLibrary)]
	public static extern void DeinitPlugin();

	static bool didRegister = false;

	string AssemblyPath { get { return new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath; } }

	public void Awake() {
		if (!didRegister) {
			InitPlugin(AssemblyPath);
			didRegister = true;
		}
	}

	const int MAX_TOUCHES = 20;

	static IntPtr[] touchIds = new IntPtr[MAX_TOUCHES] {
		IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 
		IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 
		IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 
		IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero
	};

	static Touch[] touchObjects;

	static TrackPadInput() {
		touchObjects = new Touch[20];
	}

	public static int touchCount { get { return touchObjects.Length; } } 

	public static Touch GetTouch(int i) { return touches[i]; }

	static Touch[] thisFramesTouches;

	private static int lastFrame = -1;
	public static Touch[] touches { get {
		if (lastFrame != Time.frameCount || thisFramesTouches == null) {
			thisFramesTouches = new Touch[40];
		}

		return thisFramesTouches;
	} }

	static int GetTouchId(IntPtr deviceTouchId) {
		for (var i = 0; i < touchIds.Length; ++i) {
			if (touchIds[i] == deviceTouchId) {
				return i;
			}
		}

		for (var i = 0; i < touchIds.Length; ++i) {
			if (touchIds[i] == IntPtr.Zero) {
				touchIds[i] = deviceTouchId;
				return i;
			}
		}

		return 0;
	}

	static void ReleaseTouchId(IntPtr deviceTouchId) {
		for (var i = 0; i < touchIds.Length; ++i)
			if (touchIds[i] == deviceTouchId)
				touchIds[i] = IntPtr.Zero;
	}

	static bool HandlersActive { get { return Application.isPlaying; } }

	public static void TrackPadTouchBegan(IntPtr deviceTouchId, float normalX, float normalY)
	{
		if (HandlersActive) {
			var touchId = GetTouchId(deviceTouchId);
			Debug.LogFormat("began {0}, {1}, {2}", touchId, normalX, normalY);
		}
	}

	public static void TrackPadTouchMoved(IntPtr deviceTouchId, float normalX, float normalY)
	{
		if (HandlersActive) {
			var touchId = GetTouchId(deviceTouchId);
			Debug.LogFormat("moved {0}, {1}, {2}", touchId, normalX, normalY);
		}
	}

	public static void TrackPadTouchEnded(IntPtr deviceTouchId)
	{
		if (HandlersActive) {
			var touchId = GetTouchId(deviceTouchId);
			Debug.LogFormat("ended " + touchId);
			ReleaseTouchId(deviceTouchId);
		}
	}

	public static void TrackPadTouchCancelled(IntPtr deviceTouchId)
	{
		if (HandlersActive) {
			var touchId = GetTouchId(deviceTouchId);
			Debug.LogFormat("canceled {0}", touchId);
			ReleaseTouchId(deviceTouchId);
		}
	}
}
