using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    GameMusic,

    ButtonClick,
    BusArrive,
    BusDepart,
    LevelWin,
    LevelFail,
    PassangerClick,
    PassangerAngryClick
}

[System.Serializable]
public class SoundRecord
{
    public SoundType soundType;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Random Pitch (For repetitive sfx)")]
    public bool useRandomPitch = false;
    [Range(0.5f, 1.5f)] public float minPitch = 0.9f;
    [Range(0.5f, 1.5f)] public float maxPitch = 1.1f;
}

[CreateAssetMenu(fileName = "NewSoundCatalog", menuName = "BusJam/Sound Catalog")]
public class SoundCatalogSO : ScriptableObject
{
    public List<SoundRecord> sounds = new();

    public SoundRecord GetSound(SoundType type)
    {
        return sounds.Find(s => s.soundType == type);
    }
}