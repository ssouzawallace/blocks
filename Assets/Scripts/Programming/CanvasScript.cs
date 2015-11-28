using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CanvasScript: MonoBehaviour {

	ArrayList blocks;
	
	void Start () {
		blocks = new ArrayList(GameObject.FindGameObjectsWithTag ("Block"));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
