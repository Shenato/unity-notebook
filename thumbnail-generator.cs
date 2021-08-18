using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
public class ThumbnailGenerator: MonoBehaviour {

  public Camera ThumbnailGeneratingCamera;
  //--- Reference to the special camera i attached with the solution that is configured to take a transparent background thumbnail.

  public void CreateThumbnailCam(GameObject ObjtoThumbnail) {
    int ObjDefaultLayer;
    ObjDefaultLayer = ObjtoThumbnail.layer;
    SetLayerRecursively(ObjtoThumbnail, LayerMask.NameToLayer("ThumbnailGenLayer"));

    Bounds BoundsOfObj = MeasurePrefabObj(ObjtoThumbnail);
    Vector3 OffsetVector = BoundsOfObj.size;
    Vector3 Closestpoint = BoundsOfObj.ClosestPoint(new Vector3(ObjtoThumbnail.transform.position.x + OffsetVector.x + 1f, ObjtoThumbnail.transform.position.y, ObjtoThumbnail.transform.position.z + OffsetVector.z + 1f));

    Debug.Log("closestpoint = " + Closestpoint.ToString());

    float dist = Mathf.Tan(30) * OffsetVector.y * .5f;

    Debug.Log("Distance = " + dist);

    RenderTexture Rt = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
    Camera CamTemp = Instantiate(ThumbnailGeneratingCamera, Offset(ObjtoThumbnail.transform.position, OffsetVector), Quaternion.Euler(45, 45, 0), ObjtoThumbnail.transform) as Camera;
    CamTemp.transform.LookAt(ObjtoThumbnail.transform);
    CamTemp.targetTexture = Rt;

    Texture2D texture = new Texture2D(CamTemp.targetTexture.width, CamTemp.targetTexture.height, TextureFormat.ARGB32, false);
    CamTemp.Render();
    RenderTexture.active = CamTemp.targetTexture;
    texture.ReadPixels(new Rect(0, 0, CamTemp.targetTexture.width, CamTemp.targetTexture.height), 0, 0);
    texture.alphaIsTransparency = true;

    texture.Apply();

    byte[] bytes = texture.EncodeToPNG();

    string imagePath = Time.realtimeSinceStartup.ToString() + Random.Range(1, 9999).ToString() + ".png";
    Debug.Log(Application.persistentDataPath);
    File.WriteAllBytes(Application.persistentDataPath + Path.DirectorySeparatorChar + imagePath, bytes);

    RenderTexture.active = null;
    SetLayerRecursively(ObjtoThumbnail, ObjDefaultLayer);
    DestroyObject(texture);
    Destroy(Rt);
    Destroy(CamTemp);
  }

  Vector3 Offset(Vector3 PositiontoOffsetfrom, Vector3 OffsetVector3) {
    return new Vector3(PositiontoOffsetfrom.x + OffsetVector3.x, PositiontoOffsetfrom.y + PositiontoOffsetfrom.y + OffsetVector3.y, PositiontoOffsetfrom.z + OffsetVector3.z);
  }
  Bounds MeasurePrefabObj(GameObject GotoM) {

    Renderer[] Renderers = GotoM.GetComponentsInChildren < Renderer > ();
    if (Renderers.Length > 0) {
      Bounds bounds = Renderers[0].bounds;

      for (int i = 0, Ri = Renderers.Length; i < Ri; i++) {
        bounds.Encapsulate(Renderers[i].bounds);
      }
      return bounds;
    }
    else {
      return new Bounds();
    }
  }
  void SetLayerRecursively(GameObject go, int layerNumber) {
    foreach(Transform trans in go.GetComponentsInChildren < Transform > (true)) {
      trans.gameObject.layer = layerNumber;
    }
  }

}