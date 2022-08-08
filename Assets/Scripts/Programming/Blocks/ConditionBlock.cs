using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ConditionBlock : Block {

	public string operationString;

	public LayoutElement leftLayoutElement;
	public LayoutElement rightLayoutElement;

	protected Connection connectionLeft;
	protected Connection connectionRight;

	protected Connection connectionNumberLeft;
	protected Connection connectionNumberRight;

	const float rightNumberX = 94.0f;

	private float minArgumentImageWidth;

	public override void Start () {
		base.Start();

		this.connectionLeft  = new Connection (this, 
		                                       Connection.SocketType.SocketTypeMale, 
		                                       Connection.ConnectionType.ConnectionTypeLogic, 
		                                       new Vector2 (0.0f, 0.5f),
		                                       false,
		                                       false);
		this.connectionRight = new Connection (this, 
		                                       Connection.SocketType.SocketTypeMale, 
		                                       Connection.ConnectionType.ConnectionTypeLogic, 
		                                       new Vector2 (1.0f, 0.5f),
		                                       false,
		                                       false);

		this.connectionNumberLeft  = new Connection (this, 		                     		                  
		                                             Connection.SocketType.SocketTypeFemale, 
		                                       		 Connection.ConnectionType.ConnectionTypeNumber, 
		                                       		 new Vector2 (6.0f, 0.5f),
		                                       		 true,
		                                       	  	 false);

		this.connectionNumberRight  = new Connection (this, 		                     		                  
		                                              Connection.SocketType.SocketTypeFemale, 
		                                              Connection.ConnectionType.ConnectionTypeNumber, 
		                                              new Vector2 (rightNumberX, 0.5f),
		                                              true,
		                                              false);

		this.connections.Add(connectionLeft);

		this.connections.Add(connectionNumberLeft);
		this.connections.Add(connectionNumberRight);

		this.connections.Add(connectionRight);	

		LayoutElement[] elements = gameObject.GetComponentsInChildren<LayoutElement>();
		foreach (LayoutElement element in elements) {
			if (element.gameObject.tag.Equals("Argument")) {
				minArgumentImageWidth = element.minWidth;
				break;
			}
		}
	}

	public override string GetCode () {
		string toReturn = "";

		if (this.connectionNumberLeft.GetAttachedBlock () != null) {
			toReturn += connectionNumberLeft.GetAttachedBlock().GetCode();
		} 

		toReturn += " " + this.operationString + " ";

		if (this.connectionNumberRight.GetAttachedBlock () != null) {
			toReturn += connectionNumberRight.GetAttachedBlock().GetCode();
		}

		if (this.connectionRight.GetAttachedBlock () != null) {
			toReturn += connectionRight.GetAttachedBlock().GetCode();
		}

		return toReturn;
	}

	public override void HierarchyChanged () {
		AttachmentsMayChanged ();
		
		base.HierarchyChanged ();
	}

	void AttachmentsMayChanged () {
		Block block;
		float width = 0;
		
		float leftWidth = 0.0f;
		block = this.connectionNumberLeft.GetAttachedBlock();
		while (block != null) {
			leftWidth += block.rectTransform.rect.width;
			block = (block.connections[block.connections.Count-1] as Connection).GetAttachedBlock();
		}
		
		float rightWidth = 0.0f;
		block = this.connectionNumberRight.GetAttachedBlock();
		while (block != null) {
			rightWidth += block.rectTransform.rect.width;
			block = (block.connections[block.connections.Count-1] as Connection).GetAttachedBlock();
		}
		
		// -- 
		
		if (leftWidth > 0) {
			this.leftLayoutElement.preferredWidth = this.leftLayoutElement.minWidth + (leftWidth - minArgumentImageWidth);
			
			this.connectionNumberRight.SetRelativePosition(new Vector2(rightNumberX + (leftWidth - minArgumentImageWidth),
			                                               this.connectionNumberRight.GetRelativePosition().y));	
			width -= minArgumentImageWidth;
		}
		else {
			this.leftLayoutElement.preferredWidth = this.leftLayoutElement.minWidth;
			this.connectionNumberRight.SetRelativePosition(new Vector2 (rightNumberX, 0.5f));
		}

		if (rightWidth > 0) {
			this.rightLayoutElement.preferredWidth 	= this.rightLayoutElement.minWidth + (rightWidth - minArgumentImageWidth);
			width -= minArgumentImageWidth;
		}
		else {
			this.rightLayoutElement.preferredWidth = this.rightLayoutElement.minWidth;
		}
		
		width += this.layoutElement.minWidth + leftWidth + rightWidth;

		SetWidth(width);
	}
}
