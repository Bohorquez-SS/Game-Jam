using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Entidad con máquina de estados: Patrulla → Investiga ruido → AVISA (se detiene, gira la cabeza, grita) →
    /// Persigue → Busca → Regresa. Revisa escondites (se planta frente a la puerta, respira y se va).
    /// Navega por grafo de waypoints, abre puertas, respeta persianas. Detección: distancia + cono + línea de visión.
    /// Dos aspectos: "Silueta" (la criatura) y "Doble" (tu misma ropa y cara).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class EnemyAI : MonoBehaviour
    {
        public static EnemyAI I;
        public WaypointGraph graph;
        public Transform head;
        public GameObject creatureVisual, doubleVisual;
        public CharacterAnim creatureAnim, doubleAnim;
        public AudioSource breath;

        [Header("Velocidades")]
        public float patrolSpeed = 1.05f, investigateSpeed = 1.45f, chaseSpeed = 2.85f;
        [Header("Percepción")]
        public float fov = 110f, sight = 11f, sightFlashlightOff = 7f, hearRun = 9f, hearWalk = 3.2f, catchDistance = 0.95f;
        public bool telegraph = true;
        [Header("Persistencia (18/10)")]
        public float loseTime = 4f, searchTime = 10f;   // segundos sin verte antes de buscar · segundos buscando antes de volver a patrullar

        public AIState State { get; private set; } = AIState.Inactive;
        public int[] route;
        public System.Action OnCaught, OnReturned, OnFirstSeen;
        public string Zone => RoomZone.At(transform.position);
        public bool SeesPlayer { get; private set; }
        public float TimeSinceSeen { get; private set; } = 999f;
        public bool IsDouble => doubleVisual != null && doubleVisual.activeSelf;
        public CharacterAnim Anim => IsDouble ? doubleAnim : creatureAnim;

        CharacterController cc;
        List<int> path; int pathIdx, node = -1, routeIdx;
        Vector3 goal, lastSeen; bool hasGoal;
        float wait, stateT, stepT, stuckT, noticeT;
        float detourT, unstickT; int unstickSide = 1, stuckCount;   // anti-atascos (18/10)
        Vector3 lastPos;
        int returnNode = -1;
        DoorController pendingDoor;
        HidingSpot inspecting;

        void Awake() { I = this; cc = GetComponent<CharacterController>(); }

        // ------------------------------------------------------------ control externo
        public void Spawn(Vector3 pos, float yaw, bool asDouble, int[] patrolRoute, AIState start = AIState.Patrol)
        {
            gameObject.SetActive(true);
            Teleport(pos, yaw);
            SetLook(asDouble);
            route = patrolRoute; routeIdx = 0;
            node = graph.Nearest(pos);
            TimeSinceSeen = 999f;
            SetState(start);
            if (breath) { breath.volume = 0.55f * Settings.Sfx; if (!breath.isPlaying) breath.Play(); }
        }

        public void Teleport(Vector3 pos, float yaw)
        {
            cc.enabled = false; transform.position = pos; transform.rotation = Quaternion.Euler(0, yaw, 0); cc.enabled = true;
            lastPos = pos;
        }

        public void SetLook(bool asDouble)
        {
            if (creatureVisual) creatureVisual.SetActive(!asDouble);
            if (doubleVisual) doubleVisual.SetActive(asDouble);
        }

        /// <summary>Quieta para una escena (aparición, susto, observar desde lejos).</summary>
        public void ShowAt(Vector3 pos, float yaw, string clip = "Stare", bool asDouble = false)
        {
            gameObject.SetActive(true);
            State = AIState.Scripted;
            Teleport(pos, yaw);
            SetLook(asDouble);
            if (Anim) Anim.Play(clip, 0.1f, 1f, true);
        }

        public void LookAtPlayer(float weight)
        {
            var a = Anim; if (a == null) return;
            if (weight <= 0f || PlayerController.I == null) { a.lookTarget = null; return; }
            a.lookTarget = PlayerController.I.EyePosition; a.lookWeight = weight;
        }

        public void Despawn()
        {
            State = AIState.Inactive;
            if (breath) breath.Stop();
            OnCaught = null; OnReturned = null; OnFirstSeen = null;
            gameObject.SetActive(false);
        }

        public void Investigate(Vector3 p) { lastSeen = p; SetState(AIState.Investigate); }
        public void ReturnTo(int n) { returnNode = n; SetState(AIState.Return); }
        public void ForceChase() { lastSeen = PlayerController.I.transform.position; SetState(AIState.Chase); }

        void SetState(AIState s)
        {
            State = s; stateT = 0f; path = null; hasGoal = false; wait = 0f;
            switch (s)
            {
                case AIState.Patrol: NextPatrolTarget(); break;
                case AIState.Investigate: GoTo(lastSeen); break;
                case AIState.Search: GoTo(lastSeen); break;
                case AIState.Return: if (returnNode >= 0) GoToNode(returnNode); break;
                case AIState.Notice: noticeT = 0f; break;
            }
        }

        // ------------------------------------------------------------ bucle
        void Update()
        {
            if (State == AIState.Inactive || State == AIState.Scripted) { UpdateLookAt(); return; }
            var player = PlayerController.I;
            if (player == null) return;
            stateT += Time.deltaTime;

            SeesPlayer = CanSee(player);
            if (SeesPlayer) { TimeSinceSeen = 0f; lastSeen = player.transform.position; }
            else TimeSinceSeen += Time.deltaTime;
            float dist = Vector3.Distance(transform.position, player.transform.position);

            if (dist < 5.5f && FlashlightController.I && FlashlightController.I.IsOn && Random.value < 0.012f) FlashlightController.I.Flicker(0.25f);

            if (SeesPlayer && State != AIState.Chase && State != AIState.Notice && State != AIState.Return)
            {
                if (OnFirstSeen != null) { var f = OnFirstSeen; OnFirstSeen = null; f(); }
                SetState(telegraph ? AIState.Notice : AIState.Chase);
            }
            else if (!SeesPlayer && (State == AIState.Patrol || State == AIState.Search)) Hear(player, dist);

            switch (State)
            {
                case AIState.Patrol:
                    if (!Follow(patrolSpeed)) { if (wait <= 0f) wait = Random.Range(1.5f, 4f); wait -= Time.deltaTime; PlayAnim("LookAround"); if (wait <= 0.05f) NextPatrolTarget(); }
                    else PlayAnim("Walk", 0.25f, IsDouble ? 0.9f : 0.75f);
                    CheckHideSpots(player);
                    break;
                case AIState.Investigate:
                    if (!Follow(investigateSpeed)) { PlayAnim("LookAround"); if (stateT > 1f && wait <= 0f) wait = 3f; wait -= Time.deltaTime; if (wait <= 0.05f && stateT > 2f) SetState(AIState.Search); }
                    else PlayAnim("Walk", 0.25f, 1.1f);
                    CheckHideSpots(player);
                    break;
                case AIState.Notice:
                    noticeT += Time.deltaTime;
                    FaceTowards(player.transform.position, 260f);
                    PlayAnim("Stare", 0.15f);
                    LookAtPlayer(1f);
                    if (noticeT < 0.05f && AudioManager.I) AudioManager.I.At(Pick(AudioManager.I.entityScream, AudioManager.I.sting), head ? head.position : transform.position, 0.9f);
                    if (noticeT > 1.25f) { LookAtPlayer(0f); SetState(AIState.Chase); }
                    break;
                case AIState.Chase:
                    PlayAnim("Run", 0.2f, 1f);
                    Chase(player, dist);
                    break;
                case AIState.Search:
                    if (!Follow(investigateSpeed)) { PlayAnim("LookAround"); if (stateT > searchTime) SetState(route != null && route.Length > 0 ? AIState.Patrol : AIState.Return); else if (Random.value < 0.008f) GoTo(lastSeen + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f))); }
                    else PlayAnim("Walk", 0.25f, 1.1f);
                    CheckHideSpots(player);
                    break;
                case AIState.InspectHide:
                    InspectHide(player);
                    break;
                case AIState.Return:
                    if (!Follow(patrolSpeed * 1.2f) && stateT > 0.5f) { State = AIState.Inactive; if (OnReturned != null) OnReturned(); }
                    else PlayAnim("Walk", 0.25f, 0.9f);
                    break;
            }

            bool hidden = HidingSpot.Current != null;
            if (State == AIState.Chase && !hidden && dist < catchDistance) Caught();
            if (State == AIState.Chase && hidden && Time.time - HidingSpot.HiddenSince < 0.8f && dist < 3.2f && TimeSinceSeen < 0.6f) Caught();

            float moved = (transform.position - lastPos).magnitude;
            stepT += moved;
            if (stepT > (State == AIState.Chase ? 0.9f : 0.7f)) { stepT = 0f; if (AudioManager.I) AudioManager.I.PlaySteps(transform.position, State == AIState.Chase ? 1f : 0.75f, !IsDouble); }
            lastPos = transform.position;
            UpdateLookAt();
        }

        void PlayAnim(string c, float fade = 0.25f, float speed = 1f) { var a = Anim; if (a != null) a.Play(c, fade, speed); }

        void UpdateLookAt()
        {
            var a = Anim; if (a == null || PlayerController.I == null) return;
            if (a.lookTarget.HasValue) a.lookTarget = PlayerController.I.EyePosition;
        }

        void Caught()
        {
            State = AIState.Scripted;
            var f = OnCaught; OnCaught = null;
            if (f != null) f();
        }

        void Hear(PlayerController p, float dist)
        {
            if (HidingSpot.Current != null) return;
            float range = p.Noise >= 2f ? hearRun : (p.Noise >= 1f ? hearWalk : 0f);
            if (range > 0f && dist < range) { lastSeen = p.transform.position; SetState(AIState.Investigate); }
        }

        bool CanSee(PlayerController p)
        {
            if (HidingSpot.Current != null) return false;
            Vector3 eye = head ? head.position : transform.position + Vector3.up * 1.75f;
            Vector3 target = p.EyePosition;
            Vector3 d = target - eye;
            float range = (FlashlightController.I != null && FlashlightController.I.IsOn) ? sight : sightFlashlightOff;
            if (p.IsCrouching) range *= 0.7f;
            if (d.magnitude > range) return false;
            Vector3 flat = new Vector3(d.x, 0, d.z);
            if (Vector3.Angle(transform.forward, flat) > fov * 0.5f && d.magnitude > 1.4f) return false;
            RaycastHit hit;
            if (Physics.Linecast(eye, target, out hit, ~0, QueryTriggerInteraction.Ignore))
                return hit.collider.GetComponentInParent<PlayerController>() != null;
            return true;
        }

        void Chase(PlayerController p, float dist)
        {
            if (!SeesPlayer && TimeSinceSeen > loseTime) { SetState(AIState.Search); return; }
            detourT -= Time.deltaTime;
            if (SeesPlayer && dist < 7f && detourT <= 0f)
            {
                path = null; hasGoal = false;
                MoveToward(p.transform.position, chaseSpeed);
                // en línea recta contra una silla/mesa: rodear por el grafo un rato en vez de empujar para siempre
                if (stuckT > 0.8f) { stuckT = 0f; detourT = 2.5f; GoTo(p.transform.position); }
            }
            else
            {
                if ((path == null && !hasGoal) || stateT % 1.5f < Time.deltaTime) GoTo(lastSeen);
                if (!Follow(chaseSpeed)) GoTo(lastSeen);
            }
        }

        // ------------------------------------------------------------ escondites
        void CheckHideSpots(PlayerController p)
        {
            var spot = HidingSpot.Current;
            if (spot == null || inspecting == spot) return;
            float d = Vector3.Distance(transform.position, spot.outside.position);
            // revisa si te vio entrar hace poco o si pasa cerca por casualidad
            bool suspicious = Time.time - HidingSpot.HiddenSince < 6f && (lastSeen - spot.outside.position).magnitude < 3f;
            if (d < 3.5f && (suspicious || Random.value < 0.004f))
            {
                inspecting = spot; SetState(AIState.InspectHide);
                GoTo(spot.outside.position + spot.outside.forward * 0.2f);
            }
        }

        void InspectHide(PlayerController p)
        {
            var spot = inspecting;
            if (spot == null) { SetState(AIState.Search); return; }
            if (Follow(investigateSpeed * 0.8f)) { PlayAnim("Walk", 0.3f, 0.8f); return; }
            // plantada frente al escondite
            FaceTowards(spot.transform.position, 120f);
            PlayAnim("Stare", 0.3f);
            LookAtPlayer(0.6f);
            if (wait <= 0f) { wait = Random.Range(4.5f, 6.5f); if (AudioManager.I) AudioManager.I.At(Pick(AudioManager.I.breathClose, AudioManager.I.breath), transform.position + Vector3.up * 1.6f, 1f, 6f); GameEvents.Raise("ai_inspecting_hide"); }
            wait -= Time.deltaTime;
            if (HidingSpot.Current == null && Vector3.Distance(transform.position, p.transform.position) < 2.2f
                && RoomZone.At(transform.position) == RoomZone.At(p.transform.position)) { Caught(); return; }
            if (wait <= 0.05f)
            {
                LookAtPlayer(0f);
                inspecting = null; wait = 0f;
                SetState(route != null && route.Length > 0 ? AIState.Patrol : AIState.Search);
                GameEvents.Raise("ai_left_hide");
            }
        }

        /// <summary>¿Está la entidad justo delante de este escondite?</summary>
        /// (18/10) Solo cuenta si está de verdad ahí: misma habitación, cerca de la salida y buscando/persiguiendo.
        /// Antes bastaba con estar a 2,2 m aunque fuera del otro lado de la pared o de paso → muertes injustas al salir.
        public bool Guarding(HidingSpot s)
        {
            if (s == null || !gameObject.activeInHierarchy) return false;
            if (State == AIState.Inactive || State == AIState.Scripted || State == AIState.Return) return false;
            float d = Flat(transform.position - s.outside.position).magnitude;
            if (d > 1.9f) return false;
            if (RoomZone.At(transform.position) != RoomZone.At(s.outside.position)) return false;
            if (State == AIState.InspectHide) return inspecting == s;
            if (State == AIState.Chase || State == AIState.Search || State == AIState.Investigate || State == AIState.Notice) return true;
            return d < 1.1f;   // patrullando: solo si está literalmente delante de la puerta
        }

        // ------------------------------------------------------------ navegación
        void NextPatrolTarget()
        {
            wait = 0f;
            if (route != null && route.Length > 0) { routeIdx = (routeIdx + 1) % route.Length; GoToNode(route[routeIdx]); }
            else GoToNode(Random.Range(0, graph.nodes.Length));
        }

        void GoToNode(int n)
        {
            node = graph.Nearest(transform.position);
            path = graph.Path(node, n) ?? new List<int>();
            pathIdx = 0; hasGoal = false;
        }

        void GoTo(Vector3 p)
        {
            node = graph.Nearest(transform.position);
            path = graph.Path(node, graph.Nearest(p)) ?? new List<int>();
            pathIdx = 0; goal = p; hasGoal = true;
        }

        bool Follow(float speed)
        {
            if (pendingDoor != null)
            {
                wait -= Time.deltaTime;
                if (wait > 0f) return true;
                pendingDoor = null; wait = 0f;
            }
            if (path != null && pathIdx < path.Count)
            {
                int next = path[pathIdx];
                if (node >= 0)
                {
                    var door = graph.DoorBetween(node, next);
                    if (door != null && !door.isOpen && !door.locked) { door.SetOpen(true); pendingDoor = door; wait = 1.0f; PlayAnim("Idle", 0.2f); return true; }
                    int e = graph.Edge(node, next);
                    if (e >= 0 && graph.Blocked(e)) { path = null; return false; }
                }
                Vector3 t = graph.Pos(next);
                if (Flat(t - transform.position).magnitude < 0.35f) { node = next; pathIdx++; stuckCount = 0; return true; }
                if (stuckT > 1.0f)
                {
                    // atascado camino a un punto: primero se desliza de costado; si sigue, salta al punto siguiente / recalcula
                    stuckT = 0f; stuckCount++;
                    unstickT = 0.7f; unstickSide = -unstickSide;
                    if (stuckCount >= 3) { stuckCount = 0; if (Flat(t - transform.position).magnitude < 1.2f) { node = next; pathIdx++; } else { node = graph.Nearest(transform.position); path = graph.Path(node, path[path.Count - 1]) ?? new List<int>(); pathIdx = 0; } }
                }
                MoveToward(t, speed);
                return true;
            }
            if (hasGoal)
            {
                if (Flat(goal - transform.position).magnitude < 0.5f || stuckT > 1.5f) { hasGoal = false; stuckT = 0f; return false; }
                MoveToward(goal, speed);
                return true;
            }
            return false;
        }

        void FaceTowards(Vector3 p, float degPerSec)
        {
            Vector3 d = Flat(p - transform.position);
            if (d.sqrMagnitude < 0.001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(d), degPerSec * Time.deltaTime);
        }

        void MoveToward(Vector3 t, float speed)
        {
            Vector3 d = Flat(t - transform.position);
            if (d.sqrMagnitude < 0.0001f) return;
            FaceTowards(t, 240f);
            Vector3 before = transform.position;
            if (unstickT > 0f)
            {
                unstickT -= Time.deltaTime;
                cc.SimpleMove(Quaternion.Euler(0, 75f * unstickSide, 0) * d.normalized * speed * 0.8f);
                stuckT = 0f;
                return;
            }
            cc.SimpleMove(transform.forward * speed * Mathf.Clamp01(1.2f - Vector3.Angle(transform.forward, d) / 120f));
            float progress = (transform.position - before).magnitude;
            stuckT = progress < speed * Time.deltaTime * 0.2f ? stuckT + Time.deltaTime : 0f;
        }

        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
        static AudioClip Pick(AudioClip a, AudioClip b) => a != null ? a : b;
    }
}
