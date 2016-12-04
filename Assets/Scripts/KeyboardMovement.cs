using UnityEngine;
using System.Collections;

public class KeyboardMovement : MonoBehaviour
{

	// Use this for initialization

	public float speed;
	public GameObject surroundSphere;

	private int cZ = 0;
	// currentZone
	private static int maxNumZones = 2;

	private Rigidbody bubble;
	//private GameObject[] elementsZ1;
	private System.Collections.Generic.List<GameObject>[] listElementList = new System.Collections.Generic.List<GameObject>[maxNumZones];
	private GameObject[] bucketList = new GameObject[maxNumZones];


	//private System.Collections.Generic.List <GameObject> listElementList[cZ];
	//private GameObject bucketList[cZ];
	private Vector3 selectedCubeMove;
	private GameObject lastGrabbedCube;
	private GameObject currentTouchedCube;
	private GameObject currentGrabbedCube;

	private float maxBubbleScale = 5;
	private bool isAnyCubeTouched;
	

	/*Color guide
	 * The colors are important because thats the way, we show the different state of each sphere.
	 *Green: The object is grabbed by the bubble
	 *Red: The object is touched by the bubble
	 *Clay: The object was the last object to be selected by the bubble.
	 *Yellow: The normal color of the object without any interaction
	*/

	void Start ()
	{

		bubble = GetComponent<Rigidbody> ();
		//bubble.GetComponent<Renderer> ().material.color = new Color (.04f, .16f, .35f);

		print ("Scale: " + bubble.transform.localScale);
		selectedCubeMove = new Vector3 ();
		lastGrabbedCube = null;
		currentTouchedCube = null; 
		currentGrabbedCube = null;

		for (int i = 0; i < maxNumZones; i++) {
			listElementList [i] = new System.Collections.Generic.List<GameObject> (GameObject.FindGameObjectsWithTag ("ElementZ" + i)); 
			bucketList [i] = GameObject.FindGameObjectWithTag ("BucketZ" + i);
		}
			
		print (listElementList [cZ].Count);

		isAnyCubeTouched = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		if (cZ == maxNumZones) {

			print ("Game Done!");

		} else {
			MovementPlayer ();

			IdentifyClosestCubes ();

			AssignColorsToCubes ();

			AddSurroundingTransSphere ();

			UpdateZone ();


			if (Input.GetKeyDown (KeyCode.Space)) {
				if (isAnyCubeTouched) {
					foreach (GameObject element in listElementList[cZ]) {

						if (element == currentGrabbedCube) {
							currentGrabbedCube = null;
							lastGrabbedCube = element;
						} else if (element == currentTouchedCube) {
							currentGrabbedCube = element;
							currentTouchedCube = null;
							lastGrabbedCube = element;
						}
					}
				} 
			}
		}
	}

	void MovementPlayer ()
	{

		Vector3 vector = new Vector3 ();
		
		if (Input.GetKey (KeyCode.RightArrow)) {
			vector = Vector3.right;
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			vector = Vector3.left;
		}
		if (Input.GetKey (KeyCode.UpArrow)) {
			vector = Vector3.forward;
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			vector = Vector3.back;
		}
		if (Input.GetKey (KeyCode.A)) {
			vector = Vector3.up;
		}
		if (Input.GetKey (KeyCode.Y)) {
			vector = Vector3.down;
		}
		transform.position += vector * speed * Time.deltaTime;
		selectedCubeMove = vector * speed * Time.deltaTime;
			
	}

