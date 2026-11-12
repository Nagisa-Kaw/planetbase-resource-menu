using System.Collections.Generic;
using Planetbase;
using UnityEngine;

namespace PlanetbaseVisuals
{
    internal enum RoomStyle { Research, Assembly, Treatment, Greenhouse, Shelter, Fuel, Arcade, Club, Casino, Wellness }

    internal static class InteriorArchitecture
    {
        internal static Color Accent(RoomStyle style)
        {
            switch(style){
                case RoomStyle.Club:return new Color(.8f,.16f,.45f);
                case RoomStyle.Casino:case RoomStyle.Fuel:return Architecture.Yellow;
                case RoomStyle.Greenhouse:case RoomStyle.Wellness:case RoomStyle.Treatment:return Architecture.Green;
                case RoomStyle.Assembly:return Architecture.Orange;
                case RoomStyle.Shelter:return new Color(.43f,.52f,.64f);
                default:return Architecture.Cyan;
            }
        }

        internal static void Build(GameObject model,int sizeIndex,RoomStyle style)
        {
            float r=Module.ValidSizes[sizeIndex]*.5f;Color accent=Accent(style);
            Architecture.HideOriginalMeshes(model,renderer=>!UnderBase(renderer.transform,model.transform));
            // Replace the roof identity on this cloned prefab only. The floor, base,
            // collider, component sockets and native static repair reference remain.
            foreach(var t in model.GetComponentsInChildren<Transform>(true))
                if(t.CompareTag("DomeOpaque")||t.CompareTag("DomeTranslucent")||t.CompareTag("DomeStaticTranslucent"))t.tag="Untagged";

            var floor=new Architecture(model.transform,"Own room deck");
            floor.Cylinder(new Vector3(0,.225f,0),r*.97f,.045f,style==RoomStyle.Wellness?new Color(.31f,.34f,.29f):Architecture.Steel,"Octagonal interior deck",16);
            floor.Ring(new Vector3(0,.26f,0),r*.91f,.035f,accent,"Perimeter guide",true,48);
            for(int i=0;i<4;i++){
                var q=Quaternion.Euler(0,i*90,0);
                floor.Box(q*new Vector3(0,.26f,r*.44f),new Vector3(.68f,.018f,r*.7f),Architecture.Edge,"Walking lane").localRotation=q;
                floor.Box(q*new Vector3(0,.275f,r*.68f),new Vector3(.24f,.02f,.47f),accent,"Entrance marker",true).localRotation=q;
            }
            floor.Cylinder(new Vector3(0,.265f,0),.53f,.02f,Architecture.Dark,"Room center medallion",16);
            floor.Ring(new Vector3(0,.286f,0),.41f,.025f,accent,"Center marker",false,32);
            floor.Finish(style==RoomStyle.Club);

            var roof=new Architecture(model.transform,"Room roof architecture");roof.Root.tag="DomeOpaque";
            int sides=style==RoomStyle.Greenhouse||style==RoomStyle.Wellness?12:8;
            float wall=style==RoomStyle.Shelter?1.85f:style==RoomStyle.Assembly?2.5f:2.18f;
            float top=wall+(style==RoomStyle.Greenhouse?1.4f:style==RoomStyle.Shelter?.58f:.86f);
            Color glass=new Color(.16f,.45f,.52f,style==RoomStyle.Greenhouse?.17f:.28f);
            Color body=style==RoomStyle.Club?Architecture.Dark:Architecture.Shell;
            bool windows=style==RoomStyle.Research||style==RoomStyle.Greenhouse||style==RoomStyle.Wellness||style==RoomStyle.Casino||style==RoomStyle.Arcade;
            Surface(roof,r,sides,new[]{new Vector2(.965f,.32f),new Vector2(.965f,.76f)},body,"Lower pressure hull");
            Surface(roof,r,sides,new[]{new Vector2(.965f,.76f),new Vector2(.965f,wall)},windows?glass:body,"Faceted wall panels");
            Surface(roof,r,sides,new[]{new Vector2(.965f,wall),new Vector2(.82f,wall+.32f),new Vector2(.35f,top),new Vector2(0,top)},style==RoomStyle.Greenhouse?glass:body,"Segmented pressure roof");
            for(int i=0;i<sides;i++){
                float a=(i+.5f)*Mathf.PI*2/sides;Vector3 radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                roof.Beam(radial*r*.965f+Vector3.up*.4f,radial*r*.965f+Vector3.up*wall,.16f,Architecture.Edge,.15f);
                Vector3 eave=radial*r*.965f+Vector3.up*(wall+.07f);
                Vector3 shoulder=radial*r*.82f+Vector3.up*(wall+.39f);
                Vector3 crown=radial*r*.35f+Vector3.up*(top+.07f);
                roof.Beam(eave,shoulder,.13f,Architecture.Edge,.12f);
                roof.Beam(shoulder,crown,.13f,Architecture.Edge,.12f);
                var stripe=roof.Box(radial*r*.974f+Vector3.up*.62f,new Vector3(.58f,.1f,.06f),accent,"Hull designation strip",true);stripe.localRotation=Quaternion.Euler(0,90-a*Mathf.Rad2Deg,0);
            }
            roof.Ring(new Vector3(0,wall+.07f,0),r*.95f,.065f,Architecture.Edge,"Pressure ring",false,48);
            roof.Cylinder(new Vector3(0,top+.08f,0),r*.24f,.16f,Architecture.Dark,"Roof service platform",sides);
            roof.Ring(new Vector3(0,top+.18f,0),r*.22f,.045f,accent,"Roof identification ring",true,32);
            switch(style){
                case RoomStyle.Research:
                    roof.Cylinder(new Vector3(0,top+.35f,0),.35f,.35f,Architecture.Shell,"Instrument mount",16);
                    roof.Lathe(new Vector3(0,top+.59f,0),new[]{new Vector2(.08f,0),new Vector2(.28f,.05f),new Vector2(.65f,.25f),new Vector2(.68f,.31f),new Vector2(.6f,.28f),new Vector2(.22f,.14f),new Vector2(.08f,.1f)},Architecture.Edge,"Research receiver",28).localRotation=Quaternion.Euler(17,0,20);
                    roof.Beam(new Vector3(0,top+.7f,0),new Vector3(.1f,top+1.13f,0),.04f,accent);break;
                case RoomStyle.Assembly:
                    roof.Box(new Vector3(0,top+.35f,0),new Vector3(3,.5f,1.45f),Architecture.Shell,"Assembly air handler");roof.Vents(new Vector3(0,top+.36f,-.76f),2.5f,.34f,5);
                    roof.Box(new Vector3(0,top+.64f,0),new Vector3(2.9f,.1f,1.4f),accent,"Industrial roof fascia");roof.Beacon(new Vector3(1.12f,top+.7f,0),accent);break;
                case RoomStyle.Fuel:case RoomStyle.Treatment:
                    for(int i=-1;i<=1;i++){roof.Cylinder(new Vector3(i*.57f,top+.5f,0),.22f,.7f,Architecture.Shell,"Roof process vessel",20);roof.Ring(new Vector3(i*.57f,top+.72f,0),.23f,.04f,accent);}
                    roof.Pipe(.06f,Architecture.Steel,new Vector3(-.57f,top+.25f,0),new Vector3(-.57f,top+.25f,.5f),new Vector3(.57f,top+.25f,.5f),new Vector3(.57f,top+.25f,0));break;
                case RoomStyle.Greenhouse:
                    roof.Box(new Vector3(0,top+.3f,0),new Vector3(1.6f,.28f,.9f),Architecture.Shell,"Greenhouse climate controller");for(int i=-2;i<=2;i++)roof.Box(new Vector3(i*.25f,top+.48f,0),new Vector3(.1f,.09f,.8f),accent,"Climate controller fin");break;
                case RoomStyle.Shelter:
                    roof.Box(new Vector3(0,top+.3f,0),new Vector3(2,.4f,1.3f),Architecture.Shell,"Shelter life support");roof.Vents(new Vector3(0,top+.32f,-.69f),1.5f,.26f,4);break;
                case RoomStyle.Arcade:
                    for(int i=-1;i<=1;i++)roof.Box(new Vector3(i*.52f,top+.3f,0),new Vector3(.3f,.28f,1.15f),i==0?accent:Architecture.Shell,"Arcade roof fins");break;
                case RoomStyle.Club:
                    roof.Cylinder(new Vector3(0,top+.38f,0),.73f,.38f,Architecture.Dark,"Acoustic roof hub",12);roof.Ring(new Vector3(0,top+.61f,0),.7f,.055f,accent,"Club crown",true,32);
                    for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var p=new Vector3(Mathf.Cos(a)*r*.7f,top-.3f,Mathf.Sin(a)*r*.7f);roof.Box(p,new Vector3(.3f,.12f,.3f),accent,"Club roof light",true);}break;
                case RoomStyle.Casino:
                    roof.Cylinder(new Vector3(0,top+.4f,0),.72f,.4f,Architecture.Shell,"Casino crown",8);roof.Ring(new Vector3(0,top+.65f,0),.57f,.065f,accent,"Amber crown",true,32);break;
                case RoomStyle.Wellness:
                    for(int i=0;i<3;i++)roof.Cylinder(new Vector3(0,top+.23f+i*.13f,0),.87f-i*.18f,.13f,i==1?accent:Architecture.Shell,"Terraced pavilion crown",24);break;
            }
            roof.Finish(style==RoomStyle.Club);
            var geometry=roof.Root.gameObject.AddComponent<ColonyShellGeometry>();geometry.Surfaces=roof.Root.GetComponentsInChildren<MeshFilter>(true);
            var visibility=roof.Root.gameObject.AddComponent<ColonyRoofVisibility>();visibility.NativeRoof=roof.Root.gameObject;visibility.Surfaces=roof.Root.GetComponentsInChildren<Renderer>(true);
        }

