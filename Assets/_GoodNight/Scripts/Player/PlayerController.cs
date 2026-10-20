using UnityEngine;

namespace GNight
{
    /// <summary>Controlador FPS con CharacterController: caminar, correr, agacharse, mirar y emitir ruido.</summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController I;
        public static bool CutsceneLock;   // cinemáticas de la historia
        public static bool TransitionLock; // transiciones cortas (escondites)
        public static bool MoveBlocked => CutsceneLock || TransitionLock || UIManager.ModalOpen;

        public Transform cam;
        public float walkSpeed = 2.0f, runSpeed = 3.7f, crouchSpeed = 1.0f, gravity = -15f;
        public float standEye = 1.6f, crouchEye = 0.92f;
        public AudioSource footSource;
        public AudioClip[] footsteps;
        public float stepDistance = 0.7f;

        [HideInInspector] public float LookLimit;
        public bool IsCrouching { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsMoving { get; private set; }
        /// <summary>0 = silencio, 0.35 agachado, 1 caminando, 2.2 corriendo.</summary>
        public float Noise { get; private set; }
        public Vector3 EyePosition => cam.position;

        CharacterController cc;
        float yaw, pitch, baseYaw, vy, eyeOverride = -1f, curEye, bobT, stepAcc;

        void Awake()
        {
            I = this;
            cc = GetComponent<CharacterController>();
            yaw = baseYaw = transform.eulerAngles.y;
            curEye = standEye;
            TransitionLock = false;
        }

        public void SetPose(Vector3 position, float newYaw, float newPitch, float eye)
        {
            cc.enabled = false;
            transform.position = position;
            yaw = baseYaw = newYaw; pitch = newPitch;
            eyeOverride = eye; curEye = eye > 0 ? eye : standEye; vy = 0;
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            ApplyCamera(0f, true);
            cc.enabled = true;
        }

        public void SetPose(Transform t, float eye)
        {
            float p = t.eulerAngles.x; if (p > 180f) p -= 360f;
            SetPose(t.position, t.eulerAngles.y, p, eye);
        }

        /// <summary>Gira la cámara suavemente hacia un punto (cinemáticas).</summary>
        public void LookAtPoint(Vector3 target, float t)
        {
            Vector3 d = target - cam.position;
            float targetYaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            float targetPitch = -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg;
            yaw = Mathf.LerpAngle(yaw, targetYaw, t); baseYaw = yaw;
            pitch = Mathf.Lerp(pitch, targetPitch, t);
        }

        void Update()
        {
            bool lookAllowed = !TransitionLock && !UIManager.ModalOpen && Cursor.lockState == CursorLockMode.Locked;
            if (lookAllowed && !CutsceneLockLook)
            {
                Vector2 d = Inp.Look() * Settings.Sensitivity;
                yaw += d.x;
                pitch = Mathf.Clamp(pitch - d.y, -80f, 80f);
                if (LookLimit > 0)
                {
                    yaw = baseYaw + Mathf.Clamp(Mathf.DeltaAngle(baseYaw, yaw), -LookLimit, LookLimit);
                    pitch = Mathf.Clamp(pitch, -35f, 35f);
                }
            }
            transform.rotation = Quaternion.Euler(0, yaw, 0);

            Vector3 move = Vector3.zero;
            IsMoving = false; IsRunning = false; Noise = 0f;
            bool canMove = !MoveBlocked && HidingSpot.Current == null;
            if (canMove)
            {
                Vector2 i = Inp.Move();
                if (i.sqrMagnitude > 1f) i.Normalize();
                IsCrouching = Inp.Crouch();
                IsMoving = i.sqrMagnitude > 0.01f;
                IsRunning = IsMoving && !IsCrouching && Inp.Run();
                float speed = IsCrouching ? crouchSpeed : (IsRunning ? runSpeed : walkSpeed);
                move = (transform.right * i.x + transform.forward * i.y) * speed;
                if (IsMoving) { eyeOverride = -1f; Noise = IsCrouching ? 0.35f : (IsRunning ? 2.2f : 1f); }

                if (cc.isGrounded && vy < 0) vy = -1f;
                vy += gravity * Time.deltaTime;
                Vector3 v = move; v.y = vy;
                cc.Move(v * Time.deltaTime);

                float h = IsCrouching ? 1.15f : 1.7f;
                if (!IsCrouching && cc.height < 1.69f && Physics.Raycast(transform.position + Vector3.up * cc.height, Vector3.up, 1.75f - cc.height, ~0, QueryTriggerInteraction.Ignore))
                    h = cc.height; // no levantarse bajo un mueble
                cc.height = Mathf.MoveTowards(cc.height, h, Time.deltaTime * 3f);
                cc.center = new Vector3(0, cc.height * 0.5f, 0);

                if (IsMoving && cc.isGrounded)
                {
                    stepAcc += move.magnitude * Time.deltaTime;
                    if (stepAcc >= stepDistance) { stepAcc = 0f; Step(IsCrouching ? 0.15f : (IsRunning ? 0.75f : 0.45f)); }
                }
            }
            ApplyCamera(IsMoving ? move.magnitude : 0f, false);
        }

        /// <summary>Si es true, la cinemática controla la mirada.</summary>
        public static bool CutsceneLockLook;

        void ApplyCamera(float speed, bool snap)
        {
            float target = eyeOverride > 0 ? eyeOverride : (IsCrouching ? crouchEye : standEye);
            curEye = snap ? curEye : Mathf.MoveTowards(curEye, target, Time.deltaTime * 2.2f);
            if (speed > 0.1f) bobT += Time.deltaTime * speed * 3.2f;
            float bob = Mathf.Sin(bobT) * 0.025f * Mathf.Clamp01(speed / 2f);
            cam.localPosition = new Vector3(0, curEye + bob, 0);
            cam.localRotation = Quaternion.Euler(pitch, 0, 0);
        }

        void Step(float volume)
        {
            if (footSource == null || footsteps == null || footsteps.Length == 0) return;
            footSource.pitch = Random.Range(0.9f, 1.1f);
            footSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)], volume * Settings.Sfx);
        }
    }
}
