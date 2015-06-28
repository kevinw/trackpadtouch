using UnityEngine;

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TrackpadTouch {
	public static class TrackpadInput {
		const int MAX_TOUCHES = 10;
		const float EVICT_TIME = 0.7f;

		public static List<Touch> touches {
			get {
				Init();
				if (lastFrame != Time.frameCount) { // only accumulate touches once per frame
					lastFrame = Time.frameCount;
					frameTouches.Clear();

					var oldest = Time.realtimeSinceStartup - 1.0f;

					while (nativeTouchQueue.Count > 0) {
						var nativeTouch = nativeTouchQueue.Dequeue();
						if (nativeTouch.time < oldest) continue;
						var screenPos = new Vector2(nativeTouch.normalizedX * Screen.width,
													nativeTouch.normalizedY * Screen.height);
						frameTouches.Add(CreateTouch(
							nativeTouch.fingerId,
							1,
							screenPos,
							Vector2.zero,
							0,
							nativeTouch.phase));
					}

					nativeTouchQueue.Clear();
				}

				return frameTouches;
			}
		}

		public static int touchCount { get {
			return touches.Count;
		} }

		public static Touch GetTouch(int i) { return touches[i]; }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		[DllImport ("TrackpadTouchOSX5")]
		public static extern void InitPlugin(string assemblyPath);

		[DllImport ("TrackpadTouchOSX5")]
		public static extern void DeinitPlugin();
#else
		public static void InitPlugin() {
			Debug.LogWarning("Trackpad Touch is not support on this operating system yet.");
		}

		public static void DeinitPlugin() {
		}
#endif

		static string AssemblyPath { get { return new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath; } }

		static GameObject updateObject;

		class TrackpadInputUpdate : MonoBehaviour {
			public void Update() { TrackpadInput.Update(); }
		}

		static bool didInit;

		static void Init() {
			if (didInit) return;
			didInit = true;

			InitPlugin(AssemblyPath);

			updateObject = new GameObject("Trackpad Touch");
			GameObject.DontDestroyOnLoad(updateObject);
			updateObject.AddComponent<TrackpadInputUpdate>();
		}

		public static void Update() {
			if (Time.realtimeSinceStartup - lastEventUpdate > EVICT_TIME)
				EvictStaleTouches();
		}

		static int groupId = 0;

		struct NativeTouchEvent {
			public NativeTouchEvent(int fingerId, TouchPhase phase) {
				this.fingerId = fingerId;
				this.phase = phase;
				this.normalizedX = 0;
				this.normalizedY = 0;
				this.time = Time.realtimeSinceStartup;
				this.group = groupId;
			}

			public NativeTouchEvent(int fingerId, TouchPhase phase, float normalX, float normalY) {
				this.fingerId = fingerId;
				this.phase = phase;
				this.normalizedX = normalX;
				this.normalizedY = normalY;
				this.time = Time.realtimeSinceStartup;
				this.group = groupId;
			}

			public int fingerId;
			public TouchPhase phase;
			public float normalizedX;
			public float normalizedY;
			public float time;
			public int group;
		}

		static Queue<NativeTouchEvent> nativeTouchQueue = new Queue<NativeTouchEvent>();

		struct Finger {
			public IntPtr deviceTouchId;
			public float lastUpdate;
			public int groupId;
		}

		static Finger[] touchIds = new Finger[20];
		static float lastEventUpdate = 0.0f;

		static int GetTouchId(IntPtr deviceTouchId) {
			float now = Time.realtimeSinceStartup;
			if (now - lastEventUpdate > EVICT_TIME) groupId++;
			lastEventUpdate = now;

			for (var i = 0; i < touchIds.Length; ++i) {
				if (touchIds[i].deviceTouchId == deviceTouchId) {
					touchIds[i].lastUpdate = lastEventUpdate;
					touchIds[i].groupId = groupId;
					return i;
				}
			}

			for (var i = 0; i < touchIds.Length; ++i) {
				if (touchIds[i].deviceTouchId == IntPtr.Zero) {
					touchIds[i].deviceTouchId = deviceTouchId;
					touchIds[i].lastUpdate = lastEventUpdate;
					touchIds[i].groupId = groupId;
					return i;
				}
			}

			return 0;
		}


		static bool inEvict;
		static void EvictStaleTouches() {
			if (inEvict) return;
			inEvict = true;
			float now = Time.realtimeSinceStartup;
			for (var i = 0; i < MAX_TOUCHES; ++i) {
				if (touchIds[i].deviceTouchId != IntPtr.Zero) {
					if (touchIds[i].groupId != groupId && now - touchIds[i].lastUpdate > EVICT_TIME) {
						nativeTouchQueue.Enqueue(new NativeTouchEvent(i, TouchPhase.Ended));
						ReleaseTouchId(touchIds[i].deviceTouchId);
					}
				}
			}
			inEvict = false;
		}

		static void ReleaseTouchId(IntPtr deviceTouchId) {
			for (var i = 0; i < touchIds.Length; ++i)
				if (touchIds[i].deviceTouchId == deviceTouchId)
					touchIds[i].deviceTouchId = IntPtr.Zero;
		}

		static bool HandlersActive { get { return Application.isPlaying; } }

#region NativeCallbacks
		public static void TrackpadTouchBegan(IntPtr deviceTouchId, float normalX, float normalY, float deviceW, float deviceH)
		{
			if (HandlersActive) {
				nativeTouchQueue.Enqueue(new NativeTouchEvent(GetTouchId(deviceTouchId), TouchPhase.Began, normalX, normalY));
			}
		}

		public static void TrackpadTouchMoved(IntPtr deviceTouchId, float normalX, float normalY, float deviceW, float deviceH)
		{
			if (HandlersActive) {
				nativeTouchQueue.Enqueue(new NativeTouchEvent(GetTouchId(deviceTouchId), TouchPhase.Moved, normalX, normalY));
			}
		}

		public static void TrackpadTouchEnded(IntPtr deviceTouchId)
		{
			if (HandlersActive) {
				nativeTouchQueue.Enqueue(new NativeTouchEvent(GetTouchId(deviceTouchId), TouchPhase.Ended));
				ReleaseTouchId(deviceTouchId);
			}
		}

		public static void TrackpadTouchCancelled(IntPtr deviceTouchId)
		{
			if (HandlersActive) {
				nativeTouchQueue.Enqueue(new NativeTouchEvent(GetTouchId(deviceTouchId), TouchPhase.Canceled));
				ReleaseTouchId(deviceTouchId);
			}
		}

		public static void TrackpadTouchStationary(IntPtr deviceTouchId, float normalX, float normalY, float deviceW, float deviceH) {
			if (HandlersActive) {
				nativeTouchQueue.Enqueue(new NativeTouchEvent(GetTouchId(deviceTouchId), TouchPhase.Stationary, normalX, normalY));
			}
		}

#endregion

		private static readonly List<Touch> frameTouches = new List<Touch>();
		private static int lastFrame = -1;

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

		static void OnDomainUnload(object sender, EventArgs args)
		{
			if (updateObject) {
				GameObject.Destroy(updateObject);
				updateObject = null;
			}
			DeinitPlugin();
		}

		static Touch CreateTouch(int fingerId, int tapCount, Vector2 position, Vector2 deltaPos, float timeDelta, TouchPhase phase)
		{
			var self = new Touch();
			ValueType valueSelf = self;
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
