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
			Image[] images = element.gameObject.GetComponentsInChildren<Image>();

			if (i == this.indexOfExpandedSection) {
				element.flexibleHeight = 1;

				foreach (Image image in images) {
					image.enabled = true;
				}	
			}
			else {
				element.flexibleHeight = 0;
				
				foreach (Image image in images) {
					image.enabled = false;
				}	
			}
		}
	}

	void SectionTapped (int index) {
		this.indexOfExpandedSection = index;
	}
}
