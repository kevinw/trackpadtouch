using UnityEngine;
using System.Collections;

public class TrackPadInputExample : MonoBehaviour {
	public GameObject prefab;
	
	// Update is called once per frame
	void Update () {
		foreach (var touch in TrackPadInput.touches) {
			if (touch.phase == TouchPhase.Began) {
			}
		}
	}
}
