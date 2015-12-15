using UnityEngine;
using System.Collections;

public class RandomNumberBlock : NumberBlock {

	public override string GetCode () {
		if (this.connectionRight.GetAttachedBlock () != null) {
			return "random " + this.connectionRight.GetAttachedBlock ().GetCode();
		}
		else {
			return "random ";
		}
	}
}
