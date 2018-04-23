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
    [Header("Controller Settings")]
    //Player Speed
    public float speed = 10.0f;
    //Mouse sensitivity
    public float sensitivity = 5.0f;
    //Mouse smoothing
    public float smoothing = 2.0f;

    //Bool hooks here
    bool isUnlocked = false;
    [Header("GameObjects")]
    //First Person Camera
    [SerializeField] private GameObject FPcam;
    //GUI Overlay for escape menu
    public GameObject escapeMenu;

    void Start()
    {
        GameObject escapeOverlay = (GameObject)Instantiate(escapeMenu);
        escapeOverlay.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
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

        if (Input.GetKeyDown("escape"))
        {
            //GameObject escapeOverlay = gameObject.GetGameObject("escapeOverlay");
            if (isUnlocked == true)
            {
                
                Cursor.lockState = CursorLockMode.Locked;
                //escapeOverlay.SetActive(false);
                sensitivity = 5.0f;
                speed = 10f;
                isUnlocked = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                //escapeOverlay.SetActive(true);
                sensitivity = 0f;
                speed = 0f;
                isUnlocked = true;
            }
            
           
        }

    }
}
