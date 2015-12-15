using UnityEngine;
using System.Collections;

public class ConstantNumberBlock : NumberBlock {
	public int number = 0;

	public override void Start () {
		base.Start();

		if (this.leaveClone) {
			this.text.text = "NÚMERO";
		}
		else {
			this.text.text = number.ToString();
		}
	}

	public override string GetCode () {
		if (this.connectionRight.GetAttachedBlock () != null) {
			return this.number.ToString() + " " + this.connectionRight.GetAttachedBlock ().GetCode();
		}
		else {
			return this.number.ToString();
		}
	}
}
