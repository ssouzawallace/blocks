using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NumberOperationBlock : Block {
	protected Connection connectionLeft;
	protected Connection connectionRight;
	
	public string operationString;
	
	public override void Start () {
		base.Start ();
		
		this.connectionLeft  = new Connection (this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeNumber, new Vector2 (0, -10));
		this.connectionRight = new Connection (this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeNumber, new Vector2 (16, -10));
		
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
