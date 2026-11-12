using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetbaseVisuals
{
    internal static class ResourceArchitecture
    {
        private static readonly Dictionary<string,Texture2D> Icons=new Dictionary<string,Texture2D>();
        internal static void Attach(GameObject model,string kind)
        {
            Architecture.HideOriginalMeshes(model);
            VisualAssets.Clone("resource "+kind,model.transform,t=>Build(t,kind));
        }

        private static Color Accent(string kind)
        {
            switch(kind){case "NuclearFuelRod":return Architecture.Yellow;case "FusionCapsule":return Architecture.Cyan;case "OrganicWaste":return Architecture.Orange;case "Fertilizer":return Architecture.Green;case "Regolith":return new Color(.57f,.37f,.2f);case "ResearchPacket":return new Color(.2f,.5f,.88f);default:return Architecture.Orange;}
        }

        private static void Build(Transform root,string kind)
        {
            var b=new Architecture(root,"Own resource geometry");Color accent=Accent(kind);
            if(kind=="NuclearFuelRod"||kind=="FusionCapsule"){
                b.Box(new Vector3(0,.06f,0),new Vector3(.62f,.12f,.8f),Architecture.Dark,"Transport cradle");
                float radius=kind=="NuclearFuelRod"?.15f:.18f;
                b.Cylinder(new Vector3(0,.23f,0),radius,.7f,Architecture.Shell,"Shielded fuel capsule",24).localRotation=Quaternion.Euler(90,0,0);
                for(int sign=-1;sign<=1;sign+=2){
                    b.Cylinder(new Vector3(0,.23f,sign*.36f),radius*1.05f,.05f,Architecture.Dark,"Pressure cap",20).localRotation=Quaternion.Euler(90,0,0);
                    b.Ring(new Vector3(0,.23f,sign*.23f),radius+.005f,.023f,accent,"Fuel identification ring",kind=="FusionCapsule",24).localRotation=Quaternion.Euler(90,0,0);
                }
                if(kind=="NuclearFuelRod")for(int i=-1;i<=1;i++)b.Box(new Vector3(i*.11f,.395f,0),new Vector3(.055f,.025f,.26f),accent,"Shielding warning stripe");
                else b.Box(new Vector3(0,.414f,0),new Vector3(.07f,.022f,.35f),accent,"Fusion status strip",true);
            }else if(kind=="VehicleChassis"){
                foreach(int x in new[]{-1,1})b.Box(new Vector3(x*.3f,.15f,0),new Vector3(.11f,.18f,.82f),Architecture.Steel,"Chassis rail");
                for(int i=-1;i<=1;i++)b.Box(new Vector3(0,.15f,i*.29f),new Vector3(.7f,.12f,.1f),Architecture.Edge,"Frame crossmember");
                foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})b.Box(new Vector3(x*.3f,.26f,z*.3f),new Vector3(.14f,.08f,.16f),accent,"Chassis mounting bracket");
                b.Box(new Vector3(0,.22f,0),new Vector3(.24f,.1f,.3f),Architecture.Dark,"Drive unit mount");
            }else if(kind=="Regolith"){
                b.Box(new Vector3(0,.055f,0),new Vector3(.8f,.1f,.8f),Architecture.Dark,"Mineral tray");
                foreach(int s in new[]{-1,1}){
                    b.Box(new Vector3(s*.37f,.19f,0),new Vector3(.06f,.26f,.8f),Architecture.Edge,"Tray wall");
                    b.Box(new Vector3(0,.19f,s*.37f),new Vector3(.7f,.26f,.06f),Architecture.Edge,"Tray wall");
                }
                b.Box(new Vector3(0,.18f,0),new Vector3(.65f,.14f,.65f),accent,"Loose regolith");
                for(int i=0;i<7;i++)b.Cylinder(new Vector3(Mathf.Sin(i*2.4f)*.2f,.27f,Mathf.Cos(i*2.4f)*.2f),.09f+(i%3)*.02f,.14f,new Color(.4f+i*.02f,.27f,.16f),"Regolith fragments",5);
            }else if(kind=="ResearchPacket"){
                b.Box(new Vector3(0,.12f,0),new Vector3(.76f,.23f,.66f),Architecture.Shell,"Scientific data case");
                b.Box(new Vector3(0,.25f,0),new Vector3(.6f,.035f,.49f),Architecture.Dark,"Data interface");
                for(int i=0;i<3;i++)b.Box(new Vector3(-.07f,.277f,(i-1)*.11f),new Vector3(.38f-i*.07f,.015f,.035f),accent,"Data readout",true);
                b.Box(new Vector3(.3f,.15f,.35f),new Vector3(.14f,.12f,.08f),Architecture.Edge,"Case latch");
            }else{
                bool waste=kind=="OrganicWaste",fertilizer=kind=="Fertilizer";
                b.Box(new Vector3(0,.2f,0),new Vector3(.78f,.38f,.72f),waste?Architecture.Dark:Architecture.Shell,"Sealed resource case");
                b.Box(new Vector3(0,.405f,0),new Vector3(.8f,.055f,.74f),Architecture.Edge,"Case lid");
                foreach(int s in new[]{-1,1}){
                    b.Box(new Vector3(s*.27f,.23f,0),new Vector3(.07f,.4f,.74f),accent,"Case strap");
                    b.Box(new Vector3(s*.41f,.25f,0),new Vector3(.06f,.1f,.25f),Architecture.Dark,"Carry handle");
                }
                if(waste||fertilizer){
                    b.Ring(new Vector3(0,.443f,0),.12f,.025f,accent,"Biological resource symbol",false,16);
                    if(fertilizer)for(int i=-1;i<=1;i++)b.Box(new Vector3(i*.08f,.445f,.18f),new Vector3(.04f,.015f,.09f),accent,"Nutrient designation");
                    else b.Beam(new Vector3(-.09f,.445f,-.09f),new Vector3(.09f,.445f,.09f),.035f,accent);
                }else{
                    b.Cylinder(new Vector3(-.1f,.45f,0),.13f,.035f,Architecture.Steel,"Parts gear",12);
                    b.Cylinder(new Vector3(.16f,.45f,.08f),.09f,.035f,Architecture.Steel,"Parts gear",10);
                    for(int i=-1;i<=1;i++)b.Box(new Vector3(i*.1f,.447f,-.18f),new Vector3(.055f,.02f,.1f),Architecture.Dark,"Parts catalog marking");
                }
            }
            b.Finish();
        }

        internal static Texture2D Icon(string kind)
        {
            if(Icons.TryGetValue(kind,out var existing))return existing;
            const int n=64;var colors=new Color[n*n];Color accent=Accent(kind);
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){
                float u=(x-31.5f)/28,v=(y-31.5f)/28,r=Mathf.Sqrt(u*u+v*v);bool paint=false;
                switch(kind){
                    case "NuclearFuelRod":paint=r<.14f||(r>.24f&&r<.78f&&Mathf.Cos(Mathf.Atan2(v,u)*3)>.2f);break;
                    case "FusionCapsule":paint=Mathf.Abs(r-.6f)<.1f||Mathf.Abs(u)<.09f&&Mathf.Abs(v)<.8f;break;
                    case "VehicleChassis":paint=(Mathf.Abs(u)>.5f&&Mathf.Abs(u)<.7f&&Mathf.Abs(v)<.75f)||(Mathf.Abs(v)<.1f||Mathf.Abs(Mathf.Abs(v)-.65f)<.08f)&&Mathf.Abs(u)<.7f;break;
                    case "Regolith":paint=(u+.4f)*(u+.4f)+(v+.3f)*(v+.3f)<.08f||(u-.3f)*(u-.3f)+(v+.25f)*(v+.25f)<.1f||u*u+(v-.35f)*(v-.35f)<.13f;break;
                    case "MachineParts":paint=r>.25f&&r<.55f||r>.4f&&r<.74f&&Mathf.Cos(Mathf.Atan2(v,u)*8)>.2f;break;
                    case "ResearchPacket":paint=Mathf.Abs(u)<.65f&&(Mathf.Abs(v-.45f)<.07f||Mathf.Abs(v)<.07f||Mathf.Abs(v+.45f)<.07f);break;
                    case "Fertilizer":paint=u*u*2+(v-.1f)*(v-.1f)<.48f&&Mathf.Abs(u+v*.4f)>.06f||Mathf.Abs(u)<.06f&&v<.1f&&v>-.75f;break;
                    default:paint=Mathf.Abs(u)<.55f&&v>-.65f&&v<.5f&&Mathf.Abs(u)<.35f+v*.2f||Mathf.Abs(v-.65f)<.08f&&Mathf.Abs(u)<.65f;break;
                }
                colors[y*n+x]=paint?accent:Color.clear;
            }
            var tex=new Texture2D(n,n,TextureFormat.RGBA32,false){name="Own icon "+kind,filterMode=FilterMode.Bilinear};tex.SetPixels(colors);tex.Apply();Icons.Add(kind,tex);return tex;
        }
    }
}
