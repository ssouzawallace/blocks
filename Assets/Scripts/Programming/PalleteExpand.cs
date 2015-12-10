using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class PalleteExpand : MonoBehaviour {
	LayoutElement[] childrenLayoutElements;
	Button[]		childrenButtons;

	public int indexOfExpandedSection = 1;
		
	void Start () {
		childrenLayoutElements 	= gameObject.GetComponentsInChildren<LayoutElement> ();
		childrenButtons	 		= gameObject.GetComponentsInChildren<Button> ();

		int i = 0;
		foreach (Button button in childrenButtons) {
			int index = i;
			button.onClick.AddListener(() => {
				Debug.Log(index);
				SectionTapped(index);
			});
			i++;
		}
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < this.childrenLayoutElements.Length; ++i) {
			LayoutElement element = this.childrenLayoutElements[i];

			if (i == this.indexOfExpandedSection) {
				element.flexibleHeight = 1;
			}
			else {
				element.flexibleHeight = 0;	
			}
		}
	}

	void SectionTapped (int index) {
		this.indexOfExpandedSection = index;
	}
}
