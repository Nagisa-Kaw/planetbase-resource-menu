using Planetbase;
using UnityEngine;
namespace PlanetbaseSolarFrontier
{
 public abstract class SolarWorld : Planet
 {
  public readonly float Radiation, SolarFactor, EarthDelay;
  protected SolarWorld(char letter,int template,string title,string description,Quantity atmosphere,Quantity light,float radiation,float solar,float delay)
  {
   mLetter=letter;Radiation=radiation;SolarFactor=solar;EarthDelay=delay;
   // Clone the definition so no native planet asset is edited.
   mDefinition=Object.Instantiate(ResourceUtil.loadPrefab("Prefabs/Planets/PrefabPlanetDefinition"+template).GetComponent<PlanetDefinition>());
   Object.DontDestroyOnLoad(mDefinition.gameObject);
   mDefinition.Locked=false;
   mDefinition.EnvironmentParameters=Object.Instantiate(mDefinition.EnvironmentParameters);
   Object.DontDestroyOnLoad(mDefinition.EnvironmentParameters);
   mAtmosphereDensity=atmosphere;mLightAmount=light;mMeteorRisk=Quantity.High;
   mMilestonesToUnlock=0;
   mTexures=new[]{"Terrain/Planet"+template+"/flat1","Terrain/Planet"+template+"/flat2","Terrain/Planet"+template+"/slope","Terrain/Planet"+template+"/foundations"};
   addStartingSpecialization<Worker>(3);addStartingSpecialization<Biologist>(2);addStartingSpecialization<Engineer>(2);addStartingSpecialization<Medic>(1);addStartingSpecialization<Carrier>(1);addStartingSpecialization<Constructor>(1);
   addStartingResource<Metal>(60);addStartingResource<Bioplastic>(45);addStartingResource<Meal>(35);addStartingResource<Spares>(15);addStartingResource<MedicalSupplies>(15);addStartingResource<Semiconductors>(8);
   mInitialMusicTrack=template==1?0:1;mIntruderMinPrestige=.15f;
   SolarStrings.World(letter,title,description);
   initStrings();
  }
 }
 public sealed class SolarMars:SolarWorld
 {
  public SolarMars():base('A',1,"Marte","Atmósfera tenue, tormentas de polvo y meteoros. Destino de iniciación de Frontera Solar.",Quantity.Low,Quantity.High,0,1,1){mSandstormRisk=Quantity.High;initStrings();}
 }
 public sealed class SolarMoon:SolarWorld
 {
  public SolarMoon():base('L',3,"Luna","Vacío, meteoros y radiación exterior. No funcionan las turbinas eólicas.",Quantity.None,Quantity.High,.35f,1,.5f){mSolarFlareRisk=Quantity.High;initStrings();}
 }
 public sealed class SolarEuropa:SolarWorld
 {
  public SolarEuropa():base('E',2,"Europa","Hielo, radiación intensa y poca luz. Requiere refugios y energía nuclear o geotérmica.",Quantity.None,Quantity.Low,1,.4f,1.8f){mSolarFlareRisk=Quantity.High;mDefinition.DayHours=10;mDefinition.NightHours=10;initStrings();}
 }
 public sealed class SolarTitan:SolarWorld
 {
  public SolarTitan():base('T',4,"Titán","Atmósfera densa, oscuridad y tormentas. La energía solar rinde muy poco.",Quantity.High,Quantity.Low,.1f,.2f,2.2f){mBlizzardRisk=Quantity.High;mMeteorRisk=Quantity.Low;initStrings();}
 }
}
