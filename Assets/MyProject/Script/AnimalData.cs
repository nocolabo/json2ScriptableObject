using System.Collections;
using System.Collections.Generic;
using System;


public class AnimalData
{
    public string id;
    public List<CatData> catData;
}

[Serializable]
public class CatData
{
    public int id;
    public string name;
    public string engName;
    public int maxHP;
}