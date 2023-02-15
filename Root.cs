using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class Root
{
    public List<Vector> vector { get; set; }
}

public class Vector
{
    public float x { get; set; }
    public float y { get; set; }
}
