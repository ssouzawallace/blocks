using UnityEngine;
using System.Collections;

public class IfThenBlock : Block {

	protected Connection connectionTop;
	protected Connection connectionNext;
	protected Connection connectionThen;
	protected Connection connectionCondition;

	public override void Start () {
		base.Start ();

		this.connectionTop = new Connection (this, 		                                       
		                                     Connection.SocketType.SocketTypeFemale, 
		                                     Connection.ConnectionType.ConnectionTypeRegular, 
		                                     new Vector2 (35, 0),
		                                     true,
		                                     false);

		this.connectionNext = new Connection (this,
		                                      Connection.SocketType.SocketTypeMale, 
		                                      Connection.ConnectionType.ConnectionTypeRegular, 
		                                      new Vector2 (35, 1),
		                                      true,
		                                      false);

		this.connectionThen = new Connection (this, 
		                                      Connection.SocketType.SocketTypeMale, 
		                                      Connection.ConnectionType.ConnectionTypeRegular, 
		                                      new Vector2 (104.5f, 39));

		this.connectionCondition = new Connection (this, 
		                                           Connection.SocketType.SocketTypeFemale, 
		                                           Connection.ConnectionType.ConnectionTypeLogic, 
		                                           new Vector2 (71, 12));

		this.connections.Add (connectionTop);

		this.connections.Add (connectionThen);
		this.connections.Add (connectionCondition);

		this.connections.Add (connectionNext);
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

		toReturn += "\n]";

		if (this.connectionNext.GetAttachedBlock () != null) {
			toReturn += this.connectionNext.GetAttachedBlock().GetCode();
		}

		return toReturn;
	}

	public override void HierarchyChanged () {
		AttachmentsMayChanged ();

		base.HierarchyChanged ();
	}
	
	protected virtual void AttachmentsMayChanged () {
		Debug.Log("AUI");
		if (this.connectionThen.GetAttachedBlock () == null) {
			SetHeight(this.layoutElement.minHeight);
		}
		else {
			float thenHeight = 0.0f;

			Block block = this.connectionThen.GetAttachedBlock();

			while (block != null) {
				thenHeight += block.rectTransform.rect.height;
				block = (block.connections[block.connections.Count-1] as Connection).GetAttachedBlock();
			}

			float height = this.layoutElement.minHeight + (thenHeight - singleInstructionHeight);

			SetHeight(height);
		}
	}
}
