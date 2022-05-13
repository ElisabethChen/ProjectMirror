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
    int x = 0, y = 0;
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
        drawRays2();

    }
    private void drawRays()     // original, draw one ray per frame 
    {
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
                if (hit.collider.tag != "Mirror")
                    break;
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

    private void drawRays2()    // using for-loop
    {
        // GOING THROUGH ALL SCREEN PIXEL
        increaseScreen();
        Debug.Log("x: " + x + ", y: " + y);
        ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, camPos);
        float remainingLength = maxLength;
        Color pixelColor;

        for (int i = 0; i < reflections; i++)
        {
            if (Physics.Raycast(ray.origin, ray.direction, out hit, remainingLength))
            {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);
                if (hit.collider.tag == "Mirror")
                {
                    remainingLength -= Vector3.Distance(ray.origin, hit.point);
                    ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));
                    Debug.Log("hit mirror");
                }
                else
                {
                    pixelColor = new Color(1, 0, 0);    // TODO: set object color
                    setPixelColor(x, y, pixelColor);
                    Debug.Log("ray hit a object");
                    break;
                }
            } else
            {
                 lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, ray.origin + ray.direction * remainingLength);
                if (i == 0)     // the ray from the camera did not hit any object
                {
                    Debug.Log("First ray did not hit any object");
                    break;
                }
                Debug.Log("reflection did not hit any object");
                // if the ray is a reflection of a mirror, then:
                // TODO: set the pixel color as the backgroud/Skybox
                break;
            }
        }
    }

    private void setPixelColor(int x, int y, Color color){
        // TODO: set the mirror pixel color as reflected color
    }

    /* USING IN THIS WAY:
    ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
    drawRays3(ray, rayLength, reflections);
     */
    private void drawRays3(Ray ray, float remainingLength, int bounces)    // recrusive
    {
        // NOTE: NOT DONE
        // GOING THROUGH ALL SCREEN PIXEL
        increaseScreen();
        Debug.Log("x: " + x + ", y: " + y);

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, camPos);
        // float remainingLength = maxLength;
        Color pixelColor;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, remainingLength))
        {
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);
            if (hit.collider.tag == "Mirror")
            {
                remainingLength -= Vector3.Distance(ray.origin, hit.point);
                ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));
                drawRays3(ray, remainingLength, bounces-1);
            }
            else
            {
                pixelColor = new Color(1, 0, 0);
            }
        }
    }

    void increaseScreen()   // increase x and y coordinate for the screen
    {
        // if (Time.time - t > 0.5)
        // {
        t = Time.time;
        if (x < width - 1)
        {
            x += 1;
        }
        else if (y < height - 1)
        {
            x = 0;
            y += 10;
        }
        // }
    }
}

