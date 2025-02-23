using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public BottleController FirstBottle;
    public BottleController SecondBottle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if(hit.collider == null) {
                return;
            }
           
            if(hit.collider.GetComponent<BottleController>() != null) {
                if(FirstBottle == null) {
                    // FirstBottle picked up
                    FirstBottle = hit.collider.GetComponent<BottleController>();
                    if(FirstBottle.numberOfColorsInBottle == 0 || FirstBottle.isFilled) {
                        FirstBottle = null;
                    } else {
                        FirstBottle.PickUpBottle();
                    }
                } else {
                    if(FirstBottle == hit.collider.GetComponent<BottleController>()) {
                        FirstBottle.DropBottle();
                        FirstBottle = null;
                        return;
                    } else {
                        SecondBottle = hit.collider.GetComponent<BottleController>();
                        FirstBottle.bottleControllerRef = SecondBottle;
                        
                        FirstBottle.UpdateTopColorValues();
                        SecondBottle.UpdateTopColorValues();

                        if(SecondBottle.FillBottleCheck(FirstBottle.topColor)) {
                            FirstBottle.StartColorTransfer();
                            //FirstBottle.DropBottle();
                            FirstBottle = null;
                            SecondBottle = null;

                        } else {
                            FirstBottle.DropBottle();
                            FirstBottle = null;
                            SecondBottle = null;
                        }

                    }
                }
            }

        }
    }
}
