using System;
using System.Runtime.InteropServices;
 
public static class Native
{
	const int RTLD_NOW = 2;

	public static IntPtr LoadLibrary(string fileName) {
		return dlopen(fileName, RTLD_NOW);
	}

	public static void FreeLibrary(IntPtr handle) {
		dlclose(handle);
	}

	public static IntPtr GetProcAddress(IntPtr dllHandle, string name) {
		// clear previous errors if any
		dlerror();
		var res = dlsym(dllHandle, name);
		var errPtr = dlerror();
		if (errPtr != IntPtr.Zero) {
			throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));
		}
		return res;
	}

	[DllImport("libdl.dylib")]
	private static extern IntPtr dlopen(String fileName, int flags);

	[DllImport("libdl.dylib")]
	private static extern IntPtr dlsym(IntPtr handle, String symbol);

	[DllImport("libdl.dylib")]
	private static extern int dlclose(IntPtr handle);

	[DllImport("libdl.dylib")]
	private static extern IntPtr dlerror();
}

