using SlopJam.Core;
using UnityEngine;

namespace SlopJam.Combat
{
    /// <summary>
    /// Central damage application service so different actors share rules and VFX hooks.
    /// </summary>
    public class DamageSystem : MonoBehaviour
    {
        public bool ApplyDamage(DamageRequest request)
        {
            if (request.Target == null)
            {
                Debug.LogWarning("DamageRequest has no target.");
                return false;
            }

            return request.Target.ApplyDamage(request);
        }
    }
}

