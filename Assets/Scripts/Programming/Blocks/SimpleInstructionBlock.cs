using UnityEngine;
using System.Collections;

public class SimpleInstructionBlock : Block {

	[TextArea(2,3)]
	public string instruction;

	protected Connection connectionTop;
	protected Connection connectionNext;

	public override void Start () {
		base.Start ();

		this.connectionTop  = new Connection (this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, 0));
		this.connectionNext = new Connection (this, Connection.SocketType.SocketTypeMale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, -37));

		this.connections.Add(connectionTop);
		this.connections.Add(connectionNext);
	}

	public override string GetCode () {
		string toReturn = string.Copy (this.instruction);

		if (this.connectionNext.GetAttachedBlock () != null) {
			toReturn += "\n" + this.connectionNext.GetAttachedBlock().GetCode();
		}

		return toReturn;
	}
}
