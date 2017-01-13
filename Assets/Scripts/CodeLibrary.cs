using System;
using System.Collections;
using UnityEngine;

public class Point
{
    public int x { get; set; }
    public int y { get; set; }

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(Point p)
    {
        return (this.x == p.x && this.y == p.y);
    }

    public override String ToString()
    {
        return "(x: " + x + " " + "y: " + y + ")";
    }

}

public class Tuple<T1, T2>
{
    public T1 First { get; private set; }
    public T2 Second { get; private set; }
    internal Tuple(T1 first, T2 second)
    {
        First = first;
        Second = second;
    }
}

public static class WaitFor
{
    public static IEnumerator Frames(int frameCount)
    {
        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
    }

    public static IEnumerator Seconds(float time)
    {
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }
    }
}

public static class ChangeVector
{
    public enum coord
    {
        x,
        y,
        z
    }

    public static Vector3 Three(Vector3 vec, float newx, float newy, float newz)
    {
        vec.x = newx;
        vec.y = newy;
        vec.z = newz;
        return vec;
    }

    public static Vector3 Three(Vector3 vec, coord coord, float newValue)
    { 
        return assignValue(vec, coord, newValue);
    }

    public static Vector3 Three(Vector3 vec, coord coord1, float newValue1, coord coord2, float newValue2)
    {
        vec = assignValue(vec, coord1, newValue1);
        return assignValue(vec, coord2, newValue2);
    }

    private static Vector3 assignValue(Vector3 vec, coord coord, float value)
    {
        switch (coord)
        {
            case coord.x:
                vec.x = value;
                break;
            case coord.y:
                vec.y = value;
                break;
            case coord.z:
                vec.z = value;
                break;
        }
        return vec;
    }

}