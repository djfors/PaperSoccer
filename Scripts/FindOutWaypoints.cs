using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FindOutWaypoints : MonoBehaviour
{
	DrawLine drawLine;
	GameManager GM;
	[SerializeField] private Text score;
	[SerializeField] private Text resultText;
	[SerializeField] private Toggle helperToggle;
	
	List<PotentialPoins> whereICanGo = new List<PotentialPoins>();
	
	bool newCalculation = true;
	bool initialKick = true;
	bool myTurn = true;
	bool endOfGame = false;
	private MatrixPoints currentPoint;
	private MatrixPoints[,] cloneField;
	private int whosLine, myScore = 0, hisScore = 0, bestLine = -1;
	List<MatrixPoints> waypointsListStart;
	
	void Awake()
	{
		drawLine = GameObject.Find("GameManager").GetComponent<DrawLine>();
		GM = GameObject.Find("GameManager").GetComponent<GameManager>();
		helperToggle.isOn = PlayerPrefs.GetInt("Helper", 0) == 1 ? true : false;
		GM.isHelperOn = helperToggle.isOn;
		currentPoint = GM.fieldMatrix[7,4];
	}
	
    void Update()
	{	
		if (!endOfGame)
		{
			if (newCalculation)
			{		
				newCalculation = false;
				whereICanGo.Clear();
				
				whosLine = myTurn ? 1 : 2;
				
				cloneField = CopyMatrix( GM.fieldMatrix );

				waypointsListStart = new List<MatrixPoints>();
				waypointsListStart.Add(currentPoint);
				
				CalculateRoute(waypointsListStart, cloneField);		// основа. рассчет точек
				
				if ((initialKick == true || whereICanGo.Count == 0) && noInitials() )
				{
					EndOfTheGame();
				}
				
				if (whereICanGo.Count == 0)	// если тупик, ходов не нашлось, то разводим заново по возможности
				{
					initialKick = true;
					currentPoint = cloneField[7,4];
					newCalculation = true;
				}
				else
				{
					if (myTurn)
					{
						if (GM.isHelperOn)
							ShowOrHideHelpers(whereICanGo, true);
						GC.Collect();
					}
					else
					{ 	// AI goes

						bestLine = -1;	// 0 и 14 - до ворот. вынужденный автогол тоже возможен ;)
						List<PotentialPoins> optionsToGo = new List<PotentialPoins>();
						
						for(int i = 0; i < whereICanGo.Count(); i++) // сделан клик в список доступных ходов? иначе игнор
						{
							if (whereICanGo[i].point.x > bestLine)
							{
								optionsToGo.Clear();
								optionsToGo.Add( whereICanGo[i] );	// выписываем варианты ходов до одного уровня. чтобы рандомить
								bestLine = whereICanGo[i].point.x;
							}
							if (whereICanGo[i].point.x == bestLine)
							{
								optionsToGo.Add( whereICanGo[i] );
							}
						}


						if (optionsToGo.Count > 0)
						{
							int option = Random.Range(0, optionsToGo.Count);
							Vector3[] lineCoordinatesArray = MakeItVector3Array(optionsToGo[option].waypointsList);
							drawLine.MakeLine(lineCoordinatesArray, GM.opponentColor);
								
							GM.fieldMatrix = optionsToGo[option].newFieldMatrix;
							currentPoint = optionsToGo[option].point;
							myTurn = true;
							newCalculation = true;
							
							if (bestLine == 14)
								ScoredGoal(false);	// мне гол
							if (bestLine == 0)
								ScoredGoal(true);	// он себе автогол
						}
					}
				}
			}

			if (Input.GetMouseButtonDown(0))
			{	
				RaycastHit2D hit = Physics2D.Raycast( Camera.main.ScreenToWorldPoint (Input.mousePosition) , Vector2.zero);
				if (hit.collider != null)
				{
					string[] coordinates = hit.collider.gameObject.name.Split(new char[] { '-' });
					int x = Convert.ToInt32(coordinates[0]);
					int y = Convert.ToInt32(coordinates[1]);

					foreach(PotentialPoins clickedPoint in whereICanGo) // сделан клик в список доступных ходов? иначе игнор
					{
						if (clickedPoint.point.x == x && clickedPoint.point.y == y)
						{
							Vector3[] lineCoordinatesArray = MakeItVector3Array(clickedPoint.waypointsList);
							drawLine.MakeLine(lineCoordinatesArray, GM.myColor);
							
							GM.fieldMatrix = clickedPoint.newFieldMatrix;
							currentPoint = clickedPoint.point;
							myTurn = false;
							newCalculation = true;
							initialKick = false;
							ShowOrHideHelpers(whereICanGo, false);
							if (clickedPoint.point.x == 0)
								ScoredGoal(true);	// гол
							if (clickedPoint.point.x == 14)
								ScoredGoal(false);	// автогол ((
							break;
						}
					}
				}
			}
		}
	}
	
	void StepForward(int x, int y, int nextX, int nextY, int direction, List<MatrixPoints> waypointsListOld, MatrixPoints[,] cloneFieldOld)
	{
		if (nextX >= 0 && nextX <= 14 && nextY >= 0 && nextY <= 8)
		{
			int oppositDirection = (direction + 4)%8;	// это же направление от другой точки назад. иначе зациклится
			
			MatrixPoints[,] cloneField = CopyMatrix( cloneFieldOld ); // копировать именно здесь. иначе зона видимости будет открыта другим точкам.
			MatrixPoints nextPoint = cloneField[nextX, nextY];
		//0-свободно, 1-мной зарисовано, 2-противником зарисовано, 3-разметка
			cloneField[x, y].directions[direction] = whosLine;
			cloneField[nextX, nextY].directions[oppositDirection] = whosLine;

			List<MatrixPoints> waypointsList = new List<MatrixPoints>(waypointsListOld);
			waypointsList.Add(nextPoint);

			if (iCanContinue(nextPoint, oppositDirection))		// если могу продолжить, то рекурсия туда,
			{	
			
				CalculateRoute(waypointsList, cloneField);
			}
			else
			{
				PotentialPoins potentialPoint = new PotentialPoins(nextPoint, waypointsList, cloneField);
				whereICanGo.Add( potentialPoint ); 				// иначе записываем точку в список доступных для 
			}
		}
	}
	
	void CalculateRoute(List<MatrixPoints> waypointsList, MatrixPoints[,] cloneField)
	{
		int x = waypointsList.Last().x;
		int y = waypointsList.Last().y;

		for(int i = 0; i < 8; i++)
		{
			if (initialKick & x == 7 && y == 4 && (i%2 == 0))	
				continue;	// при разведении мяча игнорим четные направления (вверх/вниз и в стороны)
				
			if (cloneField[x, y].directions[i] == 0)	// если направление не занято, 
			{
				switch (i)	// то смотрим что там в потенциальных точках.  
				{
					case 7:
						StepForward(x, y, x-1, y-1, i, waypointsList, cloneField);
						break;
					case 6:
						StepForward(x, y, x, y-1, i, waypointsList, cloneField);
						break;
					case 5:
						StepForward(x, y, x+1, y-1, i, waypointsList, cloneField);
						break;
					case 4:
						StepForward(x, y, x+1, y, i, waypointsList, cloneField);
						break;
					case 3:	
						StepForward(x, y, x+1, y+1, i, waypointsList, cloneField);
						break;
					case 2:
						StepForward(x, y, x, y+1, i, waypointsList, cloneField);
						break;
					case 1:
						StepForward(x, y, x-1, y+1, i, waypointsList, cloneField);
						break;
					case 0:
						StepForward(x, y, x-1, y, i, waypointsList, cloneField);
						break;
				}
			}
		}
	}
		
	bool iCanContinue(MatrixPoints pointInfo, int directionICameFrom)
	{
		if ((pointInfo.x == 0 || pointInfo.x == 14) && pointInfo.y >= 3 && pointInfo.y <= 5)		// мяч в воротах
		{
			return false;
		}
		
		for(int i = 0; i < 8; i++)
		{
			if (i != directionICameFrom && pointInfo.directions[i] > 0)	// есть от чего рикошетить? кроме пути откуда я
				return true;
		}
		return false;
	}
	
	void ShowOrHideHelpers(List<PotentialPoins> helperPoints, bool state) // подсветим возможные ходы
	{
		foreach(PotentialPoins helper in helperPoints)
			GameObject.Find("Points/" + helper.point.x + "-" + helper.point.y).transform.GetChild(0).gameObject.SetActive(state);
	}
		
	Vector3[] MakeItVector3Array(List<MatrixPoints> route)
	{
		Vector3[] drawLinePoints = new Vector3[ route.Count()];
		int i = 0;
		
		foreach(MatrixPoints drawPoint in route)
		{
			drawLinePoints[i] = drawPoint.pointCoordinates;
			i++;
		}
		return drawLinePoints;
	}

	MatrixPoints[,] CopyMatrix(MatrixPoints[,] originalMatrix)
	{
		MatrixPoints[,] copyMatrix = new MatrixPoints[15,9];
		for(int x = 0; x < 15; x++)
		{
			for(int y = 0; y < 9; y++)
			{
				copyMatrix[x,y] = new MatrixPoints(x,y, new Vector3(0f,0f,0f));
				copyMatrix[x,y].pointCoordinates = originalMatrix[x,y].pointCoordinates;
				for(int i = 0; i < 8; i++)
					copyMatrix[x,y].directions[i] = originalMatrix[x,y].directions[i];
			}
		}
		return copyMatrix;
	}
	
	void ScoredGoal(bool iScoredGoal)
	{
		initialKick = true;
		currentPoint = GM.fieldMatrix[7,4];
		if (iScoredGoal)
		{
			myTurn = false;
			myScore++;
		}
		else
		{
			hisScore++;
			myTurn = true;
		}
		score.text = myScore + ":" + hisScore;
		return;
	}
	
	void EndOfTheGame()
	{
		newCalculation = false;
		initialKick = false;
		endOfGame = true;

		if (myScore > hisScore)
		{
			resultText.color = GM.myColor;
			resultText.text =  "YOU WIN!!!";
		}
		else if (myScore < hisScore)
		{
			resultText.color = GM.opponentColor;
			resultText.text =  "YOU LOSE!!!";
		}
		else
		{
			resultText.color = GM.fieldColor;
			resultText.text =  "DRAW!!!";
		}
		
		GameObject.Find("Canvas/Field/Pannel").SetActive(true);
	}
	
	bool noInitials()
	{
		if (GM.fieldMatrix[7,4].directions[1] > 0 && GM.fieldMatrix[7,4].directions[3] > 0 && 
		GM.fieldMatrix[7,4].directions[5] > 0 && GM.fieldMatrix[7,4].directions[7] > 0)
			return true;
		else
			return false;
	}
	
	public void Helper()
	{
		GM.isHelperOn = helperToggle.isOn;
		PlayerPrefs.SetInt("Helper", (helperToggle.isOn ? 1 : 0));
		ShowOrHideHelpers(whereICanGo, helperToggle.isOn);
	}
}
