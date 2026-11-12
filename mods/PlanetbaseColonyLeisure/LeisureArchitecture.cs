using PlanetbaseVisuals;
using UnityEngine;

namespace PlanetbaseColonyLeisure
{
    internal static class LeisureArchitecture
    {
        internal static void Building(GameObject model,LeisureDome dome,int sizeIndex)
        {
            RoomStyle style=dome is ModuleTypeNightClub?RoomStyle.Club:dome is ModuleTypeCasino?RoomStyle.Casino:dome is ModuleTypeWellness?RoomStyle.Wellness:RoomStyle.Arcade;
            InteriorArchitecture.Build(model,sizeIndex,style);
        }

        internal static void Equipment(GameObject model,LeisureEquipment equipment)
        {
            if(equipment is DanceFloor){
                var b=new Architecture(model.transform,"Leisure equipment architecture");
                Architecture.HideOriginalMeshes(model);
                b.Box(new Vector3(0,.035f,0),new Vector3(1.75f,.07f,1.75f),Architecture.Dark,"Dance platform");
                for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)b.Box(new Vector3(x*.49f,.08f,z*.49f),new Vector3(.44f,.05f,.44f),(x+z)%2==0?new Color(.55f,.12f,.32f):Architecture.Cyan,"Dance light tile",true);
                for(int x=-1;x<=1;x+=2){
                    b.Box(new Vector3(x*.67f,.71f,.75f),new Vector3(.37f,1.35f,.32f),Architecture.Dark,"Club speaker tower");
                    b.Vents(new Vector3(x*.67f,.7f,.56f),.24f,1.05f,9);
                    b.Box(new Vector3(x*.67f,1.4f,.75f),new Vector3(.39f,.08f,.34f),Architecture.Shell,"Speaker cap");
                }
                if(Architecture.Tagged(model.transform,"AnchorPoint")==null){var anchor=new GameObject("Club guest position");anchor.tag="AnchorPoint";anchor.transform.SetParent(model.transform,false);}
                model.AddComponent<ClubAmbience>();
                b.Finish(true);return;
            }
            EquipmentArchitecture.Attach(model,equipment.GetType().Name);
        }
    }
}