	void IdentifyClosestCubes ()
	{

		if (currentGrabbedCube == null) {

			GameObject closestCubeA = null;
			float closestDistanceB = Mathf.Infinity;
			float containmentDistanceA = Mathf.Infinity;

			float minDisA = Mathf.Infinity;
			float minDisB = Mathf.Infinity;

			if (listElementList [cZ].Count == 1) {
				bubble.transform.localScale = new Vector3 (maxBubbleScale, maxBubbleScale, maxBubbleScale);
			} else if (listElementList[cZ].Count > 1) {
				foreach (GameObject element in listElementList[cZ]) {
					float dis = Vector3.Distance (bubble.transform.position, element.transform.position);
					float conD = 2 * dis + element.transform.localScale.x;

					if (dis < minDisA) {
						minDisA = dis;
						containmentDistanceA = conD;
						closestCubeA = element;
					}
				}

				foreach (GameObject element in listElementList[cZ]) {
					float dis = Vector3.Distance (bubble.transform.position, element.transform.position);
					float intD = 2 * dis - element.transform.localScale.x - 0.1f;

					if (element != closestCubeA) {
						if (dis < minDisB) {
							minDisB = dis;
							closestDistanceB = intD;
						}
					}
				}
					
				float min = Mathf.Min (containmentDistanceA, closestDistanceB);

				if (min > maxBubbleScale) {
					bubble.transform.localScale = new Vector3 (maxBubbleScale, maxBubbleScale, maxBubbleScale);
				} else {
					bubble.transform.localScale = new Vector3 (min, min, min);

				}
			}
				
		}

	}

	void AssignColorsToCubes ()
	{
		GameObject objectToDelete = null;
		foreach (GameObject element in listElementList[cZ]) {


			if (element == currentGrabbedCube) {
				element.GetComponent<Renderer> ().material.color = Color.green;
				element.transform.position += selectedCubeMove;
			} else {
				
				if (element == currentTouchedCube) {
					element.GetComponent<Renderer> ().material.color = Color.red;
				} else if (element == lastGrabbedCube) {
					element.GetComponent<Renderer> ().material.color = Color.cyan;
				} else {
					element.GetComponent<Renderer> ().material.color = Color.yellow;
				}
					
			}

			if (element == lastGrabbedCube && currentGrabbedCube == null && element.transform.position.y > 1) {

				var deltaX = Mathf.Abs (bucketList [cZ].transform.position.x - element.transform.position.x);
				var deltaY = Mathf.Abs (bucketList [cZ].transform.position.y - element.transform.position.y);
				var deltaZ = Mathf.Abs (bucketList [cZ].transform.position.z - element.transform.position.z);

				var scaleDiff = (bubble.transform.localScale + element.transform.localScale) / 2;

				if (deltaX <= scaleDiff.x && deltaY <= scaleDiff.y && deltaZ <= scaleDiff.z) {
					objectToDelete = element;

				} else {
					element.transform.position += Vector3.down * speed * Time.deltaTime;
				}
			}
		}

		//Delete the object when the object is inside the bucket
		if (objectToDelete != null) {
			listElementList [cZ].Remove (objectToDelete);
			Destroy (objectToDelete);
			print ("Destroyed");
			print ("Size: " + listElementList [cZ].Count);
		}
	}
		
	void AddSurroundingTransSphere ()
	{
		foreach (GameObject element in listElementList[cZ]) {
			if (currentGrabbedCube != null) {
				if (element == currentGrabbedCube) {

					float sizeSphere = (element.transform.localScale.x) + (float)(0.3 * element.transform.localScale.x);

					surroundSphere.transform.localScale = new Vector3 (sizeSphere, sizeSphere, sizeSphere);
					surroundSphere.transform.position = new Vector3 (element.transform.position.x, element.transform.position.y, element.transform.position.z);
				}
			} else {
				surroundSphere.transform.position = new Vector3 (100, 100, 100);
			}
		}
	}

	void UpdateZone ()
	{
		if (listElementList [cZ].Count == 0) {
			if (cZ + 1 == maxNumZones) {
				print ("You won!");
			} else {
				cZ++;
				print ("New Zone: " + cZ);
			}
		}
	}

	void OnTriggerExit (Collider coll)
	{

		if (currentGrabbedCube == null) {
			isAnyCubeTouched = false;
			currentTouchedCube = null;
		}
	}

	void OnTriggerStay (Collider coll)
	{

		if (currentTouchedCube == null) {
			isAnyCubeTouched = true;
			currentTouchedCube = coll.gameObject;
		}
	}
}
