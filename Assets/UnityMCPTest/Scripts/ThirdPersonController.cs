using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityMCPTest
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("References")]
        public Transform cameraTransform;
        public Animator animator;

        [Header("Movement")]
        public float walkSpeed = 1.8f;
        public float runSpeed = 4.5f;
        public float turnSpeed = 12f;
        public float acceleration = 16f;
        public float attackDeceleration = 10f;
        public float gravity = -22f;
        public float jumpHeight = 1.1f;
        public float groundedGrace = 0.12f;

        [Header("Camera Orbit")]
        public float mouseSensitivity = 0.12f;
        public float minPitch = -20f;
        public float maxPitch = 70f;
        public float defaultDistance = 4.5f;
        public float minDistance = 2f;
        public float maxDistance = 8f;
        public float cameraCollisionRadius = 0.28f;
        public float cameraCollisionPadding = 0.12f;
        public float cameraCollisionSnapSpeed = 45f;
        public float cameraCollisionRecoverSpeed = 6f;
        public Vector3 lookOffset = new Vector3(0f, 1.35f, 0f);

        [Header("Combat")]
        [Range(0f, 1f)] public float comboOpenFraction = 0.68f;
        [Range(0f, 1f)] public float releaseFraction = 0.88f;
        public float attackSpeed = 1f;

        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int AttackLeftId = Animator.StringToHash("AttackL");
        private static readonly int AttackRightId = Animator.StringToHash("AttackR");
        private static readonly int HeavyId = Animator.StringToHash("Heavy");
        private static readonly int GroundedId = Animator.StringToHash("Grounded");
        private static readonly int AttackSpeedId = Animator.StringToHash("AttackSpeed");
        private static readonly int PunchLeftHash = Animator.StringToHash("Punch Left");
        private static readonly int PunchRightHash = Animator.StringToHash("Punch Right");
        private static readonly int HeavyHash = Animator.StringToHash("Heavy");

        private CharacterController controller;
        private float verticalVelocity;
        private float currentSpeed;
        private float yaw;
        private float pitch = 18f;
        private float distance;
        private float resolvedDistance;
        private float airTime;
        private int punchSide;
        private bool queuedAttack;
        private bool heavyActive;
        private bool isAirborne;
        private bool cursorLocked;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            distance = defaultDistance;
            resolvedDistance = distance;
            yaw = cameraTransform != null ? cameraTransform.eulerAngles.y : 0f;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || animator == null)
            {
                return;
            }

            var mouse = Mouse.current;
            ReadCameraInput(mouse);

            bool attacking = TryGetAttackProgress(out float attackProgress);
            if (attacking)
            {
                if (attackProgress >= releaseFraction)
                {
                    heavyActive = false;
                    queuedAttack = false;
                }
                else if (queuedAttack && !heavyActive && attackProgress >= comboOpenFraction)
                {
                    StartPunch();
                    attackProgress = 0f;
                }
            }
            else
            {
                heavyActive = false;
                queuedAttack = false;
            }

            bool blocking = attacking && attackProgress < releaseFraction;
            bool canStartAction = !attacking && !isAirborne;

            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                if (canStartAction)
                {
                    StartPunch();
                }
                else if (attacking && !heavyActive && attackProgress < releaseFraction)
                {
                    queuedAttack = true;
                }
            }
            else if (keyboard.eKey.wasPressedThisFrame && canStartAction)
            {
                StartHeavy();
            }

            var move = Vector2.zero;
            if (keyboard.wKey.isPressed) move.y += 1f;
            if (keyboard.sKey.isPressed) move.y -= 1f;
            if (keyboard.dKey.isPressed) move.x += 1f;
            if (keyboard.aKey.isPressed) move.x -= 1f;
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            bool running = keyboard.leftShiftKey.isPressed;
            float targetSpeed = move.sqrMagnitude > 0.001f && !blocking
                ? (running ? runSpeed : walkSpeed)
                : 0f;

            float accel = blocking ? attackDeceleration : acceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * Time.deltaTime);

            var flatForward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            var flatRight = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
            var wishDirection = flatForward * move.y + flatRight * move.x;
            if (wishDirection.sqrMagnitude > 0.001f)
            {
                var look = Quaternion.LookRotation(wishDirection.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
            }

            bool jumpedThisFrame = false;

            if (controller.isGrounded)
            {
                if (verticalVelocity < 0f)
                {
                    verticalVelocity = -2f;
                }

                if (keyboard.spaceKey.wasPressedThisFrame && !blocking)
                {
                    verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
                    jumpedThisFrame = true;
                    airTime = groundedGrace + 1f;
                }
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            if (controller.isGrounded && !jumpedThisFrame)
            {
                airTime = 0f;
            }
            else
            {
                airTime += Time.deltaTime;
            }

            isAirborne = jumpedThisFrame || (!controller.isGrounded && airTime > groundedGrace);

            var motion = transform.forward * currentSpeed;
            motion.y = verticalVelocity;
            controller.Move(motion * Time.deltaTime);

            animator.SetBool(GroundedId, !isAirborne);
            animator.SetFloat(SpeedId, currentSpeed);
            animator.SetFloat(AttackSpeedId, attackSpeed);
        }

        private void StartPunch()
        {
            queuedAttack = false;
            heavyActive = false;
            animator.SetTrigger(punchSide == 0 ? AttackLeftId : AttackRightId);
            punchSide = 1 - punchSide;
        }

        private void StartHeavy()
        {
            queuedAttack = false;
            heavyActive = true;
            animator.SetTrigger(HeavyId);
        }

        private bool TryGetAttackProgress(out float progress)
        {
            if (animator.IsInTransition(0))
            {
                var next = animator.GetNextAnimatorStateInfo(0);
                if (IsAttackState(next.shortNameHash))
                {
                    progress = Progress(next.normalizedTime);
                    return true;
                }
            }

            var current = animator.GetCurrentAnimatorStateInfo(0);
            if (IsAttackState(current.shortNameHash))
            {
                progress = Progress(current.normalizedTime);
                return true;
            }

            progress = 0f;
            return false;
        }

        private static bool IsAttackState(int shortNameHash)
        {
            return shortNameHash == PunchLeftHash
                || shortNameHash == PunchRightHash
                || shortNameHash == HeavyHash;
        }

        private static float Progress(float normalizedTime)
        {
            return normalizedTime >= 1f ? 1f : Mathf.Repeat(normalizedTime, 1f);
        }

        private void LateUpdate()
        {
            if (cameraTransform == null)
            {
                return;
            }

            Vector3 pivot = transform.position + lookOffset;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 back = rotation * Vector3.back;

            float allowed = distance;
            var hits = Physics.SphereCastAll(
                pivot,
                cameraCollisionRadius,
                back,
                distance,
                ~0,
                QueryTriggerInteraction.Ignore);

            foreach (var hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                float candidate = hit.distance - cameraCollisionPadding;
                if (candidate < allowed)
                {
                    allowed = candidate;
                }
            }

            allowed = Mathf.Clamp(allowed, 0.35f, distance);
            float speed = allowed < resolvedDistance
                ? cameraCollisionSnapSpeed
                : cameraCollisionRecoverSpeed;
            resolvedDistance = Mathf.MoveTowards(resolvedDistance, allowed, speed * Time.deltaTime);

            Vector3 position = pivot + back * resolvedDistance;
            cameraTransform.SetPositionAndRotation(position, rotation);
        }

        private void ReadCameraInput(Mouse mouse)
        {
            if (mouse == null)
            {
                return;
            }

            if (mouse.rightButton.isPressed)
            {
                if (!cursorLocked)
                {
                    cursorLocked = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, minPitch, maxPitch);
            }
            else if (cursorLocked)
            {
                cursorLocked = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance = Mathf.Clamp(distance - scroll * 0.006f, minDistance, maxDistance);
                resolvedDistance = Mathf.Min(resolvedDistance, distance);
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10f, 10f, 640f, 28f), string.Empty);
            GUI.Label(
                new Rect(18f, 15f, 620f, 20f),
                "WASD move  |  Shift run  |  Space jump  |  LMB punch combo  |  E heavy uppercut  |  Hold RMB orbit  |  Wheel zoom");
        }
    }
}
