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
    private Ray ray;


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

    private void castAllRays()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // forRayTracing(x, y);

                ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
                RaycastHit hit;
                // NOTE: does not use layer for Physics.Raycast() since it can´t detect 
                //       if the ray hit some other object before hitting the mirror
                if (Physics.Raycast(ray.origin, ray.direction, out hit, rayLength))
                {
                    // DEBUG: test code for line
                    if (hit.collider.tag == "Mirror")
                    {
                        float remainingLength = rayLength - Vector3.Distance(ray.origin, hit.point);
                        Color color = recRayRef(hit, remainingLength, maxBounces - 1);
                        // Debug.Log("fist ray hit mirror");
                        setPixelColor(hit, color);
                    }
                }
            }
        }
    }

    private Color recRayRef(RaycastHit hit, float length, int bounces)    // recrusive
    {
        // NOTE: NOT DONE
        if (bounces <= 0)
        {
            // stop the function because exceeding number of bounces
            // draw background color
            return new Color(0, 1, 0);  // green background // TODO: change to background color   
        }

        ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));
        Color pixelColor = new Color(0, 0, 0);  // black
        if (Physics.Raycast(ray.origin, ray.direction, out hit, length))
        {
            if (hit.collider.tag == "Mirror")
            {
                // // Debug.Log("hit mirror");
                length -= Vector3.Distance(ray.origin, hit.point);
                pixelColor = recRayRef(hit, length, bounces - 1);
                setPixelColor(hit, pixelColor);
            }
            else
            {
                // Kopierad Kod:
                Renderer renderer = hit.transform.GetComponent<MeshRenderer>();
                MeshCollider meshCollider = hit.collider as MeshCollider;
                Texture2D texture2D = renderer.material.mainTexture as Texture2D;
                
                if(texture2D == null){
                    // If a reflected ray hits a material without a texture, set the color of said material to its color:
                    pixelColor = renderer.material.color;
                }
                else{
                    // TODO (low priority): Handle color changes if mirror reflected ray hits a texture  
                    pixelColor = new Color(0, 1, 1);    // cyan
                    //Debug.Log(texture2D);
                    //Vector2 pCoord = hit.textureCoord;
                    //pCoord.x *= texture2D.width;
                    //pCoord.y *= texture2D.height;
                    //Vector2 tiling = renderer.material.mainTextureScale;
                    //Color color = texture2D.GetPixel(Mathf.FloorToInt(pCoord.x * tiling.x) , Mathf.FloorToInt(pCoord.y * tiling.y));
                }

            }
        }
        else
        {
            // Debug.Log("reflected ray didn't hit any object");
            // TODO: set the pixel color as the backgroud/Skybox
            pixelColor = new Color(0, 1, 0);    // green background
        }

        return pixelColor;
    }


    private void forRayTracing(int x, int y)
    {
        // test code for line
        // LineRenderer lRend = createLineRendComp(x, y);
        // lRend.positionCount = 1;
        // lRend.SetPosition(0, camPos);

        Color pixelColor;
        float length = rayLength;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hit;

        for (int i = 0; i < maxBounces; i++)
        {
            if (Physics.Raycast(ray.origin, ray.direction, out hit, length))
            {
                // lRend.positionCount += 1;
                // lRend.SetPosition(lRend.positionCount - 1, hit.point);
                Renderer tex = (Renderer)hit.collider.gameObject.GetComponent<MeshRenderer>();
                if (hit.collider.tag == "Mirror")
                {
                    length -= Vector3.Distance(ray.origin, hit.point);
                    ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));
                    // Debug.Log("hit mirror");
                }
                else
                {
                    pixelColor = new Color(1, 0, 0);    // TODO: set object color
                    setPixelColor(hit, pixelColor);
                    // Debug.Log("ray hit a object");
                    break;
                }
            }
            else
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

    private void setPixelColor(RaycastHit hit, Color color)
    {

        // TODO: set the mirror pixel color as reflected color
        // color = new Color(1, 0, 1);
        Renderer hitRend = hit.collider.GetComponent<Renderer>();
        Texture2D hitTex = (Texture2D)hitRend.material.mainTexture;
        Vector2 texCoord = hit.textureCoord;
        texCoord.x *= hitTex.width;
        texCoord.y *= hitTex.height;
        // Debug.Log(texCoord.x);
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x), Mathf.FloorToInt(texCoord.y), color);
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x + 1), Mathf.FloorToInt(texCoord.y), color);
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x), Mathf.FloorToInt(texCoord.y + 1), color);
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x + 1), Mathf.FloorToInt(texCoord.y + 1), color);
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x - 1), Mathf.FloorToInt(texCoord.y), color);
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x), Mathf.FloorToInt(texCoord.y - 1), color);
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x - 1), Mathf.FloorToInt(texCoord.y - 1), color);
        // hitTex.SetPixel((int) texCoord.x, (int)texCoord.y, color);
        // hitTex.SetPixel((int) texCoord.x, (int)texCoord.y, new Color(1, 1, 0));
        // hitTex.SetPixel(xx, yy, new Color(1, 0, 0));
        hitTex.Apply();
        // Debug.Log("x: " + hitTex.width + ", y: " + hitTex.height);
        // Debug.Log(hit.collider);
        Debug.Log("x: " + texCoord.x + ", y: " + texCoord.y);
        // Debug.Log("x: " + xx + ", y: " + yy);
    }

    private LineRenderer createLineRendComp(int x, int y)
    {
        GameObject gObject = new GameObject("LineGameObject" + x + y);

        LineRenderer lRend = gObject.AddComponent<LineRenderer>();

        lRend.startColor = Color.red;
        lRend.endColor = Color.blue;
        lRend.startWidth = 0.1f;
        lRend.endWidth = 0.1f;
        return lRend;
    }
}
