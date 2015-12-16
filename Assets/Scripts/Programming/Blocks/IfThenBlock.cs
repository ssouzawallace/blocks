using UnityEngine;
using System.Collections;

public class IfThenBlock : Block {

	protected Connection connectionTop;
	protected Connection connectionNext;
	protected Connection connectionThen;
	protected Connection connectionCondition;

	public override void Start () {
		base.Start ();

		this.connectionTop 			= new Connection (this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, 0));

		this.connectionNext 		= new Connection (this, Connection.SocketType.SocketTypeMale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (35, -88));

		this.connectionThen 		= new Connection (this, Connection.SocketType.SocketTypeMale, Connection.ConnectionType.ConnectionTypeRegular, new Vector2 (104.5f, -38));

		this.connectionCondition 	= new Connection (this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeLogic, new Vector2 (71, -20));

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

		toReturn += "\n]";

		if (this.connectionNext.GetAttachedBlock () != null) {
			toReturn += this.connectionNext.GetAttachedBlock().GetCode();
		}

		return toReturn;
	}

	public override void HierarchyChanged () {
		base.HierarchyChanged ();
		
		ThenAttachmentMayChanged ();
	}
	
	void ThenAttachmentMayChanged () {
		if (this.connectionThen.GetAttachedBlock () == null) {
			this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.layoutElement.minHeight);
		}
		else {
			float argumentHeight = 0.0f;
			float delta = this.rectTransform.rect.height;

			ArrayList descendingBlocks = this.connectionThen.GetAttachedBlock().DescendingBlocks();
			foreach (Block block in descendingBlocks) {

				argumentHeight += block.rectTransform.rect.height;
			}

			float singleInstructionHeight = (descendingBlocks[0] as Block).rectTransform.rect.height;
			float height = this.layoutElement.minHeight + (argumentHeight - singleInstructionHeight);

			if (height > this.layoutElement.minHeight-8) {
				this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height-8);
			}


			delta -= height;

			this.connectionNext.SetRelativePosition(new Vector2(35, -this.rectTransform.rect.height +8));

			if (this.connectionNext.GetAttachedBlock() != null) {
				this.connectionNext.GetAttachedBlock().ApplyDelta(new Vector2(0, delta));
			}
		}
	}
}
