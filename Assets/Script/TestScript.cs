using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class TestScript : MonoBehaviour
{
    public int reflections;
    public float maxLength;

    private LineRenderer lineRenderer;
    private Ray ray;
    private RaycastHit hit;
    private Vector3 direction;

    // my variables
    float t = 0;
    float x = 0, y = 0;
    int width, height;
    Vector3 camPos;
    private int mirrorMask;


    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        camPos = transform.position;
        width = Camera.main.pixelWidth;
        height = Camera.main.pixelHeight;
        mirrorMask = LayerMask.GetMask("mirrorLayer");
    }

    // Update is called once per frame
    void Update()
    {
        // ray = Camera.main.ScreenPointToRay(new Vector3(0, 0, 1));
        // ray = new Ray(transform.position, new Vector3(x + camPos.x, y + camPos.y, 50));
        // Debug.Log("x: " + -(width/2));
        // ray = new Ray(camPos, transform.forward);

        // GOING THROUGH ALL SCREEN PIXEL
        increaseScreen();
        Debug.Log("x: " + x + ", y: " + y);
        ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, camPos);
        float remainingLength = maxLength;

        for (int i = 0; i < reflections; i++)
        {
            if (Physics.Raycast(ray.origin, ray.direction, out hit, remainingLength, mirrorMask))
            {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);
                // if (hit.collider.tag != "Mirror")
                //     break;
                remainingLength -= Vector3.Distance(ray.origin, hit.point);
                ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));
            }
            else
            {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, ray.origin + ray.direction * remainingLength);
            }
        }
    }

    void increaseScreen(){
        // if (Time.time - t > 0.005)
        // {
            t = Time.time;
            if (x < width - 1)
            {
                x += 1;
            }
            else if(y < height - 1)
            {
                x = 0;
                y += 10;
            }
        // }
    }

    void increase1(){
        if (x < 1)
        {
            x += 0.001f;
        }
        else if(y > -1)
        {
            x = -1;
            y -= 0.001f;
        }
        ray = new Ray(camPos, new Vector3(x, y, 1));
    }
}

