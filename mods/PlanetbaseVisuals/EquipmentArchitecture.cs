using UnityEngine;

namespace PlanetbaseVisuals
{
    internal static class EquipmentArchitecture
    {
        internal static void Attach(GameObject model,string kind)
        {
            Architecture.HideOriginalMeshes(model);
            var anchor=Architecture.Tagged(model.transform,"AnchorPoint");
            Vector3 p=anchor==null?new Vector3(0,0,1.2f):model.transform.InverseTransformPoint(anchor.position);
            Quaternion q=anchor==null?Quaternion.Euler(0,180,0):Quaternion.Inverse(model.transform.rotation)*anchor.rotation;
            var anchors=new System.Collections.Generic.List<Vector3>();
            var facings=new System.Collections.Generic.List<Quaternion>();
            foreach(var point in model.GetComponentsInChildren<Transform>(true))if(point.CompareTag("AnchorPoint")){
                anchors.Add(model.transform.InverseTransformPoint(point.position));
                facings.Add(Quaternion.Inverse(model.transform.rotation)*point.rotation);
            }
            VisualAssets.Clone("equipment "+kind,model.transform,t=>{
                Build(t,kind,p,q);
                // A native bench can expose several animation anchors even with one user.
                // Keep a visible seat at every position the native user may choose.
                if(kind=="MeditationSeat"&&anchors.Count>1){
                    var seats=new Architecture(t,"Additional native seat positions");
                    for(int i=1;i<anchors.Count;i++)Meditation(seats,anchors[i],facings[i]);
                    seats.Finish();
                }
            });
        }

        private static void Build(Transform root,string kind,Vector3 anchor,Quaternion facing)
        {
            var b=new Architecture(root,"Own equipment geometry");
            switch(kind){
                case "NuclearFuelStore":Cabinet(b,Architecture.Yellow,"FUEL",true);break;
                case "NutrientHopper":Cabinet(b,Architecture.Green,"NPK",false);break;
                case "SanitaryUnit":Sanitary(b);break;
                case "MeditationSeat":Meditation(b,anchor,facing);break;
                case "ArcadeCabinet":Arcade(b,false,anchor,facing);break;
                case "CasinoTerminal":Arcade(b,true,anchor,facing);break;
                case "ResearchDesk":Research(b);Console(b,anchor,facing,Architecture.Cyan);break;
                case "FuelCentrifuge":Centrifuge(b);Console(b,anchor,facing,Architecture.Yellow);break;
                case "WasteReprocessor":Process(b,Architecture.Green,true);Console(b,anchor,facing,Architecture.Green);break;
                case "FusionRefinery":Process(b,Architecture.Cyan,false);Console(b,anchor,facing,Architecture.Cyan);break;
                case "RegolithProcessor":Mineral(b);Console(b,anchor,facing,Architecture.Yellow);break;
                case "AdvancedAssembler":Assembly(b,true);Console(b,anchor,facing,Architecture.Orange);break;
                case "ChassisBench":Assembly(b,false);Console(b,anchor,facing,Architecture.Orange);break;
                default:Parts(b);Console(b,anchor,facing,Architecture.Orange);break;
            }
            b.Finish();
        }

