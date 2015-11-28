using UnityEngine;
using System.Collections;

public class SimpleInscructionBlock : Block {

	override protected void CreateConnections () {
		this.blockType = BlockType.BlockTypeInscrution;
		Connection previousConnection 	= new Connection(this, new Vector2(35, 42), Connection.ConnectionType.ConnectionTypeFemale);
		Connection nextConnection 		= new Connection(this, new Vector2(35, 4), Connection.ConnectionType.ConnectionTypeMale);
		
		previousConnection.SetAcceptableBlockType(BlockType.BlockTypeInscrution);
		nextConnection.SetAcceptableBlockType(BlockType.BlockTypeInscrution);
		
		this.connections.Add(previousConnection);
		
		this.connections.Add(nextConnection);
	}
}
