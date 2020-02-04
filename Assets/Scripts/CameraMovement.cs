using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {


    public int speed = 10;
    public int rotationSpeed = 10;
    public int zoomSpeed = 10;

    public float minZoomDistance = 7.5f;
    public float maxZoomDistance = 30f;



    // Update is called once per frame
    void Update()
    {
        float xMovement = Input.GetAxis("Horizontal");
        float zMovement = Input.GetAxis("Vertical");

        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");

        this.transform.Translate(new Vector3(xMovement, 0.0f, 0.0f) * Time.deltaTime * speed);
        this.transform.Translate(new Vector3(0.0f, zMovement, zMovement) * Time.deltaTime * speed);
        if ((this.transform.position.y >= minZoomDistance && mouseWheel > 0) || (this.transform.position.y <= maxZoomDistance && mouseWheel < 0))
        {
            this.transform.Translate(new Vector3(0.0f, 0.0f, mouseWheel) * Time.deltaTime * zoomSpeed);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            Vector3 rotateValue = new Vector3(0.0f, 1.0f) * Time.deltaTime * rotationSpeed;
            transform.eulerAngles = transform.eulerAngles - rotateValue;
        }

        if (Input.GetKey(KeyCode.E))
        {
            Vector3 rotateValue = new Vector3(0.0f, -1.0f) * Time.deltaTime * rotationSpeed;
            transform.eulerAngles = transform.eulerAngles - rotateValue;
        }
    }
}