        private static bool UnderBase(Transform t,Transform model){while(t!=null&&t!=model){if(t.CompareTag("DomeBase")||t.CompareTag("DomeFloor"))return true;t=t.parent;}return false;}

        private static void Surface(Architecture b,float r,int sides,Vector2[] profile,Color color,string name)
        {
            const int sectors=96;var rings=new List<Vector2>();
            for(int i=0;i<profile.Length-1;i++){
                int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(new Vector2(profile[i].x*r,profile[i].y),new Vector2(profile[i+1].x*r,profile[i+1].y))/.3f));
                for(int s=0;s<steps;s++)rings.Add(Vector2.Lerp(profile[i],profile[i+1],s/(float)steps));
            }
            rings.Add(profile[profile.Length-1]);var v=new List<Vector3>();var tri=new List<int>();float step=Mathf.PI*2/sides;
            for(int h=0;h<rings.Count;h++)for(int a=0;a<=sectors;a++){
                float angle=a*Mathf.PI*2/sectors;float face=(angle+step*.5f)%step-step*.5f;float radius=rings[h].x*r*Mathf.Cos(step*.5f)/Mathf.Cos(face);
                v.Add(new Vector3(Mathf.Cos(angle)*radius,rings[h].y,Mathf.Sin(angle)*radius));
            }
            for(int h=0;h<rings.Count-1;h++)for(int a=0;a<sectors;a++){
                int n=h*(sectors+1)+a,k=n+sectors+1;tri.Add(n);tri.Add(k);tri.Add(n+1);tri.Add(n+1);tri.Add(k);tri.Add(k+1);
            }
            var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();b.MeshPart(name,mesh,Vector3.zero,Vector3.one,color);
        }
    }
}
