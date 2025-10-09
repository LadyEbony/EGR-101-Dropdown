using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZoneManager : MonoBehaviour {

    public GameObject[] prefabs;
    public float height = 16f;
    public int copies = 128;

    [Header("Seeds")]
    public bool randomizeSeed;
    public int seed = 0;

    // Start is called before the first frame update
    void Start() {
        if (randomizeSeed) {
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        var randomStream = new System.Random(seed);
       
        var list = new List<GameObject>();
        GameObject last = null;

        GameObject GetNext()
        {
            if (list.Count == 0)
            {
                list = prefabs.ToList();
                Shuffle(list, randomStream);

                if (list[list.Count - 1] == last)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }

            var pop = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            last = pop;

            return pop;
        }

        for (var i = 0; i < copies; i++) {
            var item = GetNext();

            var position = new Vector3(0f, -height * i, 0f);

            Instantiate(item, position, Quaternion.identity);
        }
    }

    public static void Shuffle<T>(List<T> array, System.Random rng)
    {
        int n = array.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);  // Random index from 0 to n
            T value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }

}
