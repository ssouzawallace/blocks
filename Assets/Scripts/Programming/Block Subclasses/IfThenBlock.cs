using UnityEngine;
using System.Collections;

public class IfThenBlock : Block {

	override protected void CreateConnections () {
		this.blockType = BlockType.BlockTypeInscrution;
		Connection previousConnection 	= new Connection(this, new Vector2(35, 83), Connection.ConnectionType.ConnectionTypeFemale);
		Connection nextConnection 		= new Connection(this, new Vector2(35, 3), Connection.ConnectionType.ConnectionTypeMale);
		Connection thenConnection 		= new Connection(this, new Vector2(107, 43), Connection.ConnectionType.ConnectionTypeMale);
		Connection conditionConnection 	= new Connection(this, new Vector2(72, 67), Connection.ConnectionType.ConnectionTypeFemale);
		
		previousConnection.SetAcceptableBlockType(BlockType.BlockTypeInscrution);
		nextConnection.SetAcceptableBlockType(BlockType.BlockTypeInscrution);
		thenConnection.SetAcceptableBlockType(BlockType.BlockTypeInscrution);
		conditionConnection.SetAcceptableBlockType(BlockType.BlockTypeLogic);
		
		this.connections.Add(previousConnection);
		
		this.connections.Add(thenConnection);
		this.connections.Add(conditionConnection);
		
		this.connections.Add(nextConnection);
	}
}
