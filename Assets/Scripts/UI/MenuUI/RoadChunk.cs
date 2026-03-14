using UnityEngine;

public class RoadChunk : MonoBehaviour
{
    public Transform leftSlot;
    public Transform rightSlot;
    public GameObject[] housePrefabs;

    private GameObject[] _leftPool;
    private GameObject[] _rightPool;

    private GameObject _currentLeftHouse;
    private GameObject _currentRightHouse;
    public Transform entryPoint;
    public Transform exitPoint;
    private void Awake()
    {
        if (housePrefabs.Length == 0) return;

        _leftPool = new GameObject[housePrefabs.Length];
        _rightPool = new GameObject[housePrefabs.Length];

        for (int i = 0; i < housePrefabs.Length; i++)
        {
            _leftPool[i] = Instantiate(housePrefabs[i], leftSlot.position, leftSlot.rotation, leftSlot);
            _leftPool[i].SetActive(false);

            _rightPool[i] = Instantiate(housePrefabs[i], rightSlot.position, rightSlot.rotation, rightSlot);
            _rightPool[i].SetActive(false);
        }

        GenerateHouses();
    }

    public void GenerateHouses()
    {
        if (housePrefabs.Length == 0) return;

        if (_currentLeftHouse != null) _currentLeftHouse.SetActive(false);
        if (_currentRightHouse != null) _currentRightHouse.SetActive(false);

        int rndLeft = Random.Range(0, _leftPool.Length);
        _currentLeftHouse = _leftPool[rndLeft];
        _currentLeftHouse.SetActive(true);

        int rndRight = Random.Range(0, _rightPool.Length);
        _currentRightHouse = _rightPool[rndRight];
        _currentRightHouse.SetActive(true);
    }
}