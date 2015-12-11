using UnityEngine;
using System.Collections;

public class ComparatorBlock : Block {
	public enum Operation 
	{	
		OperationLessThan, 
		OperationLessThanOrEqual, 
		OperationGreaterThan, 
		OperationGreaterThanOrEqual, 
		OperationEqual, 
		OperationDifferente
	};

	protected Connection connectionLeft;
	protected Connection connectionNext;

	protected Connection connectionValue1;
	protected Connection connectionValue2;

	public Operation operation;

	public override void Start () {
		base.Start ();

		this.connections.Add (connectionLeft);
		this.connections.Add (connectionNext);
		this.connections.Add (connectionValue1);
		this.connections.Add (connectionValue2);
	}
	
	public override string GetCode () {
		throw new System.NotImplementedException ();
	}
}
