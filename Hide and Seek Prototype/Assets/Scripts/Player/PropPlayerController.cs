using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropPlayerController : MonoBehaviour
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

    [Header("GameObjects")]
    //First Person Camera
    //[SerializeField] private GameObject FPcam;
    //GUI Overlay for escape menu
    //public GameObject escapeMenu;
    //Layer Mask To Define Ground Layer
    public LayerMask groundLayer;

    [Header("Additional Controls")]

    public KeyCode sprint;

    //Bool hooks here
    bool isUnlocked = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        /*
        GameObject escapeOverlay = (GameObject)Instantiate(escapeMenu);
        escapeOverlay.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;*/
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
        

        
        //transform.localRotation = Quaternion.AngleAxis(mouseLook.x, transform.up);

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

    //Function to check if character is grounded
    private bool IsGrounded()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit check;

        return (Physics.Raycast(ray, out check, 1 + 0.1f, groundLayer));
        
    }
}
