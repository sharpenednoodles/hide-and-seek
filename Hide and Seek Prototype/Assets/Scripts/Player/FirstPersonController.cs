using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//todo add jumping

/// <summary>
/// This is a basic firstperson controller that handles firstperson control
/// Hopefully this will be easier to use than the standard asset
/// </summary>

public class FirstPersonController : MonoBehaviour
{
    Vector2 mouseLook;
    Vector2 smoothV;
    private Rigidbody rb;
    [Header("Controller Settings")]
    //Player Speed
    public float walkSpeed = 10.0f;
    //Mouse sensitivity
    public float sensitivity = 5.0f;
    //Mouse smoothing
    public float smoothing = 2.0f;
    public float jumpForce = 5f;
    public float sprintSpeed= 20f;

	// Inventory pick up system code
	public Inventory inventory;


    [Header("GameObjects")]
    //First Person Camera
    [SerializeField] private GameObject FPcam;
    //GUI Overlay for escape menu
    public GameObject escapeMenu;
    //Layer Mask To Define Ground Layer
    public LayerMask groundLayer;

    [Header("Additional Controls")]

    public KeyCode sprint;

    //Bool hooks here
    bool isUnlocked = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
  
//        GameObject escapeOverlay = (GameObject)Instantiate(escapeMenu);
//        escapeOverlay.SetActive(false);
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
    }

    void Update()
    {
        float speed = walkSpeed;
        float translation = Input.GetAxis("Vertical") * speed;
        float strafe = Input.GetAxis("Horizontal") * speed;
        translation *= Time.deltaTime;
        strafe *= Time.deltaTime;

        transform.Translate(strafe, 0, translation);

        var mouseDirection = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        mouseDirection = Vector2.Scale(mouseDirection, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
        smoothV.x = Mathf.Lerp(smoothV.x, mouseDirection.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, mouseDirection.y, 1f / smoothing);
        mouseLook += smoothV;
        mouseLook.y = Mathf.Clamp(mouseLook.y, -65f, 65f);
        

        FPcam.transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        transform.localRotation = Quaternion.AngleAxis(mouseLook.x, transform.up);

		//Handle Pause Menu (Duplicate with Game Menu Controller)
        /*if (Input.GetKeyDown("escape"))
        {
            if (isUnlocked == true)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                escapeMenu.SetActive(false);
                sensitivity = 5.0f;
                speed = 10f;
                isUnlocked = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                escapeMenu.SetActive(true);
                sensitivity = 0f;
                speed = 0f;
                isUnlocked = true;
            }
        }*/

        if  (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce * 100, ForceMode.Impulse);
        }

        //not working
        if (Input.GetKey(sprint))
        {
            speed = sprintSpeed;
        }

    }


	// Pick up weapon (if the player hit weapon it store in the list)
	private void OnControllColliderHit(ControllerColliderHit hit)
	{
		InventoryItem item = hit.collider.GetComponent<InventoryItem> ();


		if (item != null)
		{
			inventory.AddItem (item);
		}

	}


    //Function to check if character is grounded
    private bool IsGrounded()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit check;

        return (Physics.Raycast(ray, out check, 1 + 0.1f, groundLayer));
        
    }
}
