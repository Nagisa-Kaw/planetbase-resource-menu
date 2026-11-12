using Planetbase;
using PlanetbaseVisuals;
using UnityEngine;

namespace PlanetbaseSolarFrontier
{
    internal static class SolarArchitecture
    {
        internal static void Build(GameObject model, SolarModule type)
        {
            if(!type.isExterior()){
                RoomStyle style=type is ModuleTypeSurveyLab?RoomStyle.Research:type is ModuleTypeAssemblyWorks?RoomStyle.Assembly:type is ModuleTypeTreatmentPlant?RoomStyle.Treatment:type is ModuleTypeSolarGreenhouse?RoomStyle.Greenhouse:RoomStyle.Shelter;
                InteriorArchitecture.Build(model,type is ModuleTypeAssemblyWorks?2:1,style);return;
            }
            Architecture.HideOriginalMeshes(model);
            var b=new Architecture(model.transform);b.Foundation();
            if(type is ModuleTypeFusionPlant)Fusion(b);
            else if(type is ModuleTypeGeothermalPlant)Geothermal(b);
            else if(type is ModuleTypeRegolithDrill)Drill(b);
            else Shield(b);
            b.Finish();
        }

        private static void Fusion(Architecture b)
        {
            b.Cylinder(new Vector3(0,.7f,0),2.85f,.65f,Architecture.Dark,"Octagonal reactor foundation",8);
            b.Cylinder(new Vector3(0,.98f,0),2.78f,.16f,Architecture.Edge,"Reactor access deck",8);
            b.Ring(new Vector3(0,2.0f,0),1.95f,.6f,Architecture.Steel,"Toroidal confinement chamber",false,48);
            b.Ring(new Vector3(0,2.2f,0),2.46f,.035f,Architecture.Cyan,"Confinement status seam",true,48);
            b.Cylinder(new Vector3(0,1.35f,0),.8f,.65f,Architecture.Shell,"Central service core",24);
            b.Cylinder(new Vector3(0,1.71f,0),.58f,.12f,Architecture.Dark,"Core inspection surround",24);
            b.Ring(new Vector3(0,1.78f,0),.43f,.055f,Architecture.Cyan,"Core inspection window",true,32);
            for(int i=0;i<8;i++){
                float a=(i*45+22.5f)*Mathf.Deg2Rad;Vector3 r=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
                Quaternion q=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);
                var coil=b.Box(r*1.95f+Vector3.up*2,new Vector3(.7f,1.32f,1.42f),Architecture.Dark,"Magnetic coil module");coil.localRotation=q;
                b.Beam(r*2.75f+Vector3.up*1.05f,r*2.73f+Vector3.up*2.28f,.35f,Architecture.Shell,.38f);
                b.Beam(r*2.73f+Vector3.up*2.28f,r*2.35f+Vector3.up*2.69f,.35f,Architecture.Shell,.38f);
                b.Beam(r*2.35f+Vector3.up*2.69f,r*1.45f+Vector3.up*2.69f,.35f,Architecture.Shell,.38f);
                b.Beam(r*1.45f+Vector3.up*2.69f,r*1.12f+Vector3.up*2.2f,.3f,Architecture.Shell,.32f);
            }
            b.Hatch(new Vector3(0,.74f,-2.84f),.95f,.48f,Architecture.Cyan);
            b.Hazard(new Vector3(0,.47f,-2.87f),7);
            b.Beacon(new Vector3(2.15f,2.48f,1.28f),Architecture.Cyan);
        }

        private static void Geothermal(Architecture b)
        {
            b.Box(new Vector3(0,.62f,.55f),new Vector3(4.8f,.43f,2.8f),Architecture.Dark,"Geothermal service skid");
            for(int i=-1;i<=1;i+=2){
                b.Cylinder(new Vector3(i*1.25f,.84f,-1.38f),.58f,.92f,Architecture.Shell,"Borehole pressure head",20);
                b.Cylinder(new Vector3(i*1.25f,1.34f,-1.38f),.64f,.13f,Architecture.Edge,"Borehole flange",20);
                b.Hazard(new Vector3(i*1.25f,.63f,-1.96f),5);
                b.Pipe(.2f,Architecture.Steel,new Vector3(i*1.25f,1.35f,-1.38f),new Vector3(i*1.25f,2.15f,-1.38f),new Vector3(i*1.25f,2.42f,-1.1f),new Vector3(i*1.25f,2.42f,.35f));
            }
            b.Box(new Vector3(0,1.76f,.65f),new Vector3(3.4f,1.55f,1.65f),Architecture.Shell,"Heat exchanger and turbine housing");
            b.Box(new Vector3(0,1.8f,-.25f),new Vector3(1.6f,1.15f,.16f),Architecture.Dark,"Heat exchanger recess");
            for(int i=0;i<9;i++)b.Box(new Vector3((i-4)*.17f,1.8f,-.38f),new Vector3(.07f,1.04f,.2f),Architecture.Orange,"Heat exchanger fin");
            var turbine=b.Cylinder(new Vector3(1.99f,1.72f,.65f),.63f,.55f,Architecture.Dark,"Turbine end housing",24);turbine.localRotation=Quaternion.Euler(0,0,90);
            b.Vents(new Vector3(0,2.61f,.5f),1.45f,.23f,4);
            b.Hatch(new Vector3(-1.27f,1.75f,-.22f),.45f,.67f,Architecture.Orange);
            b.Beacon(new Vector3(1.3f,2.55f,1.1f),Architecture.Yellow);
        }

