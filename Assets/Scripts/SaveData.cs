using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectSaveData
{
    public string objectType;
    public string customName;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

[Serializable]
public class LevelSaveData
{
    public List<ObjectSaveData> objects = new List<ObjectSaveData>();
}