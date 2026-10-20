using System.Linq;
using System.Text;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Herramientas de desarrollo. SOLO existen en el editor y en builds marcadas como "Development Build"
    /// (en la build normal para jugadores este código no se compila).
    ///   F1–F8  saltar al punto de control de cada tramo        F9  reiniciar el punto de control actual
    ///   F10    panel de depuración (capítulo, objetivo, eventos, puzles, puertas, entidad)
    ///   F11/F12 recorrido automático hasta el final A / B (DebugAutoPlay)
    /// </summary>
    public class DebugJump : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        static readonly Stage[] stages = { Stage.C1_Noche, Stage.C2_Despertar, Stage.C2_Camaras, Stage.C2_Persecucion, Stage.C3_Cintas, Stage.C3_Persecucion, Stage.C4_Protocolo, Stage.C5_Final };
        static readonly KeyCode[] keys = { KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8 };
        bool panel; GUIStyle style; float nextText; string text = "";

        void Awake() { if (GetComponent<DebugAutoPlay>() == null) gameObject.AddComponent<DebugAutoPlay>(); }

        void Update()
        {
            for (int i = 0; i < keys.Length; i++)
                if (Input.GetKeyDown(keys[i])) { Debug.Log("[GOOD NIGHT] Salto de prueba a " + stages[i]); Checkpoint.ReloadAt(stages[i]); }
            if (Input.GetKeyDown(KeyCode.F9) && StoryManager.I != null && StoryManager.I.stage != Stage.None)
            {
                Debug.Log("[GOOD NIGHT] Reinicio del punto de control " + StoryManager.I.stage);
                Checkpoint.ReloadAt(StoryManager.I.stage);
            }
            if (Input.GetKeyDown(KeyCode.F10)) panel = !panel;
            if (panel && Time.unscaledTime >= nextText) { nextText = Time.unscaledTime + 0.25f; text = Build(); }
        }

        static string OC(DoorController d) => d == null ? "-" : (d.isOpen ? "abierta" : "cerrada") + (d.locked ? "+llave" : "");

        string Build()
        {
            var sb = new StringBuilder();
            var sm = StoryManager.I;
            sb.AppendLine("<b>GOOD NIGHT · DEBUG</b>  (F10 ocultar · F9 reiniciar tramo · F1–F8 saltar)");
            sb.AppendLine("Etapa: " + (sm ? sm.stage.ToString() : "-") + "   Guardado: " + Checkpoint.Saved + "   Hora: " + GameClock.Text);
            var om = ObjectiveManager.I;
            if (om != null && !string.IsNullOrEmpty(om.Title))
            {
                sb.AppendLine("Objetivo: " + om.Title + "   pistas " + om.HintsShown + "/" + om.HintLevels);
                for (int i = 0; i < om.Items.Count; i++) sb.AppendLine("   [" + (om.Done[i] ? "x" : " ") + "] " + om.Items[i]);
            }
            if (sm != null)
            {
                var f = sm.Flags.OrderBy(x => x).ToArray();
                sb.AppendLine("Eventos (" + f.Length + "): " + string.Join(", ", f));
                sb.AppendLine("Verdades: " + sm.TruthCount + "/4");
            }
            var r = GameRefs.I;
            if (r != null)
            {
                sb.AppendLine("Puertas: principal " + OC(r.doorFront) + (r.bolt ? (r.bolt.closed ? " (cerrojo puesto)" : " (cerrojo abierto)") : "")
                              + " · dormitorio " + OC(r.doorBedroom) + " · baño " + OC(r.doorBathroom) + " · cocina " + OC(r.doorKitchen) + " · 06 " + OC(r.doorMetal));
                if (r.windows != null) sb.AppendLine("Ventanas cerradas: " + r.windows.Count(w => w != null && !w.isOpen) + "/" + r.windows.Length);
            }
            var pp = PowerPanel.I;
            if (pp != null) sb.AppendLine("Energía: " + pp.DebugSummary());
            var e = EnemyAI.I;
            sb.AppendLine("Entidad: " + (e != null && e.gameObject.activeInHierarchy ? e.State + " en " + e.transform.position.ToString("F1") : "inactiva")
                          + "   Escondido: " + (HidingSpot.Current != null ? HidingSpot.Current.name : "no"));
            return sb.ToString();
        }

        void OnGUI()
        {
            if (!panel) return;
            if (style == null) style = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 14, wordWrap = true };
            float w = Mathf.Min(760, Screen.width - 20);
            GUI.color = new Color(0, 0, 0, 0.75f);
            GUI.DrawTexture(new Rect(10, Screen.height - 330, w, 320), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(18, Screen.height - 325, w - 16, 312), text, style);
        }
#endif
    }
}
