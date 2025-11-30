using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameSettingsManager : MonoBehaviour
{
    private static GameSettingsManager instance;
    public static GameSettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameSettingsManager");
                instance = go.AddComponent<GameSettingsManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Settings Values")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0.5f, 1.5f)]
    public float Sensitivity = 1f;

    private const string VOLUME_KEY = "MasterVolume";
    private const string SENSITIVITY_KEY = "MouseSensitivity";

    private List<AudioSource> cachedAudioSources = new List<AudioSource>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSceneReferences();
        ApplyAllSettings();
    }

    public void RefreshSceneReferences()
    {
        cachedAudioSources.Clear();
        cachedAudioSources.AddRange(FindObjectsByType<AudioSource>(FindObjectsSortMode.None));
    }

    public void SetVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeToAllAudioSources();
        SaveSettings();
    }

    public void SetSensitivity(float sensitivity)
    {
        Sensitivity = Mathf.Clamp(sensitivity, 0.5f, 1.5f);
        ApplySensitivityToAllCameras();
        SaveSettings();
    }

    private void ApplyVolumeToAllAudioSources()
    {
        if (cachedAudioSources.Count == 0)
        {
            RefreshSceneReferences();
        }

        foreach (AudioSource source in cachedAudioSources)
        {
            if (source != null)
            {
                source.volume = masterVolume;
            }
        }
    }

    private void ApplySensitivityToAllCameras()
    {
        // Find and update ZTargetingCamera
        ZTargetingCamera[] zTargetingCameras = FindObjectsByType<ZTargetingCamera>(FindObjectsSortMode.None);
        foreach (ZTargetingCamera cam in zTargetingCameras)
        {
            if (cam != null)
            {
                cam.SetSensitivity(Sensitivity);
            }
        }

        // Find and update FreeLookCamera
        FreeLookCamera[] freeLookCameras = FindObjectsByType<FreeLookCamera>(FindObjectsSortMode.None);
        foreach (FreeLookCamera cam in freeLookCameras)
        {
            if (cam != null)
            {
                cam.SetSensitivity(Sensitivity);
            }
        }

        // Find and update AimCamera
        AimCamera[] aimCameras = FindObjectsByType<AimCamera>(FindObjectsSortMode.None);
        foreach (AimCamera cam in aimCameras)
        {
            if (cam != null)
            {
                cam.SetSensitivity(Sensitivity);
            }
        }
    }


    public void ApplyAllSettings()
    {
        ApplyVolumeToAllAudioSources();
        ApplySensitivityToAllCameras();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, Sensitivity);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        Sensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, 1f);
    }
}
