using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PalleteSection : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Image[] images = GetComponentsInChildren<Image> ();
		float totalHeight = 0.0f;

		foreach (Image image in images) {
			totalHeight += image.preferredHeight;
		}
		VerticalLayoutGroup verticalLayGroup = GetComponent <VerticalLayoutGroup> ();
		totalHeight += verticalLayGroup.padding.top;
		totalHeight += verticalLayGroup.spacing * (images.Length - 1);

		GetComponent<RectTransform> ().sizeDelta = new Vector2(0, totalHeight);
	}
}
