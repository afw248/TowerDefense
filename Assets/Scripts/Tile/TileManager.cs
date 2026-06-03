using Player;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class TileRow
{
    public Tile[] row;
}

public class TileManager : MonoBehaviour
{
    [SerializeField] private Transform gridParent;
    [field: SerializeField] public TileRow[] TowerTile { get; private set; }
    [SerializeField] private AllPlayerListSO AlltowerSO;

    private List<Tile> occupiedTowerTile = new();

    private void Awake()
    {
        if (gridParent != null)
        {
            int rowCount = gridParent.childCount;

            TowerTile = new TileRow[rowCount];

            for (int y = 0; y < rowCount; y++)
            {
                Transform rowTransform =
                    gridParent.GetChild(y);

                TowerTile[y] = new TileRow();

                TowerTile[y].row =
                    rowTransform.GetComponentsInChildren<Tile>();
            }
        }
    }

    public void SpawnPlayerOnRandomTile()
    {
        if (TowerTile == null ||
            TowerTile.Length == 0)
        {
            Debug.LogError(
                "TileManager의 TowerTile 배열 전체가 비어있습니다!");

            return;
        }

        List<Tile> emptyTiles = new();

        for (int y = 0; y < TowerTile.Length; y++)
        {
            if (TowerTile[y] == null ||
                TowerTile[y].row == null)
            {
                continue;
            }

            for (int x = 0; x < TowerTile[y].row.Length; x++)
            {
                Tile tile = TowerTile[y].row[x];

                if (tile == null)
                    continue;

                if (tile.IsEmpty)
                {
                    emptyTiles.Add(tile);
                }
            }
        }

        if (emptyTiles.Count == 0)
        {
            Debug.LogWarning("모든 타일이 가득 찼습니다!");

            return;
        }

        int randomIndex =
            Random.Range(0, emptyTiles.Count);

        Tile targetTile =
            emptyTiles[randomIndex];

        GradeList selectedGrade =
            GetRandomGrade();

        AbstractPlayer playerPrefab =
            selectedGrade.tower[
                Random.Range(
                    0,
                    selectedGrade.tower.Count)
            ];

        Vector3 spawnPosition =
            new Vector3(
                targetTile.transform.position.x,
                playerPrefab.transform.position.y,
                targetTile.transform.position.z);

        AbstractPlayer player =
            Instantiate(
                playerPrefab,
                spawnPosition,
                Quaternion.identity);

        targetTile.Occupy(player);

        occupiedTowerTile.Add(targetTile);
    }

    private GradeList GetRandomGrade()
    {
        int totalWeight = 0;

        foreach (GradeList grade
            in AlltowerSO.towerList)
        {
            totalWeight += grade.weight;
        }

        int randomValue =
            Random.Range(0, totalWeight);

        int currentWeight = 0;

        foreach (GradeList grade
            in AlltowerSO.towerList)
        {
            currentWeight += grade.weight;

            if (randomValue < currentWeight)
            {
                return grade;
            }
        }

        return null;
    }

    public void RemovePlayerFromTile(int x, int y)
    {
        if (y >= TowerTile.Length ||
            x >= TowerTile[y].row.Length)
        {
            return;
        }

        Tile targetTile =
            TowerTile[y].row[x];

        if (targetTile != null &&
            !targetTile.IsEmpty)
        {
            Destroy(
                targetTile
                .CurrentOccupant
                .gameObject);

            targetTile.Vacant();

            occupiedTowerTile.Remove(targetTile);
        }
    }
}