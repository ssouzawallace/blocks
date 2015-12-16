using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class BlockWithArgument : SimpleInstructionBlock {
	RectTransform argumentRectTransform;

	protected Connection argumentConnection;
	private float minimumArgumentWidth;
	
	public override void Start () {
		base.Start ();

		Transform[] childrenTransforms = this.GetComponentsInChildren <Transform> ();
		foreach (Transform transf in childrenTransforms) {
			if (transf.gameObject.tag.Equals("Argument")) {
				argumentRectTransform = transf.gameObject.GetComponent<RectTransform>();
				minimumArgumentWidth = argumentRectTransform.rect.width;

				break;
			}
		}

		argumentConnection = new Connection (this, Connection.SocketType.SocketTypeFemale, Connection.ConnectionType.ConnectionTypeNumber, new Vector2 (105, -19));

		this.connections.Add (argumentConnection);
	}

	public override string GetCode () {
		throw new System.NotImplementedException ();
	}

	public override void HierarchyChanged () {
		base.HierarchyChanged ();

		ArgumentAttachmentMayChanged ();
	}

	void ArgumentAttachmentMayChanged () {
		if (this.argumentConnection.GetAttachedBlock () == null) {
			this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.layoutElement.minWidth);
		}
		else {
			float argumentWidth = 0.0f;
			
			ArrayList descendingBlocks = this.argumentConnection.GetAttachedBlock().DescendingBlocks();
			foreach (Block block in descendingBlocks) {
				argumentWidth += block.rectTransform.rect.width;
			}
			
			float width = this.layoutElement.minWidth + (argumentWidth - minimumArgumentWidth);
			
			this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			
		}
	}
}
