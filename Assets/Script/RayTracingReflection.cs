using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class RayTracingReflection : MonoBehaviour
{
    public int maxBounces;
    public float rayLength;
    private int width, height;
    private Ray ray;
    public bool NearestNeighborInterpolation;
    public bool LinearInterpolation;


    // Start is called before the first frame update
    void Start()
    {
        width = Camera.main.pixelWidth;
        height = Camera.main.pixelHeight;

        if(NearestNeighborInterpolation)
            LinearInterpolation = false;

        castAllRays();
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// cast one ray for each pixel on the screen the camera shows. These rays
    /// preform ray tracing reflections on the objects with the tag "Mirror".
    /// </summary>
    private void castAllRays()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
                RaycastHit hit;
                // NOTE: does not use layer for Physics.Raycast() since it can´t detect 
                //       if the ray hit some other object before hitting the mirror
                if (Physics.Raycast(ray.origin, ray.direction, out hit, rayLength))
                {
                    if (hit.collider.tag == "Mirror")
                    {
                        float remainingLength = rayLength - Vector3.Distance(ray.origin, hit.point);

                        // fist ray hit mirror
                        Color color = recRayRef(hit, remainingLength, maxBounces - 1);

                        if(LinearInterpolation)
                            linear_interpolation(hit, color);
                        else
                            nearest_neighbor_interpolation(hit, color);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Preform recrusive ray tracing reflection if the object the ray hit has the tag
    /// "Mirror". If the object does not have this tag, the color of the point the ray
    /// hit will be showned on the mirror that reflected that object. If the ray did not 
    /// hit anything, the sky box will be showned. 
    /// </summary>
    /// <param name="hit">RaycastHit object of the hitting point of the ray</param>
    /// <param name="length">the length of the ray</param>
    /// <param name="bounces">the ramaining bounces/reflections the ray can do</param>
    /// <return>color of the hitting point of the ray</return>
    private Color recRayRef(RaycastHit hit, float length, int bounces)
    {
        // NOTE: NOT DONE
        if (bounces <= 0)
        {
            // stop the function because exceeding number of bounces
            // draw background color
            return new Color(0, 1, 0);  // green background // TODO: change to background color   
        }

        ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));
        Color pixelColor = new Color(0, 0, 0);  // Default color (black)
        if (Physics.Raycast(ray.origin, ray.direction, out hit, length))
        {
            if (hit.collider.tag == "Mirror")
            {
                // reflected ray hit mirror
                length -= Vector3.Distance(ray.origin, hit.point);
                pixelColor = recRayRef(hit, length, bounces - 1);
                
                 if(LinearInterpolation)
                    linear_interpolation(hit, pixelColor);
                else
                    nearest_neighbor_interpolation(hit, pixelColor);
            }
            else
            {
                // reflected ray hit an object that is not a mirror
                pixelColor = getObjectColor(hit);
            }
        }
        else
        {
            // reflected ray didn't hit any object
            // TODO: set the pixel color as the backgroud/Skybox
            pixelColor = new Color(0, 1, 0);    // green background
        }

        return pixelColor;
    }

    /// <summary>
    /// set the color of the hitting point as the given color
    /// </summary>
    /// <param name="hit">RaycastHit object of the hitting point of the ray</param>
    /// <param name="color">the color that the hitting point should be set to</param>

    private void nearest_neighbor_interpolation(RaycastHit hit, Color color)
    {

        // TODO: set the mirror pixel color as reflected color
        Renderer hitRend = hit.collider.GetComponent<Renderer>();
        Texture2D hitTex = (Texture2D)hitRend.material.mainTexture;
        Vector2 texCoord = hit.textureCoord;
        texCoord.x *= hitTex.width;
        texCoord.y *= hitTex.height;

        // to avoid the problem where too few rays sending when there are too long distance to 
        // the camera (see report), 4 neighbors pixels (1 hitting point pixel and 3 neighbors 
        // pixels) are set to the given color
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x), Mathf.FloorToInt(texCoord.y), color);
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x + 1), Mathf.FloorToInt(texCoord.y), color);
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x), Mathf.FloorToInt(texCoord.y + 1), color);
        hitTex.SetPixel(Mathf.FloorToInt(texCoord.x + 1), Mathf.FloorToInt(texCoord.y + 1), color);
        hitTex.Apply();
    }

    
    /// <summary>
    /// Linear interpolation means that we have some surrounding points to a center point P 
    /// and those surrounding point would contribute to the colouring of P. The way that we do
    /// this is by doing it the other way around. Instead of having P being the point that checks
    /// its neighbor we let each point that hits the surface contribute somewhat to neighboring
    /// points with their colour. 
    /// </summary>

    private void linear_interpolation(RaycastHit hit, Color color)
    {   
        Renderer hitRend = hit.collider.GetComponent<Renderer>();
        Texture2D hitTex = (Texture2D)hitRend.material.mainTexture;
        Vector2 texCoord = hit.textureCoord;
        texCoord.x *= hitTex.width;
        texCoord.y *= hitTex.height;

        int end_x = Mathf.FloorToInt(texCoord.x + 1);
        int start_x = Mathf.FloorToInt(texCoord.x - 1);
        int end_y = Mathf.FloorToInt(texCoord.y + 1);
        int start_y = Mathf.FloorToInt(texCoord.y - 1);

        // Set neighbor colorings
        for (int i = start_x; i <= end_x; i++)
        {
            for (int j = start_y; j <= end_y; j++)
            {   if(!(i == j))
                    hitTex.SetPixel(i, j, (color)/4 + hitTex.GetPixel(i, j));
            }
        }

        int x = Mathf.FloorToInt(texCoord.x);
        int y = Mathf.FloorToInt(texCoord.y);

         
        //hitTex.SetPixel(x, y, color);
        hitTex.SetPixel(x, y, 2*color / 3 + hitTex.GetPixel(x, y) / 3);
        hitTex.Apply();
    }

    /// <summary>
    /// return the color of the hitting point on the object. 
    /// </summary>
    /// <param name="hit">RaycastHit object of the hitting point of the ray</param>
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
            Color color = objTex.GetPixel(Mathf.FloorToInt(texCoord.x * tiling.x), Mathf.FloorToInt(texCoord.y * tiling.y));
            return color;
        }
    }
}