        private static void Drill(Architecture b)
        {
            b.Cylinder(new Vector3(0,.48f,0),1.18f,.2f,Architecture.Dark,"Drill collar",24);
            b.Cylinder(new Vector3(0,1.74f,0),.17f,2.65f,Architecture.Steel,"Auger shaft",16);
            // A continuous helical flight, not a stack of disconnected discs.
            const int steps=96;var v=new System.Collections.Generic.List<Vector3>();var t=new System.Collections.Generic.List<int>();
            for(int i=0;i<=steps;i++){
                float a=i/(float)steps*Mathf.PI*2*4,y=.5f+i/(float)steps*2.55f;
                v.Add(new Vector3(Mathf.Sin(a)*.18f,y,Mathf.Cos(a)*.18f));v.Add(new Vector3(Mathf.Sin(a)*.51f,y+.075f,Mathf.Cos(a)*.51f));
            }
            for(int i=0;i<steps;i++){
                int a=i*2; t.Add(a);t.Add(a+1);t.Add(a+2);t.Add(a+1);t.Add(a+3);t.Add(a+2);
                t.Add(a+2);t.Add(a+1);t.Add(a);t.Add(a+2);t.Add(a+3);t.Add(a+1);
            }
            var mesh=new Mesh{name="Original continuous auger flight"};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            b.MeshPart("Visible helical drill",mesh,Vector3.zero,Vector3.one,Architecture.Edge);
            for(int i=0;i<3;i++){
                float a=(i*120+60)*Mathf.Deg2Rad;Vector3 radial=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
                b.Beam(radial*2.1f+Vector3.up*.5f,radial*.85f+Vector3.up*3.55f,.42f,Architecture.Shell,.5f);
                b.Beam(radial*1.7f+Vector3.up*.55f,radial*.9f+Vector3.up*2.8f,.12f,Architecture.Yellow);
                b.Box(radial*2.1f+Vector3.up*.53f,new Vector3(.7f,.24f,.7f),Architecture.Edge,"Drill foot");
            }
            b.Cylinder(new Vector3(0,3.25f,0),.66f,.65f,Architecture.Dark,"Geared drill motor",16);
            b.Ring(new Vector3(0,3.1f,0),.67f,.07f,Architecture.Yellow);
            b.Cylinder(new Vector3(0,3.75f,0),.96f,.45f,Architecture.Shell,"Drill head enclosure",8);
            b.Vents(new Vector3(0,3.75f,-.92f),.65f,.22f,3);
            b.Box(new Vector3(1.75f,.73f,.7f),new Vector3(1.05f,.7f,1.4f),Architecture.Dark,"Regolith receiving bin");
            b.Box(new Vector3(1.75f,1.12f,.7f),new Vector3(1.14f,.12f,1.48f),Architecture.Shell,"Receiving bin rim");
            b.Box(new Vector3(1.75f,1.2f,.7f),new Vector3(.9f,.13f,1.2f),new Color(.42f,.25f,.15f),"Regolith surface");
            b.Beacon(new Vector3(.4f,4.0f,.2f),Architecture.Yellow);
        }

        private static void Shield(Architecture b)
        {
            b.Cylinder(new Vector3(0,.82f,0),1.55f,.9f,Architecture.Shell,"Shield power base",8);
            b.Hatch(new Vector3(0,.85f,-1.52f),.65f,.7f,Architecture.Cyan);
            b.Cylinder(new Vector3(0,2.83f,0),.38f,3.55f,Architecture.Edge,"Radiation shield mast",16);
            b.Cylinder(new Vector3(0,4.7f,0),.65f,.7f,Architecture.Shell,"Emitter housing",12);
            b.Ring(new Vector3(0,4.6f,0),1.35f,.12f,Architecture.Cyan,"Shield emitter ring",true,32);
            b.Ring(new Vector3(0,5.18f,0),.68f,.07f,Architecture.Cyan,"Shield upper emitter",true,24);
            for(int i=0;i<4;i++){
                float a=(i*90+45)*Mathf.Deg2Rad;var r=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
                b.Beam(r*.45f+Vector3.up*4.1f,r*1.35f+Vector3.up*4.6f,.14f,Architecture.Shell);
                b.Beam(r*1.35f+Vector3.up*4.6f,r*.62f+Vector3.up*5.1f,.12f,Architecture.Shell);
            }
            b.Beacon(new Vector3(0,5.19f,0),Architecture.Cyan);
        }

        internal static void Equipment(GameObject model, ComponentType type)
        { EquipmentArchitecture.Attach(model,type.GetType().Name); }
    }
}
