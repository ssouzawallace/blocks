using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IfThenElseBlock : IfThenBlock {

	const float elseYMin = 90;

	protected Connection connectionElse;

	public LayoutElement upperLayoutElement;
	public LayoutElement lowerLayoutElement;

	public override void Start () {
		base.Start ();

		this.connectionElse = new Connection (this,
		                                      Connection.SocketType.SocketTypeMale, 
		                                      Connection.ConnectionType.ConnectionTypeRegular, 
		                                      new Vector2 (104.5f, elseYMin));

		this.connections.Insert(1, this.connectionElse);
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

		toReturn += "else [\n";

		if (this.connectionElse.GetAttachedBlock () != null) {
			toReturn += this.connectionElse.GetAttachedBlock ().GetCode();
		}

		toReturn += "\n]";
		
		if (this.connectionNext.GetAttachedBlock () != null) {
			toReturn += this.connectionNext.GetAttachedBlock().GetCode();
		}
		
		return toReturn;
	}
	
	protected override void AttachmentsMayChanged () {
		Block block;
		float height = 0;

		float thenHeight = 0.0f;
		block = this.connectionThen.GetAttachedBlock();
		while (block != null) {
			thenHeight += block.rectTransform.rect.height;
			block = (block.connections[block.connections.Count-1] as Connection).GetAttachedBlock();
		}

		float elseHeight = 0.0f;
		block = this.connectionElse.GetAttachedBlock();
		while (block != null) {
			elseHeight += block.rectTransform.rect.height;
			block = (block.connections[block.connections.Count-1] as Connection).GetAttachedBlock();
		}

		// -- 
	
		if (thenHeight > 0) {
			this.upperLayoutElement.preferredHeight = this.upperLayoutElement.minHeight + (thenHeight - singleInstructionHeight);
			
			this.connectionElse.SetRelativePosition(new Vector2(this.connectionElse.GetRelativePosition().x,
			                                                    elseYMin + (thenHeight - singleInstructionHeight)));

			height -= singleInstructionHeight;
		}
		else {
			this.upperLayoutElement.preferredHeight = this.upperLayoutElement.minHeight;
			this.connectionElse.SetRelativePosition(new Vector2 (104.5f, elseYMin));
		}


		if (elseHeight > 0) {
			this.lowerLayoutElement.preferredHeight 	= this.lowerLayoutElement.minHeight + (elseHeight - singleInstructionHeight);
			height -= singleInstructionHeight;
		}
		else {
			this.lowerLayoutElement.preferredHeight = this.lowerLayoutElement.minHeight;
		}

		height += this.layoutElement.minHeight + thenHeight + elseHeight;

		SetHeight(height);

		RefreshWidth();
	}
}
