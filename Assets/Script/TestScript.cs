using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class TestScript : MonoBehaviour
{
    public int maxBounces;
    public float maxLength;

    private LineRenderer lineRenderer;
    private Ray ray;

    // my variables
    float t = 0;
    int x = 0, y = 200;
    int width, height;
    Vector3 camPos;
    private int mirrorMask;
    Vector2 rayCoord;       // TODO: use recrusive to be able to reflect several mirrors
    int sxx = 0, syy = 0, xx = 0, yy = 0, exx = 100, eyy = 100;     // DEBUG: TEST VARIABLES


    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        camPos = transform.position;
        width = Camera.main.pixelWidth;
        height = Camera.main.pixelHeight;
        mirrorMask = LayerMask.GetMask("mirrorLayer");
        setXXYY(0, 0, 512, 1024);
    }

    // Update is called once per frame
    void Update()
    {
        castAllRays();
        // castRaysToMirrors();
    }

    private void setPixelColor(RaycastHit hit, Color color)
    {
        // color = new Color(1, 1, 1);     //DEBUG: reset color
        // TODO: set the mirror pixel color as the reflected color
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
        // hitTex.SetPixel(Mathf.FloorToInt(texCoord.x - 1), Mathf.FloorToInt(texCoord.y), color);
        // hitTex.SetPixel(Mathf.FloorToInt(texCoord.x), Mathf.FloorToInt(texCoord.y - 1), color);
        // hitTex.SetPixel(Mathf.FloorToInt(texCoord.x - 1), Mathf.FloorToInt(texCoord.y - 1), color);
        // hitTex.SetPixel(Mathf.FloorToInt(texCoord.x+1), Mathf.FloorToInt(texCoord.y - 1), color);
        // hitTex.SetPixel(Mathf.FloorToInt(texCoord.x - 1), Mathf.FloorToInt(texCoord.y + 1), color);
        incXXYY();
        // hitTex.SetPixel(xx, yy, new Color(1, 0, 0));
        hitTex.Apply();
        // Debug.Log("x: " + hitTex.width + ", y: " + hitTex.height);
        // Debug.Log(hit.collider);
        // Debug.Log("x: " + texCoord.x + ", y: " + texCoord.y);
        // Debug.Log("x: " + x + ", y: " + y);
        // Debug.Log("x: " + xx + ", y: " + yy);
    }

    private void setXXYY(int startX, int startY, int lengthX, int lengthY)
    {
        sxx = startX;
        syy = startY;
        xx = startX;
        yy = startY;
        exx = startX + lengthX;
        eyy = startY + lengthY;
    }

    private void incXXYY()
    {
        if (xx < exx)
        {
            xx += 1;
        }
        else if (yy < eyy)
        {
            xx = sxx;
            yy += 1;
        }
    }

    private void castAllRays()    // first recrusive iteration
    {
        increaseScreen();
        // // Debug.Log("x: " + x + ", y: " + y);

        // DEBUG: test one ray
        // hitting red cube: x=175, y=200
        // reflection hit blue cube: x=100, y=200
        // reflection hit nothing: x=160, y=160
        // x = 100;
        // y = 150;
        // DEBUG: test code for line
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, camPos);

        ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hit;
        // NOTE: does not use layer for Physics.Raycast() since it can´t detect 
        //       if the ray hit some other object before hitting the mirror
        if (Physics.Raycast(ray.origin, ray.direction, out hit, maxLength))
        {
            // DEBUG: test code for line
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);

            if (hit.collider.tag == "Mirror")
            {
                float remainingLength = maxLength - Vector3.Distance(ray.origin, hit.point);
                Color color = recRayRef(hit, remainingLength, maxBounces - 1);
                // Debug.Log("fist ray hit mirror");
                setPixelColor(hit, color);
            }
        }
        else
        {
            // Debug.Log("first ray did not hit mirror");

            // DEBUG: test code for line
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, ray.origin + ray.direction * maxLength);
        }

    }

    // private Color recRayRef(Ray ray, float length, int bounces)    // recrusive
    private void castRaysToMirrors()
    {
        increaseMirrors();
        // // Debug.Log("x: " + x + ", y: " + y);

        // DEBUG: test one ray
        // hitting red cube: x=175, y=200
        // reflection hit blue cube: x=100, y=200
        // reflection hit nothing: x=160, y=160
        // x = 100;
        // y = 150;
        // DEBUG: test code for line
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, camPos);

        ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hit;
        // NOTE: does not use layer for Physics.Raycast() since it can´t detect 
        //       if the ray hit some other object before hitting the mirror
        if (Physics.Raycast(ray.origin, ray.direction, out hit, maxLength))
        {
            // DEBUG: test code for line
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);

            if (hit.collider.tag == "Mirror")
            {
                float remainingLength = maxLength - Vector3.Distance(ray.origin, hit.point);
                Color color = recRayRef(hit, remainingLength, maxBounces - 1);
                // Debug.Log("fist ray hit mirror");
                setPixelColor(hit, color);
            }
        }
        else
        {
            // Debug.Log("first ray did not hit mirror");

            // DEBUG: test code for line
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, ray.origin + ray.direction * maxLength);
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
            // DEBUG: line code
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);

            if (hit.collider.tag == "Mirror")
            {
                // // Debug.Log("hit mirror");
                length -= Vector3.Distance(ray.origin, hit.point);
                pixelColor = recRayRef(hit, length, bounces - 1);
                setPixelColor(hit, pixelColor);
            }
            else
            {
                // DEBUG: line code
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);

                // Kopierad Kod:
                pixelColor = getObjectColor(hit);

            }
        }
        else
        {
            // Debug.Log("reflected ray didn't hit any object");
            // TODO: set the pixel color as the backgroud/Skybox
            Vector3 direction = ray.direction;
            // (RenderSettings.skybox.GetTexture("Left") as Texture2D).GetPixelBilinear((direction.z/-direction.x+1)/2, (direction.y/-direction.x+1)/2);
            pixelColor = new Color(0, 1, 0);    // green background
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, ray.origin + ray.direction * length);
        }

        return pixelColor;
    }

    private Color getObjectColor(RaycastHit hit)
    {
        Renderer objRend = hit.transform.GetComponent<MeshRenderer>();
        Texture2D objTex = objRend.material.mainTexture as Texture2D;
        if (objTex == null)
        {
            // reflected ray hit an object without texture
            return objRend.material.color;
        }
        else
        {
            // reflected ray hit an object with texture            
            Vector2 texCoord = hit.textureCoord;
            texCoord.x *= objTex.width;
            texCoord.y *= objTex.height;
            Vector2 tiling = objRend.material.mainTextureScale;
            Color color = objTex.GetPixel(Mathf.FloorToInt(texCoord.x * tiling.x) , Mathf.FloorToInt(texCoord.y * tiling.y));
            return color;
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
            y += 1;
        }
        // }
    }

    private void increaseMirrors()
    {
        GameObject[] mirrors = GameObject.FindGameObjectsWithTag("Mirror");
        foreach (GameObject mirror in mirrors)
        {
            Debug.Log(mirror);
            Vector3 pos = mirror.transform.position;
            Vector2 pos2 = Camera.main.WorldToScreenPoint(pos);
            Debug.Log(mirror + ", " + pos2);
        }
    }
}

