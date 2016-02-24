using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;

public class NativeLibrary
{
	readonly string name;
	protected IntPtr nativeLib;

	public NativeLibrary(string name) {
		//this.name = name;
		this.name = "/Users/kevin/Library/Developer/Xcode/DerivedData/TrackpadTouchOSX-fweceugkgdywfecpntbhdtvgldxj/Build/Products/Release/TrackpadTouchOSX.bundle/Contents/MacOS/TrackpadTouchOSX"; 
		Load ();
	}

	public void Load() {
		if (!File.Exists(name))
			throw new Exception(name);
		nativeLib = Native.LoadLibrary(name);
		if (nativeLib == IntPtr.Zero)
			throw new Exception ("could not load library " + name);
	}

	public virtual void Unload() {
		if (nativeLib != IntPtr.Zero) {
			Native.FreeLibrary (nativeLib);
			nativeLib = IntPtr.Zero;
		}
	}
}

namespace TrackpadTouch {

public class TrackpadTouchLib : NativeLibrary {
	public TrackpadTouchLib() : base("TrackpadTouchOSX") {}

	Dictionary<string, Delegate> nativeDelegates = new Dictionary<string, Delegate>();

	public void Reload() {
		Unload ();
		nativeDelegates.Clear ();
		_readTouchDelegate = null;
		Load ();
	}

	public override void Unload() {
		if (nativeLib != IntPtr.Zero)
			Invoke<D.DeinitPlugin>();
		base.Unload ();
	}

	D.ReadTouchEvent _readTouchDelegate;

	public bool InvokeRef(ref PlatformTouchEvent e)
    {
		const string name = "ReadTouchEvent";
		if (_readTouchDelegate == null) {
			IntPtr funcPtr = Native.GetProcAddress(nativeLib, name);
			if (funcPtr == IntPtr.Zero) {
				Debug.LogWarning("Could not gain reference to method address: " + name);
				return false;
			}
 
			_readTouchDelegate = Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(D.ReadTouchEvent)) as D.ReadTouchEvent;
		}

		return _readTouchDelegate (ref e);
    }

    public System.Object Invoke<T>(params object[] pars)
    {
		var name = typeof(T).Name;
		Delegate func;
		if (!nativeDelegates.TryGetValue(name, out func)) {
			Debug.Log ("GetProcAddress(" + nativeLib + ", " + name + ")");
			IntPtr funcPtr = Native.GetProcAddress(nativeLib, name);
			if (funcPtr == IntPtr.Zero) {
				Debug.LogWarning("Could not gain reference to method address.");
				return null;
			}
	 
			func = Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(T));
			nativeDelegates[name] = func;
		}

        return func.DynamicInvoke(pars);
    }

	struct D {
		[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
		public delegate bool ReadTouchEvent(ref PlatformTouchEvent e);

		[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
		public delegate void InitPlugin ();

		[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
		public delegate void DeinitPlugin();

		[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
		public delegate void ClearTouches();

		[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
		public delegate void DebugDump();
	}

	public bool ReadTouchEvent(ref PlatformTouchEvent e) { return InvokeRef (ref e); }
	public void InitPlugin() { Invoke<D.InitPlugin>(); }
	public void DeinitPlugin() { Invoke<D.DeinitPlugin> (); }
	public void ClearTouches() { Invoke<D.ClearTouches> (); }
	public void DebugDump() { Invoke<D.DebugDump> (); }
}
}
