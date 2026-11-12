using System;
using HarmonyLib;
using Planetbase;
using UnityEngine;
namespace PlanetbaseSolarFrontier
{
 internal sealed class FrontierWindow:GuiWindow
 {
  private readonly Character selected;
  private readonly GuiLabelItem summary=new GuiLabelItem("");
  private readonly GuiLabelItem status=new GuiLabelItem("Selecciona una opción.");
  private readonly GameGui gui;
  internal FrontierWindow(GameGui gui,int page=0):base(new GuiLabelItem("FRONTERA SOLAR · F9"))
  {
   this.gui=gui;selected=Selection.getSelected() as Character;
   var tabs=new GuiRowItem(3);tabs.setHeight(GuiStyles.getIconSizeMedium());tabs.addChild(Action("Investigación",()=>gui.setWindow(new FrontierWindow(gui,0))));tabs.addChild(Action("Unidades y capas",()=>gui.setWindow(new FrontierWindow(gui,1))));tabs.addChild(Action("Mundo y amenazas",()=>gui.setWindow(new FrontierWindow(gui,2))));mRootItem.addChild(tabs);mRootItem.addChild(summary);
   if(page==0)
   {
    mRootItem.addChild(new GuiLabelItem("Escaneo > Geotermia / Reciclaje > Robótica > Fusión"));
    for(int i=0;i<Research.Projects.Length;i++)
    {
     int index=i;var p=Research.Projects[i];string text=(Research.Has(p.Tech)?"✓ ":"")+p.Name+" · "+p.Data+" informes / "+p.Credits+" créditos";
     GuiLabelItem row=Action(text,()=>status.setText(Research.Start(index)));row.setEnabled(!Research.Has(p.Tech));mRootItem.addChild(row);
    }
   }
   else if(page==1)
   {
    mRootItem.addChild(new GuiLabelItem("Unidad: "+(selected?.getName()??"selecciona un colono, bot o camión antes de F9")));
    mRootItem.addChild(Action("Mover al punto que indique",()=>Pick("move")));
    mRootItem.addChild(Action("Escanear terreno en el punto",()=>Pick("scan")));
    mRootItem.addChild(Action("Mantener posición",()=>{if(selected!=null)ManualControl.Hold(selected);}));
    mRootItem.addChild(Action("Volver a trabajo automático",()=>{ManualControl.Release(selected);status.setText("Control automático restaurado.");}));
    mRootItem.addChild(Action("Cambiar capa: calor / regolito / protección (F6)",()=>Survey.Toggle()));
    mRootItem.addChild(new GuiLabelItem("La orden se cancela si la salud, el oxígeno o la integridad requieren atención."));
   }
   else
   {
    mRootItem.addChild(new GuiLabelItem(PlanetManager.getCurrentPlanet().getName()));
    foreach(string line in PlanetManager.getCurrentPlanet().getCharacteristicsString().Split('\n'))if(line.Length>0)mRootItem.addChild(new GuiLabelItem(line));
    mRootItem.addChild(new GuiLabelItem("Cada mundo usa una colonia y un guardado independientes."));
    mRootItem.addChild(new GuiLabelItem("Los meteoros, tormentas y erupciones solares usan los sistemas de Planetbase."));
    mRootItem.addChild(Action("Incursiones opcionales: activar / desactivar",()=>{SolarState.Current.Raids=!SolarState.Current.Raids;status.setText("Incursiones: "+(SolarState.Current.Raids?"activadas; un intruso cada 1800 s":"desactivadas"));}));
    mRootItem.addChild(new GuiLabelItem("Las galerías son refugios en superficie. Los pilones cubren 35 m cuando tienen energía."));
   }
   mRootItem.addChild(new GuiLabelItem("Pulsa el icono junto a una opción para activarla."));mRootItem.addChild(status);mRootItem.addChild(new GuiButtonItem("Cerrar",_=>gui.setWindow(null)));
  }
  public override float getWidth()=>Mathf.Min(Screen.width*.9f,Screen.height*1.1f);
  public override Exclusivity getExclusivity()=>Exclusivity.Exclusive;
  private static GuiLabelItem Action(string text,System.Action callback)
  {
   var row=new GuiLabelItem(text,ResourceList.getInstance().Icons.Tech,text);row.setHeight(GuiStyles.getIconSizeMedium());row.addCallback(_=>callback(),null);return row;
  }
  public override void update()
  {
   SolarState s=SolarState.Current;summary.setText("Informes disponibles: "+Research.DataStock().Count+" · "+(s.Project<0?"Sin proyecto en curso":Research.Projects[s.Project].Name+" · "+s.Progress.ToString("0")+" s"));
  }
  private void Pick(string order)
  {
   if(selected==null){status.setText("Selecciona una unidad antes de abrir F9.");return;}
   if(order=="scan"&&!Research.Has(typeof(TechSolarSurvey))){status.setText("Investiga Escaneo de recursos.");return;}
   ManualControl.Pending=selected;ManualControl.PendingOrder=order;gui.setWindow(null);WorldObjects.Toast("Haz clic en terreno exterior. Esc cancela.");
  }
  [HarmonyPatch(typeof(GuiMenuSystem),nameof(GuiMenuSystem.init))]
  private static class ManagementMenu
  {
   static void Postfix(GameStateGame __0,GuiMenu ___mMenuBaseManagement)
   {
    var gui=(GameGui)AccessTools.Field(typeof(GameStateGame),"mGameGui").GetValue(__0);
    ___mMenuBaseManagement.addItem(new GuiMenuItem(ResourceList.getInstance().Icons.Tech,"Frontera Solar (F9)",_=>gui.setWindow(new FrontierWindow(gui)),null,0));
   }
  }
  [HarmonyPatch(typeof(GameStateGame),nameof(GameStateGame.update))]
  private static class Input
  {
   static bool Prefix(GameStateGame __instance,GameGui ___mGameGui)
   {
    if(UnityEngine.Input.GetKeyDown(KeyCode.F9))
    {
     if(___mGameGui.getWindow() is FrontierWindow)___mGameGui.setWindow(null);
     else if(!___mGameGui.isWindowVisible())___mGameGui.setWindow(new FrontierWindow(___mGameGui));
     return false;
    }
    if(UnityEngine.Input.GetKeyDown(KeyCode.F6)&&!___mGameGui.isWindowVisible()){Survey.Toggle();return false;}
    if(ManualControl.Pending==null)return true;
    if(ManualControl.Pending.isDestroyed()||UnityEngine.Input.GetKeyDown(KeyCode.Escape)){ManualControl.Pending=null;return false;}
    if(UnityEngine.Input.GetMouseButtonUp(0)&&Camera.main!=null&&Physics.Raycast(Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition),out RaycastHit hit,300f,1<<8))
    {
     bool ok=ManualControl.Issue(ManualControl.Pending,hit.point,ManualControl.PendingOrder=="scan");ManualControl.Pending=null;WorldObjects.Toast(ok?"Orden asignada.":"No se puede asignar esa ruta; comprueba la esclusa y el estado de la unidad.");
    }
    return false;
   }
  }
 }
}
