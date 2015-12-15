using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class BlockWithArgument : SimpleInstructionBlock {
	RectTransform argumentRectTransform;

	protected Connection argumentConnection;
	
	public override void Start () {
		base.Start ();

		Transform[] childrenTransforms = this.GetComponentsInChildren <Transform> ();
		foreach (Transform transf in childrenTransforms) {
			if (transf.gameObject.tag.Equals("Argument")) {
				argumentRectTransform = transf.gameObject.GetComponent<RectTransform>();
				break;
			}
		}

		argumentConnection = new Connection (this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeNumber, new Vector2 (105, -19));

		this.connections.Add (argumentConnection);

		argumentConnection.attachmentChangedEvent += (Connection obj) => {
			ArgumentAttachmentChanged();
		};
	}

	public override string GetCode () {
		throw new System.NotImplementedException ();
	}

	void ArgumentAttachmentChanged () {
		if (this.argumentConnection.GetAttachedBlock () == null) {
			this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.layoutElement.minWidth);		
		}
		else {
			float argumentWidth = 0.0f;
			
			ArrayList descendingBlocks = this.argumentConnection.GetAttachedBlock().DescendingBlocks();
			foreach (Block block in descendingBlocks) {
				argumentWidth += block.rectTransform.rect.width;
			}
			
			float width = this.layoutElement.minWidth + (argumentWidth - argumentRectTransform.rect.width);	
			
			this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			
		}
	}
}
