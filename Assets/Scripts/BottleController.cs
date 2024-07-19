using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleController : MonoBehaviour
{
    public Color[] bottleColors;
    public SpriteRenderer bottleMaskSR;
    public AnimationCurve ScaleAndRotationMultiCurve;
    public AnimationCurve FillAmountCurve;
    public AnimationCurve RotationSpeedMultiCurve;

    public float[] fillAmounts;
    public float[] rotationValues;
    private int rotationIndex = 0;

    [Range(0,4)]
    public int numberOfColorsInBottle = 4;

    public Color topColor;
    public int numberOfTopColorLayers = 1;

    public BottleController bottleControllerRef;
    public bool justThisBottle = false;
    private int numberOfColorsToTransfer = 0;

    public Transform leftRotationPoint;
    public Transform rightRotationPoint;
    private Transform chosenRotationPoint;
    private float directionMultiplier = 1.0f;

    Vector3 originalPosition;
    Vector3 startPosition;
    Vector3 endPosition;

    float commonTimer = 1f;
    // Start is called before the first frame update
    void Start()
    {
        UpdateColorsOnShader();
        originalPosition = transform.position;
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);
        UpdateTopColorValues();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && justThisBottle) {
            UpdateTopColorValues();
            if(bottleControllerRef.FillBottleCheck(topColor)) {
                ChooseRotationPointAndDirection();
                numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, 4-bottleControllerRef.numberOfColorsInBottle);
                for (int i = 0; i < numberOfColorsToTransfer; i++)
                {
                    bottleControllerRef.bottleColors[bottleControllerRef.numberOfColorsInBottle+i] = topColor;
                }
                bottleControllerRef.UpdateColorsOnShader();

            }
            CalculateRotationIndex(4-bottleControllerRef.numberOfColorsInBottle);
            StartCoroutine(RotateBottle());
        }
    }

    public void StartColorTransfer() {
        ChooseRotationPointAndDirection();
        numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - bottleControllerRef.numberOfColorsInBottle);
        for (int i = 0; i < numberOfColorsToTransfer; i++)
        {
            bottleControllerRef.bottleColors[bottleControllerRef.numberOfColorsInBottle + i] = topColor;
        }
        bottleControllerRef.UpdateColorsOnShader();

        CalculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);
        transform.GetComponent<SpriteRenderer>().sortingOrder += 2;
        bottleMaskSR.sortingOrder += 2;
        StartCoroutine(MoveBottle());
    }

    void UpdateColorsOnShader() {
        for (int i = 0; i < 4; i++)
        {
            bottleMaskSR.material.SetColor($"_Color{i+1:00}", bottleColors[i]);
        }
    }
    public float timeToRotate = 1f;
    IEnumerator RotateBottle() {
        float t = 0;
        float lerpValue;
        float angleValue;

        float lastAngleValue = 0;

        while(t<timeToRotate) {
            lerpValue = t/timeToRotate;
            angleValue = Mathf.Lerp(0, directionMultiplier * rotationValues[rotationIndex], lerpValue);
            //transform.eulerAngles = new Vector3(0, 0, angleValue);
            transform.RotateAround(transform.position,Vector3.forward, lastAngleValue-angleValue);
            if(fillAmounts[numberOfColorsInBottle] > FillAmountCurve.Evaluate(angleValue)) {
                bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));
                bottleControllerRef.FillUp(FillAmountCurve.Evaluate(lastAngleValue)-FillAmountCurve.Evaluate(angleValue));
            }
            bottleMaskSR.material.SetFloat("_ScaleAndRotationMulti", ScaleAndRotationMultiCurve.Evaluate(angleValue));
            t += Time.deltaTime*RotationSpeedMultiCurve.Evaluate(angleValue);
            lastAngleValue = angleValue;
            yield return new WaitForEndOfFrame();
        }
        angleValue = directionMultiplier * rotationValues[rotationIndex];
        //transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_ScaleAndRotationMulti", ScaleAndRotationMultiCurve.Evaluate(angleValue));
        numberOfColorsInBottle -= numberOfColorsToTransfer;
        bottleControllerRef.numberOfColorsInBottle += numberOfColorsToTransfer;
        StartCoroutine(RotateBottleBack());
    }

    IEnumerator RotateBottleBack() {
        float t = 0;
        float lerpValue;
        float angleValue;

        float lastAngleValue = directionMultiplier * rotationValues[rotationIndex];

        while(t<timeToRotate) {
            lerpValue = t/timeToRotate;
            angleValue = Mathf.Lerp(directionMultiplier*rotationValues[rotationIndex], 0, lerpValue);
            //transform.eulerAngles = new Vector3(0, 0, angleValue);
            transform.RotateAround(transform.position,Vector3.forward, lastAngleValue-angleValue);
            bottleMaskSR.material.SetFloat("_ScaleAndRotationMulti", ScaleAndRotationMultiCurve.Evaluate(angleValue));
            lastAngleValue = angleValue;
            t += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }
        UpdateTopColorValues();
        angleValue = 0;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_ScaleAndRotationMulti", ScaleAndRotationMultiCurve.Evaluate(angleValue));
        StartCoroutine(MoveBottleBack());
    }

    public void UpdateTopColorValues() {
        if(numberOfColorsInBottle != 0) {
            numberOfTopColorLayers = 1;
            topColor = bottleColors[numberOfColorsInBottle-1];
            if(numberOfColorsInBottle==4) {
                if(bottleColors[3].Equals(bottleColors[2])) {
                    numberOfTopColorLayers++;
                    if(bottleColors[2].Equals(bottleColors[1])) {
                        numberOfTopColorLayers++;
                        if(bottleColors[1].Equals(bottleColors[0])) {
                            numberOfTopColorLayers++;
                        }
                    }
                }
            } else if(numberOfColorsInBottle==3) {
                if(bottleColors[2].Equals(bottleColors[1])) {
                    numberOfTopColorLayers++;
                    if(bottleColors[1].Equals(bottleColors[0])) {
                        numberOfTopColorLayers++;
                    }
                }
            } else if(numberOfColorsInBottle==2) {
                if(bottleColors[1].Equals(bottleColors[0])) {
                    numberOfTopColorLayers++;
                }
            }
        }

        rotationIndex = 3- (numberOfColorsInBottle - numberOfTopColorLayers);
    }

    public bool FillBottleCheck(Color colorToCheck) {
        if(numberOfColorsInBottle == 0) {
            return true;
        }
        else {
            if(numberOfColorsInBottle==4) {
                return false;
            } else {
                if(topColor.Equals(colorToCheck)) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    }

    private void CalculateRotationIndex(int numberOfEmptySpacesInSecondBottle) {
        rotationIndex = 3- (numberOfColorsInBottle - Mathf.Min(numberOfTopColorLayers, numberOfEmptySpacesInSecondBottle));
    }

    private void FillUp(float fillAmountToAdd) {
        bottleMaskSR.material.SetFloat("_FillAmount", bottleMaskSR.material.GetFloat("_FillAmount")+fillAmountToAdd);
    }

    private void ChooseRotationPointAndDirection() {
        if(transform.position.x > bottleControllerRef.transform.position.x) {
            chosenRotationPoint = leftRotationPoint;
            directionMultiplier = -1.0f;
        } else {
            chosenRotationPoint = rightRotationPoint;
            directionMultiplier = 1.0f;
        }
    }

    IEnumerator MoveBottle() {
        startPosition = transform.position;
        if(chosenRotationPoint == leftRotationPoint) {
            endPosition = bottleControllerRef.rightRotationPoint.position;
        } else {
            endPosition = bottleControllerRef.leftRotationPoint.position;
        }

        float t=0;

        while(t<commonTimer) {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime*2;
            yield return new WaitForEndOfFrame();
        }

        transform.position=endPosition;
        StartCoroutine(RotateBottle());
    }
    IEnumerator MoveBottleBack() {
        startPosition = transform.position;
        endPosition = originalPosition;

        float t=0;

        while(t<commonTimer) {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime*2;
            yield return new WaitForEndOfFrame();
        }

        transform.position=endPosition;
        
        transform.GetComponent<SpriteRenderer>().sortingOrder -= 2;
        bottleMaskSR.sortingOrder -= 2;
    }

    IEnumerator SelectBottle(float yOffset = 1f) {
        float t = 0;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(transform.position.x, transform.position.y+yOffset, transform.position.z);
        while(t<commonTimer) {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime*5;
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;
    }
    public void PickUpBottle() {
        StartCoroutine(SelectBottle(0.25f));
    }

    public void DropBottle() {
        StartCoroutine(SelectBottle(-0.25f));
    }
}
