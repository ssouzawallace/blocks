using UnityEngine;
using System.Collections;

public class PalleteScript : MonoBehaviour {

	Animator animator;

	void Start () {
		animator = gameObject.GetComponent<Animator>();
	}

	public void BlocksMoving() {
		animator.SetBool("showTrashIcon", true);
	}

	public void BlocksDropped() {
		animator.SetBool("showTrashIcon", false);
	}
}
