using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	[System.Serializable]
	public class Connection  {
		const float kMinimumAttachRadius = 20.0f;
		public enum ConnectionType {ConnectionTypeMale, ConnectionTypeFemale};

		private Block connectedBlock;

		public Block ownerBlock;
		public ConnectionType connectionType;
		public Vector2 relativePosition;

		public BlockType[] acceptableBlockTypes;

		public Connection (Block ownerBlock, Vector2 relativePosition, ConnectionType connectionType) {
			this.ownerBlock = ownerBlock;
			this.relativePosition = relativePosition;
			this.connectionType = connectionType;
		}

		public Block GetConnectedBlock () {
			return this.connectedBlock;
		}

		public Vector2 AbsolutePosition() {
			float parentScale = this.ownerBlock.transform.parent.GetComponent<RectTransform> ().localScale.x;
			return new Vector2 (this.ownerBlock.transform.position.x, this.ownerBlock.transform.position.y)
				+ new Vector2 ((this.relativePosition.x - this.ownerBlock.rectTransform.sizeDelta.x / 2) * parentScale, 
				               (this.relativePosition.y - this.ownerBlock.rectTransform.sizeDelta.y / 2) * parentScale);
					
		}
		float DistanceTo(Connection connection) {
			return Vector2.Distance (this.AbsolutePosition(), connection.AbsolutePosition());
		}
		public bool TryAttachWithBlock (Block block) {
			bool acceptable = false;

			foreach (BlockType blockType in this.acceptableBlockTypes) {
				if (blockType == block.blockType) {
					acceptable = true;
					break;
				}
			}

			if (acceptable) {
				foreach (Connection connection in block.connections) {
					if (this.connectionType != connection.connectionType
					    &&
						connection.connectedBlock == null
					    && 
					    this.connectedBlock == null				    
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

						this.connectedBlock = block;
						connection.connectedBlock = this.ownerBlock;

						return true;
					}
				}
			}

			return false;
		}
		public void Detach() {
			if (this.connectedBlock != null) {
				foreach (Connection connection in this.connectedBlock.connections) {
					if (connection.connectedBlock != null && connection.connectedBlock.Equals (this.ownerBlock)) {
						connection.connectedBlock = null;
						break;
					}
				}
				this.connectedBlock = null;
			}
		}
	}
	
	public enum BlockType {	
		BlockTypeNone,
		BlockTypeStart,			 
		BlockTypeInscrution,
		BlockTypeLogic,
		BlockTypeNumeric,
		BlockTypeConditionJoint
	};
	
	protected RectTransform rectTransform;

	protected Image image;
	protected Shadow shadow;
	public bool leaveClone = false;

	public Connection[] connections;

	public BlockType blockType;

	[TextArea(3,5)]
	public string codeFormat;

	public BlockType GetBlockType () {
		return this.blockType;				
	}

	protected void Start () {
		this.rectTransform 	= gameObject.GetComponent <RectTransform> ();
		this.image 			= gameObject.GetComponent <Image> ();
		this.shadow 			= gameObject.GetComponent <Shadow> ();

		this.shadow.enabled = false;
	}

	void OnDrawGizmos() {
		if (!Application.isPlaying) return;

		foreach (Connection connection in this.connections) {
			if (connection.connectionType == Connection.ConnectionType.ConnectionTypeMale) {
				Gizmos.color = Color.blue;
			}
			else {
				Gizmos.color = Color.red;
			}
			Gizmos.DrawSphere(connection.AbsolutePosition(), 10);
		}
	}

	public void SetShadowActive (bool active) {
		this.shadow.enabled = active;
	}
	
	public void Detach () {
		Connection firstConnection = this.connections [0] as Connection;
		firstConnection.Detach ();
	}

	public void ApplyDelta(Vector2 delta) {
		ArrayList descendingBlocks = this.DescendingBlocks ();

		foreach (Block block in descendingBlocks) {
			block.transform.position = block.transform.position + new Vector3 (delta.x, delta.y);
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
					return true;
				}
			}
		}
		return false;
	}

	public ArrayList DescendingBlocks () {
		ArrayList arrayList = new ArrayList ();
		arrayList.Add (this);

		for (int i = 1; i < this.connections.Length; ++i) {
			Connection connection = this.connections[i] as Connection;

			if (connection.GetConnectedBlock() != null && connection.GetConnectedBlock().Equals(this) == false) {
				ArrayList descendingBlocks = connection.GetConnectedBlock().DescendingBlocks();
				foreach (Block block in descendingBlocks) {
					arrayList.Add(block);
				}
			}
		}

		return arrayList;
	}

	public string GetCode () {
	
		return "";
	}

	#region Drag
	
	public void OnBeginDrag (PointerEventData eventData) {
		if (this.leaveClone == true) {
			GameObject go = Instantiate(this.gameObject);

			go.GetComponent<Block>().leaveClone = true;

			go.transform.SetParent(this.transform.parent, false);
			go.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

			this.transform.SetParent(GameObject.FindWithTag("Canvas").transform, false);
			this.leaveClone = false;

			go.GetComponent<RectTransform>().anchoredPosition = this.rectTransform.anchoredPosition;
			go.GetComponent<RectTransform>().sizeDelta = this.rectTransform.sizeDelta;
			go.GetComponent<RectTransform>().anchorMin  = this.rectTransform.anchorMin;
			go.GetComponent<RectTransform>().anchorMax  = this.rectTransform.anchorMax;
		}
		
		// Desconecta do bloco acima
		this.Detach ();

		// Deixa todos os blocos descendetes no topo da telas
		ArrayList descendingBlocks = this.DescendingBlocks ();

		foreach (Block block in descendingBlocks) {
			block.transform.SetSiblingIndex (block.transform.parent.childCount - 1);
			block.SetShadowActive (true);
		}

	}
	
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

		GameObject codeContentGO = GameObject.FindWithTag ("CodeContent");
		if (transform.parent.gameObject.Equals (codeContentGO) == false) {
			RectTransform rect = codeContentGO.GetComponent<RectTransform> ();
			Vector2 mousePos = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);

			if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos)) {
				Vector3 previousPosition = this.transform.position;
				this.transform.SetParent(codeContentGO.transform, false);
				this.transform.position = previousPosition;
			}
			else {
				Destroy(this.gameObject);
				return;
			}
		}

		lastMousePosition = Vector3.zero;

		ArrayList descendingBlocks = this.DescendingBlocks ();
		foreach (Block block in descendingBlocks) {
			block.SetShadowActive(false);
		}

		// Tenta conectar com algum bloco
		GameObject[] GOs = GameObject.FindGameObjectsWithTag ("Block");

		foreach (GameObject GO in GOs) {
			Block block = GO.GetComponent<Block>() as Block;

			if (this.TryAttachInSomeConnectionWithBlock (block.GetComponent<Block>())) {
				break;
			}
		}
	}

	#endregion

}
