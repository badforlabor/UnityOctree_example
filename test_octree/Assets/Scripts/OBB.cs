using System.Collections.Generic;
using UnityEngine;

public class OBB
{
    public Vector3 _center;   // obb center
    public Vector3 _xAxis;    // x axis of obb, unit vector
    public Vector3 _yAxis;    // y axis of obb, unit vecotr
    public Vector3 _zAxis;    // z axis of obb, unit vector
    public Vector3 _extentX;  // _xAxis * _extents.x
    public Vector3 _extentY;  // _yAxis * _extents.y
    public Vector3 _extentZ;  // _zAxis * _extents.z
    public Vector3 _extents;  // obb length along each axis


    public OBB(Matrix4x4 mat)
    {
        _center = Vector3.zero;
        _xAxis = new Vector3(1.0f, 0.0f, 0.0f);
        _yAxis = new Vector3(0.0f, 1.0f, 0.0f);
        _zAxis = new Vector3(0.0f, 0.0f, 1.0f);

        _extents = Vector3.one / 2;
        Transform(mat);
    }
    public OBB(Bounds aabb)
    {
        _center = (aabb.min + aabb.max) / 2;
        _xAxis = new Vector3(1.0f, 0.0f, 0.0f);
        _yAxis = new Vector3(0.0f, 1.0f, 0.0f);
        _zAxis = new Vector3(0.0f, 0.0f, 1.0f);

        _extents = (aabb.max - aabb.min) / 2;

        computeExtAxis();
    }
    void computeExtAxis()
    {
        _extentX = _xAxis * _extents.x;
        _extentY = _yAxis * _extents.y;
        _extentZ = _zAxis * _extents.z;  
    }

    public void getCorners(List<Vector3> verts)
    {
        verts.Add(_center - _extentX + _extentY + _extentZ);//左上顶点坐标  
        
        //z轴正方向的面  
        verts.Add(_center - _extentX - _extentY + _extentZ);//左下顶点坐标  
        verts.Add(_center + _extentX - _extentY + _extentZ);//右下顶点坐标 
        verts.Add(_center + _extentX + _extentY + _extentZ);//右上顶点坐标  

        //z轴负方向的面  
        verts.Add(_center + _extentX + _extentY - _extentZ);//右上顶点坐标  
        verts.Add(_center + _extentX - _extentY - _extentZ);//右下顶点坐标  
        verts.Add(_center - _extentX - _extentY - _extentZ);//左下顶点坐标
        verts.Add(_center - _extentX + _extentY - _extentZ);//左上顶点坐标  
    }

    public static float projectPoint(Vector3 point, Vector3 axis)
    {
        float dot = Vector3.Dot(axis, point);
        float ret = dot * point.magnitude;
        return ret;
    }

    public void getInterval(OBB box, Vector3 axis, ref float min, ref float max)
    {
        List<Vector3> corners = new List<Vector3>();
        box.getCorners(corners);
        
        min = max = projectPoint(axis, corners[0]);
        for (int i = 1; i < corners.Count; i++)
        {
            float v = projectPoint(axis, corners[i]);
            min = Mathf.Min(min, v);
            max = Mathf.Max(max, v);
        }
    }

    Vector3 getEdgeDirection(int idx)
    {
        List<Vector3> corners = new List<Vector3>();
        getCorners(corners);

        Vector3 tmpLine;
        switch (idx)
        {
            case 0:// x轴方向  
                tmpLine = corners[5] - corners[6];
                break;
            case 1:// y轴方向
                tmpLine = corners[7] - corners[6];
                break;
            case 2:// z轴方向  
                tmpLine = corners[1] - corners[6];
                break;
            default:
                throw new System.Exception("123");
        }

        tmpLine.Normalize();
        return tmpLine;
    }
    Vector3 getFaceDirection(int idx)
    {
        List<Vector3> corners = new List<Vector3>();
        getCorners(corners);
        Vector3 ret;
        switch (idx)
        {
            case 0:
                {
                    var v0 = corners[2] - corners[1];
                    var v1 = corners[0] - corners[1];
                    ret = Vector3.Cross(v0, v1);
                }
                break;
            case 1:
                {
                    var v0 = corners[5] - corners[2];
                    var v1 = corners[3] - corners[2];
                    ret = Vector3.Cross(v0, v1);
                }
                break;
            case 2:
                {
                    var v0 = corners[1] - corners[2];
                    var v1 = corners[5] - corners[2];
                    ret = Vector3.Cross(v0, v1);
                }
                break;
            default:
                throw new System.Exception("123");
        }

        ret.Normalize();

        return ret;
    }
    public bool Intersects(Bounds box)
    {
        return Intersects(new OBB(box));
    }
    public bool Intersects(OBB box)
    {
        float min1 = 0, max1 = 0, min2 = 0, max2 = 0;
        for (int i = 0; i < 3; i++)
        {
            getInterval(this, getFaceDirection(i), ref min1, ref max1);
            getInterval(box, getFaceDirection(i), ref min2, ref max2);
            if (max1 < min2 || max2 < min1) 
                return false;
        }
        for (int i = 0; i < 3; i++)
        {
            getInterval(this, box.getFaceDirection(i), ref min1, ref max1);
            getInterval(box, box.getFaceDirection(i), ref min2, ref max2);
            if (max1 < min2 || max2 < min1)
                return false;
        }
        for (int i = 0; i <3; i++)  
        {  
            for (int j = 0; j <3; j++)  
            {  
                Vector3 axis = Vector3.Cross(getEdgeDirection(i), box.getEdgeDirection(j));
                getInterval(this, axis, ref min1, ref max1);
                getInterval(box, axis, ref min2, ref max2);
                if (max1 < min2 || max2 < min1) 
                    return false;  
            }  
        }
        return true;
    }
    public void Transform(Matrix4x4 mat)
    {
        _center = mat.MultiplyPoint(_center);

        _xAxis = mat.MultiplyVector(_xAxis);
        _yAxis = mat.MultiplyVector(_yAxis);
        _zAxis = mat.MultiplyVector(_zAxis);

        _xAxis.Normalize();
        _yAxis.Normalize();
        _zAxis.Normalize();

        Vector3 scale = Vector3.one;
        scale.x = mat.GetColumn(0).magnitude;
        scale.y = mat.GetColumn(1).magnitude;
        scale.z = mat.GetColumn(2).magnitude;

        _extents.x *= scale.x;
        _extents.y *= scale.y;
        _extents.z *= scale.z;
        computeExtAxis();
    }
}