        private static void Cabinet(Architecture b,Color accent,string label,bool nuclear)
        {
            b.Box(new Vector3(0,.82f,0),new Vector3(1.18f,1.62f,.58f),Architecture.Shell,"Storage enclosure");
            b.Box(new Vector3(0,.84f,.31f),new Vector3(.97f,1.3f,.08f),Architecture.Dark,"Recessed storage face");
            if(nuclear){
                for(int x=-1;x<=1;x+=2){
                    b.Box(new Vector3(x*.24f,.87f,.365f),new Vector3(.4f,1.19f,.06f),Architecture.Edge,"Shielded cabinet door");
                    b.Box(new Vector3(x*.08f,.85f,.41f),new Vector3(.04f,.25f,.04f),Architecture.Dark,"Door handle");
                }
                b.Hazard(new Vector3(0,.22f,.42f),6,180);
                b.Ring(new Vector3(0,1.2f,.42f),.13f,.025f,accent,"Shielding symbol",false,16).localRotation=Quaternion.Euler(90,0,0);
            }else{
                for(int y=0;y<3;y++){
                    b.Box(new Vector3(0,.43f+y*.37f,.37f),new Vector3(.9f,.29f,.09f),Architecture.Edge,"Nutrient cassette");
                    b.Box(new Vector3(0,.44f+y*.37f,.43f),new Vector3(.36f,.055f,.025f),accent,"Cassette grip");
                }
            }
            b.Box(new Vector3(0,1.65f,0),new Vector3(1.2f,.13f,.6f),accent,"Storage category fascia");
            b.Text(label,new Vector3(0,1.57f,.445f),.065f,Architecture.Dark,new Vector3(0,180,0));
            b.Box(new Vector3(.43f,1.37f,.37f),new Vector3(.07f,.08f,.03f),accent,"Storage indicator",true);
        }

