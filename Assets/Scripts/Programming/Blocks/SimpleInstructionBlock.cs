using UnityEngine;
using System.Collections;

public class SimpleInstructionBlock : Block {

	[TextArea(2,3)]
	public string instruction;

	private Connection connectionTop;
	private Connection connectionNext;

	public override void Start () {
		base.Start ();

		this.connectionTop  = new Connection (this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, 40));
		this.connectionNext = new Connection (this, Connection.SocketType.SocketTypeMale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, 4));

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
