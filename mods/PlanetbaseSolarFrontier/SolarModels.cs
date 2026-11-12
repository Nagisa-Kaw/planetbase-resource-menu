using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Planetbase;
using UnityEngine;
namespace PlanetbaseSolarFrontier
{
 internal static class SolarModels
 {
  internal static readonly Color Steel=new Color(.6f,.7f,.75f),Dark=new Color(.1f,.15f,.2f);
  internal static Transform Part(Transform parent,PrimitiveType shape,Vector3 p,Vector3 scale,Color color)
  {
   GameObject g=GameObject.CreatePrimitive(shape);g.name="Frontier "+shape;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.layer=parent.gameObject.layer;
   Collider c=g.GetComponent<Collider>();c.enabled=false;Object.Destroy(c);Renderer r=g.GetComponent<Renderer>();r.sharedMaterial=ResourceList.getInstance().Materials.PlainColor;
   var tint=g.AddComponent<SolarTint>();tint.Value=color;return g.transform;
  }
  internal static void Build(GameObject model,SolarModule type) => SolarArchitecture.Build(model,type);
  internal static void Label(Transform parent,string text,Vector3 p,float size)
  {
   var g=new GameObject("Frontier label");g.transform.SetParent(parent,false);g.transform.localPosition=p;var m=g.AddComponent<TextMesh>();m.font=ResourceList.getInstance().FontMain;m.text=text;m.fontSize=48;m.characterSize=size;m.anchor=TextAnchor.MiddleCenter;m.alignment=TextAlignment.Center;g.GetComponent<Renderer>().sharedMaterial=m.font.material;
  }
  [HarmonyPatch(typeof(ComponentType),nameof(ComponentType.createModel))]
  private static class Equipment
  {
   static void Postfix(ComponentType __instance,GameObject __result)
   {
    if(!(__instance is SolarMachine||__instance is SanitaryUnit||__instance is NutrientHopper))return;
    SolarArchitecture.Equipment(__result,__instance);
   }
  }
  [HarmonyPatch]
  private static class Vehicles
  {
   static IEnumerable<MethodBase> TargetMethods(){yield return AccessTools.Method(typeof(Specialization),nameof(Specialization.createModel),new[]{typeof(ObjectList<Material>),typeof(Human.Gender),typeof(int),typeof(int),typeof(int)});yield return AccessTools.Method(typeof(Specialization),nameof(Specialization.createModelExterior));}
   static void Postfix(Specialization __instance,GameObject __result)
   {
    if(!(__instance is SolarBot))return;
    PlanetbaseVisuals.Architecture.HideOriginalMeshes(__result);
    var visual=PlanetbaseVisuals.VehicleArchitecture.Create(__result.transform,__instance.GetType().Name);
    visual.name="Frontier vehicle model";
   }
  }
  [HarmonyPatch(typeof(Character),nameof(Character.update))]
  private static class Cargo
  {
   static void Postfix(Character __instance)
   {
    if(!(__instance.getSpecialization() is CargoTruck)||__instance.isDestroyed())return;
    var view=__instance.getGameObject().GetComponent<SolarCargoView>();if(view==null)view=__instance.getGameObject().AddComponent<SolarCargoView>();view.Owner=__instance;
   }
  }
  [HarmonyPatch(typeof(ResourceType),nameof(ResourceType.createModel))]
  private static class ResourceModels
  {
   static void Postfix(ResourceType __instance,GameObject __result){if(__instance is SolarResource)PlanetbaseVisuals.ResourceArchitecture.Attach(__result,__instance.GetType().Name);}
  }
  [HarmonyPatch(typeof(ResourceType),nameof(ResourceType.getIcon))]
  private static class ResourceIcons
  {
   static void Postfix(ResourceType __instance,ref Texture2D __result){if(__instance is SolarResource)__result=PlanetbaseVisuals.ResourceArchitecture.Icon(__instance.GetType().Name);}
  }
 }
 public sealed class SolarTint:MonoBehaviour
 {
  public Color Value;private Renderer target;private MaterialPropertyBlock properties;
  private void LateUpdate(){if(target==null)target=GetComponent<Renderer>();if(target==null)return;if(properties==null)properties=new MaterialPropertyBlock();target.GetPropertyBlock(properties);properties.SetColor("_Color",Value);target.SetPropertyBlock(properties);}
 }
 public sealed class SolarCargoView:MonoBehaviour
 {
  internal Character Owner;
  private void LateUpdate(){if(Owner==null||Owner.isDestroyed())return;Resource cargo=Owner.getLoadedResource();if(cargo!=null){Transform socket=null;foreach(var t in transform.GetComponentsInChildren<Transform>())if(t.name=="Cargo socket"){socket=t;break;}cargo.setPosition(socket!=null?socket.position:transform.TransformPoint(new Vector3(0,.82f,-.73f)));cargo.setRotation(transform.rotation);}}
 }
}
