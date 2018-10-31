using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMap  {
    public Vector3 position;
    public List<GameObject> heatCircle = new List<GameObject>();
    public GameObject heatMap;
    
    public HeatMap(float x, float y, float z)
    {
        position = new Vector3(x, y, z);
        heatMap = new GameObject();
        heatMap.transform.localPosition = position;
        heatMap.name = "HeatMap";
    }

    public void addCircle(Vector3 pos)
    {
        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        circle.transform.parent = heatMap.transform;
        circle.transform.localPosition = pos;
        circle.transform.localRotation = Quaternion.Euler(90, 0, 0);
        circle.transform.localScale = new Vector3(0.1f,0.001f,0.1f);
        circle.name = "HeatCircle";
        circle.GetComponent<Collider>().enabled = false;
        Material material = circle.GetComponent<Renderer>().material;       
        material.SetFloat("_Mode",2);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
		material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
		material.SetInt("_ZWrite", 0);
		material.DisableKeyword("_ALPHATEST_ON");
		material.EnableKeyword("_ALPHABLEND_ON");
		material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		material.renderQueue = 3000;
        material.color = new Color(1.0f,0f,0f,0.3f);
        heatCircle.Add(circle);
    }

    public void setActive(bool active)
    {
        heatMap.SetActive(active);
    }
}
