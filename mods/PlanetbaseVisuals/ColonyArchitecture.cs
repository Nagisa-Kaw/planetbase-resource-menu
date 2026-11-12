using System;
using System.Collections.Generic;
using Planetbase;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

// Shared source, compiled into each mod: no extra plugin or runtime dependency.
namespace PlanetbaseVisuals
{
    internal sealed class Architecture
    {
        internal static readonly Color Shell = new Color(.76f, .79f, .77f);
        internal static readonly Color Edge = new Color(.48f, .53f, .55f);
        internal static readonly Color Dark = new Color(.075f, .105f, .12f);
        internal static readonly Color Steel = new Color(.32f, .39f, .42f);
        internal static readonly Color Yellow = new Color(.95f, .63f, .08f);
        internal static readonly Color Cyan = new Color(.13f, .7f, .8f);
        internal static readonly Color Orange = new Color(.95f, .3f, .065f);
        internal static readonly Color Green = new Color(.25f, .63f, .38f);
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        private static Mesh box;
        private readonly Transform root;
        private readonly List<GameObject> pieces = new List<GameObject>();
        private readonly Dictionary<Material, List<CombineInstance>> batches = new Dictionary<Material, List<CombineInstance>>();
        internal Transform Root => root;

        internal Architecture(Transform parent, string name = "Architecture v2")
        {
            var go = new GameObject(name);
            go.layer = 11; // Native structure layer: participates in the sun's light/shadow mask.
            go.transform.SetParent(parent, false);
            root = go.transform;
        }

        internal static void HideOriginalMeshes(GameObject model)
        { HideOriginalMeshes(model, null); }

        internal static void HideOriginalMeshes(GameObject model, Predicate<Renderer> filter)
        {
            // Keep all tags, colliders, native placement points and animation/interaction anchors.
            // The game reads the original mesh bounds for repair distance. Retain that mesh.
            // A cloned component reapplies the hidden state after native construction toggles.
            var hidden = model.AddComponent<ColonyNativeMeshHider>();
            var surfaces=new List<Renderer>();
            foreach(var r in model.GetComponentsInChildren<Renderer>(true))if(filter==null||filter(r))surfaces.Add(r);
            hidden.Surfaces = surfaces.ToArray();
            foreach (var renderer in hidden.Surfaces) { renderer.enabled=false; renderer.forceRenderingOff=true; }
        }

