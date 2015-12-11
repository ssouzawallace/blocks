using UnityEngine;
using System.Collections;

public class IfThenElseThenBlock : IfThenBlock {

	protected Connection connectionElseThen;

	public override void Start () {
		base.Start ();
	
		this.connectionElseThen 		= new Connection (this, Connection.SocketType.SocketTypeMale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, 83));

		this.connections.Add (connectionElseThen);
	}

	public override string GetCode () {
		string toReturn = base.GetCode ();

		toReturn += "else [\n";

		if (this.connectionElseThen.GetAttachedBlock() != null) {		
			toReturn += this.connectionElseThen.GetAttachedBlock().GetCode();
		}

		toReturn += "]";

		return toReturn;
	}
}
