using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleController : MonoBehaviour
{
    public Color[] bottleColors;
    public bool[] isHidden;
    public SpriteRenderer bottleMaskSR;
    public AnimationCurve ScaleAndRotationMultiCurve;
    public AnimationCurve FillAmountCurve;
    public AnimationCurve RotationSpeedMultiCurve;

    public float[] fillAmounts;
    public float[] rotationValues;
    private int rotationIndex = 0;

    [Range(0, 4)]
    public int numberOfColorsInBottle = 4;

    public Color topColor;
    public int numberOfTopColorLayers = 1;

    public BottleController bottleControllerRef;
    private int numberOfColorsToTransfer = 0;

    public Transform leftRotationPoint;
    public Transform rightRotationPoint;
    public Transform leftPourPoint;
    public Transform rightPourPoint;
    private Transform chosenRotationPoint;
    public Transform bottomPoint;
    private float directionMultiplier = 1.0f;

    Vector3 originalPosition;
    Vector3 startPosition;
    Vector3 endPosition;

    float commonTimer = 1f;
    public GameObject filledParticleEffect;
    public LineRenderer lineRenderer;
    public bool isFilled = false;
    // Start is called before the first frame update
    void Start()
    {
        //Initialize();
    }

    public void Initialize()
    {
        UpdateColorsOnShader();
        originalPosition = transform.position;
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);
        UpdateTopColorValues();
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void StartColorTransfer()
    {
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

    void UpdateColorsOnShader()
    {

        for (int i = 0; i < 4; i++)
        {
            if (isHidden[i])
            {
                bottleMaskSR.material.SetColor($"_Color{i + 1:00}", Color.black);
            }
            else
            {
                bottleMaskSR.material.SetColor($"_Color{i + 1:00}", bottleColors[i]);
            }
        }
    }
    public float timeToRotate = 1f;
    IEnumerator RotateBottle()
    {
        float t = 0;
        float lerpValue;
        float angleValue;

        float lastAngleValue = 0;

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(0, directionMultiplier * rotationValues[rotationIndex], lerpValue);
            //transform.eulerAngles = new Vector3(0, 0, angleValue);
            transform.RotateAround(transform.position, Vector3.forward, lastAngleValue - angleValue);
            Vector3 rotationPointPosition = (chosenRotationPoint == rightRotationPoint) ? rightPourPoint.position : leftPourPoint.position;

            if (fillAmounts[numberOfColorsInBottle] > FillAmountCurve.Evaluate(angleValue) + 0.005f)
            {
                if (lineRenderer.enabled == false)
                {
                    HapticFeedback.TriggerHaptic(HapticFeedback.shortPattern, -1);
                    AudioHandler.instance.PlayAudio(AudioType.liquidPour);
                    lineRenderer.startColor = topColor;
                    lineRenderer.endColor = topColor;
                    lineRenderer.SetPosition(0, rotationPointPosition);
                    lineRenderer.SetPosition(1, bottleControllerRef.bottomPoint.position);
                    lineRenderer.enabled = true;
                }
                lineRenderer.SetPosition(0, rotationPointPosition);
                bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));
                bottleControllerRef.FillUp(FillAmountCurve.Evaluate(lastAngleValue) - FillAmountCurve.Evaluate(angleValue));
            }
            bottleMaskSR.material.SetFloat("_ScaleAndRotationMulti", ScaleAndRotationMultiCurve.Evaluate(angleValue));
            t += Time.deltaTime * RotationSpeedMultiCurve.Evaluate(angleValue);
            lastAngleValue = angleValue;
            yield return new WaitForEndOfFrame();
        }
        angleValue = directionMultiplier * rotationValues[rotationIndex];
        //transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_ScaleAndRotationMulti", ScaleAndRotationMultiCurve.Evaluate(angleValue));
        numberOfColorsInBottle -= numberOfColorsToTransfer;
        bottleControllerRef.numberOfColorsInBottle += numberOfColorsToTransfer;
        bottleControllerRef.CheckIfBottleIsFullOfOneColor();
        lineRenderer.enabled = false;
        AudioHandler.instance.StopAudio(AudioType.liquidPour);
        StartCoroutine(RotateBottleBack());
    }

    IEnumerator RotateBottleBack()
    {
        float t = 0;
        float lerpValue;
        float angleValue;

        float lastAngleValue = directionMultiplier * rotationValues[rotationIndex];

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(directionMultiplier * rotationValues[rotationIndex], 0, lerpValue);
            //transform.eulerAngles = new Vector3(0, 0, angleValue);
            transform.RotateAround(transform.position, Vector3.forward, lastAngleValue - angleValue);
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

    public void UpdateTopColorValues()
    {
        if (numberOfColorsInBottle != 0)
        {
            numberOfTopColorLayers = 1;
            topColor = bottleColors[numberOfColorsInBottle - 1];
            if (numberOfColorsInBottle == 4)
            {
                if (bottleColors[3].Equals(bottleColors[2]) && !isHidden[2])
                {
                    numberOfTopColorLayers++;
                    if (bottleColors[2].Equals(bottleColors[1]) && !isHidden[1])
                    {
                        numberOfTopColorLayers++;
                        if (bottleColors[1].Equals(bottleColors[0]) && !isHidden[0])
                        {
                            numberOfTopColorLayers++;
                        }
                    }
                }
            }
            else if (numberOfColorsInBottle == 3)
            {
                if (bottleColors[2].Equals(bottleColors[1]) && !isHidden[1])
                {
                    numberOfTopColorLayers++;
                    if (bottleColors[1].Equals(bottleColors[0]) && !isHidden[0])
                    {
                        numberOfTopColorLayers++;
                    }
                }
            }
            else if (numberOfColorsInBottle == 2)
            {
                if (bottleColors[1].Equals(bottleColors[0]) && !isHidden[0])
                {
                    numberOfTopColorLayers++;
                }
            }
        }

        rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayers);
    }

    public bool FillBottleCheck(Color colorToCheck)
    {
        if (numberOfColorsInBottle == 0)
        {
            return true;
        }
        else
        {
            if (numberOfColorsInBottle == 4)
            {
                return false;
            }
            else
            {
                if (topColor.Equals(colorToCheck))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    private void CalculateRotationIndex(int numberOfEmptySpacesInSecondBottle)
    {
        rotationIndex = 3 - (numberOfColorsInBottle - Mathf.Min(numberOfTopColorLayers, numberOfEmptySpacesInSecondBottle));
    }

    private void FillUp(float fillAmountToAdd)
    {
        bottleMaskSR.material.SetFloat("_FillAmount", bottleMaskSR.material.GetFloat("_FillAmount") + fillAmountToAdd);
    }

    private void ChooseRotationPointAndDirection()
    {
        if (transform.position.x > bottleControllerRef.transform.position.x)
        {
            chosenRotationPoint = leftRotationPoint;
            directionMultiplier = -1.0f;
        }
        else
        {
            chosenRotationPoint = rightRotationPoint;
            directionMultiplier = 1.0f;
        }
    }

    IEnumerator MoveBottle()
    {
        startPosition = transform.position;
        if (chosenRotationPoint == leftRotationPoint)
        {
            endPosition = bottleControllerRef.rightRotationPoint.position;
        }
        else
        {
            endPosition = bottleControllerRef.leftRotationPoint.position;
        }

        float t = 0;

        while (t < commonTimer)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;
        StartCoroutine(RotateBottle());
    }
    IEnumerator MoveBottleBack()
    {
        startPosition = transform.position;
        endPosition = originalPosition;

        float t = 0;

        while (t < commonTimer)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;

        transform.GetComponent<SpriteRenderer>().sortingOrder -= 2;
        bottleMaskSR.sortingOrder -= 2;

        CheckTopColorHidden();
    }

    void CheckTopColorHidden()
    {
        // Put original color in top color if it was black prior
        int negativeIndex = 1;
        Color prevColor;
        if (numberOfColorsInBottle != 0)
        {
            if (isHidden[numberOfColorsInBottle - negativeIndex])
            {
                isHidden[numberOfColorsInBottle - negativeIndex] = false;
                prevColor = bottleColors[numberOfColorsInBottle - negativeIndex];
                negativeIndex++;
                if ((numberOfColorsInBottle - negativeIndex) < 0 || prevColor != bottleColors[numberOfColorsInBottle - negativeIndex])
                {
                UpdateColorsOnShader();
                    return;
                }
                if (isHidden[numberOfColorsInBottle - negativeIndex])
                {
                    isHidden[numberOfColorsInBottle - negativeIndex] = false;
                    prevColor = bottleColors[numberOfColorsInBottle - negativeIndex];
                    negativeIndex++;
                    if ((numberOfColorsInBottle - negativeIndex) < 0 || prevColor != bottleColors[numberOfColorsInBottle - negativeIndex])
                    {
                UpdateColorsOnShader();
                        return;
                    }
                    if (isHidden[numberOfColorsInBottle - negativeIndex])
                    {
                        isHidden[numberOfColorsInBottle - negativeIndex] = false;
                        prevColor = bottleColors[numberOfColorsInBottle - negativeIndex];
                        negativeIndex++;
                        if ((numberOfColorsInBottle - negativeIndex) < 0 || prevColor != bottleColors[numberOfColorsInBottle - negativeIndex])
                        {
                UpdateColorsOnShader();
                            return;
                        }
                        if (isHidden[numberOfColorsInBottle - negativeIndex])
                        {
                            isHidden[numberOfColorsInBottle - negativeIndex] = false;
                        }
                    }
                }
                UpdateColorsOnShader();
            }
        }
    }

    IEnumerator SelectBottle(float yOffset = 1f)
    {
        float t = 0;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
        while (t < commonTimer)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 5;
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;
    }
    public void PickUpBottle()
    {
        HapticFeedback.TriggerHaptic(HapticFeedback.shortPattern, -1);
        AudioHandler.instance.PlayAudio(AudioType.bottlePickup);
        StartCoroutine(SelectBottle(0.25f));
    }

    public void DropBottle()
    {
        HapticFeedback.TriggerHaptic(HapticFeedback.shortPattern, -1);
        AudioHandler.instance.PlayAudio(AudioType.bottleDrop);
        StartCoroutine(SelectBottle(-0.25f));
    }

    public void EnableBottle(bool enable)
    {
        this.gameObject.GetComponent<BoxCollider2D>().enabled = enable;
    }

    public void CheckIfBottleIsFullOfOneColor()
    {
        if (numberOfColorsInBottle == 4)
        {
            if (bottleColors[0].Equals(bottleColors[1]) && bottleColors[1].Equals(bottleColors[2]) && bottleColors[2].Equals(bottleColors[3]))
            {
                Debug.Log("Bottle is full of one color");
                isFilled = true;
                GameObject particleEffect = Instantiate(filledParticleEffect, bottomPoint.position, Quaternion.identity);
                Destroy(particleEffect, 3f);
                EnableBottle(false);
                LevelManager.instance.BottleFilledUp(this);
            }
        }
    }
}