        private static void Base(Architecture b,float w=1.55f,float d=1.15f)
        {
            b.Box(new Vector3(0,.12f,0),new Vector3(w,.24f,d),Architecture.Dark,"Machine base");
            foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})b.Box(new Vector3(x*(w*.42f),.08f,z*(d*.4f)),new Vector3(.16f,.16f,.2f),Architecture.Edge,"Machine foot");
        }

        private static void Research(Architecture b)
        {
            foreach(int x in new[]{-1,1})b.Box(new Vector3(x*.67f,.5f,0),new Vector3(.18f,.96f,1.1f),Architecture.Shell,"Research desk support");
            b.Box(new Vector3(0,1.02f,0),new Vector3(1.72f,.15f,1.2f),Architecture.Edge,"Research work surface");
            b.Box(new Vector3(-.35f,1.44f,-.32f),new Vector3(.87f,.68f,.12f),Architecture.Dark,"Research monitor");
            b.Screen(new Vector3(-.35f,1.44f,-.24f),new Vector2(.7f,.48f),Architecture.Cyan);
            b.Cylinder(new Vector3(.49f,1.12f,-.05f),.24f,.12f,Architecture.Dark,"Sample scanner base",20);
            b.Lathe(new Vector3(.49f,1.15f,-.05f),new[]{new Vector2(.2f,0),new Vector2(.2f,.42f),new Vector2(0,.42f)},new Color(.15f,.65f,.7f,.28f),"Sample chamber",24);
            b.Ring(new Vector3(.49f,1.59f,-.05f),.2f,.04f,Architecture.Cyan,"Sample chamber rim",true,20);
            b.Box(new Vector3(.5f,1.36f,-.05f),new Vector3(.1f,.22f,.12f),Architecture.Shell,"Scientific sample");
            b.Box(new Vector3(-.34f,1.12f,.33f),new Vector3(.65f,.055f,.24f),Architecture.Dark,"Research controls");
            b.Text("SCIENCE",new Vector3(0,.89f,.61f),.043f,Architecture.Cyan,new Vector3(0,180,0));
        }

        private static void Centrifuge(Architecture b)
        {
            Base(b,1.6f,1.15f);
            b.Box(new Vector3(0,.62f,-.05f),new Vector3(1.38f,.84f,.86f),Architecture.Shell,"Centrifuge drive housing");
            for(int i=-1;i<=1;i++){
                b.Cylinder(new Vector3(i*.45f,1.32f,-.13f),.18f,.92f,Architecture.Steel,"Centrifuge vessel",20);
                b.Cylinder(new Vector3(i*.45f,1.8f,-.13f),.2f,.13f,Architecture.Shell,"Shielded vessel cap",20);
                b.Ring(new Vector3(i*.45f,1.6f,-.13f),.185f,.04f,Architecture.Yellow,"Fuel warning band");
                b.Pipe(.045f,Architecture.Edge,new Vector3(i*.45f,1.07f,-.13f),new Vector3(i*.45f,1.07f,-.5f),new Vector3(i*.45f,.65f,-.5f));
            }
            b.Hazard(new Vector3(0,.5f,.43f),8,180);
            b.Text("FUEL",new Vector3(0,.86f,.425f),.07f,Architecture.Dark,new Vector3(0,180,0));
        }

        private static void Process(Architecture b,Color accent,bool recycling)
        {
            Base(b);
            b.Box(new Vector3(0,.45f,0),new Vector3(1.43f,.42f,.98f),Architecture.Shell,"Process pump housing");
            for(int x=-1;x<=1;x+=2){
                b.Cylinder(new Vector3(x*.4f,1.12f,-.08f),.27f,1.04f,Architecture.Shell,"Process vessel",24);
                b.Ring(new Vector3(x*.4f,1.49f,-.08f),.275f,.055f,accent,"Vessel designation");
                b.Cylinder(new Vector3(x*.4f,1.68f,-.08f),.23f,.11f,Architecture.Dark,"Pressure closure",20);
            }
            b.Pipe(.07f,Architecture.Steel,new Vector3(-.4f,1.7f,-.08f),new Vector3(-.4f,1.88f,-.08f),new Vector3(.4f,1.88f,-.08f),new Vector3(.4f,1.7f,-.08f));
            b.Vents(new Vector3(0,.45f,.52f),.8f,.22f,4,180);
            b.Text(recycling?"BIO":"FUSION",new Vector3(0,.66f,.52f),.07f,accent,new Vector3(0,180,0));
        }

        private static void Mineral(Architecture b)
        {
            Base(b,1.65f,1.25f);
            b.Box(new Vector3(0,.52f,-.1f),new Vector3(1.35f,.55f,1.05f),Architecture.Shell,"Mineral separator");
            b.Wedge(new Vector3(0,.82f,-.12f),1.15f,new[]{new Vector2(-.4f,0),new Vector2(-.55f,.6f),new Vector2(.55f,.6f),new Vector2(.4f,0)},Architecture.Edge,"Intake hopper");
            b.Box(new Vector3(0,1.43f,-.12f),new Vector3(.98f,.025f,.85f),Architecture.Dark,"Hopper opening");
            for(int i=-2;i<=2;i++)b.Box(new Vector3(i*.17f,1.46f,-.12f),new Vector3(.045f,.035f,.9f),Architecture.Steel,"Hopper safety grate");
            b.Box(new Vector3(0,.6f,.5f),new Vector3(.85f,.3f,.35f),Architecture.Dark,"Mineral output chute");
            b.Hazard(new Vector3(0,.9f,.45f),7,180);
        }

        private static void Parts(Architecture b)
        {
            Base(b,1.7f,1.2f);
            b.Box(new Vector3(0,.56f,0),new Vector3(1.5f,.63f,.92f),Architecture.Shell,"Parts machining station");
            b.Box(new Vector3(0,.94f,0),new Vector3(1.68f,.14f,1.14f),Architecture.Steel,"Machining bed");
            b.Box(new Vector3(-.5f,1.23f,-.18f),new Vector3(.3f,.5f,.43f),Architecture.Dark,"Tool spindle housing");
            b.Beam(new Vector3(-.5f,1.5f,-.18f),new Vector3(.23f,1.5f,-.18f),.18f,Architecture.Shell,.23f);
            b.Cylinder(new Vector3(.23f,1.31f,-.18f),.075f,.3f,Architecture.Edge,"Machining spindle",12);
            b.Cylinder(new Vector3(.23f,1.05f,-.18f),.19f,.07f,Architecture.Orange,"Machined part",12);
            b.Box(new Vector3(0,.58f,.49f),new Vector3(.72f,.3f,.08f),Architecture.Dark,"Tool drawer");
            b.Box(new Vector3(0,.59f,.55f),new Vector3(.38f,.055f,.05f),Architecture.Orange,"Tool drawer grip");
        }

        private static void Assembly(Architecture b,bool robots)
        {
            Base(b,1.8f,1.35f);
            b.Box(new Vector3(0,.6f,0),new Vector3(1.58f,.75f,1.05f),Architecture.Shell,"Assembly pedestal");
            b.Box(new Vector3(0,1.04f,0),new Vector3(1.72f,.13f,1.2f),Architecture.Dark,"Assembly jig");
            foreach(int x in new[]{-1,1}){
                b.Beam(new Vector3(x*.77f,1.05f,-.43f),new Vector3(x*.77f,2.02f,-.43f),.13f,Architecture.Shell);
                b.Box(new Vector3(x*.48f,1.14f,.13f),new Vector3(.12f,.14f,.8f),Architecture.Edge,"Assembly fixture rail");
            }
            b.Box(new Vector3(0,2.02f,-.43f),new Vector3(1.72f,.23f,.28f),Architecture.Orange,"Assembly gantry");
            b.Beam(new Vector3(.34f,1.87f,-.4f),new Vector3(.34f,1.47f,-.05f),.1f,Architecture.Dark);
            b.Box(new Vector3(.34f,1.42f,-.05f),new Vector3(.24f,.11f,.19f),Architecture.Steel,"Assembly gripper");
            if(robots)b.Cylinder(new Vector3(0,1.28f,.13f),.2f,.24f,Architecture.Shell,"Robot assembly blank",12);
            else foreach(int z in new[]{-1,1})b.Box(new Vector3(0,1.2f,z*.21f),new Vector3(.95f,.09f,.1f),Architecture.Orange,"Chassis crossmember");
            b.Text(robots?"ROBOTICS":"CHASSIS",new Vector3(0,.74f,.55f),.06f,Architecture.Dark,new Vector3(0,180,0));
        }

        private static void Console(Architecture b,Vector3 anchor,Quaternion facing,Color accent)
        {
            if(anchor.sqrMagnitude<.2f)return;
            Vector3 p=anchor+facing*Vector3.forward*.43f;p.y=.67f;
            var post=b.Box(p,new Vector3(.16f,1.05f,.19f),Architecture.Edge,"Operator console stand");post.localRotation=facing;
            p.y=1.25f;b.Screen(p,new Vector2(.56f,.3f),accent,facing.eulerAngles.y+180);
        }

        private static void Sanitary(Architecture b)
        {
            b.Box(new Vector3(0,.38f,-.12f),new Vector3(.53f,.73f,.39f),Architecture.Shell,"Sanitary service pedestal");
            b.Lathe(new Vector3(0,.79f,.06f),new[]{new Vector2(.06f,-.1f),new Vector2(.27f,-.02f),new Vector2(.31f,.05f),new Vector2(.27f,.09f),new Vector2(.13f,.01f),new Vector2(.06f,-.02f)},Architecture.Steel,"Sanitary basin",28);
            b.Pipe(.025f,Architecture.Edge,new Vector3(.14f,.77f,-.12f),new Vector3(.14f,1.04f,-.12f),new Vector3(.14f,1.04f,.06f));
            b.Cylinder(new Vector3(-.26f,.48f,-.09f),.085f,.55f,Architecture.Edge,"Water filter",16);
            b.Ring(new Vector3(-.26f,.68f,-.09f),.09f,.025f,Architecture.Green,"Filter purity band");
            b.Box(new Vector3(0,.48f,.09f),new Vector3(.23f,.055f,.025f),Architecture.Green,"Sanitary status",true);
        }

        private static void Chair(Architecture b,Vector3 anchor,Quaternion q,bool wellness)
        {
            // Place the new cushion at the original animation anchor, preserving seated poses.
            Vector3 p=anchor;p.y=.43f;
            b.Box(p,new Vector3(wellness?.86f:.61f,.16f,.58f),Architecture.Dark,"Seat cushion").localRotation=q;
            b.Box(p+q*new Vector3(0,.28f,-.28f),new Vector3(wellness?.86f:.61f,.58f,.13f),wellness?Architecture.Shell:Architecture.Dark,"Seat back").localRotation=q;
            b.Cylinder(new Vector3(p.x,.2f,p.z),.075f,.35f,Architecture.Edge,"Seat pedestal",12);
            b.Cylinder(new Vector3(p.x,.055f,p.z),.3f,.09f,Architecture.Dark,"Seat base",16);
            if(wellness)foreach(int x in new[]{-1,1}){
                Vector3 side=p+q*new Vector3(x*.47f,.13f,0);
                b.Box(side,new Vector3(.14f,.4f,.6f),Architecture.Shell,"Meditation armrest").localRotation=q;
                b.Box(side+Vector3.up*.22f,new Vector3(.13f,.04f,.4f),Architecture.Green,"Calming armrest light",true).localRotation=q;
            }
        }

        private static void Meditation(Architecture b,Vector3 anchor,Quaternion facing)
        {
            Chair(b,anchor,facing,true);
            Vector3 p=anchor+facing*new Vector3(0,0,-.42f);p.y=.11f;
            b.Box(p,new Vector3(1.17f,.12f,1.1f),Architecture.Edge,"Wellness platform").localRotation=facing;
            b.Ring(p+Vector3.up*.08f,.48f,.025f,Architecture.Green,"Wellness floor ring",true,32);
        }

        private static void Arcade(Architecture b,bool casino,Vector3 anchor,Quaternion facing)
        {
            Color accent=casino?Architecture.Yellow:Architecture.Cyan;
            b.Box(new Vector3(0,.74f,.04f),new Vector3(1.24f,1.45f,.64f),Architecture.Shell,"Entertainment cabinet");
            b.Box(new Vector3(0,1.5f,.02f),new Vector3(1.36f,.2f,.78f),accent,"Cabinet marquee");
            b.Box(new Vector3(0,1.15f,.385f),new Vector3(1.07f,.61f,.09f),Architecture.Dark,"Entertainment display surround");
            if(casino){
                for(int i=-1;i<=1;i++){
                    b.Box(new Vector3(i*.29f,1.15f,.44f),new Vector3(.23f,.43f,.03f),Architecture.Edge,"Slot reel");
                    b.Ring(new Vector3(i*.29f,1.15f,.464f),.065f,.018f,accent,"Reel symbol",true,12).localRotation=Quaternion.Euler(90,0,0);
                }
            }else{
                b.Screen(new Vector3(0,1.15f,.43f),new Vector2(.84f,.45f),accent);
                for(int i=0;i<5;i++)b.Box(new Vector3((i-2)*.14f,1.15f+(i%2)*.1f,.53f),new Vector3(.09f,.09f,.025f),i%2==0?accent:Architecture.Orange,"Arcade game sprite",true);
            }
            b.Box(new Vector3(0,.82f,.48f),new Vector3(1.18f,.13f,.41f),Architecture.Dark,"Control deck");
            b.Cylinder(new Vector3(-.29f,.96f,.5f),.04f,.18f,Architecture.Edge,"Joystick shaft",10);
            b.Cylinder(new Vector3(-.29f,1.07f,.5f),.065f,.07f,accent,"Joystick grip",12);
            for(int i=0;i<3;i++)b.Cylinder(new Vector3(.13f+i*.17f,.905f,.49f),.05f,.04f,accent,"Play button",12);
            b.Text(casino?"CASINO":"ARCADE",new Vector3(0,1.5f,.425f),.043f,Architecture.Dark,new Vector3(0,180,0));
            b.Box(new Vector3(-.49f,.58f,.36f),new Vector3(.045f,.78f,.025f),accent,"Cabinet side light",true);
            b.Box(new Vector3(.49f,.58f,.36f),new Vector3(.045f,.78f,.025f),accent,"Cabinet side light",true);
            Chair(b,anchor,facing,false);
        }
    }
}
