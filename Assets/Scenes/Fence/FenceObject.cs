using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceObject : MonoBehaviour  {

    public GameObject prefab;

    public GameObject gameObj;

    public Vector3 pointA;
    public Vector3 pointB;

    public FenceObject (Vector3 A,Vector3 B,PlayerSetup pSetup) {
        this.pointA = A;
        this.pointB = B;
        //prefab = pSetup.GetComponent<FenceObject>().prefab;
        this.gameObj = Instantiate(Resources.Load("FenceObject")) as GameObject;
        this.Draw();
    }

    public void Draw() {
        Vector3 diff = new Vector3(pointB.x - pointA.x, 0, pointB.z - pointA.z);

        float xscale = Mathf.Round(diff.magnitude);


        float middlePointx = (pointA.x + pointB.x) / 2;
        float middlePointy = (pointA.y);
        float middlePointz = (pointA.z + pointB.z) / 2;


        this.gameObj.transform.position = new Vector3(middlePointx, middlePointy, middlePointz);

        this.gameObj.transform.localScale = new Vector3(xscale, 1f, 1f);

        MeshRenderer[] meshRenderers = this.gameObj.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mr in meshRenderers) {
            mr.material.mainTextureScale = new Vector2(xscale, 1);
        }


        float dX = pointB.x - pointA.x;
        float dY = pointB.z - pointA.z;

        float angle = Mathf.Atan(dY / dX) * 180 / Mathf.PI;
        Debug.Log("Angle: " + angle);
        gameObj.transform.eulerAngles = new Vector3(90, 0, angle);
    }

}
