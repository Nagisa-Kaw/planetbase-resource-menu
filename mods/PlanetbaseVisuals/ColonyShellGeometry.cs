using System.Collections.Generic;
using HarmonyLib;
using Planetbase;
using UnityEngine;
using Module=Planetbase.Module;

namespace PlanetbaseVisuals
{
    // The native corridor cut only visits direct children. Our batched shell lives in
    // its own roof group, so apply the same cut to its per-instance mesh copies.
    internal sealed class ColonyShellGeometry:MonoBehaviour
    {
        public MeshFilter[] Surfaces;
        private readonly Dictionary<MeshFilter,Mesh> owned=new Dictionary<MeshFilter,Mesh>();
        internal void Cut(Module owner,Connection link)
        {
            if(Surfaces==null)return;
            var other=(Module)(link.getLink(0)==owner?link.getLink(1):link.getLink(0));
            Vector3 direction=other.getPosition()-owner.getPosition();direction.y=0;direction.Normalize();
            float offset=link.getSizeIndex()==0?.25f:.375f;
            float height=link.getSizeIndex()==0?.5f:.25f;
            float radius=link.getSizeIndex()==0?2:2.5f;
            Vector3 center=owner.getPosition()+direction*(owner.getRadius()+offset)+Vector3.up*height;
            foreach(var filter in Surfaces)if(filter!=null&&filter.sharedMesh!=null){
                if(!owned.TryGetValue(filter,out var mesh)){mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);mesh.name="Own corridor-cut shell";filter.sharedMesh=mesh;owned.Add(filter,mesh);}
                MeshUtil.makeSphericHole(mesh,filter.transform.InverseTransformPoint(center),radius);mesh.RecalculateBounds();
            }
        }
        private void OnDestroy(){foreach(var mesh in owned.Values)if(mesh!=null)UnityEngine.Object.Destroy(mesh);owned.Clear();}
    }

    [HarmonyPatch(typeof(Module),nameof(Module.removeGeometry))]
    internal static class OwnShellCorridorCuts
    {
        private static void Postfix(Module __instance,Connection link,GameObject ___mModel)
        {
            // Removing a link rebuilds the model and destroys the previous one at the
            // end of the frame. Scope the cut to the current model, not a stale sibling.
            var shell=___mModel==null?null:___mModel.GetComponentInChildren<ColonyShellGeometry>(true);
            if(shell!=null)shell.Cut(__instance,link);
        }
    }
}
