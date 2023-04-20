using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Cube
{
    public Vector3 position;
    public Color color;
};

public class ShaderTesting : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    public int repetition = 1;
    public int count = 50;
    public Material material;
    public Mesh mesh;
    private List<GameObject> objects;
    private Cube[] data;
    // Start is called before the first frame update
    void Start()
    {
        CreateCubes();
    }

    private void CreateCubes()
    {
        objects = new List<GameObject>();

        data = new Cube[count * count];

        for(int x = 0; x < count; x++)
        {
            for(int y = 0; y < count; y++)
            {
                CreateCube(x, y);
            }
        }
    }

    private void CreateCube(int x, int y)
    {
        GameObject cube = new GameObject("Cube  " + x * count + y, typeof(MeshFilter), typeof(MeshRenderer));
        cube.GetComponent<MeshFilter>().mesh = mesh;
        cube.GetComponent<MeshRenderer>().material = material;
        cube.transform.position = new Vector3(x, y, Random.Range(-0.1f, 0.1f));

        Color color = Random.ColorHSV();
        cube.GetComponent<MeshRenderer>().material.color = color;

        objects.Add(cube);

        Cube cubeData = new Cube();
        cubeData.position = cube.transform.position;
        cubeData.color = color;
        data[x * count + y] = cubeData;

    }


    //private void OnRenderImage(RenderTexture src, RenderTexture dest)
    //{
    //    if (renderTexture == null)
    //    {
    //        renderTexture = new RenderTexture(256, 256, 24);
    //        renderTexture.enableRandomWrite = true;
    //        renderTexture.Create();
    //    }

    //    computeShader.SetTexture(0, "Result", renderTexture);
    //    computeShader.SetFloat("Resolution", renderTexture.width);
    //    computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
    //    Graphics.Blit(renderTexture, dest);
    //}

    public void OnRandomizeCPU()
    {
        for (int i = 0; i < repetition; i++)
        {
            for (int j = 0; j < objects.Count; j++)
            {
                GameObject obj = objects[j];
                obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, Random.Range(-0.1f, 0.1f));
                obj.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());
            }
        }
    }

    public void OnRandomizeGPU()
    {
        int colorSize = sizeof(float) * 4;
        int vector3Size = sizeof(float) * 3;
        int totalSize = colorSize + vector3Size;

        ComputeBuffer cubeBuffer = new ComputeBuffer(data.Length,totalSize);
        cubeBuffer.SetData(data);

        computeShader.SetBuffer(0, "cubes", cubeBuffer);
        computeShader.SetFloat("Resolution", data.Length);
        computeShader.SetFloat("Repetitions", repetition);
        computeShader.Dispatch(0, data.Length / 10, 1, 1);

        cubeBuffer.GetData(data);

        for (int j = 0; j < objects.Count; j++)
        {
            GameObject obj = objects[j];
            Cube cube = data[j];
            obj.transform.position = cube.position;
            obj.GetComponent<MeshRenderer>().material.SetColor("_Color", cube.color);
        }

        cubeBuffer.Dispose();
    }

    private void OnGUI()
    {
        if (objects == null)
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), "Create"))
            {
                CreateCubes();
            }
        }
        else
        {
            if(GUI.Button(new Rect(0,0,100,50),"Randomize CPU"))
            {
                OnRandomizeCPU();
            }
            else if(GUI.Button(new Rect(100, 0, 100, 50), "Randomize GPU"))
            {
                OnRandomizeGPU();
            }
        }
    }


}
