using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Terror psicológico de fondo (18/10): de vez en cuando, un sonido discreto y espacial cerca del jugador
    /// (crujido detrás, golpe en la pared, pasos en otra habitación, un susurro apenas audible).
    /// Solo desde el cap. 2, nunca durante cinemáticas, persecuciones, aparatos ni escondites. Sin repetir el mismo dos veces seguidas.
    /// </summary>
    public class AmbientDread : MonoBehaviour
    {
        float timer = 45f; int last = -1;

        void Update()
        {
            var sm = StoryManager.I; var pc = PlayerController.I; var am = AudioManager.I;
            if (sm == null || pc == null || am == null) return;
            if ((int)sm.stage < (int)Stage.C2_Despertar) return;
            if (PlayerController.CutsceneLock || FocusSystem.Active || InspectSystem.Active || HidingSpot.Current != null || UIManager.ModalOpen) return;
            var e = EnemyAI.I;
            bool hunting = e != null && e.gameObject.activeInHierarchy && e.State != AIState.Inactive;
            if (hunting) { timer = Mathf.Max(timer, 20f); return; }   // con la entidad suelta ya hay tensión de sobra
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = Random.Range(30f, 55f);

            int k; do k = Random.Range(0, 4); while (k == last); last = k;
            Vector3 eye = pc.EyePosition;
            Vector3 back = -pc.cam.forward; back.y = 0; back.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, back) * (Random.value < 0.5f ? 1f : -1f);
            switch (k)
            {
                case 0: am.At(am.creakNear != null ? am.creakNear : am.woodCreak, eye + back * 2.2f + Vector3.down * 1.2f, 0.45f, 10f); break;   // crujido justo detrás
                case 1: am.At(am.knockWall != null ? am.knockWall : am.knock, eye + side * 2.5f, 0.4f, 12f); break;                             // golpe en una pared
                case 2: for (int i = 0; i < 3; i++) Invoke(nameof(FarStep), 0.55f * i); break;                                                     // pasos lejos
                default: am.At(am.whisperFar != null ? am.whisperFar : am.whisper, eye + back * 1.4f, 0.18f, 6f); break;                       // susurro apenas audible
            }
        }

        void FarStep()
        {
            var pc = PlayerController.I; var am = AudioManager.I; if (pc == null || am == null) return;
            Vector3 back = -pc.cam.forward; back.y = 0; back.Normalize();
            am.PlaySteps(pc.transform.position + back * 5f + Vector3.up * 0.1f, 0.35f);
        }
    }
}
