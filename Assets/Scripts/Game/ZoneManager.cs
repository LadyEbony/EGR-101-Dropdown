using System.Collections;
using System.Collections.Generic;
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
       
        for (var i = 0; i < copies; i++) {
            var index = randomStream.Next(prefabs.Length);
            var item = prefabs[index];

            var position = new Vector3(0f, -height * i, 0f);

            Instantiate(item, position, Quaternion.identity);
        }
    }

}
