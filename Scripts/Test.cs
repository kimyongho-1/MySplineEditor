using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class m
{
    [SerializeField] int c = 1;       
}

[System.Serializable]
public class ele 
{
    [SerializeField] m b;
    [SerializeField] int a;
    [SerializeField] Vector2 pos;
    public ele(int i, Vector2 v)
    { a = i; pos = v; b = new m() ; }
}

public class Test : MonoBehaviour
{
    [SerializeField] List<ele> list = new List<ele>();
    [SerializeField] Dictionary<int, ele> dic = new Dictionary<int, ele>();
    public void Add()
    {
        list.Add(new ele(list.Count, new Vector2(0.5f * list.Count , 0.5f * list.Count)));
    }
}
