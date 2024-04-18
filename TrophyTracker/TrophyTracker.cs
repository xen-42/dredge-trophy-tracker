using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Winch.Core;

namespace TrophyTracker;

[HarmonyPatch]
public class TrophyTracker : MonoBehaviour
{
    private static string _currentFilePath;

    public static string GetModFolder() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public void Start()
    {
        new Harmony(nameof(TrophyTracker)).PatchAll();

        SceneManager.activeSceneChanged += OnSceneChanged;
        OnSceneChanged(default, SceneManager.GetActiveScene());

        WinchCore.Log.Debug($"{nameof(TrophyTracker)} has loaded!");
    }

    public void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene prev, Scene current)
    {
        WinchCore.Log.Debug($"Scene changed to {current.name}");

        if (current.name == "Game")
        {
            try
            {
                var folderPath = Path.Combine(GetModFolder(), "data");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                _currentFilePath = Path.Combine(folderPath, $"{DateTime.Now:dd_MM_yyyy_hh_mm_ss}.csv");
                File.WriteAllText(_currentFilePath, $"ID, NAME, RAW SIZE, FORMATTED SIZE, IS TROPHY, MIN SIZE, MAX SIZE");
                
                WinchCore.Log.Debug($"Saving data this run to {_currentFilePath}");
            }
            catch (Exception ex)
            {
                WinchCore.Log.Error($"Failed to create new data file - no fish info will be recorded! {ex}");
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SaveData), nameof(SaveData.RecordFishSize))]
    public static void SaveData_RecordFishSize(SaveData __instance, FishItemData fishItemData, float size)
    {
        WinchCore.Log.Debug($"GRUH");

        try
        {
            // Get it to always use cm for this
            var currentUnits = GameManager.Instance.SettingsSaveData.units;
            GameManager.Instance.SettingsSaveData.units = 0;

            var name = fishItemData.itemNameKey.GetLocalizedString();
            var formattedSize = GameManager.Instance.ItemManager.GetFormattedFishSizeString(size, fishItemData);
            var id = fishItemData.id;
            var isTrophy = size > GameManager.Instance.GameConfigData.TrophyMaxSize;
            var minSize = fishItemData.minSizeCentimeters;
            var maxSize = fishItemData.maxSizeCentimeters;

            // Revert units after
            GameManager.Instance.SettingsSaveData.units = currentUnits;

            using StreamWriter write = new(_currentFilePath);
            write.WriteLine($"\n{id}, {name}, {size}, {formattedSize}, {isTrophy}, {minSize}, {maxSize}");
        }
        catch (Exception e)
        {
            WinchCore.Log.Error($"Failed to record fish data for {fishItemData.id} - {e}");
        }
    }
}
