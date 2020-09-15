using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Slot : MonoBehaviour
{
    public Image currImage;
    public Sprite Red, Yellow, Black;

    public void ChangeSlotState(GlobalEnums.SlotState _CurrSlot)
    {
        if (currImage != null)
        {
            switch (_CurrSlot)
            {
                case GlobalEnums.SlotState.Empty:
                    currImage.enabled = false;
                    break;
                case GlobalEnums.SlotState.Red:
                    currImage.sprite = Red;
                    currImage.enabled = true;
                    break;
                case GlobalEnums.SlotState.Yellow:
                    currImage.sprite = Yellow;
                    currImage.enabled = true;
                    break;
                case GlobalEnums.SlotState.Black:
                    currImage.sprite = Black;
                    currImage.enabled = true;
                    break;
            }
        }
        else Debug.Log("cur Image is Null, pass reference");
    }
}
