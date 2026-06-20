using Player;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [field: SerializeField]
    public AbstractPlayer CurrentOccupant { get; private set; }

    public bool IsEmpty => CurrentOccupant == null;

    public void Occupy(AbstractPlayer occupant)
    {
        if (occupant == null)
            return;

        CurrentOccupant = occupant;
    }

    public void Vacant()
    {
        CurrentOccupant = null;
    }
}