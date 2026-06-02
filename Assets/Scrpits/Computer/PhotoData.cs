using UnityEngine;

[CreateAssetMenu(fileName = "NewPhoto", menuName = "Horror/PhotoData")]
public class PhotoData : ScriptableObject
{
    public string photoName;
    public Texture2D normalVersion;
    public Texture2D demonVersion;
    [Range(0f, 1f)] public float demonChance = 0.25f; 
}