        internal static Transform Tagged(Transform parent, string tag)
        {
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true)) if (t.CompareTag(tag)) return t;
            return null;
        }

        private static Material Paint(Color color, bool light)
        {
            string key = ColorUtility.ToHtmlStringRGBA(color) + (light ? "L" : "S");
            if (Materials.TryGetValue(key, out var existing)) return existing;
            bool glass=color.a<.99f;
            Shader shader = (glass?Shader.Find("Standard"):Shader.Find("Planetbase/OpaqueEmission")) ?? Shader.Find("Standard") ?? Shader.Find("Legacy Shaders/Diffuse");
            var material = new Material(shader) { name = "Colony enamel " + key, color = color };
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex",Texture2D.whiteTexture);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", light ? .05f : .22f);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", .28f);
            if (light) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * .75f); }
            if(glass){material.SetFloat("_Mode",3);material.SetInt("_SrcBlend",1);material.SetInt("_DstBlend",10);material.SetInt("_ZWrite",0);material.EnableKeyword("_ALPHAPREMULTIPLY_ON");material.renderQueue=3000;}
            Materials.Add(key, material);
            return material;
        }

        internal Transform MeshPart(string name, Mesh mesh, Vector3 position, Vector3 scale, Color color, bool light = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false); go.layer = root.gameObject.layer;
            go.transform.localPosition = position; go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = Paint(color, light);
            r.shadowCastingMode = light ? ShadowCastingMode.Off : ShadowCastingMode.On;
            r.receiveShadows = true;
            pieces.Add(go);
            return go.transform;
        }

        internal Transform Box(Vector3 position, Vector3 size, Color color, string name = "Beveled housing", bool light = false)
        {
            if (box == null) box = BoxMesh();
            return MeshPart(name, box, position, size, color, light);
        }

        internal Transform Cylinder(Vector3 position, float radius, float height, Color color, string name = "Capped cylinder", int sides = 24)
        {
            return Lathe(position, new[] { new Vector2(0, -height/2), new Vector2(radius*.94f, -height/2), new Vector2(radius, -height/2+.06f), new Vector2(radius, height/2-.06f), new Vector2(radius*.94f, height/2), new Vector2(0, height/2) }, color, name, sides);
        }

        internal Transform Lathe(Vector3 position, Vector2[] profile, Color color, string name, int segments = 32)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int h = 0; h < profile.Length; h++)
                for (int s = 0; s <= segments; s++) {
                    float a = s * Mathf.PI * 2 / segments;
                    vertices.Add(new Vector3(Mathf.Cos(a)*profile[h].x, profile[h].y, Mathf.Sin(a)*profile[h].x));
                }
            for (int h = 0; h < profile.Length-1; h++) for (int s = 0; s < segments; s++) {
                int a = h*(segments+1)+s, b=a+1,c=a+segments+1,d=c+1;
                triangles.Add(a);triangles.Add(c);triangles.Add(b);triangles.Add(b);triangles.Add(c);triangles.Add(d);
            }
            return MeshPart(name, MakeMesh(name,vertices,triangles), position, Vector3.one, color);
        }

        internal Transform Ring(Vector3 position, float radius, float tube, Color color, string name = "Service ring", bool light = false, int segments = 40)
        {
            const int sides=8;
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int s=0;s<=segments;s++) for(int t=0;t<=sides;t++) {
                float a=s*Mathf.PI*2/segments,b=t*Mathf.PI*2/sides;
                vertices.Add(new Vector3(Mathf.Cos(a)*(radius+Mathf.Cos(b)*tube),Mathf.Sin(b)*tube,Mathf.Sin(a)*(radius+Mathf.Cos(b)*tube)));
            }
            for(int s=0;s<segments;s++)for(int t=0;t<sides;t++) {
                int a=s*(sides+1)+t,b=a+1,c=a+sides+1,d=c+1;
                triangles.Add(a);triangles.Add(b);triangles.Add(c);triangles.Add(b);triangles.Add(d);triangles.Add(c);
            }
            return MeshPart(name,MakeMesh(name,vertices,triangles),position,Vector3.one,color,light);
        }

        internal void Beam(Vector3 a, Vector3 b, float width, Color color, float depth = -1)
        {
            Transform t=Box((a+b)*.5f,new Vector3(width,(b-a).magnitude,depth<0?width:depth),color,"Structural rib");
            t.localRotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
        }

        internal void Pipe(float radius, Color color, params Vector3[] points)
        {
            // Curved route supplied as a short polyline; smooth cylinders and joint collars.
            for(int i=0;i<points.Length-1;i++){
                Vector3 a=points[i],b=points[i+1];
                var t=Cylinder((a+b)*.5f,radius,(b-a).magnitude+.035f,color,"Coolant pipe",12);
                t.localRotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
                if(i==0||i==points.Length-2){
                    var collar=Cylinder(a,radius*1.3f,.09f,Edge,"Pipe coupling",12);
                    collar.localRotation=t.localRotation;
                }
            }
        }

        internal void Vents(Vector3 position, float width, float height, int count, float yaw = 0)
        {
            var panel=Box(position,new Vector3(width+.12f,height+.12f,.12f),Dark,"Vent housing");panel.localRotation=Quaternion.Euler(0,yaw,0);
            Quaternion q=Quaternion.Euler(0,yaw,0);
            for(int i=0;i<count;i++){
                Vector3 p=position+q*new Vector3(0,(-.5f+(i+.5f)/count)*height,-.085f);
                var slat=Box(p,new Vector3(width,.035f,.06f),Edge,"Vent louver");slat.localRotation=q;
            }
        }

        internal void Hatch(Vector3 p, float width, float height, Color accent, float yaw = 0)
        {
            Quaternion q=Quaternion.Euler(0,yaw,0);
            var frame=Box(p,new Vector3(width+.18f,height+.18f,.16f),Dark,"Recessed maintenance hatch");frame.localRotation=q;
            var door=Box(p+q*new Vector3(0,0,-.09f),new Vector3(width,height,.08f),Edge);door.localRotation=q;
            var light=Box(p+q*new Vector3(0,height*.37f,-.145f),new Vector3(width*.55f,.065f,.035f),accent,"Hatch indicator",true);light.localRotation=q;
            var handle=Box(p+q*new Vector3(width*.3f,-height*.12f,-.16f),new Vector3(.06f,.23f,.06f),Shell);handle.localRotation=q;
        }

        internal void Beacon(Vector3 p, Color color)
        {
            Cylinder(p,.18f,.1f,Dark,"Beacon mount",12);
            Box(p+Vector3.up*.17f,new Vector3(.15f,.27f,.15f),color,"Status beacon",true);
            Cylinder(p+Vector3.up*.32f,.12f,.06f,Edge,"Beacon cap",12);
        }

        internal void Foundation(float radius = 3.5f)
        {
            Cylinder(new Vector3(0,.14f,0),radius,.28f,Edge,"Octagonal foundation",16);
            Cylinder(new Vector3(0,.3f,0),radius-.12f,.12f,Dark,"Foundation deck",16);
            for(int i=0;i<8;i++){
                float a=(i+.5f)*Mathf.PI/4;Vector3 p=new Vector3(Mathf.Sin(a)*(radius-.08f),.18f,Mathf.Cos(a)*(radius-.08f));
                var l=Box(p,new Vector3(.4f,.07f,.05f),new Color(.7f,.78f,.72f),"Deck marker",true);l.localRotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);
            }
        }

        internal void Hazard(Vector3 p, int count, float yaw = 0)
        {
            Quaternion q=Quaternion.Euler(0,yaw,0);
            for(int i=0;i<count;i++){
                var t=Box(p+q*new Vector3((i-(count-1)*.5f)*.18f,0,0),new Vector3(.14f,.22f,.025f),i%2==0?Yellow:Dark,"Safety stripe");
                t.localRotation=Quaternion.Euler(0,yaw,24);
            }
        }

        internal void Finish(bool pulse = false)
        {
            foreach(var go in pieces){
                var material=go.GetComponent<Renderer>().sharedMaterial;
                if(!batches.TryGetValue(material,out var list)){list=new List<CombineInstance>();batches.Add(material,list);}
                list.Add(new CombineInstance{mesh=go.GetComponent<MeshFilter>().sharedMesh,transform=root.worldToLocalMatrix*go.transform.localToWorldMatrix});
            }
            var renderers=new List<Renderer>();var colors=new List<Color>();var emissions=new List<Color>();var merged=new List<Mesh>();
            foreach(var pair in batches){
                var go=new GameObject("Batched "+pair.Key.name);go.layer=root.gameObject.layer;go.transform.SetParent(root,false);
                var mesh=new Mesh{name="Colony architectural batch"};mesh.CombineMeshes(pair.Value.ToArray(),true,true);mesh.RecalculateBounds();
                merged.Add(mesh);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=pair.Key;
                Color emission=pair.Key.IsKeywordEnabled("_EMISSION")?pair.Key.GetColor("_EmissionColor"):Color.black;
                r.shadowCastingMode=emission.maxColorComponent>0?ShadowCastingMode.Off:ShadowCastingMode.On;
                renderers.Add(r);colors.Add(pair.Key.color);emissions.Add(emission);
            }
            // Native createModel can clone the prefab in this same frame. Detach temporary
            // pieces immediately so the clone contains only the finished material batches.
            var temporary=new HashSet<Mesh>();
            foreach(var go in pieces){var mesh=go.GetComponent<MeshFilter>().sharedMesh;if(mesh!=null&&mesh!=box)temporary.Add(mesh);go.SetActive(false);go.transform.SetParent(null,false);Object.Destroy(go);}
            foreach(var mesh in temporary)Object.Destroy(mesh);
            var lifetime=root.gameObject.AddComponent<ColonyOwnedMeshes>();lifetime.OwnerId=lifetime.GetInstanceID();lifetime.Meshes=merged.ToArray();
            var palette=root.gameObject.AddComponent<ColonySurfacePalette>();palette.Surfaces=renderers.ToArray();palette.Colors=colors.ToArray();palette.Emissions=emissions.ToArray();palette.Pulse=pulse;
        }

        internal Transform Wedge(Vector3 position,float width,Vector2[] yz,Color color,string name)
        {
            var v=new List<Vector3>();var t=new List<int>();int count=yz.Length;
            for(int side=0;side<2;side++){
                for(int i=1;i<count-1;i++){
                    int n=v.Count;v.Add(new Vector3(side==0?-width/2:width/2,yz[0].y,yz[0].x));v.Add(new Vector3(side==0?-width/2:width/2,yz[i].y,yz[i].x));v.Add(new Vector3(side==0?-width/2:width/2,yz[i+1].y,yz[i+1].x));
                    t.Add(n);t.Add(side==0?n+2:n+1);t.Add(side==0?n+1:n+2);
                }
            }
            for(int i=0;i<count;i++){
                int j=(i+1)%count,n=v.Count;v.Add(new Vector3(-width/2,yz[i].y,yz[i].x));v.Add(new Vector3(width/2,yz[i].y,yz[i].x));v.Add(new Vector3(width/2,yz[j].y,yz[j].x));v.Add(new Vector3(-width/2,yz[j].y,yz[j].x));
                t.Add(n);t.Add(n+2);t.Add(n+1);t.Add(n);t.Add(n+3);t.Add(n+2);
            }
            return MeshPart(name,MakeMesh(name,v,t),position,Vector3.one,color);
        }

        internal void Screen(Vector3 p,Vector2 size,Color accent,float yaw=0)
        {
            Quaternion q=Quaternion.Euler(0,yaw,0);
            Box(p,new Vector3(size.x+.12f,size.y+.12f,.1f),Dark,"Display surround").localRotation=q;
            Box(p+q*Vector3.forward*.06f,new Vector3(size.x,size.y,.025f),new Color(.025f,.09f,.105f),"Display glass").localRotation=q;
            for(int i=0;i<3;i++)Box(p+q*new Vector3(-size.x*.1f,(i-1)*size.y*.23f,.082f),new Vector3(size.x*(.65f-i*.12f),size.y*.05f,.012f),accent,"Display data",true).localRotation=q;
        }

        internal void Text(string text,Vector3 p,float size,Color color,Vector3 euler)
        {
            var go=new GameObject("Identifier "+text);go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localRotation=Quaternion.Euler(euler);go.layer=root.gameObject.layer;
            var mesh=go.AddComponent<TextMesh>();mesh.font=ResourceList.getInstance().FontMain;mesh.fontSize=48;mesh.characterSize=size;mesh.anchor=TextAnchor.MiddleCenter;mesh.alignment=TextAlignment.Center;mesh.text=text;mesh.color=color;
            go.GetComponent<Renderer>().sharedMaterial=mesh.font.material;
        }

        private static Mesh MakeMesh(string name,List<Vector3> vertices,List<int> triangles)
        {
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }

        private static Mesh BoxMesh()
        {
            // Four perimeter rings produce real bevel faces, including the top and bottom edges.
            var verts=new List<Vector3>();var tris=new List<int>();
            Vector2[] corners={new Vector2(-.42f,-.5f),new Vector2(.42f,-.5f),new Vector2(.5f,-.42f),new Vector2(.5f,.42f),new Vector2(.42f,.5f),new Vector2(-.42f,.5f),new Vector2(-.5f,.42f),new Vector2(-.5f,-.42f)};
            var rings=new Vector3[4,8];
            for(int h=0;h<4;h++)for(int i=0;i<8;i++){
                float inset=h==0||h==3?.94f:1;float y=h==0?-.5f:h==1?-.42f:h==2?.42f:.5f;
                rings[h,i]=new Vector3(corners[i].x*inset,y,corners[i].y*inset);
            }
            for(int h=0;h<3;h++)for(int i=0;i<8;i++){
                int j=(i+1)%8,n=verts.Count;verts.Add(rings[h,i]);verts.Add(rings[h+1,i]);verts.Add(rings[h+1,j]);verts.Add(rings[h,j]);
                tris.Add(n);tris.Add(n+1);tris.Add(n+2);tris.Add(n);tris.Add(n+2);tris.Add(n+3);
            }
            for(int face=0;face<2;face++)for(int i=0;i<8;i++){
                int n=verts.Count,h=face==0?0:3,j=(i+1)%8;
                verts.Add(new Vector3(0,face==0?-.5f:.5f,0));verts.Add(rings[h,face==0?i:j]);verts.Add(rings[h,face==0?j:i]);tris.Add(n);tris.Add(n+1);tris.Add(n+2);
            }
            return MakeMesh("Original beveled colony panel",verts,tris);
        }
    }

    // Cloned visuals share their immutable template meshes. Only the component that
    // created a mesh disposes it; destroying one cargo item must not break its peers.
    internal sealed class ColonyOwnedMeshes : MonoBehaviour
    {
        public int OwnerId;public Mesh[] Meshes;
        private void OnDestroy(){if(GetInstanceID()!=OwnerId||Meshes==null)return;foreach(var mesh in Meshes)if(mesh!=null)Object.Destroy(mesh);}
    }

    internal sealed class ColonySurfacePalette : MonoBehaviour
    {
        public Renderer[] Surfaces; public Color[] Colors,Emissions;public bool Pulse;
        private MaterialPropertyBlock block;private float clock;
        private void LateUpdate(){
            if(Surfaces==null)return;if(block==null)block=new MaterialPropertyBlock();
            if(Pulse&&!Singleton<TimeManager>.getInstance().isPaused())clock+=Time.deltaTime;
            for(int i=0;i<Surfaces.Length;i++)if(Surfaces[i]!=null){
                Surfaces[i].GetPropertyBlock(block);block.SetColor("_Color",Colors[i]);
                block.SetColor("_EmissionColor",Emissions[i]*(Pulse?.8f+.2f*Mathf.Sin(clock*2+i):1));Surfaces[i].SetPropertyBlock(block);
            }
        }
    }

    internal sealed class ColonyRoofVisibility : MonoBehaviour
    {
        public GameObject NativeRoof;public Renderer[] Surfaces;private Planetbase.Module owner;
        private void LateUpdate(){
            if(Surfaces==null)return;
            if(owner==null&&transform.parent!=null&&transform.parent.parent!=null)owner=Construction.find(transform.parent.parent.gameObject) as Planetbase.Module;
            bool visible=NativeRoof!=null?NativeRoof.activeInHierarchy:owner==null||owner.isTopOn();
            foreach(var r in Surfaces)if(r!=null)r.enabled=visible;
        }
    }

    internal sealed class ColonyNativeMeshHider : MonoBehaviour
    {
        public Renderer[] Surfaces;
        private void LateUpdate(){if(Surfaces==null)return;foreach(var r in Surfaces)if(r!=null){r.enabled=false;r.forceRenderingOff=true;}}
    }
}
