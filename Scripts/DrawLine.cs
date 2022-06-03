using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
	Transform linesParent;
	private float lineWidth = 0.05f;
	
	void Start()
	{
		linesParent = GameObject.Find("Lines").transform;
	}

	public void MakeLine(Vector3[] fromTo, Color color, string name = "Line")
	{
		GameObject lineObj = new GameObject(name);
		lineObj.transform.SetParent(linesParent);
		LineRenderer line = lineObj.AddComponent<LineRenderer>();
		line.material = new Material(Shader.Find("Sprites/Default"));
		line.startColor = color;
		line.endColor = color;
		line.widthMultiplier = lineWidth;
		line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = fromTo.Length;
		line.SetPositions(fromTo);
	}
	
	
}
