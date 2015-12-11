using UnityEngine;
using System.Collections;

public class SimpleInstructionBlock : Block {

	[TextArea(2,3)]
	public string instruction;

	new void Start () {
		base.Start ();

		this.connections.Add(new Connection(this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, 40)));
		this.connections.Add(new Connection(this, Connection.SocketType.SocketTypeMale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, 4)));
	}

	public override string GetCode () {
		string toReturn = string.Copy (this.instruction);

		Connection connectionNext = this.connections[this.connections.Count-1] as Connection;

		if (connectionNext.GetAttachedBlock () != null) {
			toReturn += "\n" + connectionNext.GetAttachedBlock().GetCode();
		}

		return toReturn;
	}
}
