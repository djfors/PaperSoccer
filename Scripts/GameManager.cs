using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{	
    private float cellSize;
    
	LineRenderer fieldMarking;
	[SerializeField] private Toggle fieldMarkingToggle;
	
	Transform pointsParent;
	public GameObject pointPrefab;
	
	public bool withSoccerFieldMarking;
	public bool isHelperOn;
	float startX, startY;
	
	public Color myColor = Color.blue;
	public Color opponentColor = Color.red;
	public Color fieldColor = Color.black;
	
	public MatrixPoints[,] fieldMatrix = new MatrixPoints[15, 9];
	
	void Start()
	{
		withSoccerFieldMarking = PlayerPrefs.GetInt("FieldMarking", 1) == 1 ? true : false;
		fieldMarkingToggle.isOn = PlayerPrefs.GetInt("FieldMarking", 1) == 1 ? true : false;		
		
		pointsParent = GameObject.Find("Points").transform;
		
		Transform topLeftPoint = GameObject.Find("TopLeftPoint").transform;
		Transform bottomRightPoint = GameObject.Find("BottomRightPoint").transform;
		startX = topLeftPoint.position.x;
		startY = topLeftPoint.position.y;
		cellSize = (topLeftPoint.position.x - bottomRightPoint.position.x)/8;
		
		InitPointMatrix();
		DrawFieldMarking();		
	}

	void InitPointMatrix()	// заготовка всех точек по всему полю (расстановка на поле и массив для рассчетов)
	{
		float pointX = startX;
		GameObject objToSpawn;
		
		// ставим все пустые точки на поле
		for(int x = 0; x < 15; x++)
		{
			for(int y = 0; y < 9; y++)
			{
				objToSpawn = Instantiate(pointPrefab);
				objToSpawn.transform.name = System.String.Format("{0}-{1}", x, y);
				objToSpawn.transform.SetParent(pointsParent);
				
				objToSpawn.transform.position = new Vector2(pointX, startY);
				fieldMatrix[x,y] = new MatrixPoints(x, y, new Vector2(pointX, startY) );
				pointX -= cellSize;
			}
			startY += cellSize;
			pointX = startX;
		}
		
		//0-свободно, 1-мной зарисовано, 2-противником зарисовано, 3-разметка
		
		// тут заполняем точки разметки (углы, разметку, ворота и тд) по которым будут отражения
		// вертикальные линии
		for(int x = 1; x < 14; x++)
		{
			fieldMatrix[x,0].directions = new int[8] { 3,0,0,0,3,3,3,3 };
			fieldMatrix[x,8].directions = new int[8] { 3,3,3,3,3,0,0,0 };
		}
		
		// помечаем штанги, ворота и заднюю линию
		for(int y = 0; y < 9; y++)
		{
			fieldMatrix[0,y].directions = new int[8] { 3,3,3,3,3,3,3,3 };
			fieldMatrix[14,y].directions = new int[8] { 3,3,3,3,3,3,3,3 };
			
			if (withSoccerFieldMarking)		// если играем с доп разметкой поля, то метим и их
			{
				fieldMatrix[4,y].directions[2] = 3;
				fieldMatrix[4,y].directions[6] = 3;
				fieldMatrix[7,y].directions[2] = 3;
				fieldMatrix[7,y].directions[6] = 3;
				fieldMatrix[10,y].directions[2] = 3;
				fieldMatrix[10,y].directions[6] = 3;
			}
			
			if (y < 3 || y > 5)
			{
				fieldMatrix[1,y].directions = new int[8] { 3,3,3,3,3,3,3,3 };
				fieldMatrix[14,y].directions = new int[8] { 3,3,3,3,3,3,3,3 };
				fieldMatrix[1,y].directions = new int[8] { 3,3,3,0,0,0,3,3 };
				fieldMatrix[13,y].directions = new int[8] { 0,0,3,3,3,3,3,0 };
			}
		}
		
		fieldMatrix[0,3].directions[3] = 0; fieldMatrix[0,5].directions[5] = 0;
		fieldMatrix[0,4].directions[3] = 0; fieldMatrix[0,4].directions[4] = 0; fieldMatrix[0,4].directions[5] = 0;
		fieldMatrix[1,5].directions[0] = 3; fieldMatrix[1,5].directions[2] = 3;
		fieldMatrix[1,3].directions[0] = 3; fieldMatrix[1,3].directions[6] = 3;
		fieldMatrix[14,3].directions[1] = 0; fieldMatrix[14,5].directions[7] = 0;
		fieldMatrix[14,4].directions[0] = 0; fieldMatrix[14,4].directions[1] = 0; fieldMatrix[14,4].directions[7] = 0;
		fieldMatrix[13,5].directions[2] = 3; fieldMatrix[13,5].directions[4] = 3;
		fieldMatrix[13,3].directions[6] = 3; fieldMatrix[13,3].directions[4] = 3;
		
		// углы поля
		fieldMatrix[1,0].directions = new int[8] { 3,3,3,3,3,3,3,3 };
		fieldMatrix[1,8].directions = new int[8] { 3,3,3,3,3,3,3,3 };
		fieldMatrix[13,0].directions = new int[8] { 3,3,3,3,3,3,3,3 };
		fieldMatrix[13,8].directions = new int[8] { 3,3,3,3,3,3,3,3 };
		
		//fieldMatrix[6,4].directions = new int[8] { 3,3,3,3,3,3,3,3 }; // test
	}
	
	
	void DrawFieldMarking()
	{
		var drawLine = gameObject.GetComponent<DrawLine>();
		
		if (withSoccerFieldMarking)	// горизонтальная разметка поля. об них тоже можно отражаться.
		{
			drawLine.MakeLine(new Vector3[]{fieldMatrix[4,0].pointCoordinates, fieldMatrix[4,8].pointCoordinates}, fieldColor, "HorizontalMarking");
			drawLine.MakeLine(new Vector3[]{fieldMatrix[7,0].pointCoordinates, fieldMatrix[7,8].pointCoordinates}, fieldColor, "HorizontalMarking");
			drawLine.MakeLine(new Vector3[]{fieldMatrix[10,0].pointCoordinates, fieldMatrix[10,8].pointCoordinates}, fieldColor, "HorizontalMarking");
		}
	}
	
	public void Restart()
	{
		GameObject.Find("Canvas/Field/Pannel").SetActive(false);
		SceneManager.LoadScene(0);
	}
	
	public void SoccerFieldMarking()
	{
		PlayerPrefs.SetInt("FieldMarking", (fieldMarkingToggle.isOn ? 1 : 0));
		withSoccerFieldMarking = fieldMarkingToggle.isOn;
	}
	
	public void QuitGame()
	{
		PlayerPrefs.DeleteAll();
		Application.Quit();
	}
	
	public void ShowMenu()
	{
		GameObject.Find("Canvas/Field/Pannel").SetActive(true);
	}
	
	public void Back()
	{
		GameObject.Find("Canvas/Field/Pannel").SetActive(false);
	}
}