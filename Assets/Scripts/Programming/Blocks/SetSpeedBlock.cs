using UnityEngine;
using System.Collections;

public class SetSpeedBlock : BlockWithArgument {

	public override string GetCode () {
		string toReturn = "abcd, setpower ";
		
		if (this.argumentConnection.GetAttachedBlock() != null) {
			toReturn += this.argumentConnection.GetAttachedBlock().GetCode();
		}

		toReturn += "\n";

		if (this.connectionNext.GetAttachedBlock() != null) {
			toReturn += this.connectionNext.GetAttachedBlock().GetCode();
			toReturn += "\n";
		}

		return toReturn;
	}
}
