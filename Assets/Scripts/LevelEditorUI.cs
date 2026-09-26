using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelEditorUI : MonoBehaviour
{
    public Dropdown categoryDropdown;
    public Transform paletteContainer;
    public GameObject paletteButtonPrefab;

    public Transform sceneObjectsContainer;
    public GameObject sceneObjectButtonPrefab;

    public GameObject infoPanel;
    public InputField nameInputField;
    public Slider rotationSlider;
    public Slider heightSlider;
    public Text objectTypeText;
    public Button deleteButton;

    public Button saveButton;
    public Button loadButton;
    public Button clearButton;

    string[] folders = { "Blocks", "Props", "Nature", "Lights", "Mechanisms" };

    void Start()
    {
        // заполняем dropdown
        if (categoryDropdown != null)
        {
            categoryDropdown.ClearOptions();
            List<string> opts = new List<string>();
            opts.Add("Блоки");
            opts.Add("Декорации");
            opts.Add("Природа");
            opts.Add("Свет");
            opts.Add("Механизмы");
            categoryDropdown.AddOptions(opts);
            categoryDropdown.onValueChanged.AddListener(ChangeCategory);
        }

        if (saveButton != null)
            saveButton.onClick.AddListener(Save);
        if (loadButton != null)
            loadButton.onClick.AddListener(Load);
        if (clearButton != null)
            clearButton.onClick.AddListener(Clear);
        if (deleteButton != null)
            deleteButton.onClick.AddListener(Delete);

        if (nameInputField != null)
            nameInputField.onEndEdit.AddListener(Rename);

        if (rotationSlider != null)
            rotationSlider.onValueChanged.AddListener(Rotate);

        if (heightSlider != null)
            heightSlider.onValueChanged.AddListener(ChangeHeight);

        if (infoPanel != null)
            infoPanel.SetActive(false);

        // загружаем первую категорию
        LoadPalette(0);
    }

    void ChangeCategory(int index)
    {
        LoadPalette(index);
    }

    void LoadPalette(int index)
    {
        // чистим старые кнопки
        foreach (Transform child in paletteContainer)
        {
            Destroy(child.gameObject);
        }

        if (index < 0 || index >= folders.Length)
            return;

        string folder = folders[index];
        GameObject[] prefabs = Resources.LoadAll<GameObject>(folder);

        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject prefab = prefabs[i];
            string path = folder + "/" + prefab.name;

            GameObject btnGO = Instantiate(paletteButtonPrefab, paletteContainer);
            Text t = btnGO.GetComponentInChildren<Text>();
            if (t != null)
                t.text = prefab.name;

            Button b = btnGO.GetComponent<Button>();
            if (b != null)
            {
                // локальная переменная нужна, иначе все кнопки будут грузить последний префаб
                string p = path;
                b.onClick.AddListener(() => {
                    if (LevelEditorManager.Instance != null)
                        LevelEditorManager.Instance.SpawnObjectForPlacement(p);
                });
            }
        }
    }

    public void RefreshSceneObjectList()
    {
        foreach (Transform child in sceneObjectsContainer)
        {
            Destroy(child.gameObject);
        }

        if (LevelEditorManager.Instance == null)
            return;

        List<LevelEditorObject> list = LevelEditorManager.Instance.PlacedObjects;

        for (int i = 0; i < list.Count; i++)
        {
            LevelEditorObject obj = list[i];
            if (obj == null)
                continue;

            GameObject btnGO = Instantiate(sceneObjectButtonPrefab, sceneObjectsContainer);
            Text t = btnGO.GetComponentInChildren<Text>();
            if (t != null)
                t.text = (i + 1) + ". " + obj.customName;

            Button b = btnGO.GetComponent<Button>();
            if (b != null)
            {
                LevelEditorObject o = obj;
                b.onClick.AddListener(() => {
                    LevelEditorManager.Instance.SelectObject(o);
                });
            }
        }
    }

    public void ShowInfoPanel(LevelEditorObject obj)
    {
        if (infoPanel == null || obj == null)
            return;

        infoPanel.SetActive(true);

        if (objectTypeText != null)
            objectTypeText.text = "Тип: " + obj.objectType;

        if (nameInputField != null)
            nameInputField.text = obj.customName;

        if (rotationSlider != null)
        {
            rotationSlider.minValue = 0;
            rotationSlider.maxValue = 360;
            rotationSlider.value = obj.transform.eulerAngles.y;
        }

        if (heightSlider != null)
        {
            heightSlider.minValue = 0;
            heightSlider.maxValue = 10;
            heightSlider.value = obj.transform.position.y;
        }
    }

    public void HideInfoPanel()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    void Rename(string newName)
    {
        if (LevelEditorManager.Instance != null)
            LevelEditorManager.Instance.RenameSelected(newName);
    }

    void Rotate(float val)
    {
        if (LevelEditorManager.Instance != null)
            LevelEditorManager.Instance.RotateSelected(val);
    }

    void ChangeHeight(float val)
    {
        if (LevelEditorManager.Instance != null)
            LevelEditorManager.Instance.SetSelectedHeight(val);
    }

    void Delete()
    {
        if (LevelEditorManager.Instance != null)
            LevelEditorManager.Instance.DeleteSelected();
    }

    void Save()
    {
        if (LevelEditorManager.Instance != null)
            LevelEditorManager.Instance.SaveLevel();
    }

    void Load()
    {
        if (LevelEditorManager.Instance != null)
            LevelEditorManager.Instance.LoadLevel();
    }

    void Clear()
    {
        if (LevelEditorManager.Instance != null)
            LevelEditorManager.Instance.ClearLevel();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}