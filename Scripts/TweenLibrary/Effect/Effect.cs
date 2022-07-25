using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Effect 
{
    [SerializeField]ScriptableObject data;
    [SerializeField] Vector2 pos = new Vector2(0,0);
    [SerializeField] float speed=1f;
    [SerializeField]int i = 1;
    public Effect(int i2, ScriptableObject _data= null)
    { i = i2;  data = _data; }
}