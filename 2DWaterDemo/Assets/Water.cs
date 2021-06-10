using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Water : MonoBehaviour
{
    /*.................. VARIABLES ............................*/

    private LineRenderer bodyOfWater; // create a Line Renderer to render springs

    // spring properties
    public float springConstant = 0.02f;
    public float springDamping = 0.04f;
    public float springSpread = 0.05f;
    public float springMass = 40;
    public float radiusWave = 6;
    public int density = 5;


    // physic array of springs, contains position, velocity and acceleration of them
    internal float[] xPositionOfSrpings;
    internal float[] yPositionOfSrpings;
    internal float[] velocities;
    internal float[] accelerations;

    
    private GameObject[] meshObjects; // each spring is a GameObject, so we need shape and collision for all of them

    private Mesh[] meshes;  // shape for colliders;

    private GameObject waterMat;  //add Mesh Render refer from original GameObject

    // properties of water
    private float heightOfWater;
    private float widthOfWater;


    /*.................. DEFAULT METHODS ......................*/
    void Start()
    {
        heightOfWater = transform.localScale.y;
        widthOfWater = transform.localScale.x;
        SpawnWater();
    }

    void FixedUpdate()
    {
        for (int i = 0; i < xPositionOfSrpings.Length; i++)
        {
            float force = springConstant * (yPositionOfSrpings[i] - (transform.position.y + heightOfWater / 2)) + velocities[i] * springDamping;
            accelerations[i] = -force;
            yPositionOfSrpings[i] += velocities[i];
            velocities[i] += accelerations[i];
            bodyOfWater.SetPosition(i, new Vector3(xPositionOfSrpings[i], yPositionOfSrpings[i], transform.position.z));
        }

        // store the difference in heights
        float[] leftDeltas = new float[xPositionOfSrpings.Length];
        float[] rightDeltas = new float[xPositionOfSrpings.Length];

        // make wave's radius
        for (int j = 0; j < radiusWave; j++)
        {
            for (int i = 0; i < xPositionOfSrpings.Length; i++)
            {
                // check the heights of the nearby nodes and change heights of them
                if (i > 0)
                {
                    leftDeltas[i] = springSpread * (yPositionOfSrpings[i] - yPositionOfSrpings[i - 1]);
                    velocities[i - 1] += leftDeltas[i];
                }

                if (i < xPositionOfSrpings.Length - 1)
                {
                    rightDeltas[i] = springSpread * (yPositionOfSrpings[i] - yPositionOfSrpings[i + 1]);
                    velocities[i + 1] += rightDeltas[i];
                }
            }

            // apply a difference in position
            for (int i = 0; i < xPositionOfSrpings.Length; i++)
            {
                if (i > 0)
                    yPositionOfSrpings[i - 1] += leftDeltas[i];
                if (i < xPositionOfSrpings.Length - 1)
                    yPositionOfSrpings[i + 1] += rightDeltas[i];
            }
        }

        //update the meshes to reflect this
        UpdateMeshes();
    }
    /*.................. SUPPORT METHODS ......................*/

    void SpawnFakeWaterMat()
    {
        waterMat = new GameObject();
        waterMat.transform.SetParent(gameObject.transform);
        waterMat.transform.parent = null;
        waterMat.AddComponent<MeshRenderer>();
        waterMat.GetComponent<MeshRenderer>().material = gameObject.GetComponent<SpriteRenderer>().material;
        waterMat.AddComponent<MeshFilter>();

        // disable original renderer, because we do not need anymore
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    void SpawnWater()
    {
        SpawnFakeWaterMat();


        // count the number of edges we will create
        int edgeCount = Mathf.RoundToInt(widthOfWater) * density;
        int nodeCount = edgeCount + 1;

        // setup the line renderer for water
        bodyOfWater = gameObject.AddComponent<LineRenderer>();
        bodyOfWater.material = waterMat.GetComponent<MeshRenderer>().material; // add material for water
        bodyOfWater.material.renderQueue = 2000; // set Rendering Queue tag is geometry
        bodyOfWater.positionCount = nodeCount; // set the number of vertices
        bodyOfWater.startWidth = 0.1f;
        bodyOfWater.endWidth = 0.1f;

        // setup physic array of springs
        xPositionOfSrpings = new float[nodeCount];
        yPositionOfSrpings = new float[nodeCount];
        velocities = new float[nodeCount];
        accelerations = new float[nodeCount];

        // setup new gamebjects of spring
        meshObjects = new GameObject[nodeCount];
        // colliders = new GameObject[nodeCount];
        meshes = new Mesh[nodeCount];


        // for nodes, set the line renderer for each of them
        for (int i = 0; i < nodeCount; i++)
        {
            // because xPosition of original is further, so we must minus x value
            xPositionOfSrpings[i] = (transform.position.x - widthOfWater / 2) + (i * widthOfWater) / nodeCount;
            // because yPosition of original is lower, so we must plus more y value 
            yPositionOfSrpings[i] = transform.position.y + heightOfWater / 2;

            bodyOfWater.SetPosition(i, new Vector3(xPositionOfSrpings[i], yPositionOfSrpings[i], transform.position.z));
            accelerations[i] = 0;
            velocities[i] = 0;
        }

        // and we setup mesh for each node
        for (int i = 0; i < edgeCount; i++)
        {
            meshes[i] = new Mesh();

            //let's convert the old shape into a shape with 4 vertices: A, B, C and D. And setting position for them
            Vector3[] newVertices = new Vector3[4];

            newVertices[0] = new Vector3(xPositionOfSrpings[i], yPositionOfSrpings[i], transform.position.z);
            newVertices[1] = new Vector3(xPositionOfSrpings[i + 1], yPositionOfSrpings[i], transform.position.z);
            newVertices[2] = new Vector3(xPositionOfSrpings[i], yPositionOfSrpings[i] - heightOfWater,
                transform.position.z);
            newVertices[3] = new Vector3(xPositionOfSrpings[i + 1], yPositionOfSrpings[i] - heightOfWater,
                transform.position.z);


            // set the UVs of the texture
            Vector2[] UVs = new Vector2[4];

            UVs[0] = new Vector2(0, 1);
            UVs[1] = new Vector2(1, 1);
            UVs[2] = new Vector2(0, 0);
            UVs[3] = new Vector2(1, 0);
            
            // set where the triangles should be
            int[] tris = new int[6] {0, 1, 3, 3, 2, 0};

            // add data to the mesh
            meshes[i].vertices = newVertices;
            meshes[i].uv = UVs;
            meshes[i].triangles = tris;


            // create a holder for the mesh, set it to be the manager's child
            meshObjects[i] = Instantiate(waterMat, Vector3.zero, Quaternion.identity);
            meshObjects[i].name = "WaterColumn" + i;
            meshObjects[i].GetComponent<MeshFilter>().mesh = meshes[i];
            meshObjects[i].transform.parent = transform;

            //create collider for those child, add a WaterDetector and make sure they're triggers
            meshObjects[i].AddComponent<BoxCollider2D>();
            meshObjects[i].GetComponent<BoxCollider2D>().isTrigger = true;
            meshObjects[i].AddComponent<WaterDetector>();
        }
    }

    void UpdateMeshes()
    {
        for (int i = 0; i < Mathf.RoundToInt(widthOfWater) * density; i++)
        {
            Vector3[] newVertices = new Vector3[4];

            newVertices[0] = new Vector3(xPositionOfSrpings[i], yPositionOfSrpings[i], transform.position.z);
            newVertices[1] = new Vector3(xPositionOfSrpings[i + 1], yPositionOfSrpings[i], transform.position.z);
            newVertices[2] = new Vector3(xPositionOfSrpings[i], yPositionOfSrpings[i] - heightOfWater, transform.position.z);
            newVertices[3] = new Vector3(xPositionOfSrpings[i + 1], yPositionOfSrpings[i] - heightOfWater, transform.position.z);

            meshes[i].vertices = newVertices;
        }
    }

    public void SpawnWave(float xpos, float velocity)
    {
        //If the position is within the bounds of the water:
        if (xpos >= xPositionOfSrpings[0] && xpos <= xPositionOfSrpings[meshObjects.Length - 1])
        {
            //Offset the x position to be the distance from the left side
            xpos -= xPositionOfSrpings[0];

            //Find which spring we're touching
            int index = Mathf.RoundToInt((meshObjects.Length - 1) * (xpos / (xPositionOfSrpings[meshObjects.Length - 1] - xPositionOfSrpings[0])));
            Debug.Log(index);
            //Add the velocity of the falling object to the spring
            velocities[index] += velocity;
        }
    }
}

public class WaterDetector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Rigidbody2D>() != null)
        {
            transform.parent.GetComponent<Water>().SpawnWave(transform.GetComponent<BoxCollider2D>().offset.x, other.GetComponent<Rigidbody2D>().velocity.y * other.GetComponent<Rigidbody2D>().mass / transform.parent.GetComponent<Water>().springMass);
        }
    }
}