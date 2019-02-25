using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeOfGun : MonoBehaviour
{
    public myType type;

    public enum myType
    {
        pistol,
        throwable,
        box,
        largebox,
        rifle
    }
}
