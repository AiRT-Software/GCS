using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using System.Diagnostics;

public class Youtube : MonoBehaviour {
    public GameObject aux;
    long time;
    private void Start()
    {
        Stopwatch watch = new Stopwatch();
        //byte[] buffer = new byte[16 * 1024];
        /*
        Stream stream = File.OpenRead("D:/Videos/Flor.jpg");
        using (MemoryStream ms = new MemoryStream())
        {
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            array = ms.ToArray();
        }
        */
        
        


        //Texture2D tex = new Texture2D(4, 4);
        /*
        int width = 640;
        int height = 480;

        tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        //tex.filterMode = FilterMode.Point;
        watch.Start();

        Color32[] colors = new Color32[640 * 480];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                colors[i + j] = new Color32((byte)i, (byte)j, 0, 0);
            }
        }
        
        tex.SetPixels32(colors);
        */

        time = watch.ElapsedMilliseconds;
        UnityEngine.Debug.Log(time);


        //tex.LoadImage(array);
        //tex.Apply();

        //aux.GetComponent<MeshRenderer>().material.mainTexture = tex;
       
        //watch.Stop();

        //IEnumerator coroutine = download();

        //StartCoroutine(coroutine);
    }

    // Use this for initialization
    IEnumerator download () {
        var escName = WWW.EscapeURL("D:/Videos/Tekken 7/Tekken 7 360 2018.01.26 - 15.59.07.58.jpg");
        escName = "file:///" + escName;
        WWW youtubeObj = new WWW(escName);
        // Create a stream for the file
        yield return youtubeObj;

        // Make sure the movie is ready to start before we start playing
        var movieTexture = youtubeObj.texture;


        var gt = aux.GetComponent<MeshRenderer>().material.mainTexture;

        // Initialize gui texture to be 1:1 resolution centered on screen
        gt = movieTexture;

        transform.localScale = new Vector3(0, 0, 0);
        transform.position = new Vector3(0.5f, 0.5f, 0.0f);
        

        // Assign clip to audio source
        // Sync playback with audio
        var aud = GetComponent<AudioSource>();
        //aud.clip = movieTexture.audioClip;

        // Play both movie & sound
        //movieTexture.Play();
        //aud.Play();

    }
    
    // Update is called once per frame
    void Update () {
		
	}
}



