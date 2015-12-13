using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PalleteSection : MonoBehaviour {


	// Use this for initialization
	void Start () {
		float totalHeight = 0.0f;
		int cont = 0;
		LayoutElement[] childrenLayoutElements = this.gameObject.GetComponentsInChildren<LayoutElement> ();
		
		foreach (LayoutElement layoutElement in childrenLayoutElements) {
			if (layoutElement.transform.parent == this.transform) {
				totalHeight += layoutElement.minHeight;
				cont++;
			}
		}

		VerticalLayoutGroup verticalLayGroup = GetComponent <VerticalLayoutGroup> ();
		totalHeight += verticalLayGroup.padding.top;
		totalHeight += verticalLayGroup.spacing * (cont);

		GetComponent<RectTransform> ().sizeDelta = new Vector2(0, totalHeight);
	}
}
