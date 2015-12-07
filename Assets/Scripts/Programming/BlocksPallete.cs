using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BlocksPallete : MonoBehaviour {
	// Prefab of section game object
	public GameObject sectionGameObject;

	[Header("Blocks")]

	// Motions Blocks
	public GameObject[] motionBlocks;

	// Flow Blocks
	public GameObject[] flowBlocks;

	// Logical Blocks
	public GameObject[] logicalBlocks;

	// Numbers Blocks
	public GameObject[] numbersBlocks;

	// time Blocks
	public GameObject[] timeBlocks;

	// Use this for initialization
	void Start () {
		// Initialize the sections
		GameObject 	section;
		Text 		sectionTitle;
		ScrollRect 	sectionScrollRect;
		Transform 	sectionContentTransform;

		// Motion
		section 				= Instantiate (this.sectionGameObject);
		sectionTitle 			= section.GetComponent<Text> ();
		sectionScrollRect 		= section.GetComponentInChildren<ScrollRect> ();
		sectionContentTransform	= sectionScrollRect.viewport.GetChild(0);

		sectionTitle.text = "Movimento";
		foreach (GameObject motionGameObject in this.motionBlocks) {
			GameObject gameObject = Instantiate(motionGameObject);

			gameObject.transform.SetParent(sectionContentTransform, false);
		}
		section.transform.SetParent (this.gameObject.transform, false);

		// Flow
		section 				= Instantiate (this.sectionGameObject);
		sectionTitle 			= section.GetComponent<Text> ();
		sectionScrollRect 		= section.GetComponentInChildren<ScrollRect> ();
		sectionContentTransform	= sectionScrollRect.viewport.GetChild(0);
		
		sectionTitle.text = "Controle de fluxo";
		foreach (GameObject flowGameObject in this.flowBlocks) {
			GameObject gameObject = Instantiate(flowGameObject);
			
			gameObject.transform.SetParent(sectionContentTransform, false);
		}
		section.transform.SetParent (this.gameObject.transform, false);
	}
}
