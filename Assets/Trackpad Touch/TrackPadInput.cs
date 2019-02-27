using UnityEngine;

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TrackpadTouch {

	[StructLayout(LayoutKind.Sequential)]
	public struct PlatformTouchEvent
	{
		public byte touchId;
		public byte phase;
		public float normalizedX;
		public float normalizedY;
		public float deviceWidth;
		public float deviceHeight;
	}

	public static class TrackpadInput {

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		[DllImport("TrackpadTouchOSX")]
		public static extern void InitPlugin();

		[DllImport("TrackpadTouchOSX")]
		public static extern void DeinitPlugin();

		[DllImport("TrackpadTouchOSX")]
		static extern bool ReadTouchEvent(ref PlatformTouchEvent e);

		[DllImport("TrackpadTouchOSX")]
		static extern void ClearTouches();
#else
		static bool didPlatformWarning = false;

		public static void InitPlugin() {}
		public static void DeinitPlugin() {}
		static bool ReadTouchEvent(ref PlatformTouchEvent e) {
			if (!didPlatformWarning) {
				Debug.LogWarning("TrackpadTouch will not receive touches on this platform");
				didPlatformWarning = true;
			}
			return false;
		}
		static void ClearTouches() {}
#endif

		private static readonly List<Touch> frameTouches = new List<Touch>();
		private static int lastFrame = -1;
		private static float lastTime;
		private static bool didInit;

		public static int touchCount { get { return touches.Count; } }

		public static Touch GetTouch(int i) { Debug.Log("GetTouch(" + i + ")"); return touches[i]; }

		static Dictionary<int, Touch> prevTouches = new Dictionary<int, Touch>();

		public static List<Vector2> deviceSizes = new List<Vector2>();

		public static List<Touch> touches {
			get {
				Init();
				lastTime = Time.unscaledTime;
				if (lastFrame != Time.frameCount) { // only accumulate touches once per frame
					lastFrame = Time.frameCount;

					prevTouches.Clear();
					foreach (var touch in frameTouches)
						prevTouches[touch.fingerId] = touch;

					frameTouches.Clear();
					deviceSizes.Clear();

					PlatformTouchEvent e;
					e.touchId = 0;
					e.phase = 0;
					e.normalizedX = 0;
					e.normalizedY = 0;
					e.deviceWidth = 0;
					e.deviceHeight = 0;

					int count = 0;
					while (ReadTouchEvent(ref e)) {
						count++;
						var screenPos = new Vector2(e.normalizedX * Screen.width,
													e.normalizedY * Screen.height);
						var deltaPos = new Vector2(0, 0);

						Touch prevTouch = new Touch();
						if (prevTouches.TryGetValue(e.touchId, out prevTouch))
							deltaPos = screenPos - prevTouch.position;

						var timeDelta = Time.unscaledTime - lastTime;
						var newTouch = CreateTouch(
							e.touchId, 1, screenPos, deltaPos, timeDelta,
							byteToTouchPhase(e.phase));
						frameTouches.Add(newTouch);
						
						deviceSizes.Add(new Vector2(e.deviceWidth, e.deviceHeight));
					}

					lastTime = Time.unscaledTime;
				}

				return frameTouches;
			}
		}

		static TouchPhase byteToTouchPhase(byte touchPhase) {
			switch (touchPhase) {
				case 0: return TouchPhase.Began;
				case 1: return TouchPhase.Moved;
				case 2: return TouchPhase.Ended;
				case 3: return TouchPhase.Canceled;
				case 4: return TouchPhase.Stationary;
				default: return TouchPhase.Ended;
			}
		}

		class FocusNoticer : MonoBehaviour {
			void OnApplicationFocus(bool focusStatus) {
				TrackpadInput.ClearTouches ();
			}
		}

		static FocusNoticer focusNoticer;

		static void Init() {
			if (!focusNoticer) {
				focusNoticer = new GameObject ().AddComponent<FocusNoticer>();
				focusNoticer.gameObject.name = "Focus Noticer";
				focusNoticer.gameObject.hideFlags = HideFlags.HideAndDontSave;
			}
			if (didInit) return;
			didInit = true;
			InitPlugin();
		}

		// called when scripts reload
		static void OnDomainUnload(object sender, EventArgs args) {
			DeinitPlugin();
		}

		// for setting private variables on Touch objects
		readonly static FieldInfo Touch_deltaTime;
		readonly static FieldInfo Touch_tapCount;
		readonly static FieldInfo Touch_phase;
		readonly static FieldInfo Touch_deltaPosition;
		readonly static FieldInfo Touch_fingerId;
		readonly static FieldInfo Touch_position;
		//static FieldInfo Touch_rawPosition;

		const BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
		static TrackpadInput() {
			var type = typeof(Touch);
			Touch_deltaTime = type.GetField("m_TimeDelta", flag);
			Touch_tapCount = type.GetField("m_TapCount", flag);
			Touch_phase = type.GetField("m_Phase", flag);
			Touch_deltaPosition = type.GetField("m_PositionDelta", flag);
			Touch_fingerId = type.GetField("m_FingerId", flag);
			Touch_position = type.GetField("m_Position", flag);
			//Touch_rawPosition = type.GetField("m_RawPosition", flag);
			AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
		}

		static Touch touchObj = new Touch();

		static Touch CreateTouch(int fingerId, int tapCount, Vector2 position, Vector2 deltaPos, float timeDelta, TouchPhase phase)
		{
			ValueType valueSelf = touchObj;
			Touch_deltaTime.SetValue(valueSelf, timeDelta);
			Touch_tapCount.SetValue(valueSelf, tapCount);
			Touch_phase.SetValue(valueSelf, phase);
			Touch_deltaPosition.SetValue(valueSelf, deltaPos);
			Touch_fingerId.SetValue(valueSelf, fingerId);
			Touch_position.SetValue(valueSelf, position);
			return (Touch)valueSelf;
		}

	}
}
