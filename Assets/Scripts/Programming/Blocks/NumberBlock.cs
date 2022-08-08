using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public abstract class NumberBlock : Block {

	protected Connection connectionLeft;
	protected Connection connectionRight;

	protected Text text;

	public override void Start () {
		base.Start ();
		
		this.connectionLeft  = new Connection (this, 
		                                       Connection.SocketType.SocketTypeMale, 
		                                       Connection.ConnectionType.ConnectionTypeNumber, 
		                                       new Vector2 (0.0f, 0.5f),
		                                       false,
		                                       false);
		this.connectionRight = new Connection (this, 
		                                       Connection.SocketType.SocketTypeMale, 
		                                       Connection.ConnectionType.ConnectionTypeNumber, 
		                                       new Vector2 (1.0f, 0.5f),
		                                       false,
		                                       false);
		
		this.connections.Add(connectionLeft);
		this.connections.Add(connectionRight);

		this.text = this.gameObject.GetComponentInChildren<Text> ();

		RefreshSize ();
	}

	public override string GetCode () {
		throw new System.NotImplementedException ();
	}

	public virtual void Update () {
		RefreshSize ();
	}

	private void RefreshSize() {
		if (text.preferredWidth < 70.0f - 20.0f) {
			this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 70);
		}
		else {
			this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, text.preferredWidth + 20.0f);
		}
	}
}
