using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem
{
    public static void Save(GameData game)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/GameData.blin";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(game);
        
        formatter.Serialize(stream, data);
        stream.Close();
    }
    public static SaveData Load()
    {
        string path = Application.persistentDataPath + "/GameData.blin";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();
            return data;
        }
        else
        {
            return null;
        }
    }
}

[System.Serializable]
public class SaveData
{
    public TankBuildTemp TankBuild;

    public SaveData(GameData game)
    {
        TankBuild = game.PlayersTank;
    }
}