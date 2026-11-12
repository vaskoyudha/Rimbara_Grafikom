using System.Text;
using UnityEngine;
var ui=GameObject.Find("UI - RIMBARA").transform;
Transform F(string n, Transform r){ if(r.name==n) return r; foreach(Transform c in r){var x=F(n,c); if(x!=null) return x;} return null; }
var sb=new StringBuilder();
void R(string n){
  var t=F(n,ui); if(t==null){sb.AppendLine(n+" MISSING");return;}
  var rt=(RectTransform)t; var w=new Vector3[4]; rt.GetWorldCorners(w);
  sb.AppendLine(n+"|"+Mathf.Round(w[0].x)+","+Mathf.Round(w[0].y)+","+Mathf.Round(w[2].x)+","+Mathf.Round(w[2].y)+"|active="+t.gameObject.activeInHierarchy);
}
foreach(var n in new[]{"Start restoration","Controls button","Menu quit","Main menu card","Restoration counters","Restoration bar background","Restoration bar fill","Seedling counter","Tree counter","Fire counter","Arunika field map","Map title","Zone A","Zone B","Zone C","A label","B label","C label","Interaction prompt","Next destination"}) R(n);
return sb.ToString();
