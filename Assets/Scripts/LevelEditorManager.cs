using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelEditorManager : MonoBehaviour
{
    public static LevelEditorManager Instance;

    public LayerMask groundLayer;
    public LayerMask objectLayer;
    public Transform levelParent;
    public LevelEditorUI uiManager;

    public List<LevelEditorObject> PlacedObjects = new List<LevelEditorObject>();

    LevelEditorObject selectedObject;
    LevelEditorObject placingObject;
    bool isPlacing = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() && !isPlacing)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (isPlacing && placingObject != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 500f, groundLayer))
            {
                Vector3 pos = hit.point;
                pos.y += placingObject.transform.localScale.y * 0.5f;
                placingObject.transform.position = pos;
            }

            if (Input.GetMouseButtonDown(0))
            {
                PlacedObjects.Add(placingObject);
                LevelEditorObject placed = placingObject;
                placingObject = null;
                isPlacing = false;

                if (uiManager != null)
                    uiManager.RefreshSceneObjectList();

                SelectObject(placed);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Destroy(placingObject.gameObject);
                placingObject = null;
                isPlacing = false;
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 500f, objectLayer))
            {
                LevelEditorObject obj = hit.collider.GetComponentInParent<LevelEditorObject>();
                if (obj != null)
                {
                    SelectObject(obj);
                }
            }
            else
            {
                DeselectCurrent();
            }
        }
    }

    public void SpawnObjectForPlacement(string path)
    {
        DeselectCurrent();

        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
            return;

        GameObject go = Instantiate(prefab, levelParent);
        LevelEditorObject leo = go.GetComponent<LevelEditorObject>();
        if (leo == null)
            leo = go.AddComponent<LevelEditorObject>();

        leo.objectType = path;
        leo.customName = prefab.name;
        go.name = prefab.name;

        SetLayer(go, LayerMask.NameToLayer("EditorObjects"));

        placingObject = leo;
        isPlacing = true;
    }

    public void SelectObject(LevelEditorObject obj)
    {
        DeselectCurrent();

        selectedObject = obj;
        if (selectedObject != null)
        {
            selectedObject.SetSelected(true);
            if (uiManager != null)
                uiManager.ShowInfoPanel(selectedObject);
        }
    }

    public void DeselectCurrent()
    {
        if (selectedObject != null)
        {
            selectedObject.SetSelected(false);
            selectedObject = null;
        }
        if (uiManager != null)
            uiManager.HideInfoPanel();
    }

    public void DeleteSelected()
    {
        if (selectedObject != null)
        {
            PlacedObjects.Remove(selectedObject);
            Destroy(selectedObject.gameObject);
            selectedObject = null;

            if (uiManager != null)
            {
                uiManager.HideInfoPanel();
                uiManager.RefreshSceneObjectList();
            }
        }
    }

    public void RenameSelected(string newName)
    {
        if (selectedObject != null)
        {
            selectedObject.customName = newName;
            selectedObject.gameObject.name = newName;
            if (uiManager != null)
                uiManager.RefreshSceneObjectList();
        }
    }

    public void RotateSelected(float angleY)
    {
        if (selectedObject != null)
        {
            Vector3 rot = selectedObject.transform.eulerAngles;
            rot.y = angleY;
            selectedObject.transform.eulerAngles = rot;
        }
    }

    public void SetSelectedHeight(float yPos)
    {
        if (selectedObject != null)
        {
            Vector3 pos = selectedObject.transform.position;
            pos.y = yPos;
            selectedObject.transform.position = pos;
        }
    }

    public void SaveLevel()
    {
        LevelSaveData data = new LevelSaveData();

        for (int i = 0; i < PlacedObjects.Count; i++)
        {
            LevelEditorObject obj = PlacedObjects[i];
            if (obj == null) continue;

            ObjectSaveData item = new ObjectSaveData();
            item.objectType = obj.objectType;
            item.customName = obj.customName;
            item.position = obj.transform.position;
            item.rotation = obj.transform.eulerAngles;
            item.scale = obj.transform.localScale;

            data.objects.Add(item);
        }

        string json = JsonUtility.ToJson(data, true);
        string path = Application.dataPath + "/level_save.json";
        File.WriteAllText(path, json);
        Debug.Log("Сохранено!");
    }

    public void LoadLevel()
    {
        string path = Application.dataPath + "/level_save.json";
        if (!File.Exists(path))
            return;

        ClearLevel();

        string json = File.ReadAllText(path);
        LevelSaveData data = JsonUtility.FromJson<LevelSaveData>(json);

        for (int i = 0; i < data.objects.Count; i++)
        {
            ObjectSaveData item = data.objects[i];
            GameObject prefab = Resources.Load<GameObject>(item.objectType);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, item.position, Quaternion.Euler(item.rotation), levelParent);
                go.transform.localScale = item.scale;
                go.name = item.customName;

                LevelEditorObject leo = go.GetComponent<LevelEditorObject>();
                if (leo == null)
                    leo = go.AddComponent<LevelEditorObject>();

                leo.objectType = item.objectType;
                leo.customName = item.customName;

                SetLayer(go, LayerMask.NameToLayer("EditorObjects"));
                PlacedObjects.Add(leo);
            }
        }

        if (uiManager != null)
            uiManager.RefreshSceneObjectList();
    }

    public void ClearLevel()
    {
        DeselectCurrent();

        for (int i = 0; i < PlacedObjects.Count; i++)
        {
            if (PlacedObjects[i] != null)
                Destroy(PlacedObjects[i].gameObject);
        }

        PlacedObjects.Clear();

        if (uiManager != null)
            uiManager.RefreshSceneObjectList();
    }

    void SetLayer(GameObject go, int layer)
    {
        if (layer < 0) return;
        go.layer = layer;
        foreach (Transform t in go.transform)
        {
            SetLayer(t.gameObject, layer);
        }
    }
}