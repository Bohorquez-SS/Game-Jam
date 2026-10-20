using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Controlador en primera persona con sensación física: aceleración suave, balanceo sutil de cabeza,
    /// respiración en reposo, inclinación al desplazarse de lado, agacharse suave y movimientos de cámara
    /// cinematográficos (levantarse, acostarse, esconderse) mediante MoveCamera().
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController I;
        /// <summary>Cinemática: sin moverse ni mirar.</summary>
        public static bool CutsceneLock;
        /// <summary>La historia controla la mirada (aunque se pueda caminar).</summary>
        public static bool CutsceneLockLook;
        /// <summary>Transiciones cortas internas (escondites, cámara movida por script).</summary>
        public static bool TransitionLock;

        public static bool LookBlocked => CutsceneLock || CutsceneLockLook || TransitionLock || UIManager.ModalOpen || InspectSystem.Active || FocusSystem.Active;
        public static bool MoveBlocked => CutsceneLock || TransitionLock || UIManager.ModalOpen || InspectSystem.Active || FocusSystem.Active || HidingSpot.Current != null;

        public Transform cam;
        public Camera mainCam;
        [Header("Movimiento")]
        public float walkSpeed = 2.0f, runSpeed = 3.7f, crouchSpeed = 1.05f, gravity = -15f, accel = 9f;
        [Header("Cámara")]
        public float standEye = 1.62f, crouchEye = 0.95f, baseFov = 68f;
        [Header("Pasos")]
        public AudioSource footSource;
        public AudioClip[] footsteps;
        public float stepDistance = 0.72f;

        [HideInInspector] public float LookLimit;
        public bool IsCrouching { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsMoving { get; private set; }
        /// <summary>0 silencio · 0.35 agachado · 1 caminando · 2.2 corriendo.</summary>
        public float Noise { get; private set; }
        public Vector3 EyePosition => cam.position;
        public float Yaw => yaw;
        public float Pitch => pitch;

        CharacterController cc;
        Vector3 vel;
        float yaw, pitch, roll, baseYaw, vy, eyeOverride = -1f, curEye, bobT, stepAcc, breathT, runFov, landDip, prevSpeed;

        void Awake()
        {
            I = this;
            cc = GetComponent<CharacterController>();
            yaw = baseYaw = transform.eulerAngles.y;
            curEye = standEye;
            TransitionLock = false; CutsceneLockLook = false; ExternalCam = false;
            CameraFX.Reset();
        }

        // ------------------------------------------------------------------ poses
        public void SetPose(Vector3 position, float newYaw, float newPitch, float eye)
        {
            cc.enabled = false;
            transform.position = position;
            yaw = baseYaw = newYaw; pitch = newPitch; roll = 0f;
            eyeOverride = eye; curEye = eye > 0 ? eye : standEye; vy = 0; vel = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            ApplyCamera(0f, true);
            cc.enabled = true;
        }

        public void SetPose(Transform t, float eye)
        {
            float p = t.eulerAngles.x; if (p > 180f) p -= 360f;
            SetPose(t.position, t.eulerAngles.y, p, eye);
        }

        /// <summary>Mueve cuerpo y cámara suavemente (cinemáticas: levantarse, acostarse, esconderse).</summary>
        public IEnumerator MoveCamera(Vector3 bodyPos, float toYaw, float toPitch, float eye, float toRoll, float duration, bool collideAfter = true)
        {
            cc.enabled = false;
            Vector3 p0 = transform.position; float y0 = yaw, pi0 = pitch, e0 = curEye, r0 = roll;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = Ease.InOut(t / duration);
                transform.position = Vector3.Lerp(p0, bodyPos, k);
                yaw = Mathf.LerpAngle(y0, toYaw, k); pitch = Mathf.Lerp(pi0, toPitch, k);
                curEye = Mathf.Lerp(e0, eye, k); roll = Mathf.Lerp(r0, toRoll, k);
                transform.rotation = Quaternion.Euler(0, yaw, 0);
                ApplyCamera(0f, true);
                yield return null;
            }
            transform.position = bodyPos; yaw = baseYaw = toYaw; pitch = toPitch; curEye = eye; roll = toRoll;
            eyeOverride = eye;
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            ApplyCamera(0f, true);
            // Dentro de un escondite NO se reactiva la colisión: el CharacterController quedaría metido en el collider
            // del mueble y Unity lo expulsaría (a veces a través de la pared, fuera de la casa).
            cc.enabled = collideAfter;
        }

        /// <summary>Gira la mirada hacia un punto (llamar cada frame en cinemáticas).</summary>
        public void LookAtPoint(Vector3 target, float t)
        {
            Vector3 d = target - cam.position;
            float ty = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            float tp = -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg;
            yaw = Mathf.LerpAngle(yaw, ty, t); baseYaw = yaw;
            pitch = Mathf.Lerp(pitch, tp, t);
        }

        /// <summary>Ángulo horizontal entre la mirada y un punto (0 = de frente, 180 = a la espalda).</summary>
        public float AngleTo(Vector3 p)
        {
            Vector3 d = p - cam.position; d.y = 0;
            Vector3 f = cam.forward; f.y = 0;
            return Vector3.Angle(f, d);
        }

        public void ReleaseEyeOverride() { eyeOverride = -1f; }

        // ------------------------------------------------------------------ bucle
        void Update()
        {
            float dt = Time.deltaTime;
            if (!LookBlocked && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 d = Inp.Look() * Settings.Sensitivity;
                yaw += d.x;
                pitch = Mathf.Clamp(pitch - d.y, -82f, 82f);
                if (LookLimit > 0)
                {
                    yaw = baseYaw + Mathf.Clamp(Mathf.DeltaAngle(baseYaw, yaw), -LookLimit, LookLimit);
                    pitch = Mathf.Clamp(pitch, -30f, 30f);
                }
            }
            transform.rotation = Quaternion.Euler(0, yaw, 0);

            Vector2 input = Vector2.zero;
            bool canMove = !MoveBlocked && cc.enabled;
            if (canMove)
            {
                input = Inp.Move();
                if (input.sqrMagnitude > 1f) input.Normalize();
                IsCrouching = Inp.Crouch();
            }
            bool phone = Phone3D.IsOpen;
            IsMoving = input.sqrMagnitude > 0.01f;
            IsRunning = IsMoving && !IsCrouching && !phone && Inp.Run() && input.y > 0.1f;
            float speed = IsCrouching ? crouchSpeed : (IsRunning ? runSpeed : walkSpeed);
            if (phone) speed *= 0.6f;
            Vector3 target = (transform.right * input.x + transform.forward * input.y) * speed;
            vel = Vector3.MoveTowards(vel, target, accel * dt * (IsMoving ? 1f : 1.6f));
            float hs = vel.magnitude;
            Noise = !IsMoving ? 0f : (IsCrouching ? 0.35f : (IsRunning ? 2.2f : 1f));
            if (IsMoving) eyeOverride = -1f;

            if (cc.enabled)
            {
                if (cc.isGrounded && vy < 0) vy = -1f;
                vy += gravity * dt;
                Vector3 v = vel; v.y = vy;
                cc.Move(v * dt);
                float h = IsCrouching ? 1.15f : 1.7f;
                if (!IsCrouching && cc.height < 1.69f &&
                    Physics.Raycast(transform.position + Vector3.up * cc.height, Vector3.up, 1.75f - cc.height, ~0, QueryTriggerInteraction.Ignore))
                    h = cc.height;
                cc.height = Mathf.MoveTowards(cc.height, h, dt * 3f);
                cc.center = new Vector3(0, cc.height * 0.5f, 0);
                if (hs > 0.2f && cc.isGrounded)
                {
                    stepAcc += hs * dt;
                    if (stepAcc >= stepDistance) { stepAcc = 0f; Step(IsCrouching ? 0.12f : (IsRunning ? 0.7f : 0.42f)); }
                }
            }
            // detenerse tras correr: pequeña caída de cámara
            if (prevSpeed > 3f && hs < 1f) landDip = 0.035f;
            prevSpeed = hs;
            float strafe = Vector3.Dot(vel, transform.right);
            roll = Mathf.Lerp(roll, -strafe * 0.9f, dt * 5f);
            ApplyCamera(hs, false);
        }

        /// <summary>Otro sistema (FocusSystem) controla la cámara.</summary>
        public static bool ExternalCam;

        void ApplyCamera(float speed, bool snap)
        {
            if (cam == null || ExternalCam) return;
            float dt = Time.deltaTime;
            float target = eyeOverride > 0 ? eyeOverride : (IsCrouching ? crouchEye : standEye);
            if (!snap) curEye = Mathf.Lerp(curEye, target, dt * 6f);
            if (speed > 0.15f) bobT += dt * (IsRunning ? 11.5f : 8f) * Mathf.Clamp01(speed / 2f + 0.3f);
            breathT += dt;
            float bobAmt = IsRunning ? 0.04f : 0.016f;
            float k = Mathf.Clamp01(speed / 2f);
            float bobY = Mathf.Sin(bobT) * bobAmt * k;
            float bobX = Mathf.Cos(bobT * 0.5f) * bobAmt * 0.7f * k;
            float breath = Mathf.Sin(breathT * 1.7f) * 0.006f * (1f - k);
            landDip = Mathf.MoveTowards(landDip, 0f, dt * 0.12f);
            cam.localPosition = new Vector3(bobX, curEye + bobY + breath - landDip, 0);
            float fovKick;
            Vector3 shake = CameraFX.Tick(dt, out fovKick);
            float breathPitch = Mathf.Sin(breathT * 1.7f + 0.6f) * 0.25f * (1f - k);
            cam.localRotation = Quaternion.Euler(pitch + breathPitch + shake.x, shake.y, roll + shake.z + Mathf.Sin(bobT * 0.5f) * 0.25f * k);
            runFov = Mathf.Lerp(runFov, IsRunning ? 5f : 0f, dt * 4f);
            if (mainCam) mainCam.fieldOfView = baseFov + runFov + fovKick;
        }

        void Step(float volume)
        {
            if (footSource == null || footsteps == null || footsteps.Length == 0) return;
            footSource.pitch = Random.Range(0.9f, 1.1f);
            footSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)], volume * Settings.Sfx);
        }
    }
}
