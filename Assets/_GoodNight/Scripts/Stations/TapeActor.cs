using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Doble que actúa dentro de una cinta: recorre puntos y reproduce animaciones.</summary>
    public class TapeActor : MonoBehaviour
    {
        public CharacterAnim anim;
        public Transform[] points;
        public float[] waits;
        public string walkClip = "Walk", idleClip = "Idle", endClip = "Stare";
        public bool endLookAtCamera = true;
        public Transform cameraToLook;
        public IEnumerator Run()
        {
            if (points == null || points.Length == 0) yield break;
            transform.position = points[0].position; transform.rotation = points[0].rotation;
            for (int i = 1; i < points.Length; i++)
            {
                if (waits != null && i - 1 < waits.Length && waits[i - 1] > 0f) { anim.Play(idleClip, 0.3f); yield return new WaitForSeconds(waits[i - 1]); }
                Vector3 a = transform.position, b = points[i].position;
                Quaternion ra = transform.rotation, rb = Quaternion.LookRotation(Flat(b - a).sqrMagnitude > 0.001f ? Flat(b - a) : transform.forward);
                float dur = Mathf.Max(0.3f, (b - a).magnitude / 1.1f);
                anim.Play(walkClip, 0.3f, 0.9f);
                for (float t = 0; t < dur; t += Time.deltaTime)
                {
                    transform.position = Vector3.Lerp(a, b, t / dur);
                    transform.rotation = Quaternion.Slerp(ra, rb, Mathf.Clamp01(t / 0.4f));
                    yield return null;
                }
                transform.position = b;
                transform.rotation = points[i].rotation;
            }
            anim.Play(endClip, 0.3f);
            if (endLookAtCamera && cameraToLook) { anim.lookTarget = cameraToLook.position; anim.lookWeight = 1f; }
        }
        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
    }
}
