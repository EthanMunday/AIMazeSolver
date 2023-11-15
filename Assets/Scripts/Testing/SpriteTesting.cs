using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTesting : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        //spriteRenderer.sprite = Instantiate(spriteRenderer.sprite);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Texture2D tex = new Texture2D(3250, 1650);
            //Vector2[] spriteVerts = spriteRenderer.sprite.vertices;
            //ushort[] spriteTris = spriteRenderer.sprite.triangles;
            //for (int i = 0; i < spriteVerts.Length; i++)
            //{
            //    spriteVerts[i] += Vector2.one;
            //}

            for (int z = 0; z < 50; z++)
            {
                for (int y = 0; y < 50; y++) 
                {
                   tex.SetPixel(z,y,new Color(0,0,0,0));
                }
            }
            for (int z = 50; z < tex.width; z++)
            {
                for (int y = 50; y < tex.height; y++)
                {
                    tex.SetPixel(z, y, new Color(1, 0, 0, 1));
                }
            }
            tex.Apply();
            Sprite x = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0f, 0f));
            
            spriteRenderer.sprite = x;
        }
    }
}
