using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpollation : MonoBehaviour
{   
    Texture2D texture;
    
    void Start()
    {
        texture = (Texture2D)GetComponent<Renderer>().material.mainTexture;
        Debug.Log(texture);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void paintWhite(){
        //Color white = new Color(0,0,0);
        //texture.setPixel(0, 0, white);
    }
}
