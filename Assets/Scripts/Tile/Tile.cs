using Player;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public AbstractPlayer CurrentOccupant { get; private set; }

    // 타일에 누군가 진입했을 때 호출
    public void Occupy(AbstractPlayer occupant)
    {
        CurrentOccupant = occupant;
    }
    public void Vacant()
    {
        CurrentOccupant = null;
    }
    public bool IsEmpty => CurrentOccupant == null;

}
