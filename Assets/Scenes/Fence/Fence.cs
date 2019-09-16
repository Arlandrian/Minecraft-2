using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.Networking;

[Serializable]
public struct MyVector3 {
    public float x;
    public float y;
    public float z;

    public MyVector3(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

[Serializable]
public class FenceData {
    public string ownerName;
    public MyVector3[] polygon;

}

public class Fence {

    public string ownerName;

    public List<Vector3> polygon;

    public Fence(string ownerName, List<Vector3> points) {
        this.ownerName = ownerName;
        if (points != null)
            this.polygon = points;
        else {
            this.polygon = new List<Vector3>();
        }
    }

    public bool Load() //read data from file
    {
        string filename = Application.persistentDataPath + "/savedata/" + Utils.seed + "/fence/"+ownerName;
        if (File.Exists(filename)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filename, FileMode.Open);
            FenceData fenceData = new FenceData();
            fenceData = (FenceData)bf.Deserialize(file);
            if (polygon == null)
                polygon = new List<Vector3>();

            for (int i = 0; i < fenceData.polygon.Length; i++) {

                polygon.Add(new Vector3(fenceData.polygon[i].x, fenceData.polygon[i].y, fenceData.polygon[i].z));
            }
            ownerName = fenceData.ownerName;

            file.Close();

            return true;
        }

        return false;
    }
    
    public static Fence DataToFence(FenceData fData) {
        Fence fence = new Fence(fData.ownerName);
        for (int i = 0; i < fData.polygon.Length; i++) {

            fence.polygon.Add(new Vector3(fData.polygon[i].x, fData.polygon[i].y, fData.polygon[i].z));
        }
        return fence;
    }

    public static FenceData FenceToData(Fence fi) {
        FenceData fenceData = new FenceData {
            ownerName = fi.ownerName,
            polygon = new MyVector3[fi.polygon.Count]
        };
        for (int i = 0; i < fi.polygon.Count; i++) {

            fenceData.polygon[i] = new MyVector3(fi.polygon[i].x, fi.polygon[i].y, fi.polygon[i].z);
        }
        return fenceData;
    }

    public void Save() //write data to file
    {
        string filename = Application.persistentDataPath + "/savedata/" + Utils.seed + "/fence/" + ownerName;

        if (!File.Exists(filename)) {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
        }
        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = File.Open(filename, FileMode.OpenOrCreate);

        FenceData fenceData = new FenceData();
        fenceData.ownerName = ownerName;
        fenceData.polygon = new MyVector3[polygon.Count];
        for (int i = 0; i < polygon.Count; i++) {

            fenceData.polygon[i] = new MyVector3(polygon[i].x, polygon[i].y, polygon[i].z);
        }

        bf.Serialize(file, fenceData);
        file.Close();
    }

    public Fence(string ownerName) {
        this.ownerName = ownerName;
        polygon = new List<Vector3>();
    }

    bool onSegment(Vector2 p, Vector2 q, Vector2 r) {
        if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
            return true;
        return false;
    }

    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are colinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    int orientation(Vector2 p, Vector2 q, Vector2 r) {
        float val = (q.y - p.y) * (r.x - q.x) -
                  (q.x - p.x) * (r.y - q.y);

        if (val == 0) return 0;  // colinear
        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }

    // The function that returns true if line segment 'p1q1'
    // and 'p2q2' intersect.
    bool doIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2) {
        // Find the four orientations needed for general and
        // special cases
        int o1 = orientation(p1, q1, p2);
        int o2 = orientation(p1, q1, q2);
        int o3 = orientation(p2, q2, p1);
        int o4 = orientation(p2, q2, q1);

        // General case
        if (o1 != o2 && o3 != o4)
            return true;

        // Special Cases
        // p1, q1 and p2 are colinear and p2 lies on segment p1q1
        if (o1 == 0 && onSegment(p1, p2, q1)) return true;

        // p1, q1 and p2 are colinear and q2 lies on segment p1q1
        if (o2 == 0 && onSegment(p1, q2, q1)) return true;

        // p2, q2 and p1 are colinear and p1 lies on segment p2q2
        if (o3 == 0 && onSegment(p2, p1, q2)) return true;

        // p2, q2 and q1 are colinear and q1 lies on segment p2q2
        if (o4 == 0 && onSegment(p2, q1, q2)) return true;

        return false; // Doesn't fall in any of the above cases
    }

    private bool isInside(Vector2[] polygon, int n, Vector2 p) {
        // There must be at least 3 vertices in polygon[]
        if (n < 3) return false;

        // Create a point for line segment from p to infinite

        Vector2 extreme = new Vector2(1000000, p.y);
        // Count intersections of the above line with sides of polygon
        int count = 0, i = 0;
        do {
            int next = (i + 1) % n;

            // Check if the line segment from 'p' to 'extreme' intersects
            // with the line segment from 'polygon[i]' to 'polygon[next]'
            if (doIntersect(polygon[i], polygon[next], p, extreme)) {
                // If the point 'p' is colinear with line segment 'i-next',
                // then check if it lies on segment. If it lies, return true,
                // otherwise false
                if (orientation(polygon[i], p, polygon[next]) == 0)
                    return onSegment(polygon[i], p, polygon[next]);

                count++;
            }
            i = next;
        } while (i != 0);

        // Return true if count is odd, false otherwise
        return (count & 1) == 1 ? true : false;  // Same as (count%2 == 1)
    }

    public bool IsInside(Vector3 point) {
        int n = polygon.Count;
        // There must be at least 3 vertices in polygon[]
        if (n < 3) return false;

        Vector2 p = new Vector2(point.x, point.z);
        // Create a point for line segment from p to infinite

        Vector2 extreme = new Vector2(100000, p.y);
        // Count intersections of the above line with sides of polygon
        int count = 0, i = 0;
        do {
            int next = (i + 1) % n;

            // Check if the line segment from 'p' to 'extreme' intersects
            // with the line segment from 'polygon[i]' to 'polygon[next]'
            if (doIntersect(V3To2(polygon[i]), V3To2( polygon[next]), p, extreme)) {
                // If the point 'p' is colinear with line segment 'i-next',
                // then check if it lies on segment. If it lies, return true,
                // otherwise false
                if (orientation(V3To2(polygon[i]), p, V3To2(polygon[next])) == 0)
                    return onSegment(V3To2(polygon[i]), p, V3To2(polygon[next]));

                count++;
            }
            i = next;
        } while (i != 0);

        // Return true if count is odd, false otherwise
        return (count & 1) == 1 ? true : false;  // Same as (count%2 == 1)
    }

    Vector2 V3To2(Vector3 V) {
        return new Vector2(V.x, V.z);
    }

}
