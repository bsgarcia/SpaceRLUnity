using UnityEngine;
using Random = System.Random;
using System.Collections;
// import parallel job system
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;


namespace Project.Scripts.Fractures
{
    // [ExecuteInEditMode]
    public class FractureThis : MonoBehaviour
    {
        [SerializeField] private Anchor anchor = Anchor.Bottom;
        [SerializeField] private int chunks = 500;
        [SerializeField] private float density = 50;
        [SerializeField] private float internalStrength = 100;

        [SerializeField] private Material insideMaterial;
        [SerializeField] private Material outsideMaterial;
        // public NativeArray<Entity> Chunks;

        private Random rng = new Random();


        // constructor called when AddComponent is called
        private void Awake()
        {

            // if no inside material is set, use the default material that is located in resources, caalled defaultMat

            if (insideMaterial == null)
            {
                insideMaterial = Resources.Load<Material>("defaultMat");
            }

            // if no outside material is set, use the default material
            if (outsideMaterial == null)
            {
                outsideMaterial = Resources.Load<Material>("defaultMat");
            }
        }

        // Start is called before the first frame update

        private void Start()
        {
            // if no inside material is set, use the default material that is located in resources, caalled defaultMat

            if (insideMaterial == null)
            {
                insideMaterial = Resources.Load<Material>("defaultMat");
            }

            // if no outside material is set, use the default material
            if (outsideMaterial == null)
            {
                outsideMaterial = Resources.Load<Material>("defaultMat");
            }

            ChunkGraphManager fractureGameObject = FractureGameobject();
            /*  // Rigidbody[] chunks = fractureGameObject.GetComponentsInChildren<Rigidbody>();
             int chunksToDestroy = (int)(chunks.Length * (percentage/100f)); // percentage as a value between 0 and 1
             // var chunksToRemove = chunks[0..chunksToDestroy];
             // var chunksToRemove = new Rigidbody[chunksToDestroy];
             var chunksToRemove = chunks[0..chunksToDestroy];
             // right everything is falling apart when add force is called

             for (int i = 0; i < chunksToDestroy; i++)
             {
                 var chunk = chunks[i];
                 // add a force to the chunk
                 chunk.AddForce(1000f * transform.forward);

                 // then destroy it after a short delay
                 Destroy(chunk.gameObject, 3f);
             }
  */
            // we want all the non-destroyed chunks to stay in place
            // for (int i = chunksToDestroy; i < chunks.Length; i++)
            // {
            // stay in place
            // chunks[i].isKinematic = true;
            // chunks[i].useGravity = false;

            // }



            // gameObject.SetActive(false);
        }

        // corountine to destroy the chunks without blocking the main thread
        public void FracturePercentage(int percentage)
        {
            Debug.Log("Fracturing " + gameObject.name);

            ChunkGraphManager fractureGameObject = FractureGameobject();
            // Rigidbody[] chunks = fractureGameObject.GetComponentsInChildren<Rigidbody>();
            // int chunksToDestroy = (int)(chunks.Length * (percentage / 100f)); // percentage as a value between 0 and 1

            /* var destroyChunkJob = new DestroyChunkJob
            {
                Chunks = new NativeArray<Entity>(chunks.Length, Allocator.TempJob),
                ChunksToDestroy = chunksToDestroy
            };

            for (int i = 0; i < chunks.Length; i++)
            {
                destroyChunkJob.Chunks[i] = chunks[i].gameObject.GetComponent<Entity>();
            }

            destroyChunkJob.Schedule(chunks.Length, 64).Complete();
            destroyChunkJob.Chunks.Dispose();
 */
            // var chunksToRemove = chunks[0..chunksToDestroy];
            // var chunksToRemove = new Rigidbody[chunksToDestroy];
            // var chunksToRemove = chunks[0..chunksToDestroy];
            // right everything is falling apart when add force is called
            // for (int i = 0; i < chunksToDestroy; i++)
            // {
                // add a force to the chunk
                // chunks[i].AddForce(1000f * transform.forward);

                // then destroy it after a short delay
                // Destroy(chunks[i].gameObject);


                // call the job to destroy the chunks

                // }

                // gameObject.SetActive(false);
            // }
        }

        public ChunkGraphManager FractureGameobject()
        {
            // Debug.Log("Fracturing " + gameObject.name);
            var seed = rng.Next();
            return Fracture.FractureGameObject(
                gameObject,
                anchor,
                seed,
                chunks,
                insideMaterial,
                outsideMaterial,
                internalStrength,
                density
            );
        }
    }
}

/* [BurstCompile]
public struct DestroyChunkJob : IJobParallelFor
{
    public NativeArray<Rigidbody> Chunks;
    public int ChunksToDestroy;

    public void Execute(int index)
    {
        if (index < ChunksToDestroy)
        {
            // Add a force to the chunk
            Chunks[index].AddForce(1000f * Vector3.forward);

            // Then destroy it after a short delay
            Object.Destroy(Chunks[index].gameObject, 3f);
        }
    }
} */
