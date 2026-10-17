using System.Collections.Generic;
using UnityEngine;

namespace GladiusAI
{
    public class GladiatorPool
    {
        private readonly IGladiatorFactory factory;
        private readonly GladiatorKind kind;
        private readonly Queue<GladiatorNPC> available = new Queue<GladiatorNPC>();

        public GladiatorPool(IGladiatorFactory factory, GladiatorKind kind)
        {
            this.factory = factory;
            this.kind = kind;
        }

        public GladiatorNPC Get(Vector3 position, Quaternion rotation)
        {
            GladiatorNPC instance = available.Count > 0 ? available.Dequeue() : factory.Create(kind, position, rotation);
            if (instance == null) return null;

            instance.gameObject.SetActive(true);
            instance.TeleportTo(position);
            instance.transform.rotation = rotation;
            instance.ResetForReuse();
            return instance;
        }

        public void Release(GladiatorNPC instance)
        {
            if (instance == null) return;
            instance.gameObject.SetActive(false);
            available.Enqueue(instance);
        }
    }
}
