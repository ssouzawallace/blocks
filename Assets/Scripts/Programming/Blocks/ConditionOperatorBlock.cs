using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ConditionOperatorBlock : Block {
	protected Connection connectionLeft;
	protected Connection connectionRight;
	
	public string operationString;
	
	public override void Start () {
		base.Start ();
		
		this.connectionLeft  = new Connection (this, 
		                                       Connection.SocketType.SocketTypeFemale, 
		                                       Connection.ConnectionType.ConnectionTypeLogic, 
		                                       new Vector2 (0, 0.5f),
		                                       false,
		                                       false);
		this.connectionRight = new Connection (this,
		                                       Connection.SocketType.SocketTypeFemale, 
		                                       Connection.ConnectionType.ConnectionTypeLogic, 
		                                       new Vector2 (1, 0.5f),
		                                       false,
		                                       false);
		
		this.connections.Add(connectionLeft);
		this.connections.Add(connectionRight);
	}
	
	
	
	public override string GetCode () {
		if (this.connectionRight.GetAttachedBlock () != null) {
			return this.operationString + " " + this.connectionRight.GetAttachedBlock ().GetCode();
		} else {
			return this.operationString;
		}
	}
}
