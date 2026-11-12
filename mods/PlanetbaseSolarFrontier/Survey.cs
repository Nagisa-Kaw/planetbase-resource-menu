using System.Linq;
using HarmonyLib;
using Planetbase;
using UnityEngine;
namespace PlanetbaseSolarFrontier
{
 public static class Survey
 {
  public static float Heat(Vector3 p)=>Mathf.PerlinNoise(p.x*.012f+PlanetIndex()*5.3f,p.z*.012f+7.1f);
  public static float Richness(Vector3 p)=>Mathf.PerlinNoise(p.x*.018f+13,p.z*.018f+PlanetIndex()*3.7f);
  private static int PlanetIndex()=>Singleton<PlanetManager>.getInstance().getCurrentPlanetIndex();
  public static bool IsScanned(Vector3 p)=>WorldObjects.Game!=null&&SolarState.Current.Surveys.Any(s=>new Vector2(s.x-p.x,s.z-p.z).sqrMagnitude<=45*45);
  internal static void Complete(Vector3 p)
  {
   if(!SolarState.Current.Surveys.Any(s=>(s-p).sqrMagnitude<20*20))SolarState.Current.Surveys.Add(new Vector3(p.x,0,p.z));
   WorldObjects.Toast("Escaneo terminado: calor "+Heat(p).ToString("P0")+", regolito "+Richness(p).ToString("P0"));SolarApi.Scanned(p);Refresh();
  }
  internal static bool Protected(Vector3 p)=>WorldObjects.Modules.Any(m=>m.getModuleType() is ModuleTypeRadiationPylon&&m.isOperational()&&(m.getPosition()-p).sqrMagnitude<=35*35);
  internal static int Layer;
  private static GameObject overlay;
  internal static void Toggle(){Layer=(Layer+1)%4;Refresh();WorldObjects.Toast(new[]{"Capas desactivadas","Calor: rojo = mayor potencial; gris = sin explorar","Regolito: verde = mayor concentración; gris = sin explorar","Protección: azul = cobertura de pilones"}[Layer]);}
  internal static void Refresh()
  {
   if(overlay!=null)Object.Destroy(overlay);if(Layer==0||WorldObjects.Game==null)return;
   overlay=new GameObject("Frontera Solar: capa "+Layer);
   Vector3 center=Selection.getSelected()?.getPosition()??WorldObjects.Modules.FirstOrDefault()?.getPosition()??Vector3.zero;
   for(int x=-5;x<=5;x++)for(int z=-5;z<=5;z++)
   {
    Vector3 p=center+new Vector3(x*12,0,z*12);if(!PhysicsUtil.findFloor(p,out Vector3 ground))continue;
    float value=Layer==1?Heat(p):Layer==2?Richness(p):Protected(p)?1:0;
    Color c=Layer==1?Color.Lerp(Color.blue,Color.red,value):Layer==2?Color.Lerp(new Color(.35f,.25f,.1f),Color.green,value):Color.Lerp(Color.gray,Color.cyan,value);
    if(Layer!=3&&!IsScanned(p))c=new Color(.3f,.3f,.3f);
    SolarModels.Part(overlay.transform,PrimitiveType.Cylinder,ground+Vector3.up*.12f,new Vector3(2.2f,.04f,2.2f),c);
   }
  }
  [HarmonyPatch(typeof(GameStateGame),nameof(GameStateGame.destroy))]
  private static class Clear {static void Prefix(){Layer=0;if(overlay!=null)Object.Destroy(overlay);}}
 }
}
