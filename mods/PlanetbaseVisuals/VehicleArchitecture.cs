using System.Collections.Generic;
using Planetbase;
using UnityEngine;

namespace PlanetbaseVisuals
{
    internal static class VehicleArchitecture
    {
        private static readonly Color Rubber=new Color(.035f,.045f,.052f);
        private static readonly Color Glass=new Color(.06f,.12f,.16f);

        internal static GameObject Create(Transform parent,string kind,List<Transform> wheels=null)
        {
            GameObject go=VisualAssets.Clone("vehicle "+kind,parent,t=>Build(t,kind));
            if(wheels!=null)for(int i=0;i<4;i++)wheels.Add(go.transform.Find("Wheel "+i));
            return go;
        }

        private static void Build(Transform root,string kind)
        {
            var b=new Architecture(root,"Vehicle body");
            if(kind=="ScoutDrone"){Drone(root,b);return;}
            if(kind=="CargoTruck")Truck(b);
            else if(kind=="ScannerBot")Scanner(b);
            else Rover(b);
            int count=kind=="CargoTruck"?6:4;
            float halfWidth=kind=="CargoTruck"?.95f:kind=="ScannerBot"?.68f:.86f;
            float axle=kind=="CargoTruck"?1:kind=="ScannerBot"?.49f:.88f;
            float radius=kind=="ScannerBot"?.26f:.35f;
            int index=0;
            for(int x=-1;x<=1;x+=2)for(int j=0;j<count/2;j++){
                float z=count==6?(j-1)*axle:(j==0?-axle:axle);
                Wheel(root,new Vector3(x*halfWidth,radius+.035f,z),radius,"Wheel "+index++);
                b.Beam(new Vector3(x*halfWidth*.83f,.67f,z),new Vector3(x*halfWidth,.37f,z),.11f,Architecture.Steel);
                for(int s=0;s<3;s++)b.Ring(new Vector3(x*halfWidth*.89f,.52f+s*.05f,z),.075f,.018f,Architecture.Edge,"Suspension spring",false,12);
            }
            b.Finish();
            if(kind!="ColonyRover"){
                var motion=root.gameObject.AddComponent<VehicleMotion>();motion.Wheels=new Transform[count];for(int i=0;i<count;i++)motion.Wheels[i]=root.Find("Wheel "+i);
            }
        }

        private static void Truck(Architecture b)
        {
            b.Box(new Vector3(0,.5f,-.03f),new Vector3(1.6f,.28f,2.86f),Architecture.Dark,"Truck chassis");
            b.Box(new Vector3(0,.72f,-.7f),new Vector3(1.58f,.15f,1.48f),Architecture.Steel,"Cargo platform");
            b.Wedge(Vector3.zero,1.57f,new[]{new Vector2(.26f,.75f),new Vector2(.26f,1.78f),new Vector2(1.03f,1.78f),new Vector2(1.5f,1.02f),new Vector2(1.5f,.75f)},Architecture.Shell,"Sloped autonomous cab");
            b.Box(new Vector3(0,1.42f,1.31f),new Vector3(1.32f,.77f,.045f),Glass,"Panoramic windshield").localRotation=Quaternion.Euler(-32,0,0);
            b.Box(new Vector3(0,.64f,1.47f),new Vector3(1.7f,.23f,.22f),Architecture.Dark,"Front bumper");
            foreach(int x in new[]{-1,1}){
                b.Box(new Vector3(x*.8f,.91f,1.38f),new Vector3(.18f,.48f,.28f),Architecture.Orange,"Cab corner guard");
                b.Box(new Vector3(x*.51f,.86f,1.5f),new Vector3(.28f,.14f,.05f),new Color(.85f,.88f,.75f),"Headlight",true);
                b.Box(new Vector3(x*.79f,1.44f,.68f),new Vector3(.035f,.4f,.53f),Glass,"Cab side window");
                b.Box(new Vector3(x*.84f,.77f,-.72f),new Vector3(.16f,.2f,1.5f),Architecture.Orange,"Cargo side protection");
                b.Beam(new Vector3(x*.8f,.91f,-1.43f),new Vector3(x*.8f,1.19f,-1.43f),.065f,Architecture.Edge);
                b.Beam(new Vector3(x*.8f,.91f,-.04f),new Vector3(x*.8f,1.19f,-.04f),.065f,Architecture.Edge);
                b.Beam(new Vector3(x*.8f,1.19f,-1.43f),new Vector3(x*.8f,1.19f,-.04f),.065f,Architecture.Edge);
            }
            b.Beam(new Vector3(-.8f,1.19f,-1.43f),new Vector3(.8f,1.19f,-1.43f),.065f,Architecture.Edge);
            b.Box(new Vector3(0,1.84f,.56f),new Vector3(.43f,.13f,.32f),Architecture.Dark,"Autonomous navigation sensor");
            b.Box(new Vector3(0,1.85f,.73f),new Vector3(.22f,.055f,.025f),Architecture.Cyan,"Navigation indicator",true);
            b.Vents(new Vector3(0,.64f,1.6f),.7f,.13f,3,180);
            b.Text("CARGO",new Vector3(.873f,.78f,-.72f),.035f,Architecture.Dark,new Vector3(0,-90,0));
            Socket(b.Root.parent,"Cargo socket",new Vector3(0,.82f,-.73f));
        }

