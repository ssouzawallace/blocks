using UnityEngine;
using System.Collections;

public class SimpleInstructionBlock : Block {

	[TextArea(2,3)]
	public string instruction;

	new void Start () {
		base.Start ();

		Connection connectionTop 	= new Connection (this, new Vector2 (35, 40), Connection.ConnectionType.ConnectionTypeFemale);
		Connection connectionNext 	= new Connection (this, new Vector2 (35, 4), Connection.ConnectionType.ConnectionTypeMale);


		this.connections.Add(connectionTop);
		this.connections.Add(connectionNext);
	}

	public override string GetCode () {
		string toReturn = string.Copy (this.instruction);

		Connection connectionNext = this.connections[this.connections.Count-1] as Connection;

		if (connectionNext.GetConnectedBlock () != null) {
			toReturn += "\n" + connectionNext.GetConnectedBlock().GetCode();
		}

		return toReturn;
	}
}
