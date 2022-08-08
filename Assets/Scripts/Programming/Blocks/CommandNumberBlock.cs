using UnityEngine;
using System.Collections;

public class CommandNumberBlock : NumberBlock {
	public string command;

	public override string GetCode () {
		if (this.connectionRight.GetAttachedBlock () != null) {
			return command + this.connectionRight.GetAttachedBlock ().GetCode();
		}
		else {
			return command;
		}
	}
}
