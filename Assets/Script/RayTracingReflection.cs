using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingReflection : MonoBehaviour
{
    public int maxBounces;
    public float rayLength;
    public GameObject mirror_obj;
    public float ray_step_size = 0.02f;
    private int mirror_type;
    private ArrayList mirror_types;
    private Ray ray;
    

    /// <summary>
    /// A built in function used to simple print the instructions for the player and the mirror currently displayed.
    /// There is a counter called 'mirror_type' that is used to index what the current mirror is as well as the next one.
    /// To check for a next mirror image we can easily look at the next index and take modulo on the lengt of the collection of mirrors.
    /// In C# the length of a collection is accessible to us via the function ".Count".
    /// The code snippet "mirror_types[(mirror_type +1)%mirror_types.Count]);" as such means that we are always looking at the next element
    /// modulo the amount of elements that we have within that list. 
    /// </summary>

    void OnGUI()
    {   
        GUI.Label(new Rect(10, 20, 500, 20), "Instructions:");
        GUI.Label(new Rect(10, 30, 500, 20), "Press up and down key to move the camera view forward and backward.");
        
        // Instructions for rendering a new mirror:
        GUI.Label(new Rect(10, 40, 500, 20), "Press space to re-render the reflection surface with as the next mirror type.");
        GUI.Label(new Rect(10, 60, 500, 20), "Current Mirror Type: " + mirror_types[mirror_type]);
        GUI.Label(new Rect(10, 70, 500, 20), "Next Mirror Type: " + mirror_types[(mirror_type +1)%mirror_types.Count]);

    }
    
    // Start is called before the first frame update
    void Start()
    {   
        // Add the different mirror types:
        mirror_types = new ArrayList();
        mirror_types.Add("Normal Mirror");
        mirror_types.Add("Distorted Height");
        mirror_type = 0;
        Set_White();
        castAllRays();
        
    }

    /// <summary>
    /// A built in function used to simple print the instructions for the player and the mirror currently displayed.
    /// There is a counter called 'mirror_type' that 
    /// </summary>

    void repaint(){
        Set_White();
        castAllRays();
    }

    /// <summary>
    /// Updates the mirror on key input from the user as well as movement of the camera view. 
    /// Press Space to re-render the reflection surface. Press up and down key to move the camera along the z-axis.  
    /// </summary>
    void Update()
    {
        if(Input.GetKeyDown("space")){
            print("Right pressed");
            mirror_type += 1;
            mirror_type %= mirror_types.Count;
            repaint();
        }

        if(Input.GetKeyDown("up")){
            this.transform.position += new Vector3(0, 0, 1);
        }
        
        if(Input.GetKeyDown("down")){
            this.transform.position -= new Vector3(0, 0, 1);
        }
    }


    /// <summary>
    /// A function that sets the entire texture white. We just iterate through each pixel and set them to be white.
    /// </summary>

    void Set_White(){
        Renderer whiteRenderer = mirror_obj.GetComponent<Renderer>();
        Texture2D whiteTexture = (Texture2D)whiteRenderer.material.mainTexture;
        Color32 white = new Color32(255, 255, 255, 0);
        Color32[] pixels = whiteTexture.GetPixels32();

     for (int i = 0; i < pixels.Length; i++) {
        pixels[i] = white;
     }
      
        whiteTexture.SetPixels32(pixels);
        whiteTexture.Apply();
    }

    Ray distort_height(Ray ray, RaycastHit hit){
        return new Ray(hit.point, new Vector3(ray.direction.x/16, 4*ray.direction.y, ray.direction.z));
    }

    /// <summary>
    /// cast one ray from the camera to each pixel on the screen (i.e. image plane). 
    /// These rays preform ray tracing reflections on the objects with the tag "Mirror".
    /// </summary>
    private void castAllRays()
    {   
        // Testar 100x100 rays:
        float height = mirror_obj.transform.localScale.y;
        float width = mirror_obj.transform.localScale.x;
        float step_size = ray_step_size;
       

        for (float x = 0; x < width; x+= step_size){
            for(float y = 0; y < height; y+= step_size){
                ray.origin = this.transform.position;
                Vector3 P2 = mirror_obj.transform.position + new Vector3(-width/2 + x, height/2 - y);
                ray.direction = P2 - ray.origin;
                RaycastHit hit;

                if (Physics.Raycast(ray.origin, ray.direction, out hit, rayLength))
                {   

                    if (hit.collider.tag == "Mirror")
                    {   

                        float remainingLength = rayLength - Vector3.Distance(ray.origin, hit.point);

                        // fist ray hit mirror
                       Color color = recRayRef(hit, remainingLength, maxBounces - 1);
                       
                       // Apply Color:
                       NN_interpolation(hit, color);
                        
                    }
                }
            }
        }
    }

    /// <summary>
    /// A function that uses Nearest Neighbor Interpolation. When a pixel with coordinates (x,y) is painted in a color, its neighbors too are painted.  
    /// </summary>

    private void NN_interpolation(RaycastHit hit, Color color)
    {
        Renderer hitRend = hit.collider.GetComponent<Renderer>();
        Texture2D hitTex = (Texture2D)hitRend.material.mainTexture;
        Vector2 texCoord = hit.textureCoord;
        texCoord.x *= hitTex.width;
        texCoord.y *= hitTex.height;
        
        for(int x = 0; x < 3; x++){
            for(int y = 0; y < 3; y++){
                hitTex.SetPixel(Mathf.FloorToInt(texCoord.x) + x, Mathf.FloorToInt(texCoord.y) + y, color);
            }
        }
        hitTex.Apply();
    }

    /// <summary>
    /// Preform recrusive ray tracing reflection if the object the ray hit has the tag
    /// "Mirror". If the object does not have this tag, the color of the point the ray
    /// hit will be showned on the reflected mirror. If the ray did not hit anything, 
    /// green color will be showned. 
    /// </summary>
    /// <param name="hit">RaycastHit object of the hitting point of the ray</param>
    /// <param name="length">the length of the ray</param>
    /// <param name="bounces">the ramaining bounces/reflections the ray can do</param>
    /// <return>color of the hitting point of the ray</return>
    
    private Color recRayRef(RaycastHit hit, float length, int bounces)
    {
        ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));

        // Distortions:
        if(mirror_type == 1){
            ray = distort_height(ray, hit);
        }
        else if(mirror_type == 2){

        }
        // Distorts the mirror:
        //ray = new Ray(hit.point, distorted_direction_vector);

        if (bounces <= 0)
        {
            // stop the function because exceeding number of bounces
            return new Color(0, 1, 0);  // green color // TODO: change to background color   
        }

        Color pixelColor = new Color(0, 0, 0);  // Default color (black)
        if (Physics.Raycast(ray.origin, ray.direction, out hit, length))
        {
            if (hit.collider.tag == "Mirror")
            {
                // reflected ray hit mirror
                length -= Vector3.Distance(ray.origin, hit.point);
                pixelColor = recRayRef(hit, length, bounces - 1);
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
    /// return the color of the hitting point on the object. 
    /// </summary>
    /// <param name="hit">RaycastHit object of the hitting point of the ray</param>
    /// <return>Color of the hitting point on the object</return>
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
