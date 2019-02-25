using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyHUD :MonoBehaviour
{
    [Header("Player Id to Get")]
    public int playerId;

    public Text uiName;
    public Image lifeBar;

    [Header("Weapons Amount")]
    public Text rifleAmount;
    public Text grenadeAmount;
    public Text boxAmount;
    public Text largeBoxAmount;
}
