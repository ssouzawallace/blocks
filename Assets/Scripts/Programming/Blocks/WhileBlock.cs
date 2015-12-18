using UnityEngine;
using System.Collections;

public class WhileBlock : IfThenBlock {

	public override string GetCode () {
		string toReturn = "while (";
		
		if (this.connectionCondition.GetAttachedBlock () != null) {
			toReturn += this.connectionCondition.GetAttachedBlock ().GetCode();
		}
		
		toReturn += ") [\n";
		
		if (this.connectionThen.GetAttachedBlock () != null) {
			toReturn += this.connectionThen.GetAttachedBlock ().GetCode();
		}
		
		toReturn += "\n]";
		
		if (this.connectionNext.GetAttachedBlock () != null) {
			toReturn += this.connectionNext.GetAttachedBlock().GetCode();
		}
		
		return toReturn;
	}
}
