using UnityEngine;
using System.Collections;

public class ComparatorBlock : Block {
	public enum ComparatorType {
		ComparatorTypeNone,
		ComparatorTypeLessThan,
		ComparatorTypeLessThanOrEqual,
		ComparatorTypeEqual,
		ComparatorTypeGreaterThan,
		ComparatorTypeGreaterThanOrEqual,
		ComparatorTypeDifferent
	};

	public ComparatorType comparatorType;

	override protected void CreateConnections () {
		this.blockType = BlockType.BlockTypeLogic;
		Connection previousConnection 	= new Connection(this, new Vector2(2, 20), Connection.ConnectionType.ConnectionTypeMale);
		Connection nextConnection 		= new Connection(this, new Vector2(199, 20), Connection.ConnectionType.ConnectionTypeMale);
		
		previousConnection.SetAcceptableBlockType(BlockType.BlockTypeInscrution | BlockType.BlockTypeConditionJoint);
		nextConnection.SetAcceptableBlockType(BlockType.BlockTypeConditionJoint);

		this.connections.Add(previousConnection);
		
		this.connections.Add(nextConnection);
	}
}
