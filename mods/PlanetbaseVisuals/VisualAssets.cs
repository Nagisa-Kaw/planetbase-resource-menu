using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetbaseVisuals
{
    internal static class VisualAssets
    {
        private static readonly Dictionary<string,GameObject> Prefabs=new Dictionary<string,GameObject>();
        internal static GameObject Clone(string key,Transform parent,Action<Transform> build)
        {
            if(!Prefabs.TryGetValue(key,out var prefab)||prefab==null){
                prefab=new GameObject("Own visual template "+key);prefab.SetActive(false);UnityEngine.Object.DontDestroyOnLoad(prefab);build(prefab.transform);Prefabs[key]=prefab;
            }
            var model=UnityEngine.Object.Instantiate(prefab);model.name="Own model "+key;model.transform.SetParent(parent,false);model.SetActive(true);return model;
        }
    }
}
