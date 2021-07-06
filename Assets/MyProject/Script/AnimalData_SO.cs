using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(menuName = "MyScriptable/Create AnimalData_SO")]
public class AnimalData_SO : ScriptableObject
{
    public string id;
    public List<CatData_SO> catData = new List<CatData_SO>();
}

[Serializable]
public class CatData_SO : CatData { }