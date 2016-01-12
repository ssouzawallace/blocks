using UnityEngine;
using System.Collections;

public class StartBlock : Block {

	protected Connection connectionTop;
	protected Connection connectionNext;

	public override void Start () {
		base.Start ();

		this.connectionTop  = new Connection (this, 
		                                      Connection.SocketType.SocketTypeFemale, 
		                                      Connection.ConnectionType.ConnectionTypeForbidden, 
		                                      new Vector2 (35.0f, 0.0f),
		                                      true,
		                                      false);
		
		this.connectionNext = new Connection (this, 
		                                      Connection.SocketType.SocketTypeMale, 
		                                      Connection.ConnectionType.ConnectionTypeRegular, 
		                                      new Vector2 (35.0f, 1.0f),
		                                      true,
		                                      false);

		this.connections.Add(connectionTop);
		this.connections.Add(connectionNext);
	}
	
	public override string GetCode () {
		string toReturn = "to start";
		
		if (this.connectionNext.GetAttachedBlock () != null) {
			toReturn += "\n" + this.connectionNext.GetAttachedBlock().GetCode();
		}

		toReturn += "\nend";

		return toReturn;
	}
}
