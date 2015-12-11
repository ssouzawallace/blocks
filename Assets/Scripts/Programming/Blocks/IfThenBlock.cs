using UnityEngine;
using System.Collections;

public class IfThenBlock : Block {

	protected Connection connectionTop;
	protected Connection connectionNext;
	protected Connection connectionThen;
	protected Connection connectionCondition;

	public override void Start () {
		base.Start ();

		this.connectionTop 			= new Connection (this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, 83));

		this.connectionNext 		= new Connection (this, Connection.SocketType.SocketTypeMale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, 83));
		this.connectionThen 		= new Connection (this, Connection.SocketType.SocketTypeMale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, 83));

		this.connectionCondition 	= new Connection (this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeLogic, new Vector2 (35, 83));

		this.connections.Add (connectionTop);
		this.connections.Add (connectionNext);
		this.connections.Add (connectionThen);
		this.connections.Add (connectionCondition);
	}

	public override string GetCode () {
		string toReturn = "if (";

		if (this.connectionCondition.GetAttachedBlock () != null) {
			toReturn += this.connectionCondition.GetAttachedBlock ().GetCode();
		}

		toReturn += ") [\n";

		if (this.connectionThen.GetAttachedBlock () != null) {
			toReturn += this.connectionThen.GetAttachedBlock ().GetCode();
		}

		toReturn += "]";

		return toReturn;
	}
}
