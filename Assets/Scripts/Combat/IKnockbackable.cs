using UnityEngine;

namespace SlopJam.Combat
{
    public interface IKnockbackable
    {
        void ApplyKnockback(Vector3 impulse);
    }
}

