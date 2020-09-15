using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_GameController : MonoBehaviour
{
    public void Btn_Slot(string _Index)
    {
        SC_GameLogic.Instance.Btn_Slot(_Index);
    }

    public void Btn_PopUp_Restart()
    {
        SC_GameLogic.Instance.Btn_PopUp_Restart();
    }

    public void PointerEnter(string _Index)
    {
        SC_GameLogic.Instance.PointerEnter(_Index);
    }

    public void PointerExit(string _Index)
    {
        SC_GameLogic.Instance.PointerExit(_Index);
    }
}