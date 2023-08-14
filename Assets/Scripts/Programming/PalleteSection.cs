using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PalleteSection : MonoBehaviour {


	// Use this for initialization
	void Start () {
		float totalHeight = 0.0f;
		int count = 0;
		LayoutElement[] childrenLayoutElements = this.gameObject.GetComponentsInChildren<LayoutElement> ();
		
		foreach (LayoutElement layoutElement in childrenLayoutElements) {
			if (layoutElement.transform.parent == this.transform) {
				totalHeight += layoutElement.minHeight;
				count++;
			}
		}

		VerticalLayoutGroup verticalLayGroup = GetComponent <VerticalLayoutGroup> ();
		totalHeight += verticalLayGroup.padding.top;
		totalHeight += verticalLayGroup.spacing * (count);

		GetComponent<RectTransform>().sizeDelta = new Vector2(0, totalHeight);
	}
}
