using UnityEngine;

namespace GNight
{
    /// <summary>Solo en el editor / builds de desarrollo: F1–F8 saltan al punto de control de cada tramo (para probar rápido).</summary>
    public class DebugJump : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        static readonly Stage[] stages = { Stage.C1_Noche, Stage.C2_Despertar, Stage.C2_Camaras, Stage.C2_Persecucion, Stage.C3_Cintas, Stage.C3_Persecucion, Stage.C4_Protocolo, Stage.C5_Final };
        static readonly KeyCode[] keys = { KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8 };
        void Awake() { if (GetComponent<DebugAutoPlay>() == null) gameObject.AddComponent<DebugAutoPlay>(); }

        void Update()
        {
            for (int i = 0; i < keys.Length; i++)
                if (Input.GetKeyDown(keys[i])) { Debug.Log("[GOOD NIGHT] Salto de prueba a " + stages[i]); Checkpoint.ReloadAt(stages[i]); }
        }
#endif
    }
}
