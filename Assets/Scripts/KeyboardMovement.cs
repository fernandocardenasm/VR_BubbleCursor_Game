using UnityEngine;
using System.Collections;

public class KeyboardMovement : MonoBehaviour
{

	// Use this for initialization

	public float speed;

	private Rigidbody bubble;
	//private GameObject[] elementsZ1;
	private System.Collections.Generic.List <GameObject> elementsZ1;
	private GameObject bucketZ1;
	private Vector3 selectedCubeMove;
	private GameObject lastGrabbedCube;
	private GameObject currentTouchedCube;
	private GameObject currentGrabbedCube;

	private float maxBubbleScale = 5;
	private float minBubbleScale = 1;

	//Material
	private Material primMat;
	

	/*Color guide
	 * The colors are important because thats the way, in which state is each object and then we
	 * execute actions regarding to it.
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

		//elementsZ1 = GameObject.FindGameObjectsWithTag("ElementZ1");
		bucketZ1 = GameObject.FindGameObjectWithTag ("BucketZ1");
		selectedCubeMove = new Vector3 ();
		lastGrabbedCube = null;
		currentTouchedCube = null; 
		currentGrabbedCube = null;

		elementsZ1 = new System.Collections.Generic.List<GameObject> (GameObject.FindGameObjectsWithTag ("ElementZ1"));

		print (elementsZ1.Count);

		primMat = GetComponent<Renderer> ().material;
	}
	
	// Update is called once per frame
	void Update ()
	{
		/*float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

		rb.AddForce (movement * speed);
		*/

		MovementPlayer ();

		IdentifyClosestCubes ();

		AssignTouchedCube ();

		AssignColorsToCubes ();



		if (Input.GetKeyDown (KeyCode.Space)) {
			if (isAnyCubeTouched ()) {
				foreach (GameObject element in elementsZ1) {

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
			GameObject closestCubeB = null;
			float closestDistanceA = Mathf.Infinity;
			float closestDistanceB = Mathf.Infinity;
			float containmentDistanceA = Mathf.Infinity;
			float containmentDistanceB = Mathf.Infinity;

			foreach (GameObject element in elementsZ1) {

				var deltaX = Mathf.Abs (bubble.transform.position.x - element.transform.position.x);
				var deltaY = Mathf.Abs (bubble.transform.position.y - element.transform.position.y);
				var deltaZ = Mathf.Abs (bubble.transform.position.z - element.transform.position.z);

				float intD = Vector3.Distance (bubble.transform.position, element.transform.position);
				float conD = Vector3.Distance (bubble.transform.position, element.transform.position) + element.transform.localScale.x;

				if (intD > 0) {
					if (closestCubeA == null) {
						closestCubeA = element;
						closestDistanceA = intD;
						containmentDistanceA = conD;
					} else if (closestCubeB == null) {
						closestCubeB = element;
						closestDistanceB = intD;
						containmentDistanceB = conD;
					} else {
						if (closestDistanceA > intD) {
							if (closestDistanceA > closestDistanceB) {
								closestCubeA = element;
								closestDistanceA = intD;
								containmentDistanceA = conD;
							}
						} else if (closestDistanceB > intD) {
							if (closestDistanceB > closestDistanceA) {
								closestCubeB = element;
								closestDistanceB = intD;
								containmentDistanceB = conD;
							}
						}
					}
				}
			}

			if (closestDistanceA > closestDistanceB) {
				var tempElementB = closestCubeB; 
				var tempIntB = closestDistanceB;
				var tempConB = containmentDistanceB;

				closestDistanceB = closestDistanceA;
				containmentDistanceB = containmentDistanceA;
				closestCubeB = closestCubeA;

				closestCubeA = tempElementB;
				closestDistanceA = tempIntB;
				containmentDistanceA = tempConB;
			}

			//print ("DA: " + closestDistanceA + " DB: " + closestDistanceB);

			float min = Mathf.Min (containmentDistanceA, closestDistanceB);

			if (min > maxBubbleScale) {
				bubble.transform.localScale = new Vector3 (maxBubbleScale, maxBubbleScale, maxBubbleScale);
			} else {
				bubble.transform.localScale = new Vector3 (min, min, min);
			}
		}
	}

	void AssignTouchedCube ()
	{
		if(isAnyCubeTouched() && currentGrabbedCube == null){
			int cont = 0;
			foreach (GameObject element in elementsZ1) {

				var deltaX = Mathf.Abs (bubble.transform.position.x - element.transform.position.x);
				var deltaY = Mathf.Abs (bubble.transform.position.y - element.transform.position.y);
				var deltaZ = Mathf.Abs (bubble.transform.position.z - element.transform.position.z);

				var scaleDiff = (bubble.transform.localScale + element.transform.localScale) / 2;

				if (deltaX < scaleDiff.x && deltaY < scaleDiff.y && deltaZ < scaleDiff.z) {
					currentTouchedCube = element;
					cont++;

				}
			}
			if (cont == 0) {
				currentTouchedCube = null;
			}
		}
	}

	void AssignColorsToCubes ()
	{
		GameObject objectToDelete = null;
		foreach (GameObject element in elementsZ1) {


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
			//var distance = Vector3.Distance(bubble.transform.position, element.transform.position);
			//print ("position: " + element.name + " Distance: " + distance);

			if (element == lastGrabbedCube && currentGrabbedCube == null && element.transform.position.y > 1) {

				var deltaX = Mathf.Abs (bucketZ1.transform.position.x - element.transform.position.x);
				var deltaY = Mathf.Abs (bucketZ1.transform.position.y - element.transform.position.y);
				var deltaZ = Mathf.Abs (bucketZ1.transform.position.z - element.transform.position.z);

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
			elementsZ1.Remove (objectToDelete);
			Destroy (objectToDelete);
			print ("Destroyed");
			print ("Size: " + elementsZ1.Count);
		}
	}

	bool isAnyCubeTouched ()
	{
		foreach (GameObject element in elementsZ1) {
			var deltaX = Mathf.Abs (bubble.transform.position.x - element.transform.position.x);
			var deltaY = Mathf.Abs (bubble.transform.position.y - element.transform.position.y);
			var deltaZ = Mathf.Abs (bubble.transform.position.z - element.transform.position.z);

			var scaleDiff = (bubble.transform.localScale + element.transform.localScale) / 2;

			if (deltaX <= scaleDiff.x && deltaY <= scaleDiff.y && deltaZ <= scaleDiff.z) {
				return true;
			}
		}
		return false;
	}
		
}
