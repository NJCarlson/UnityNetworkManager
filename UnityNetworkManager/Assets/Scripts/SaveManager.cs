using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;


/// <summary>
/// This static class is used to save and load user data.
/// The current standard file format is JSON
/// </summary>
public static class SaveManager 
{
    public static List<ExerciseData> saves;
    public static string stagedToDelete;

    /// <summary>
    /// Loads all the JSON files that are stored on local device;
    /// </summary>
    public static void LoadSessionData()
    {
        try
        {
            saves = new List<ExerciseData>();
            bool loading = true;
            int i = 0;

            while (loading)
            {
                if (File.Exists(Application.persistentDataPath + "/ExerciseData" + i + ".json"))
                {
                    ExerciseData save = new ExerciseData();

                    using (StreamReader r = new StreamReader(Application.persistentDataPath + "/ExerciseData" + i + ".json"))
                    {
                        string json = r.ReadToEnd();
                        JsonUtility.FromJsonOverwrite(json, save);
                        Debug.Log(json);
                        r.Close();
                    }

                    if (string.IsNullOrEmpty(save.id))
                    {
                        save.id = Application.persistentDataPath + "/ExerciseData" + i + ".json";
                    }

                    saves.Add(save);
                    i++;
                }
                else
                {
                    loading = false;
                    Debug.Log(" Loaded " + i + " Files.");
                }
            }
        }
        catch (Exception e )
        {
            Debug.Log(e);
        }
    }

    /// <summary>
    /// Saves the generated JSON file to the local device.
    /// </summary>
    /// <param name="newSave"></param>
    public static void SaveLocalExerciseDataJSON(ExerciseData newSave)
    {
        LoadSessionData();
        string filename = Application.persistentDataPath + "/ExerciseData" + (saves.Count) + ".json";
        if (string.IsNullOrEmpty(newSave.id))
        {
            newSave.id = filename;
        }
        string json = JsonUtility.ToJson(newSave);
        System.IO.File.WriteAllText(filename, json);
    }

    /// <summary>
    /// Example of loading a single json file.
    /// </summary>
    /// <returns></returns>
    public static ExerciseData LoadLocalExerciseDataJSON()
    {
        ExerciseData save = new ExerciseData();
        using (StreamReader r = new StreamReader(Application.persistentDataPath + "/ExerciseData.json"))
        {
            string json = r.ReadToEnd();  
            JsonUtility.FromJsonOverwrite(json, save);
            Debug.Log(json);
            r.Close();
        }
        
        return save;
    }

    /// <summary>
    /// Example of saving a binary game save file.
    /// </summary>
    /// <param name="newSave"></param>
    public static void SaveLocalExerciseData(ExerciseData newSave)
    {
        // ExerciseData newSave = new ExerciseData();
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
        bf.Serialize(file, newSave);
        file.Close();
    }

    /// <summary>
    /// Example of loading a binary game save file.
    /// </summary>
    /// <returns></returns>
    public static ExerciseData LoadLocalExerciseData()
    {
        ExerciseData loadedSave = new ExerciseData();

        // 1
        if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
        {

            // 2
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            loadedSave = (ExerciseData)bf.Deserialize(file);
            file.Close();
            return loadedSave;

        }
        else
        {
            Debug.Log("No file found!");
            return null;
        }
    }

    /// <summary>
    /// Deletes the file with the specified path / file name.
    /// </summary>
    /// <param name="path">The directory path to the file that will be delted.</param>
    public static void DeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(path, FileMode.Open);
                File.Delete(path);
            }
            else
            {
                Debug.Log("Failed to find file");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    /// <summary>
    /// Deletes the file that has been staged for deletion by the CPR Practice Manager Script.
    /// </summary>
    public static bool DeleteStagedFile()
    {
        try
        {
            if (File.Exists(stagedToDelete))
            {
                BinaryFormatter bf = new BinaryFormatter();
                File.Delete(stagedToDelete);
                return true;
            }
            else
            {
                Debug.Log("Failed to find file");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        return false;
    }

}
