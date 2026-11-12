using Planetbase;
using PlanetbaseVisuals;
using UnityEngine;

namespace PlanetbaseNuclearTransport
{
    internal static class ExpansionArchitecture
    {
        internal static void Build(GameObject model, ExpansionModule type)
        {
            if(type is ModuleTypeFuelWorkshop){InteriorArchitecture.Build(model,1,RoomStyle.Fuel);return;}
            Architecture.HideOriginalMeshes(model);
            var b=new Architecture(model.transform);
            b.Foundation();
            if(type is ModuleTypeNuclearReactor) Reactor(b);
            else if(type is ModuleTypeCoolingTower) Cooling(b);
            else Garage(b);
            b.Finish();
            if(type is ModuleTypeRoverGarage){
                for(int i=0;i<2;i++){
                    var parked=ExpansionModels.Rover(model.transform,new Vector3((i==0?-1:1)*1.3f,.36f,.1f),null);
                    parked.name="Parked rover "+i;parked.transform.localScale=Vector3.one*.75f;
                }
                model.AddComponent<GarageDisplay>();
            }
        }

        private static void Reactor(Architecture b)
        {
            b.Cylinder(new Vector3(0,.58f,0),2.25f,.55f,Architecture.Dark,"Containment pedestal");
            b.Lathe(Vector3.zero,new[]{new Vector2(0,.72f),new Vector2(1.95f,.72f),new Vector2(2.08f,.9f),new Vector2(2.08f,3.72f),new Vector2(1.97f,4.08f),new Vector2(1.6f,4.53f),new Vector2(.95f,4.9f),new Vector2(.45f,5.02f),new Vector2(0,5.02f)},Architecture.Shell,"Reinforced containment vessel",40);
            foreach(float y in new[]{1.45f,3.25f})b.Ring(new Vector3(0,y,0),2.1f,.065f,Architecture.Yellow,"Yellow containment band");
            for(int i=0;i<8;i++){
                float a=(i*45+22.5f)*Mathf.Deg2Rad;Vector3 radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                b.Beam(radial*2.12f+Vector3.up*.85f,radial*2.12f+Vector3.up*3.8f,.18f,Architecture.Edge,.14f);
                b.Beam(radial*2.12f+Vector3.up*3.8f,radial*1.53f+Vector3.up*4.64f,.16f,Architecture.Edge);
                b.Beam(radial*1.53f+Vector3.up*4.64f,radial*.45f+Vector3.up*5.05f,.14f,Architecture.Edge);
            }
            b.Cylinder(new Vector3(0,5.06f,0),.52f,.13f,Architecture.Edge,"Containment top hatch",16);
            for(int i=-1;i<=1;i+=2){
                b.Box(new Vector3(i*1.23f,.96f,-2.15f),new Vector3(.87f,1.15f,.9f),Architecture.Dark,"Coolant radiator");
                b.Vents(new Vector3(i*1.23f,.97f,-2.62f),.65f,.85f,9);
                b.Pipe(.16f,Architecture.Steel,new Vector3(i*1.23f,1.5f,-2.15f),new Vector3(i*1.23f,2.1f,-2.15f),new Vector3(i*1.23f,2.35f,-1.9f),new Vector3(i*1.23f,2.35f,-1.55f));
            }
            b.Hatch(new Vector3(0,1.04f,-2.17f),.8f,1.04f,Architecture.Yellow);
            b.Hazard(new Vector3(0,.58f,-2.28f),6);
            b.Beacon(new Vector3(1.7f,4.05f,-.6f),Architecture.Yellow);
        }

