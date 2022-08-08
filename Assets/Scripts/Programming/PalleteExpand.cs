using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class PalleteExpand : MonoBehaviour {
	ArrayList childrenLayoutElements = new ArrayList ();
	Button[]		childrenButtons;

	public int indexOfExpandedSection = 1;
		
	void Start () {
		Transform[] childrenTransforms = this.gameObject.GetComponentsInChildren<Transform> ();

		foreach (Transform transf in childrenTransforms) {
			if (transf.parent == this.transform) {
				childrenLayoutElements.Add(transf.GetComponent<LayoutElement>());
			}
		}

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

		for (int i = 0; i < this.childrenLayoutElements.Count; ++i) {
			LayoutElement element = this.childrenLayoutElements[i] as LayoutElement;

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
