using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBox : MonoBehaviour
{
    public int gx;
    public int gy;

    public Gem heldGem;

    public bool occupied;
    [HideInInspector] public GemSO gemType;

    public void SetCoords(int x, int y)
    {
        gx = x;
        gy = y;
    }

    public void SetGem(Gem g)
    {
        heldGem = g;
    }
}