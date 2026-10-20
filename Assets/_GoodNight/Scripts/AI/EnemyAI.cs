using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Entidad: máquina de estados Inactivo → Patrulla → Investiga sonido → Persigue → Busca → Regresa.
    /// Navega con el grafo de waypoints, abre puertas, respeta persianas y colisiones.
    /// Detecta al jugador por distancia + campo de visión + línea de visión; también por ruido.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class EnemyAI : MonoBehaviour
    {
        public static EnemyAI I;
        public WaypointGraph graph;
        public Transform head;
        public GameObject silhouetteVisual, doppelVisual;
        public AudioSource breath;

        [Header("Velocidades")]
        public float patrolSpeed = 1.1f, investigateSpeed = 1.6f, chaseSpeed = 3.0f;
        [Header("Percepción")]
        public float fov = 110f, sight = 10f, sightFlashlightOff = 6f, hearRun = 9f, hearWalk = 3.5f, catchDistance = 0.9f;

        public AIState State { get; private set; } = AIState.Inactive;
        public int[] route;
        public System.Action OnCaught;
        public System.Action OnReturned;
        public string Zone => RoomZone.At(transform.position);
        public bool SeesPlayer { get; private set; }
        public float TimeSinceSeen { get; private set; } = 999f;

        CharacterController cc;
        List<int> path; int pathIdx = 0, node = -1, routeIdx;
        Vector3 goal; bool hasGoal;
        float wait, stateT, stepT, stuckT, twitchT;
        Vector3 lastPos, lastSeen;
        int returnNode = -1;
        DoorController pendingDoor;

        void Awake() { I = this; cc = GetComponent<CharacterController>(); }

        // ------------------------------------------------------------ control externo
        public void Spawn(Vector3 pos, float yaw, bool doppel, int[] patrolRoute)
        {
            gameObject.SetActive(true);
            cc.enabled = false; transform.position = pos; transform.rotation = Quaternion.Euler(0, yaw, 0); cc.enabled = true;
            if (silhouetteVisual) silhouetteVisual.SetActive(!doppel);
            if (doppelVisual) doppelVisual.SetActive(doppel);
            route = patrolRoute; routeIdx = 0;
            node = graph.Nearest(pos);
            SetState(AIState.Patrol);
            if (breath) { breath.volume = 0.6f * Settings.Sfx; if (!breath.isPlaying) breath.Play(); }
        }

        /// <summary>Muestra a la entidad quieta (susto/muerte), sin IA.</summary>
        public void ShowAt(Vector3 pos, float yaw)
        {
            gameObject.SetActive(true);
            State = AIState.Inactive;
            cc.enabled = false; transform.position = pos; transform.rotation = Quaternion.Euler(0, yaw, 0); cc.enabled = true;
            if (silhouetteVisual && doppelVisual && !silhouetteVisual.activeSelf && !doppelVisual.activeSelf) silhouetteVisual.SetActive(true);
        }

        public void Despawn()
        {
            State = AIState.Inactive;
            if (breath) breath.Stop();
            gameObject.SetActive(false);
        }

        /// <summary>Va a investigar un punto (ruido, aislamiento fallido...).</summary>
        public void Investigate(Vector3 p) { lastSeen = p; SetState(AIState.Investigate); }

        /// <summary>Regresa a un nodo y avisa al llegar.</summary>
        public void ReturnTo(int n) { returnNode = n; SetState(AIState.Return); }

        void SetState(AIState s)
        {
            State = s; stateT = 0f; path = null; hasGoal = false; wait = 0f;
            switch (s)
            {
                case AIState.Patrol: NextPatrolTarget(); break;
                case AIState.Investigate: GoTo(lastSeen); break;
                case AIState.Search: GoTo(lastSeen); break;
                case AIState.Return: if (returnNode >= 0) GoToNode(returnNode); break;
            }
        }

        // ------------------------------------------------------------ bucle
        void Update()
        {
            if (State == AIState.Inactive) return;
            var player = PlayerController.I;
            if (player == null) return;
            stateT += Time.deltaTime;

            SeesPlayer = CanSee(player);
            if (SeesPlayer) { TimeSinceSeen = 0f; lastSeen = player.transform.position; }
            else TimeSinceSeen += Time.deltaTime;

            // Flicker de la linterna si está cerca
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < 5f && FlashlightController.I && FlashlightController.I.IsOn && Random.value < 0.01f) FlashlightController.I.Flicker(0.25f);

            if (SeesPlayer && State != AIState.Chase && State != AIState.Return) SetState(AIState.Chase);
            else if (!SeesPlayer && (State == AIState.Patrol || State == AIState.Search || State == AIState.Return)) Hear(player, dist);

            switch (State)
            {
                case AIState.Patrol:
                    if (!Follow(patrolSpeed)) { if (wait <= 0f) wait = Random.Range(1.5f, 4.5f); wait -= Time.deltaTime; Twitch(); if (wait <= 0.05f) NextPatrolTarget(); }
                    break;
                case AIState.Investigate:
                    if (!Follow(investigateSpeed)) { Twitch(); if (stateT > 1f && wait <= 0f) wait = 3f; wait -= Time.deltaTime; if (wait <= 0.05f && stateT > 2f) SetState(AIState.Search); }
                    break;
                case AIState.Chase:
                    Chase(player, dist);
                    break;
                case AIState.Search:
                    if (!Follow(investigateSpeed)) { Twitch(); if (stateT > 9f) SetState(route != null && route.Length > 0 ? AIState.Patrol : AIState.Return); else if (Random.value < 0.01f) GoTo(lastSeen + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f))); }
                    break;
                case AIState.Return:
                    if (!Follow(patrolSpeed * 1.2f) && stateT > 0.5f) { State = AIState.Inactive; if (OnReturned != null) OnReturned(); }
                    break;
            }

            // atrapado
            bool hidden = HidingSpot.Current != null;
            if (State == AIState.Chase && !hidden && dist < catchDistance) { State = AIState.Inactive; if (OnCaught != null) OnCaught(); }
            // te vio esconderte
            if (State == AIState.Chase && hidden && Time.time - HidingSpot.HiddenSince < 0.6f && dist < 3f && TimeSinceSeen < 0.5f)
            { State = AIState.Inactive; if (OnCaught != null) OnCaught(); }

            // pasos
            float moved = (transform.position - lastPos).magnitude;
            stepT += moved;
            if (stepT > 0.75f) { stepT = 0f; if (AudioManager.I) AudioManager.I.PlaySteps(transform.position, State == AIState.Chase ? 1f : 0.7f); }
            lastPos = transform.position;
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
            Vector3 eye = head ? head.position : transform.position + Vector3.up * 1.8f;
            Vector3 target = p.EyePosition;
            Vector3 d = target - eye;
            float range = (FlashlightController.I != null && FlashlightController.I.IsOn) ? sight : sightFlashlightOff;
            if (p.IsCrouching) range *= 0.75f;
            if (d.magnitude > range) return d.magnitude < 1.3f;
            Vector3 flat = new Vector3(d.x, 0, d.z);
            if (Vector3.Angle(transform.forward, flat) > fov * 0.5f && d.magnitude > 1.3f) return false;
            RaycastHit hit;
            if (Physics.Linecast(eye, target, out hit, ~0, QueryTriggerInteraction.Ignore))
                return hit.collider.GetComponent<PlayerController>() != null;
            return true;
        }

        void Chase(PlayerController p, float dist)
        {
            if (!SeesPlayer && TimeSinceSeen > 3.5f) { SetState(AIState.Search); return; }
            if (SeesPlayer && dist < 7f)
            {
                path = null; hasGoal = false;
                MoveToward(p.transform.position, chaseSpeed);
                if (stuckT > 1.2f) { stuckT = 0f; GoTo(p.transform.position); }
            }
            else
            {
                if ((path == null && !hasGoal) || stateT % 1.5f < Time.deltaTime) GoTo(lastSeen);
                if (!Follow(chaseSpeed)) GoTo(lastSeen);
            }
        }

        // ------------------------------------------------------------ navegación
        void NextPatrolTarget()
        {
            wait = 0f;
            if (route != null && route.Length > 0)
            {
                routeIdx = (routeIdx + 1) % route.Length;
                GoToNode(route[routeIdx]);
            }
            else GoToNode(Random.Range(0, graph.nodes.Length));
        }

        void GoToNode(int n)
        {
            if (node < 0) node = graph.Nearest(transform.position);
            path = graph.Path(node, n);
            pathIdx = 0; hasGoal = false;
            if (path == null) path = new List<int>();
        }

        void GoTo(Vector3 p)
        {
            node = graph.Nearest(transform.position);
            int target = graph.Nearest(p);
            path = graph.Path(node, target) ?? new List<int>();
            pathIdx = 0; goal = p; hasGoal = true;
        }

        /// <summary>Sigue el camino actual. Devuelve false cuando llegó.</summary>
        bool Follow(float speed)
        {
            if (pendingDoor != null)
            {
                wait -= Time.deltaTime;
                if (wait > 0f) return true;
                pendingDoor = null;
            }
            if (path != null && pathIdx < path.Count)
            {
                int next = path[pathIdx];
                if (node >= 0)
                {
                    var door = graph.DoorBetween(node, next);
                    if (door != null && !door.isOpen && !door.locked)
                    {
                        door.SetOpen(true); pendingDoor = door; wait = 0.9f; return true;
                    }
                    int e = graph.Edge(node, next);
                    if (e >= 0 && graph.Blocked(e)) { path = null; return false; }
                }
                Vector3 t = graph.Pos(next);
                if (Flat(t - transform.position).magnitude < 0.35f) { node = next; pathIdx++; return true; }
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

        void MoveToward(Vector3 t, float speed)
        {
            Vector3 d = Flat(t - transform.position);
            if (d.sqrMagnitude < 0.0001f) return;
            Quaternion look = Quaternion.LookRotation(d);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 220f * Time.deltaTime);
            Vector3 before = transform.position;
            cc.SimpleMove(transform.forward * speed * Mathf.Clamp01(1.2f - Vector3.Angle(transform.forward, d) / 120f));
            float progress = (transform.position - before).magnitude;
            stuckT = progress < speed * Time.deltaTime * 0.2f ? stuckT + Time.deltaTime : 0f;
        }

        void Twitch()
        {
            twitchT -= Time.deltaTime;
            if (twitchT > 0f) return;
            twitchT = Random.Range(0.6f, 2.2f);
            transform.Rotate(0, Random.Range(-70f, 70f), 0);
        }

        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
    }
}
