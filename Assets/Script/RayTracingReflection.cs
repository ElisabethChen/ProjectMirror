using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class RayTracingReflection : MonoBehaviour
{
    public int maxBounces;
    public float rayLength;
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
        // // Debug.Log("pos: " + pos);
        // // Debug.Log("for: " + forward);

        // RaycastHit rayHit;
        // if (Physics.Raycast(ray, out rayHit, rayLength))
        // {
        //     // // Debug.Log("Distance" + rayHit.distance);
        //     // // Debug.Log("Name" + rayHit.transform.gameObject.name);
        // }
    }

    private void castAllRays(){
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                forRayTracing(x, y);

            }
        }
    }

    private void forRayTracing(int x, int y){
        // test code for line
        // LineRenderer lRend = createLineRendComp(x, y);
        // lRend.positionCount = 1;
        // lRend.SetPosition(0, camPos);

        Color pixelColor;
        float length = rayLength;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));

        for (int i = 0; i < maxBounces; i++)
        {
            if (Physics.Raycast(ray.origin, ray.direction, out hit, length))
            {
                // lRend.positionCount += 1;
                // lRend.SetPosition(lRend.positionCount - 1, hit.point);
                Renderer tex = (Renderer) hit.collider.gameObject.GetComponent<MeshRenderer>();
                if (hit.collider.tag == "Mirror")
                {
                    length -= Vector3.Distance(ray.origin, hit.point);
                    ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));
                    // Debug.Log("hit mirror");
                }
                else
                {
                    pixelColor = new Color(1, 0, 0);    // TODO: set object color
                    setPixelColor(x, y, pixelColor);
                    // Debug.Log("ray hit a object");
                    break;
                }
            } else
            {
                // lRend.positionCount += 1;
                // lRend.SetPosition(lRend.positionCount - 1, ray.origin + ray.direction * remainingLength);
                if (i == 0)     // the ray from the camera did not hit any object
                {
                    // // Debug.Log("First ray did not hit any object");
                    break;
                }
                // Debug.Log("reflection did not hit any object");
                // if the ray is a reflection of a mirror, then:
                // TODO: set the pixel color as the backgroud/Skybox
                break;
            }
        }
    } 

    private void setPixelColor(int x, int y, Color color){

        // TODO: set the mirror pixel color as reflected color
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
