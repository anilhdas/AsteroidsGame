using UnityEngine;

[CreateAssetMenu]
public class GameContent : ScriptableObject
{
    public GameObject bulletPrefab;
    public GameObject missilePrefab;

    // AI
    public GameObject aIPrefab;
    public GameObject[] aIPathPrefabs;
    public GameObject genericAsteroidPrefab;
    public Sprite[] smallAsteroidSprites, largeAsteroidSprites;
    public GameObject alienShipPrefab;
}