        private static void Rover(Architecture b)
        {
            b.Box(new Vector3(0,.48f,-.14f),new Vector3(1.57f,.26f,2.65f),Architecture.Dark,"Rover chassis");
            b.Box(new Vector3(0,.7f,-.95f),new Vector3(1.5f,.15f,1.05f),Architecture.Steel,"Rover cargo bed");
            foreach(int x in new[]{-1,1}){
                b.Box(new Vector3(x*.75f,.91f,-.95f),new Vector3(.11f,.32f,1.05f),Architecture.Shell,"Cargo bed wall");
                b.Box(new Vector3(x*.78f,.68f,.4f),new Vector3(.2f,.16f,1.5f),Architecture.Orange,"Rover side rail");
                b.Box(new Vector3(x*.67f,.89f,1.1f),new Vector3(.33f,.43f,.4f),Architecture.Shell,"Front wheel fender");
                b.Box(new Vector3(x*.57f,.84f,1.32f),new Vector3(.28f,.12f,.05f),new Color(.8f,.86f,.74f),"Rover light",true);
                b.Beam(new Vector3(x*.69f,.75f,.02f),new Vector3(x*.69f,1.75f,.02f),.065f,Architecture.Shell);
            }
            b.Beam(new Vector3(-.69f,1.75f,.02f),new Vector3(.69f,1.75f,.02f),.065f,Architecture.Shell);
            b.Box(new Vector3(0,.75f,.38f),new Vector3(.68f,.16f,.6f),Architecture.Dark,"Driver seat cushion");
            b.Box(new Vector3(0,1.03f,.08f),new Vector3(.68f,.55f,.12f),Architecture.Dark,"Driver backrest").localRotation=Quaternion.Euler(-10,0,0);
            b.Box(new Vector3(0,1.13f,.99f),new Vector3(.92f,.16f,.28f),Architecture.Shell,"Driver dashboard");
            b.Screen(new Vector3(.2f,1.22f,.93f),new Vector2(.3f,.15f),Architecture.Cyan,180);
            b.Ring(new Vector3(-.2f,1.27f,.85f),.15f,.027f,Architecture.Dark,"Steering wheel",false,20).localRotation=Quaternion.Euler(58,0,0);
            b.Box(new Vector3(0,.59f,1.37f),new Vector3(1.52f,.16f,.18f),Architecture.Dark,"Rover bumper");
            b.Hazard(new Vector3(0,.61f,-1.49f),6);
        }

        private static void Scanner(Architecture b)
        {
            b.Box(new Vector3(0,.46f,0),new Vector3(1.15f,.3f,1.5f),Architecture.Dark,"Scanner undercarriage");
            b.Wedge(Vector3.zero,1.12f,new[]{new Vector2(-.66f,.57f),new Vector2(-.66f,.98f),new Vector2(.3f,.98f),new Vector2(.7f,.7f),new Vector2(.7f,.57f)},Architecture.Shell,"Scanner armored body");
            b.Box(new Vector3(0,.73f,.71f),new Vector3(.65f,.12f,.04f),Glass,"Front sensor window");
            b.Box(new Vector3(0,.74f,.74f),new Vector3(.35f,.035f,.02f),Architecture.Cyan,"Range finder",true);
            b.Cylinder(new Vector3(0,1.17f,-.15f),.09f,.48f,Architecture.Edge,"Sensor mast",16);
            b.Cylinder(new Vector3(0,1.43f,-.15f),.3f,.12f,Architecture.Dark,"Scanner array",16);
            b.Ring(new Vector3(0,1.52f,-.15f),.27f,.035f,Architecture.Cyan,"Scanner ring",true,24);
            b.Beam(new Vector3(0,1.5f,-.15f),new Vector3(.1f,1.78f,-.15f),.035f,Architecture.Shell);
            b.Box(new Vector3(0,.99f,-.49f),new Vector3(.85f,.08f,.24f),Architecture.Cyan,"Scanner identification band");
        }

