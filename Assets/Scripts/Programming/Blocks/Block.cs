using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class Block : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	public class Connection  {
		const float kMinimumAttachRadius = 20.0f;

		public enum SocketType 				{SocketTypeMale, SocketTypeFemale};
		public enum ConnectionType		 	{ConnectionTypeRegular, ConnectionTypeLogic, ConnectionTypeNumber};
		public enum RelativePositionType 	{RelativePositionTypeFactor, RelativePositionTypeAbsolute};

		private Block 			attachedBlock;

		private Block 			ownerBlock;

		private SocketType 				socketType;
		private ConnectionType 			connectionType;
		private Vector2 				relativePosition;

		private bool	xOffsetType = true;
		private bool	yOffsetType = true;

		public Connection (Block ownerBlock, SocketType socketType, ConnectionType connectionType, Vector2 relativePosition) {
			this.ownerBlock 			= ownerBlock;

			this.socketType 			= socketType;
			this.connectionType 		= connectionType;
			this.relativePosition 		= relativePosition;
		}
		public Connection (Block ownerBlock, SocketType socketType, ConnectionType connectionType, Vector2 relativePosition, bool xOffsetType, bool	yOffsetType) 
			: this(ownerBlock, socketType, connectionType, relativePosition) {
			this.xOffsetType = xOffsetType;
			this.yOffsetType = yOffsetType;
		}

		public SocketType GetSocketType() {
			return this.socketType;
		}

		public Block GetAttachedBlock () {
			return this.attachedBlock;
		}

		public void SetRelativePosition(Vector2 relativePosition) {
			this.relativePosition = relativePosition;
		}
		public Vector2 GetRelativePosition() {
			return this.relativePosition;
		}

		public void Attach(Block block, Connection connection) {
			if (this.attachedBlock == null) {
				this.attachedBlock = block;

				connection.Attach (this.ownerBlock, this);
			}
		}
		public void Detach() {
			if (this.attachedBlock != null) {
				Connection attachedBlockConnection = null;
				foreach (Connection c in this.attachedBlock.connections) {
					if (c.attachedBlock != null && c.attachedBlock.Equals(this.ownerBlock)) {
						attachedBlockConnection = c;
					}
				}

				this.attachedBlock = null;

				if (attachedBlockConnection != null) {
					attachedBlockConnection.Detach();				
				}
			}
		}

		public Vector3 AbsolutePosition() {
			float xOffset = this.relativePosition.x;
			float yOffset = -this.relativePosition.y;

			if (this.xOffsetType == false) {
				xOffset = this.relativePosition.x*this.ownerBlock.rectTransform.rect.width;
			}

			if (yOffsetType == false) {
				yOffset = this.relativePosition.y*(-this.ownerBlock.rectTransform.rect.height);
			}

			Vector3 offset = new Vector3(xOffset,
			                             yOffset);

			return this.ownerBlock.transform.position + offset;
		}
		public float DistanceTo(Connection connection) {
			return Vector2.Distance (this.AbsolutePosition(), connection.AbsolutePosition());
		}
		public bool TryAttachWithBlock (Block block) {

			foreach (Connection connection in block.connections) {
				if (this.socketType != connection.socketType
				    &&
				    this.connectionType == connection.connectionType
				    &&
					connection.GetAttachedBlock() == null
				    && 
				    this.GetAttachedBlock() == null				    
				    && !(this.ownerBlock.connections.IndexOf(this) == 0 && block.connections.IndexOf(connection) == 0)
				    &&
					this.DistanceTo (connection) < kMinimumAttachRadius) {

					if (this.ownerBlock.connections[0].Equals(this)) {
						Vector2 delta =  connection.AbsolutePosition() - this.AbsolutePosition();
						
						this.ownerBlock.ApplyDelta(delta);
					}
					else {
						Vector2 delta =  this.AbsolutePosition() - connection.AbsolutePosition();						

						block.ApplyDelta(delta);
					}

					this.Attach(block, connection);

					return true;
				}
			}

			return false;
		}
	}

	public static float singleInstructionHeight = 37;

	[HideInInspector]
	public RectTransform rectTransform;
	[HideInInspector]
	public LayoutElement layoutElement;
	[HideInInspector]
	public Shadow[] shadows;

	public ArrayList connections = new ArrayList();

	public bool leaveClone = false;

	public virtual void Start () {
		this.rectTransform 	= gameObject.GetComponent <RectTransform> ();
		this.layoutElement 	= gameObject.GetComponent <LayoutElement> ();

		this.shadows 		= gameObject.GetComponentsInChildren <Shadow> ();

		SetShadowActive (false);
	}

	public virtual void HierarchyChanged() {
		Connection firstConnection = this.connections [0] as Connection;

		if (firstConnection.GetAttachedBlock () != null) {
			firstConnection.GetAttachedBlock ().HierarchyChanged();
		}
	}

	public void Detach () {
		Connection firstConnection = this.connections [0] as Connection;
		Block previousBlock = firstConnection.GetAttachedBlock ();
		firstConnection.Detach ();

		if (previousBlock != null) {
			previousBlock.HierarchyChanged ();
		}
	}

	public ArrayList DescendingBlocks () {
		ArrayList arrayList = new ArrayList ();
		arrayList.Add (this);
		
		for (int i = 1; i < this.connections.Count; ++i) {
			Connection connection = this.connections[i] as Connection;
			
			if (connection.GetAttachedBlock() != null && connection.GetAttachedBlock().Equals(this) == false) {
				ArrayList descendingBlocks = connection.GetAttachedBlock().DescendingBlocks();
				foreach (Block block in descendingBlocks) {
					arrayList.Add(block);
				}
			}
		}
		
		return arrayList;
	}

	public void ApplyDelta(Vector2 delta) {
		ArrayList descendingBlocks = this.DescendingBlocks ();

		foreach (Block block in descendingBlocks) {
			block.transform.position += new Vector3 (delta.x, delta.y);
		}
	}

	public void SetShadowActive (bool active) {
		foreach (Shadow shadow in this.shadows) {
			shadow.enabled = active;
		}
	}

	public void SetHeight (float height) {
		this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		RefreshDescendingPosition();
	}

	public void SetWidth (float width) {
		this.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		RefreshDescendingPosition();
	}

	void RefreshDescendingPosition() {
		foreach (Connection c in this.connections) {
			if (connections.IndexOf(c) == 0) {
				continue;
			}
			
			if (c.GetAttachedBlock() != null ) {
				foreach (Connection c1 in c.GetAttachedBlock().connections) {
					if (c1.GetAttachedBlock() != null && c1.GetAttachedBlock().Equals(this)) {
						if (c1.DistanceTo(c) > 0.0001f) {
							c.GetAttachedBlock().ApplyDelta(c.AbsolutePosition() - c1.AbsolutePosition());
						}
					}
				}
			}		
		}
			
	}

	public bool TryAttachInSomeConnectionWithBlock (Block block) {
		if (this.Equals (block)) {
			return false;
		}

		ArrayList descendingBlocks = this.DescendingBlocks ();
		
		foreach (Block aBlock in descendingBlocks) {		
			foreach (Connection conection in aBlock.connections) {
				if (conection.TryAttachWithBlock (block)) {
					Connection firstConnection = this.connections [0] as Connection;
					Block previousBlock = firstConnection.GetAttachedBlock ();

					if (previousBlock != null) {
						previousBlock.HierarchyChanged ();
					}

					return true;
				}
			}
		}
		return false;
	}

	void LeaveClone () {
		GameObject go = Instantiate (this.gameObject);
		
		go.GetComponent<Block> ().leaveClone = true;
		
		go.transform.SetParent (this.transform.parent, false);
		go.transform.SetSiblingIndex (this.transform.GetSiblingIndex ());
		
		go.GetComponent<RectTransform> ().anchoredPosition = this.rectTransform.anchoredPosition;
		go.GetComponent<RectTransform> ().sizeDelta = this.rectTransform.sizeDelta;
		go.GetComponent<RectTransform> ().anchorMin = this.rectTransform.anchorMin;
		go.GetComponent<RectTransform> ().anchorMax = this.rectTransform.anchorMax;
	}

	bool DestroyHierarchyIfNeeded () {
		GameObject codeContentGO = CodeContent();
		
		RectTransform rect 	= codeContentGO.GetComponent<RectTransform> ();
		Vector2 mousePos 	= new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		
		if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos)) {

			ArrayList descending = DescendingBlocks ();
			foreach (Block block in descending) {
				Vector3 previousPosition = block.transform.position;
				block.transform.SetParent(codeContentGO.transform, false);
				block.transform.position = previousPosition;
			}

			return false;
		}
		else {
			// Coloca blocos no topo
			ArrayList descending = DescendingBlocks ();
			foreach (Block b in descending) {
				Destroy(b.gameObject);
			}
			
			return true;
		}
	}

	GameObject CodeContent() {
		return GameObject.FindWithTag ("CodeContent");
	}

	#region Abstract

	// Todo bloco deve retornar algum código relacionado a ele
	public abstract string GetCode ();

	#endregion

	#region Drag
	
	public void OnBeginDrag (PointerEventData eventData) {
		if (this.leaveClone == true) {
			LeaveClone();

			this.leaveClone = false;
		}

		// Desconecta do bloco acima
		this.Detach ();

		ArrayList descendingBlocks = this.DescendingBlocks();

		foreach (Block block in descendingBlocks) {
			block.transform.SetSiblingIndex (block.transform.parent.childCount - 1);
			block.SetShadowActive (true);
		}

		foreach (Block b in descendingBlocks) {
			Vector3 previousPosition = b.transform.position;		
			b.transform.SetParent(GameObject.FindWithTag("Canvas").transform, false);					
			b.transform.position = previousPosition;
		}
	}

	// Movendo bloco
	Vector3 lastMousePosition = Vector3.zero;
	public void OnDrag (PointerEventData eventData) {

		// Aplica delta em função do drag
		if (lastMousePosition == Vector3.zero) {
			lastMousePosition = Input.mousePosition;
		}
		else {
			this.ApplyDelta (Input.mousePosition - lastMousePosition);

			lastMousePosition = Input.mousePosition;
		}
	}

	public void OnEndDrag (PointerEventData eventData) {
		if (DestroyHierarchyIfNeeded() == false) {
			lastMousePosition = Vector3.zero;

			ArrayList descendingBlocks = this.DescendingBlocks ();
			foreach (Block block in descendingBlocks) {
				block.SetShadowActive(false);
			}

			// Tenta conectar com algum bloco
			Block[] codeContentBlocks =  CodeContent().GetComponentsInChildren<Block>();

			foreach (Block block in codeContentBlocks) {
				if (this.TryAttachInSomeConnectionWithBlock (block.GetComponent<Block>())) {
					break;
				}
			}

			Debug.Log (GetCode ());
		}
	}

	#endregion

	void OnDrawGizmos() {
		if (!Application.isPlaying) return;
		
		foreach (Connection connection in this.connections) {
			if (connection.GetSocketType() == Connection.SocketType.SocketTypeMale) {
				Gizmos.color = Color.blue;
			}
			else {
				Gizmos.color = Color.red;
			}
			Gizmos.DrawSphere(connection.AbsolutePosition(), 5);

			if (connection.GetAttachedBlock() != null) {
				Gizmos.DrawLine(connection.AbsolutePosition(), connection.GetAttachedBlock().transform.position);
			}
		}
	}
}
