using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct Bottle {
    public Color[] colors;
    public bool[] isHidden;
    public int numberOfColors;
}
[System.Serializable]
public struct Level {
    public Bottle[] bottles;
}

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;

    [SerializeField]
    public List<Level> levels;
    public GameObject bottlePrefab;
    public Vector3[] offsets;
    public GameObject confettiPrefab;

    void Awake() {
        if(instance == null) {
            instance = this;
        } else {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate the first level
        StartLevel(0);
    }

    Vector3[] CalculateOffsets(int numBottles)
    {
        Vector3[] offsets = new Vector3[numBottles];

        if (numBottles <= 4)
        {
            // Calculate offsets for a single row
            float startX = -((numBottles - 1) * 0.5f);
            for (int i = 0; i < numBottles; i++)
            {
                offsets[i] = new Vector3(startX + i, 0f, 0f);
            }
        }
        else
        {
            // Calculate offsets for two rows
            int bottlesPerRow = Mathf.CeilToInt(numBottles / 2f);
            float additionalOffset = (numBottles % 2 == 0) ? 0f : -0.5f;
            float startX = -((bottlesPerRow - 1) * 0.5f);
            for (int i = 0; i < bottlesPerRow; i++)
            {
                offsets[i] = new Vector3(startX + i + additionalOffset, 1f, 0f);
            }
            for (int i = 0; i < bottlesPerRow; i++)
            {
                if(i+bottlesPerRow >= numBottles) {
                    break;
                }
                offsets[i + bottlesPerRow] = new Vector3(startX + i, -1f, 0f);
            }
        }

        return offsets;
    }

    public void StartLevel(int level) {
        foreach(Transform bottle in this.transform) {
            Destroy(bottle.gameObject);
        }
        Bottle[] bottles = levels[level].bottles;
        offsets = new Vector3[bottles.Length];
        offsets =  CalculateOffsets(bottles.Length);
        for(int i=0; i<bottles.Length; i++) {
            Bottle bottle = bottles[i];
            // Instantiate bottle
            GameObject bottleObj = Instantiate(bottlePrefab,this.transform);
            bottleObj.transform.position += offsets[i];

                bottleObj.GetComponent<BottleController>().bottleColors = bottle.colors;
                bottleObj.GetComponent<BottleController>().isHidden = bottle.isHidden;
                bottleObj.GetComponent<BottleController>().numberOfColorsInBottle = bottle.numberOfColors;
                bottleObj.GetComponent<BottleController>().Initialize();
           
        }
    }

    public void BottleFilledUp(BottleController bottleController) {
        HapticFeedback.Vibrate(HapticFeedback.successPattern, 0);
        // Check if all bottles are filled up
        bool allFilled = true;
        foreach(Transform bottle in this.transform) {
            if(bottle.GetComponent<BottleController>().isFilled == false) {
                if(bottle.GetComponent<BottleController>().numberOfColorsInBottle != 0) {
                    allFilled = false;
                    break;
                }
            }
        }
        if(allFilled) {
            Debug.Log("Level Completed");
            // Start confetti particles
            GameObject confetti = Instantiate(confettiPrefab, this.transform);
        }
    }

}
