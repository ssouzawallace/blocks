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

	// Time Blocks
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

		// Logic
		section 				= Instantiate (this.sectionGameObject);
		sectionTitle 			= section.GetComponent<Text> ();
		sectionScrollRect 		= section.GetComponentInChildren<ScrollRect> ();
		sectionContentTransform	= sectionScrollRect.viewport.GetChild(0);
		
		sectionTitle.text = "Condição";
		foreach (GameObject logicGameObject in this.logicalBlocks) {
			GameObject gameObject = Instantiate(logicGameObject);
			
			gameObject.transform.SetParent(sectionContentTransform, false);
		}
		section.transform.SetParent (this.gameObject.transform, false);

		// Number
		section 				= Instantiate (this.sectionGameObject);
		sectionTitle 			= section.GetComponent<Text> ();
		sectionScrollRect 		= section.GetComponentInChildren<ScrollRect> ();
		sectionContentTransform	= sectionScrollRect.viewport.GetChild(0);
		
		sectionTitle.text = "Números";
		foreach (GameObject numberGameObject in this.numbersBlocks) {
			GameObject gameObject = Instantiate(numberGameObject);
			
			gameObject.transform.SetParent(sectionContentTransform, false);
		}
		section.transform.SetParent (this.gameObject.transform, false);

		// Time
		section 				= Instantiate (this.sectionGameObject);
		sectionTitle 			= section.GetComponent<Text> ();
		sectionScrollRect 		= section.GetComponentInChildren<ScrollRect> ();
		sectionContentTransform	= sectionScrollRect.viewport.GetChild(0);
		
		sectionTitle.text = "Tempo";
		foreach (GameObject timeGameObject in this.timeBlocks) {
			GameObject gameObject = Instantiate(timeGameObject);
			
			gameObject.transform.SetParent(sectionContentTransform, false);
		}
		section.transform.SetParent (this.gameObject.transform, false);
	}
}
