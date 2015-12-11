using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class Block : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	public class Connection  {
		const float kMinimumAttachRadius = 20.0f;

		public enum SocketType {SocketTypeMale, SocketTypeFemale};
		public enum ConnectionType {ConnectionTypeRegular, ConnectionTypeLogic, ConnectionTypeNumber};

		private Block 			attachedBlock;
		private Connection 		attachedConnection;

		private Block 			ownerBlock;

		private SocketType 		socketType;
		private ConnectionType 	connectionType;
		private Vector2 		relativePosition;

		public Connection (Block ownerBlock, SocketType socketType, ConnectionType connectionType, Vector2 relativePosition) {
			this.ownerBlock 		= ownerBlock;

			this.socketType 		= socketType;
			this.connectionType 	= connectionType;
			this.relativePosition 	= relativePosition;
		}

		public SocketType GetSocketType() {
			return this.socketType;
		}

		public Block GetAttachedBlock () {
			return this.attachedBlock;
		}
		public void Attach(Block block, Connection connection) {
			this.attachedBlock 		= block;
			this.attachedConnection = connection;
		}
		public void Detach() {
			if (this.attachedBlock != null) {
				this.attachedBlock = null;

				this.attachedConnection.Detach();
				this.attachedConnection = null;
			}
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

			foreach (Connection connection in block.connections) {
				if (this.socketType != connection.socketType
				    &&
				    this.connectionType == connection.connectionType
				    &&
					connection.GetAttachedBlock() == null
				    && 
				    this.GetAttachedBlock() == null				    
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

					this.attachedBlock = block;
					connection.attachedBlock = this.ownerBlock;

					return true;
				}
			}

			return false;
		}
	}
	
	protected RectTransform rectTransform;

	protected Image image;
	protected Shadow shadow;
	protected ArrayList connections = new ArrayList();

	public bool leaveClone = false;

	public virtual void Start () {
		this.rectTransform 	= gameObject.GetComponent <RectTransform> ();
		this.image 			= gameObject.GetComponent <Image> ();
		this.shadow 			= gameObject.GetComponent <Shadow> ();

		this.shadow.enabled = false;
	}

	void OnDrawGizmos() {
		if (!Application.isPlaying) return;

		foreach (Connection connection in this.connections) {
			if (connection.GetSocketType() == Connection.SocketType.SocketTypeMale) {
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

	#region Abstract

	public abstract string GetCode ();

	#endregion

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

		Debug.Log (GetCode ());
	}

	#endregion

}