        private static void Cooling(Architecture b)
        {
            // Continuous inner wall and an open mouth, rather than a painted flat top.
            b.Lathe(Vector3.zero,new[]{new Vector2(2.45f,.65f),new Vector2(2.35f,1.05f),new Vector2(1.86f,2.15f),new Vector2(1.54f,3.5f),new Vector2(1.63f,4.55f),new Vector2(1.98f,5.65f),new Vector2(1.82f,5.65f)},Architecture.Shell,"Open cooling tower outer wall",40);
            b.Lathe(Vector3.zero,new[]{new Vector2(1.82f,5.65f),new Vector2(1.48f,4.5f),new Vector2(1.39f,3.5f),new Vector2(1.7f,2.15f),new Vector2(2.27f,.65f)},Architecture.Dark,"Recessed cooling tower inner wall",40);
            b.Cylinder(new Vector3(0,.51f,0),2.5f,.3f,Architecture.Dark,"Cooling plenum");
            b.Ring(new Vector3(0,.76f,0),2.42f,.13f,Architecture.Cyan,"Coolant distribution manifold");
            b.Ring(new Vector3(0,5.66f,0),1.93f,.1f,Architecture.Edge,"Tower rim");
            b.Ring(new Vector3(0,5.96f,0),1.95f,.035f,Architecture.Yellow,"Rim safety rail");
            for(int i=0;i<12;i++){
                float a=i*Mathf.PI/6;Vector3 radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                b.Beam(radial*1.95f+Vector3.up*5.65f,radial*1.95f+Vector3.up*5.96f,.035f,Architecture.Edge);
                b.Beam(radial*2.47f+Vector3.up*.55f,radial*1.91f+Vector3.up*2.15f,.15f,Architecture.Edge);
                b.Beam(radial*1.91f+Vector3.up*2.15f,radial*1.59f+Vector3.up*3.5f,.09f,Architecture.Edge);
                b.Beam(radial*1.59f+Vector3.up*3.5f,radial*1.68f+Vector3.up*4.55f,.09f,Architecture.Edge);
                b.Beam(radial*1.68f+Vector3.up*4.55f,radial*2.03f+Vector3.up*5.65f,.1f,Architecture.Edge);
            }
            b.Box(new Vector3(1.7f,.92f,-1.85f),new Vector3(.85f,1.05f,.8f),Architecture.Shell,"Coolant pump enclosure");
            b.Hatch(new Vector3(1.7f,.94f,-2.29f),.52f,.72f,Architecture.Cyan);
            b.Pipe(.16f,Architecture.Steel,new Vector3(.9f,.7f,-2.2f),new Vector3(1.25f,.7f,-2.2f),new Vector3(1.45f,1.22f,-2.2f),new Vector3(1.7f,1.22f,-2.2f));
        }

        private static void Garage(Architecture b)
        {
            b.Box(new Vector3(0,.43f,0),new Vector3(5.7f,.15f,4.2f),Architecture.Steel,"Garage service deck");
            foreach(float x in new[]{-2.65f,0,2.65f})b.Box(new Vector3(x,1.6f,.2f),new Vector3(x==0?.2f:.3f,2.3f,3.6f),Architecture.Shell,"Bay structural frame");
            b.Box(new Vector3(0,1.6f,1.95f),new Vector3(5.4f,2.3f,.22f),Architecture.Dark,"Recessed workshop wall");
            b.Box(new Vector3(0,2.88f,.15f),new Vector3(5.65f,.35f,4.1f),Architecture.Shell,"Garage roof");
            b.Box(new Vector3(0,2.78f,-1.97f),new Vector3(5.7f,.48f,.3f),Architecture.Orange,"Orange garage fascia");
            for(int i=-1;i<=1;i+=2){
                b.Box(new Vector3(i*1.3f,2.47f,-1.95f),new Vector3(1.5f,.09f,.07f),new Color(.8f,.8f,.65f),"Bay work light",true);
                b.Hazard(new Vector3(i*1.3f,.48f,-1.94f),8);
                b.Vents(new Vector3(i*2.82f,1.28f,.3f),1.3f,.65f,6,i<0?90:-90);
            }
            b.Box(new Vector3(.3f,3.22f,.5f),new Vector3(2.3f,.48f,1.15f),Architecture.Shell,"Garage roof service module");
            b.Vents(new Vector3(.3f,3.24f,-.1f),1.9f,.28f,4);
            b.Beacon(new Vector3(1.95f,3.1f,1.1f),Architecture.Yellow);
        }

        internal static void Equipment(GameObject model, ComponentType type)
        { EquipmentArchitecture.Attach(model,type.GetType().Name); }
    }
}
