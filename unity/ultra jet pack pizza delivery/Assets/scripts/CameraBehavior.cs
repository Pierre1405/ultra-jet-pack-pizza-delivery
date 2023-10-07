using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public GameObject salaryMan;
    public GameObject salaryManRender;
    public GameObject colliderParent;
    public float xPositionAdaptater;
    public float cameraTransitionDuration;

    private float deltaPosition = 0;
    private float targetDeltaPosition = 0;
    private float minXCamera;
    private float maxXCamera;
    private float minYCamera;
    private float maxYCamera;
    private Bounds allColiderBounds = new Bounds();

    void Start()
    {
        Collider2D[] allColider = colliderParent.GetComponentsInChildren<Collider2D>();
        foreach(Collider2D collider in allColider)
        {
            allColiderBounds.Encapsulate(collider.bounds);
        }
        Camera camera = GetComponent<Camera>();
        float screenHeight = camera.orthographicSize;
        float screenWidth = camera.orthographicSize / Screen.height * Screen.width;
        minXCamera = allColiderBounds.min.x + screenWidth;
        maxXCamera = allColiderBounds.max.x - screenWidth;
        minYCamera = allColiderBounds.min.y + screenHeight;
        maxYCamera = allColiderBounds.max.y - screenHeight;
    }

    void Update()
    {
        transform.position = new Vector3(
            Mathf.Clamp(salaryMan.transform.position.x + deltaPosition, minXCamera, maxXCamera),
            Mathf.Clamp(salaryMan.transform.position.y, minYCamera, maxYCamera), 
            transform.position.z
        );
    }

    public void FixedUpdate()
    {
        targetDeltaPosition = salaryManRender.transform.localScale.x * xPositionAdaptater;
        float xPositionInFrontOfSalaryMan = salaryMan.transform.position.x + targetDeltaPosition;
        if (xPositionInFrontOfSalaryMan < minXCamera)
        {
            targetDeltaPosition = minXCamera - transform.position.x;
        }
        if (xPositionInFrontOfSalaryMan > maxXCamera)
        {
            targetDeltaPosition = maxXCamera - transform.position.x;
        }
        deltaPosition += (targetDeltaPosition - deltaPosition) / cameraTransitionDuration;
    }
}
