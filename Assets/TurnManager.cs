using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public bool CanMove { get; private set; } = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void LockInput()
    {
        CanMove = false;
    }

    public void UnlockInput()
    {
        CanMove = true;
    }
}
