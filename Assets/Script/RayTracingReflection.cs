using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class RayTracingReflection : MonoBehaviour
{
    public float rayLength;
    public int reflections;
    private LineRenderer testLine;
    private int mirrorMask;
    private int width, height;
    private Vector3 camPos;
    private RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        testLine = GetComponent<LineRenderer>();
        mirrorMask = LayerMask.GetMask("mirrorLayer");
        camPos = transform.position;
        width = Camera.main.pixelWidth;
        height = Camera.main.pixelHeight;

        castAllRays();
    }

    // Update is called once per frame
    void Update()
    {
        // Ray ray = Camera.main.ScreenPointToRay(new Vector3(0, 0, 20));
        // Vector3 pos = transform.position;
        // Vector3 forward = transform.forward;

        // Ray ray = new Ray(transform.position, transform.forward);
        // Debug.Log("pos: " + pos);
        // Debug.Log("for: " + forward);

        // RaycastHit rayHit;
        // if (Physics.Raycast(ray, out rayHit, rayLength))
        // {
        //     // Debug.Log("Distance" + rayHit.distance);
        //     // Debug.Log("Name" + rayHit.transform.gameObject.name);
        // }
    }

    private void castAllRays(){
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // test code for line
                LineRenderer lRend = createLineRendComp(x, y);
                lRend.positionCount = 1;
                lRend.SetPosition(0, camPos);

                float remainingLength = rayLength;
                for (int i = 0; i < reflections; i++)
                {
                    Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
                    if (Physics.Raycast(ray.origin, ray.direction, out hit, rayLength, mirrorMask))
                    {
                        // test code for line
                        lRend.positionCount += 1;
                        lRend.SetPosition(lRend.positionCount - 1, hit.point);

                        remainingLength -= Vector3.Distance(ray.origin, hit.point);
                        ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));
                    } else
                    {   
                        // test code for line: will cast a ray though object that are not mirrors
                        lRend.positionCount += 1;
                        lRend.SetPosition(testLine.positionCount - 1, ray.origin + ray.direction * remainingLength);
                        break;
                    }
                }
            }
        }
    }

    private LineRenderer createLineRendComp(int x, int y){
        GameObject gObject = new GameObject("LineGameObject" + x + y);

        LineRenderer lRend = gObject.AddComponent<LineRenderer>();

        lRend.startColor = Color.red;
        lRend.endColor = Color.blue;
        lRend.startWidth = 0.1f;
        lRend.endWidth = 0.1f;
        return lRend;
    }
}
