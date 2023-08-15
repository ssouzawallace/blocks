using UnityEngine;
using System.Collections;

public class ForeverBlock : IfThenBlock {

	// Use this for initialization
	public override void Start () {
		base.Start();

		this.connections.Remove(this.connectionCondition);
		this.connections.Remove(this.connectionNext);
	}
	
	public override string GetCode () {
		string toReturn = "forever [\n";
		
		if (this.connectionThen.GetAttachedBlock () != null) {
			toReturn += this.connectionThen.GetAttachedBlock ().GetCode();
		}
		
		toReturn += "\n]";
		
		return toReturn;
	}
}
