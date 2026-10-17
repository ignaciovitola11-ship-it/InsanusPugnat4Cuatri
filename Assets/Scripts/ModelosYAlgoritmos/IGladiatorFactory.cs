using UnityEngine;

namespace GladiusAI
{
    public interface IGladiatorFactory
    {
        GladiatorNPC Create(GladiatorKind kind, Vector3 position, Quaternion rotation);
    }
}
