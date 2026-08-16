using UnityEngine;
using System.Collections;

public class TriggerDoor : MonoBehaviour {

	public Camera cam;

	RaycastHit hit;

	public GameObject reticle;
	Animator reticleAnimator;

	// Use this for initialization
	void Start () 
	{
		//cam = gameObject.GetComponent<Camera>();
		reticleAnimator = reticle.gameObject.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		Physics.Raycast(cam.transform.position, cam.transform.forward, out hit,4);

		if(Input.GetKeyDown(KeyCode.E))
		{
			reticleAnimator.SetBool("ButtonHold", true);
		}

		else if(Input.GetKeyUp(KeyCode.E))
		{
			reticleAnimator.SetBool("ButtonHold", false);
		}

		if(Input.GetKeyDown(KeyCode.E) && hit.transform == false)
		{
			return;
		}
		else if(Input.GetKeyDown(KeyCode.E) && hit.transform.tag == "Door")
		{

			GameObject currentDoor = hit.collider.transform.parent.gameObject;

			Animator anim = currentDoor.GetComponent<Animator>();

			anim.SetTrigger("Activate");
		}

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}


	}
}
