using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Envoltorio de animaciones Legacy del modelo riggeado (Idle, Walk, Run, LookAround, Stare, Attack, Peek, LieIdle, SitUp)
    /// + mirada de la cabeza hacia un punto (LateUpdate, sobre la animación) + "tics" antinaturales para la entidad.
    /// </summary>
    public class CharacterAnim : MonoBehaviour
    {
        public Animation anim;
        public Transform root, head;
        public bool twitchy;
        [HideInInspector] public string current;
        public Vector3? lookTarget;
        public float lookWeight;
        float lookCur, twitchT, twitchAngle, twitchCur;
        Quaternion headOffset; bool hasOffset;

        void Awake()
        {
            if (anim == null) anim = GetComponentInChildren<Animation>();
            if (root == null) root = transform;
            if (head == null) head = FindDeep(transform, "Head");
            if (anim != null)
                foreach (AnimationState s in anim)
                {
                    string n = Short(s.name);
                    s.wrapMode = (n == "Attack" || n == "SitUp" || n == "Peek") ? WrapMode.ClampForever : WrapMode.Loop;
                }
        }

        void Start()
        {
            if (head != null)
            {
                headOffset = Quaternion.Inverse(Quaternion.LookRotation(root.forward, Vector3.up)) * head.rotation;
                hasOffset = true;
            }
            if (string.IsNullOrEmpty(current)) Play("Idle", 0f);
        }

        public static string Short(string clip) { int i = clip.LastIndexOf('|'); return i >= 0 ? clip.Substring(i + 1) : clip; }

        string Full(string shortName)
        {
            if (anim == null) return null;
            foreach (AnimationState s in anim) if (Short(s.name) == shortName) return s.name;
            return null;
        }

        public void Play(string clip, float fade = 0.25f, float speed = 1f, bool restart = false)
        {
            if (anim == null) return;
            string f = Full(clip);
            if (f == null) return;
            if (current == clip && !restart) { anim[f].speed = speed; return; }
            current = clip;
            anim[f].speed = speed;
            if (restart) anim[f].time = 0f;
            if (fade <= 0f) anim.Play(f); else anim.CrossFade(f, fade);
        }

        public float Length(string clip) { string f = Full(clip); return f != null ? anim[f].length : 1f; }

        static Transform FindDeep(Transform t, string n)
        {
            if (t.name == n || t.name.EndsWith(":" + n)) return t;
            foreach (Transform c in t) { var r = FindDeep(c, n); if (r != null) return r; }
            return null;
        }

        void LateUpdate()
        {
            if (head == null || !hasOffset) return;
            lookCur = Mathf.MoveTowards(lookCur, lookTarget.HasValue ? lookWeight : 0f, Time.deltaTime * 1.5f);
            if (twitchy)
            {
                twitchT -= Time.deltaTime;
                if (twitchT <= 0f) { twitchT = Random.Range(0.8f, 3.5f); twitchAngle = Random.value < 0.35f ? Random.Range(-28f, 28f) : 0f; }
                twitchCur = Mathf.MoveTowards(twitchCur, twitchAngle, Time.deltaTime * 400f); // brusco
                head.rotation = Quaternion.AngleAxis(twitchCur, head.position - (root.position + Vector3.up * 1f)) * head.rotation;
            }
            if (lookCur > 0.001f && lookTarget.HasValue)
            {
                Vector3 d = lookTarget.Value - head.position;
                Vector3 flat = new Vector3(d.x, 0, d.z);
                float ang = Vector3.SignedAngle(root.forward, flat, Vector3.up);
                ang = Mathf.Clamp(ang, -120f, 120f); // la entidad puede girar demasiado el cuello
                Vector3 dir = Quaternion.AngleAxis(ang, Vector3.up) * root.forward;
                dir.y = d.normalized.y;
                Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up) * headOffset;
                head.rotation = Quaternion.Slerp(head.rotation, target, lookCur);
            }
        }
    }
}