        private static void Drone(Transform root,Architecture b)
        {
            root.localPosition=Vector3.up*1.6f;
            b.Box(new Vector3(0,0,0),new Vector3(.72f,.28f,1.03f),Architecture.Dark,"Drone lower fuselage");
            b.Box(new Vector3(0,.15f,0),new Vector3(.76f,.16f,1.08f),Architecture.Shell,"Drone upper shell");
            b.Box(new Vector3(0,.08f,.53f),new Vector3(.69f,.1f,.1f),Architecture.Orange,"Drone nose guard");
            b.Box(new Vector3(0,.22f,-.22f),new Vector3(.34f,.04f,.25f),Architecture.Edge,"Avionics panel");
            b.Cylinder(new Vector3(0,-.27f,.31f),.14f,.23f,Architecture.Dark,"Camera gimbal",16).localRotation=Quaternion.Euler(90,0,0);
            b.Ring(new Vector3(0,-.27f,.44f),.085f,.025f,Architecture.Cyan,"Camera lens",true,20).localRotation=Quaternion.Euler(90,0,0);
            for(int x=-1;x<=1;x+=2){
                b.Beam(new Vector3(x*.27f,-.12f,-.28f),new Vector3(x*.34f,-.4f,-.28f),.05f,Architecture.Edge);
                b.Beam(new Vector3(x*.27f,-.12f,.32f),new Vector3(x*.34f,-.4f,.32f),.05f,Architecture.Edge);
                b.Beam(new Vector3(x*.34f,-.4f,-.44f),new Vector3(x*.34f,-.4f,.49f),.07f,Architecture.Dark);
            }
            b.Cylinder(new Vector3(.23f,.39f,-.31f),.025f,.36f,Architecture.Dark,"Drone antenna",8);
            b.Box(new Vector3(.23f,.58f,-.31f),new Vector3(.05f,.045f,.05f),Architecture.Cyan,"Antenna indicator",true);
            var rotors=new List<Transform>();
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){
                Vector3 p=new Vector3(x*.85f,.02f,z*.61f);
                b.Beam(new Vector3(x*.28f,-.02f,z*.32f),p,.12f,Architecture.Dark,.17f);
                b.Lathe(p,new[]{new Vector2(.32f,-.09f),new Vector2(.4f,-.09f),new Vector2(.4f,.09f),new Vector2(.32f,.09f),new Vector2(.32f,-.09f)},Architecture.Shell,"Protected rotor duct",32);
                b.Box(p+new Vector3(0,0,z*.395f),new Vector3(.15f,.19f,.03f),Architecture.Orange,"Rotor safety marking");
                var pivot=new GameObject("Rotor "+rotors.Count).transform;pivot.SetParent(root,false);pivot.localPosition=p;
                var rotor=new Architecture(pivot,"Rotor blades");rotor.Cylinder(Vector3.zero,.075f,.16f,Architecture.Edge,"Rotor hub",16);
                for(int blade=0;blade<3;blade++)rotor.Box(Vector3.zero,new Vector3(.065f,.025f,.58f),Architecture.Dark,"Propeller blade").localRotation=Quaternion.Euler(0,blade*60,0);
                rotor.Finish();rotors.Add(pivot);
            }
            b.Finish();var motion=root.gameObject.AddComponent<VehicleMotion>();motion.Rotors=rotors.ToArray();
        }

        private static void Wheel(Transform parent,Vector3 p,float radius,string name)
        {
            var pivot=new GameObject(name).transform;pivot.SetParent(parent,false);pivot.localPosition=p;pivot.localRotation=Quaternion.Euler(0,0,90);
            var b=new Architecture(pivot,"Wheel geometry");
            b.Ring(Vector3.zero,radius*.74f,radius*.26f,Rubber,"Off-road tire",false,32);
            b.Cylinder(Vector3.zero,radius*.65f,.19f,Architecture.Steel,"Recessed wheel hub",20);
            b.Cylinder(Vector3.zero,radius*.28f,.22f,Architecture.Edge,"Axle cap",16);
            for(int i=0;i<16;i++){
                float a=i*Mathf.PI/8;
                b.Box(new Vector3(Mathf.Sin(a)*radius*.94f,0,Mathf.Cos(a)*radius*.94f),new Vector3(radius*.32f,.21f,radius*.17f),Rubber,"Tire tread").localRotation=Quaternion.Euler(0,i*22.5f,0);
            }
            b.Finish();
        }
        private static void Socket(Transform root,string name,Vector3 p){var g=new GameObject(name);g.transform.SetParent(root,false);g.transform.localPosition=p;}
    }

    internal sealed class VehicleMotion:MonoBehaviour
    {
        public Transform[] Wheels,Rotors;private Vector3 previous;private bool initialized;
        private void LateUpdate(){
            if(!initialized){previous=transform.position;initialized=true;}
            if(!Singleton<TimeManager>.getInstance().isPaused()){
                float delta=Vector3.Distance(transform.position,previous);
                if(Wheels!=null)foreach(var wheel in Wheels)if(wheel!=null)wheel.Rotate(Vector3.up,delta*160,Space.Self);
                if(Rotors!=null)foreach(var rotor in Rotors)if(rotor!=null)rotor.Rotate(Vector3.up,Time.deltaTime*900,Space.Self);
            }
            previous=transform.position;
        }
    }
}
