using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Interactions
{
    public Interaction[] interactions;

    public Interactions(Interaction[] interactions)
    {
        this.interactions = interactions;
    }
}
