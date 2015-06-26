using UnityEngine;
using System.Collections;

namespace TrackpadTouch {

public class TrackpadInputExample : MonoBehaviour {
	public GameObject prefab;
	
	// Update is called once per frame
	void Update () {
		foreach (var touch in TrackpadInput.touches) {
			if (touch.phase == TouchPhase.Began) {
			}
		}
	}
}

}