using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MatrixPoints
{
	private int _x;
	private int _y;
	private int[] _directions;
	private Vector3 _pointCoordinates;
	
    public MatrixPoints(int x, int y, Vector3 pointCoordinates)
	{
		this._x = x;
		this._y = y;
		this._directions = new int[8] { 0,0,0,0,0,0,0,0 };
		this._pointCoordinates = pointCoordinates;
	}
	
	public MatrixPoints(MatrixPoints obj)
	{
		this._x = obj.x;
		this._y = obj.y;
		this._directions = obj.directions;
		this._pointCoordinates = obj.pointCoordinates;
	}

	public int x 
	{
		get { return _x; }
		set { _x = value; }
	}
	
	public int y
	{
		get { return _y; }
		set { _y = value; }
	}

	public int[] directions
	{
      get { return _directions; }
      set { _directions = value; }
	}
	
	public Vector3 pointCoordinates
	{
		get { return _pointCoordinates; }
		set { _pointCoordinates = value; }
	}
}


public struct PotentialPoins
{
	private MatrixPoints _point;
	private List<MatrixPoints> _waypointsList;
	private MatrixPoints[,] _newFieldMatrix;
	
	public PotentialPoins(MatrixPoints point, List<MatrixPoints> waypointsList, MatrixPoints[,] newFieldMatrix)
	{
		_point = point;
		_waypointsList = waypointsList;
		_newFieldMatrix = newFieldMatrix;
	}
	
	public MatrixPoints point
	{
		get { return _point; }
		set { _point = value; }
	}
	
	public List<MatrixPoints> waypointsList
	{
		get { return _waypointsList; }
		set { _waypointsList = value; }
	}
	
	public MatrixPoints[,] newFieldMatrix
	{
		get { return _newFieldMatrix; }
		set { _newFieldMatrix = value; }
	}
}