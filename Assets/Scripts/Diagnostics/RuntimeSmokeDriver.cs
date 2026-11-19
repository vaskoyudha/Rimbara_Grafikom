using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rimbara.Audio;
using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.Player;
using Rimbara.UI;
using Rimbara.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace Rimbara.Diagnostics
{
    // Installed only by --rimbara-smoke. Normal games never create this object,
    // virtual devices, fixtures, automatic actions, or diagnostic files.
    public sealed class RuntimeSmokeDriver : MonoBehaviour
    {
        [Serializable]
        private sealed class CheckResult
        {
            public string Name;
            public bool Passed;
            public string Detail;
        }

        [Serializable]
        private sealed class Report
        {
            public bool Passed;
            public string Unity;
            public string Profile;
            public string Utc;
            public string GraphicsDevice;
            public int Width;
            public int Height;
            public float MeanFrameMilliseconds;
            public float P95FrameMilliseconds;
            public CheckResult[] Checks;
            public string[] Captures;
            public string[] Errors;
        }

        private readonly List<CheckResult> checks = new List<CheckResult>();
        private readonly List<string> captures = new List<string>();
        private readonly List<string> errors = new List<string>();
        private readonly List<float> frameTimes = new List<float>();
        private readonly List<MissionStage> stages = new List<MissionStage>();
        private Keyboard keyboard;
        private Mouse mouse;
        private readonly List<InputDevice> temporarilyDisabledDevices = new List<InputDevice>();
        private GameState state;
        private MissionSystem missions;
        private GameFlow flow;
        private VolunteerRig rig;
        private VolunteerInput input;
        private VolunteerActor actor;
        private InteractionDetector detector;
        private WaterSprayer water;
        private RimbaraHud hud;
        private WorldGuide guide;
        private GameAudio audioSystem;
        private SeedlingPickup[] seeds;
        private PlantingPoint[] beds;
        private FirePoint[] fires;
        private string output;
        private string profileName;
        private Vector3 spawn;
        private float originalVolume;
        private bool volumeCaptured;
        private bool finished;
        private InputSettings.BackgroundBehavior originalBackgroundBehavior;
        private int collectedEvents;
        private int plantedEvents;
        private int fireEvents;
        private int completedEvents;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (CommandLine.Value("--rimbara-smoke") == null) return;
            var root = new GameObject("RIMBARA opt-in runtime smoke driver");
            DontDestroyOnLoad(root);
            root.AddComponent<RuntimeSmokeDriver>();
        }

        private void Awake()
        {
            Application.logMessageReceived += RecordLog;
            originalBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            output = Path.GetFullPath(CommandLine.Value("--rimbara-output")
                ?? Path.Combine(Application.persistentDataPath, "Smoke"));
            Directory.CreateDirectory(output);
            keyboard = InputSystem.AddDevice<Keyboard>("RimbaraSmokeKeyboard");
            mouse = InputSystem.AddDevice<Mouse>("RimbaraSmokeMouse");
            // Native pointer events must not overwrite our virtual UI pointer between
            // press/release frames. This affects this opt-in game process only, not OS input.
            foreach (InputDevice device in InputSystem.devices.ToArray())
                if (device.enabled && device != keyboard && device != mouse && (device is Mouse || device is Keyboard))
                {
                    temporarilyDisabledDevices.Add(device);
                    InputSystem.DisableDevice(device);
                }
        }

        // Flatten nested iterators so exceptions in every fixture/action write a
        // failure report instead of silently stopping a Unity coroutine.
        private IEnumerator Start()
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(Run());
            while (stack.Count > 0)
            {
                object current = null;
                bool more = false;
                Exception failure = null;
                try
                {
                    more = stack.Peek().MoveNext();
                    if (more) current = stack.Peek().Current;
                }
                catch (Exception exception) { failure = exception; }
                if (failure != null)
                {
                    checks.Add(new CheckResult { Name = "Smoke execution completed", Passed = false, Detail = failure.ToString() });
                    break;
                }
                if (!more) { stack.Pop(); continue; }
                if (current is IEnumerator nested) stack.Push(nested);
                else yield return current;
            }
            Finish();
        }

        private void Update()
        {
            if (!finished && flow != null && flow.Mode == ScreenMode.Playing && Time.unscaledDeltaTime > 0f)
                frameTimes.Add(Time.unscaledDeltaTime * 1000f);
        }

        private IEnumerator Run()
        {
            yield return new WaitForSecondsRealtime(1.2f);
            FindScene();
            profileName = state.Profile.DisplayName;
            spawn = rig.transform.position;
            originalVolume = audioSystem.Volume;
            volumeCaptured = true;
            if (missions.DisasterChapter != null)
            {
                yield return RunDisaster();
                yield break;
            }
            stages.Add(missions.Stage);
            missions.MissionChanged += stage => stages.Add(stage);
            missions.Completed += () => completedEvents++;
            state.SeedlingCollected += id => collectedEvents++;
            state.TreePlanted += id => plantedEvents++;
            state.FireExtinguished += id => fireEvents++;

            Check("Menu starts with gameplay disabled", flow.Mode == ScreenMode.Menu && !state.SessionRunning
                && !rig.ControlsEnabled && hud.View.Menu.activeInHierarchy);
            Check("Profile selects the authored object supply", seeds.Count(s => s.gameObject.activeInHierarchy) == state.Profile.Seedlings
                && beds.Count(b => b.gameObject.activeInHierarchy) == state.Profile.Trees
                && fires.Count(f => f.gameObject.activeInHierarchy) == state.Profile.Fires,
                state.Profile.Seedlings + "/" + state.Profile.Trees + "/" + state.Profile.Fires);
            yield return Capture("01-menu.png");
            yield return Click(hud.View.OpenControls);
            Check("Mouse click opens the controls screen", flow.Mode == ScreenMode.Controls && hud.View.Controls.activeInHierarchy);
            yield return Click(hud.View.ControlsBack);
            Check("Mouse click returns from controls to menu", flow.Mode == ScreenMode.Menu);
            hud.View.Volume.value = 0.37f;
            Check("Volume slider is wired", Mathf.Abs(audioSystem.Volume - 0.37f) < 0.001f);
            hud.View.Volume.value = originalVolume;
            yield return Click(hud.View.Start);
            Check("Mouse click opens profile-specific briefing", flow.Mode == ScreenMode.Briefing && hud.View.Briefing.activeInHierarchy
                && hud.View.BriefingTargets.text.Contains(state.Profile.Seedlings + " BIBIT LOKAL"));
            QueueKeys(Key.W, Key.LeftShift);
            yield return new WaitForSecondsRealtime(0.25f);
            Check("Briefing ignores movement input", HorizontalDistance(spawn, rig.transform.position) < 0.04f && !state.SessionRunning);
            QueueKeys();
            yield return Capture("02-briefing.png");
            yield return Click(hud.View.Begin);
            yield return new WaitForSecondsRealtime(0.5f);
            Check("Begin enables the official motor", flow.Mode == ScreenMode.Playing && state.SessionRunning
                && rig.OfficialController.enabled && rig.OfficialController.GetType().Assembly.GetName().Name == "Unity.StarterAssets");
            Check("Vendor input map stays deactivated", !rig.GetComponent<PlayerInput>().inputIsActive);
            Check("Mixamo avatar animates with valid bones", rig.Animator.isHuman && rig.Animator.avatar.isValid
                && rig.Animator.GetBoneTransform(HumanBodyBones.Hips) != null
                && rig.Animator.GetBoneTransform(HumanBodyBones.RightHand) != null);
            Bounds modelBounds = rig.GetComponentsInChildren<SkinnedMeshRenderer>().First().bounds;
            Check("Character mesh has human-scale bounds", modelBounds.size.y > 1.2f && modelBounds.size.y < 2.8f,
                "Height " + modelBounds.size.y.ToString("0.000") + " m");
            Check("Idle, walk and run clips are present", rig.Animator.runtimeAnimatorController.animationClips.Length >= 3);

            rig.SetCameraOrbit(0f, 15f);
            yield return new WaitForSecondsRealtime(0.3f);
            Vector3 before = rig.transform.position;
            QueueKeys(Key.W);
            yield return new WaitForSecondsRealtime(0.8f);
            float walkDistance = HorizontalDistance(before, rig.transform.position);
            float walkBlend = rig.Animator.GetFloat("Speed");
            Check("W moves forward through the official motor", walkDistance > 1f && rig.transform.position.z > before.z + 1f,
                walkDistance.ToString("0.000") + " m in 0.8 s");
            Check("Walking drives the locomotion blend", walkBlend > 2f && walkBlend < 3.2f, walkBlend.ToString("0.000"));
            QueueKeys();
            yield return new WaitForSecondsRealtime(0.3f);
            before = rig.transform.position;
            QueueKeys(Key.W, Key.LeftShift);
            yield return new WaitForSecondsRealtime(0.8f);
            float runDistance = HorizontalDistance(before, rig.transform.position);
            Check("Shift running is faster than walking", runDistance > walkDistance * 1.45f,
                "Walk " + walkDistance.ToString("0.000") + " / run " + runDistance.ToString("0.000") + " m");
            Check("Running drives the run animation", rig.Animator.GetFloat("Speed") > 4.4f);
            QueueKeys();
            yield return new WaitForSecondsRealtime(0.3f);
            QueueKeys(Key.W, Key.D);
            yield return new WaitForSecondsRealtime(0.12f);
            Check("Diagonal input is normalized", input.Move.x > 0.6f && input.Move.y > 0.6f && input.Move.magnitude <= 1.001f);
            QueueKeys();
            yield return new WaitForSecondsRealtime(0.2f);
            before = rig.transform.position;
            QueueKeys(Key.S);
            yield return new WaitForSecondsRealtime(0.4f);
            Check("S moves backward relative to the camera", rig.transform.position.z < before.z - 0.5f);
            QueueKeys();
            yield return new WaitForSecondsRealtime(0.2f);
            before = rig.transform.position;
            QueueKeys(Key.A);
            yield return new WaitForSecondsRealtime(0.4f);
            Check("A moves left relative to the camera", rig.transform.position.x < before.x - 0.5f);
            QueueKeys();
            yield return new WaitForSecondsRealtime(0.2f);
            before = rig.transform.position;
            QueueKeys(Key.D);
            yield return new WaitForSecondsRealtime(0.4f);
            Check("D moves right relative to the camera", rig.transform.position.x > before.x + 0.5f);
            QueueKeys();
            yield return new WaitForSecondsRealtime(0.3f);
            float yaw = rig.CameraTarget.eulerAngles.y;
            QueueMouse(new Vector2(100f, 0f));
            yield return new WaitForSecondsRealtime(0.25f);
            Check("Mouse delta orbits the third-person camera", Mathf.Abs(Mathf.DeltaAngle(yaw, rig.CameraTarget.eulerAngles.y)) > 6f);
            QueueMouse(new Vector2(0f, 1800f));
            yield return new WaitForSecondsRealtime(0.2f);
            float pitch = Mathf.DeltaAngle(0f, rig.CameraTarget.eulerAngles.x);
            Check("Camera pitch clamps at the official limit", pitch <= rig.OfficialController.TopClamp + rig.OfficialController.CameraAngleOverride + 0.2f
                && pitch > 50f, pitch.ToString("0.0") + " degrees");
            QueueMouse();
            rig.SetCameraOrbit(0f, 15f);

            rig.Teleport(Ground(seeds[0].transform.position + Vector3.back * 1.5f));
            yield return new WaitForSecondsRealtime(0.25f);
            Check("Seedlings are gated by First Steps", !seeds[0].Interact(actor).Success && state.Snapshot.Seedlings == 0);
            Check("Water cannot skip mission order", !fires[0].ReceiveWater(100f) && state.Snapshot.Fires == 0);
            before = rig.transform.position;
            QueueKeys(Key.Escape);
            yield return new WaitForSecondsRealtime(0.1f);
            Check("Escape pauses and freezes time", flow.Mode == ScreenMode.Paused && Time.timeScale == 0f
                && !state.SessionRunning && !rig.ControlsEnabled && rig.Animator.speed == 0f);
            QueueKeys(Key.W, Key.LeftShift, Key.E);
            QueueMouse(default, true);
            yield return new WaitForSecondsRealtime(0.35f);
            Check("Pause blocks motion, interactions and water", HorizontalDistance(before, rig.transform.position) < 0.04f
                && input.Move == Vector2.zero && !water.Spraying && !state.CollectSeedling("paused-fixture")
                && !missions.ReachInspectionMarker() && state.Snapshot.Seedlings == 0);
            QueueKeys();
            QueueMouse();
            RectTransform volumeRect = (RectTransform)hud.View.Volume.transform;
            Vector3 volumePoint = volumeRect.TransformPoint(new Vector3(volumeRect.rect.xMin + volumeRect.rect.width * 0.25f,
                volumeRect.rect.center.y, 0f));
            yield return ClickAt(RectTransformUtility.WorldToScreenPoint(null, volumePoint));
            Check("Mouse pointer changes the pause volume slider", Mathf.Abs(audioSystem.Volume - 0.25f) < 0.06f);
            hud.View.Volume.value = originalVolume;
            yield return Capture("03-pause.png");
            QueueKeys();
            QueueMouse();
            yield return new WaitForSecondsRealtime(0.1f);
            QueueKeys(Key.Escape);
            yield return new WaitForSecondsRealtime(0.1f);
            Check("Escape resumes the session", flow.Mode == ScreenMode.Playing && Time.timeScale == 1f && rig.ControlsEnabled);
            QueueKeys();
            rig.Teleport(new Vector3(70f, -12f, 12f));
            yield return new WaitForSecondsRealtime(0.2f);
            Check("Out-of-bounds recovery preserves progress", HorizontalDistance(spawn, rig.transform.position) < 0.1f
                && rig.transform.position.y > -1f && state.Snapshot.Seedlings == 0);

            yield return MoveTo(missions.ActiveDestination.position);
            yield return new WaitForSecondsRealtime(0.1f);
            Check("Physical inspection trigger starts Seeds of Hope", missions.Stage == MissionStage.SeedsOfHope);
            yield return MoveTo(Ground(beds[0].transform.position + Vector3.back * 1.5f));
            Check("Empty inventory cannot plant", !beds[0].Interact(actor).Success && !beds[0].Planted && state.Snapshot.Trees == 0);
            yield return MoveTo(Ground(seeds[0].transform.position + Vector3.back * 4f));
            detector.Scan();
            Check("Distant seedlings cannot be selected or collected", detector.Focused != seeds[0] && !seeds[0].Interact(actor).Success);
            yield return MoveTo(Ground(seeds[0].transform.position + Vector3.back * 1.5f), 0.2f);
            detector.Scan();
            Check("Nearby detector selects a seedling", detector.Focused == seeds[0]);
            GameObject obstacle = Blocker(Vector3.Lerp(rig.transform.position + Vector3.up * 0.8f,
                seeds[0].FocusPosition, 0.5f), new Vector3(0.6f, 1.1f, 0.3f));
            detector.Scan();
            Check("World obstruction blocks interaction focus", detector.Focused != seeds[0]);
            yield return PressE();
            Check("E cannot collect through a world obstacle", !seeds[0].Collected && state.Snapshot.Seedlings == 0);
            Destroy(obstacle);
            yield return new WaitForSecondsRealtime(0.1f);
            detector.Scan();
            Check("Removing the obstruction restores focus", detector.Focused == seeds[0]);
            yield return Capture("04-seedling.png");
            yield return PressE();
            Check("E collects one seedling and inventory", seeds[0].Collected && state.Snapshot.Seedlings == 1
                && state.Snapshot.CarriedSeedlings == 1 && collectedEvents == 1);
            Check("Duplicate pickup cannot reward twice", !seeds[0].Interact(actor).Success && !state.CollectSeedling(seeds[0].ObjectId)
                && state.Snapshot.CarriedSeedlings == 1);
            yield return new WaitForSecondsRealtime(0.1f);
            Check("Guidance switches to an available planting bed", guide.Destination != null && guide.Destination.GetComponent<PlantingPoint>() != null);
            yield return MoveTo(Ground(beds[0].transform.position + Vector3.back * 1.5f), 0.2f);
            yield return PressE();
            Check("E plants a tree and consumes inventory", beds[0].Planted && beds[0].PlantedTree != null
                && state.Snapshot.Trees == 1 && state.Snapshot.CarriedSeedlings == 0 && plantedEvents == 1);
            Check("Duplicate planting cannot consume another seedling", !beds[0].Interact(actor).Success && !state.PlantTree(beds[0].ObjectId)
                && state.Snapshot.Trees == 1);
            yield return new WaitForSecondsRealtime(1.9f);
            Check("Planted tree grows to its authored size", Mathf.Abs(beds[0].PlantedTree.transform.localScale.y - 1f) < 0.02f);
            yield return Capture("05-new-roots.png");
            for (int i = 1; i < state.Profile.Seedlings; i++)
            {
                yield return MoveTo(Ground(seeds[i].transform.position + Vector3.back * 1.5f), 0.25f);
                yield return PressE();
                Check("Seedling " + (i + 1) + " collected by E", seeds[i].Collected && state.Snapshot.Seedlings == i + 1);
                if (i >= state.Profile.Trees) continue;
                yield return MoveTo(Ground(beds[i].transform.position + Vector3.back * 1.5f), 0.25f);
                yield return PressE();
                Check("Tree " + (i + 1) + " planted by E", beds[i].Planted && state.Snapshot.Trees == i + 1
                    && state.Snapshot.CarriedSeedlings == 0);
            }
            Check("Planting goals unlock Stop the Fire", missions.Stage == MissionStage.StopTheFire
                && state.Snapshot.Seedlings == state.Profile.Seedlings && state.Snapshot.Trees == state.Profile.Trees);
            Check("HUD counters use committed progress", hud.View.Seedlings.text.Contains(state.Profile.Seedlings + " / " + state.Profile.Seedlings)
                && hud.View.Trees.text.Contains(state.Profile.Trees + " / " + state.Profile.Trees)
                && Mathf.Abs(hud.View.RestorationFill.fillAmount - state.Snapshot.Restoration) < 0.001f);

            // Leave the planting clearing sideways before heading north. In the
            // Week 1 profile a straight route from bed 1 would hit its new trunk.
            yield return MoveTo(Ground(new Vector3(3f, 0f, rig.transform.position.z)), 0.35f);
            yield return MoveTo(Ground(fires[0].transform.position + Vector3.back * (state.Profile.SprayRange + 2.2f)), 0.25f);
            Check("Route around planted trees remains traversable", HorizontalDistance(rig.transform.position, fires[0].transform.position)
                < state.Profile.SprayRange + 3f);
            yield return AimAt(fires[0]);
            Check("Camera crosshair selects the distant fire", water.AimedFire == fires[0]);
            yield return SprayFor(0.3f);
            Check("Water range prevents distant suppression", fires[0].SuppressionFraction == 0f && water.AimHint.Contains("Dekati"));
            yield return MoveTo(Ground(fires[0].transform.position + Vector3.back * 4.5f), 0.25f);
            yield return AimAt(fires[0]);
            Check("Near fire is aimable through Cinemachine", water.AimedFire == fires[0]);
            // Put the wall ahead of the volunteer, not behind the shoulder rig:
            // a wall behind the rig correctly makes Cinemachine move the camera
            // forward and would no longer block the resulting camera ray.
            obstacle = Blocker(Vector3.Lerp(rig.transform.position + Vector3.up * 1.5f,
                fires[0].AimPosition, 0.6f), new Vector3(1.2f, 2f, 0.35f));
            yield return SprayFor(0.25f);
            Check("Camera-ray obstruction blocks suppression", water.AimedFire == null && fires[0].SuppressionFraction == 0f,
                "Aim " + (water.AimedFire == null ? "none" : water.AimedFire.ObjectId) + " / contact " + fires[0].SuppressionFraction);
            Destroy(obstacle);
            yield return new WaitForSecondsRealtime(0.1f);
            yield return AimAt(fires[0]);
            rig.transform.rotation = Quaternion.Euler(0f, rig.ViewCamera.transform.eulerAngles.y, 0f);
            water.UpdateAim();
            Transform nozzle = rig.transform.Find("Water nozzle");
            obstacle = Blocker(Vector3.Lerp(nozzle.position, fires[0].AimPosition, 0.24f), Vector3.one * 0.2f);
            water.UpdateAim();
            Check("Nozzle-only obstruction fixture leaves the crosshair clear", water.AimedFire == fires[0]
                && Physics.Linecast(nozzle.position, fires[0].AimPosition, 1 << 8, QueryTriggerInteraction.Ignore));
            Check("Nozzle linecast independently blocks water", !water.ApplySpray(0.5f) && fires[0].SuppressionFraction == 0f
                && water.AimHint.Contains("terhalang"));
            Destroy(obstacle);
            yield return new WaitForSecondsRealtime(0.1f);
            yield return AimAt(fires[0]);
            input.EnableTestDriver(false);
            QueueMouse(default, true);
            yield return new WaitForSecondsRealtime(0.6f);
            Check("Left mouse sprays particles and accumulates contact", water.Spraying
                && nozzle.GetComponentInChildren<ParticleSystem>().isPlaying
                && fires[0].SuppressionFraction > 0.05f && fires[0].SuppressionFraction < 0.85f);
            yield return Capture("06-fire-suppression.png");
            QueueMouse();
            yield return new WaitForSecondsRealtime(0.1f);
            float contact = fires[0].SuppressionFraction;
            yield return new WaitForSecondsRealtime(0.4f);
            Check("Releasing water does not erase contact", Mathf.Abs(fires[0].SuppressionFraction - contact) < 0.0001f && !water.Spraying);
            QueueKeys(Key.Escape);
            yield return new WaitForSecondsRealtime(0.1f);
            float elapsed = flow.ElapsedSeconds;
            QueueKeys();
            QueueMouse(default, true);
            yield return new WaitForSecondsRealtime(0.3f);
            Check("Pause preserves fire contact and mission time", flow.Mode == ScreenMode.Paused
                && Mathf.Abs(fires[0].SuppressionFraction - contact) < 0.0001f
                && Mathf.Abs(flow.ElapsedSeconds - elapsed) < 0.0001f && !water.Spraying);
            QueueMouse();
            yield return Click(hud.View.Resume);
            yield return new WaitForSecondsRealtime(0.1f);
            Check("Mouse click resumes gameplay", flow.Mode == ScreenMode.Playing && state.SessionRunning);
            yield return Extinguish(fires[0]);
            Check("Fire completion is one-shot", !fires[0].ReceiveWater(100f) && !state.ExtinguishFire(fires[0].ObjectId)
                && state.Snapshot.Fires == 1 && fireEvents == 1);
            Check("Completed fire stops flames, smoke and collision", fires[0].GetComponentsInChildren<ParticleSystem>(true)
                .Where(p => p.name == "Flames" || p.name == "Smoke").All(p => !p.isPlaying)
                && fires[0].GetComponentsInChildren<Collider>(true).All(c => !c.enabled));
            for (int i = 1; i < state.Profile.Fires; i++)
            {
                yield return MoveTo(Ground(fires[i].transform.position + Vector3.back * 4.5f), 0.25f);
                yield return AimAt(fires[i]);
                yield return Extinguish(fires[i]);
                Check("Fire " + (i + 1) + " extinguished by left mouse", fires[i].Extinguished && state.Snapshot.Fires == i + 1);
            }
            yield return new WaitForSecondsRealtime(0.2f);
            Check("100 percent requires returning to the post", state.Snapshot.Restored && state.Snapshot.Restoration == 1f
                && missions.Stage == MissionStage.ANewBeginning && flow.Mode == ScreenMode.Playing && completedEvents == 0);
            Check("HUD and guidance reflect full restoration", hud.View.Restoration.text == "100% DIPULIHKAN"
                && hud.View.RestorationFill.fillAmount == 1f && guide.Destination == missions.ActiveDestination);
            yield return Capture("07-restored-forest.png");
            Vector3 post = missions.ActiveDestination.position;
            yield return MoveTo(Ground(new Vector3(3f, 0f, 35f)), 0.5f);
            yield return MoveTo(Ground(new Vector3(3f, 0f, -18f)), 0.5f);
            yield return MoveTo(post, 1.2f, true);
            yield return new WaitForSecondsRealtime(0.3f);
            Check("Physical post trigger wins exactly once", missions.Stage == MissionStage.Complete && completedEvents == 1
                && flow.Mode == ScreenMode.Victory && !state.SessionRunning);
            Check("Victory screen releases controls and cursor", hud.View.Victory.activeInHierarchy && !hud.View.Gameplay.activeInHierarchy
                && !rig.ControlsEnabled && Cursor.lockState == CursorLockMode.None && !water.Spraying);
            Check("Action events match profile goals", collectedEvents == state.Profile.Seedlings
                && plantedEvents == state.Profile.Trees && fireEvents == state.Profile.Fires);
            Check("All four missions execute in order", stages.SequenceEqual(new[] { MissionStage.FirstSteps, MissionStage.SeedsOfHope,
                MissionStage.StopTheFire, MissionStage.ANewBeginning, MissionStage.Complete }), string.Join(" -> ", stages));
            yield return Capture("08-victory.png");
            yield return Click(hud.View.Replay);
            yield return new WaitForSecondsRealtime(0.7f);
            FindScene();
            Check("Replay reloads a clean briefing", flow.Mode == ScreenMode.Briefing && !state.SessionRunning
                && state.Snapshot.Seedlings == 0 && state.Snapshot.Trees == 0 && state.Snapshot.Fires == 0);
            Check("Replay restores visual objects and the profile", seeds.Where(s => s.gameObject.activeInHierarchy).All(s => !s.Collected)
                && beds.Where(b => b.gameObject.activeInHierarchy).All(b => !b.Planted)
                && fires.Where(f => f.gameObject.activeInHierarchy).All(f => !f.Extinguished && f.SuppressionFraction == 0f)
                && state.Profile.DisplayName == profileName);
            yield return Click(hud.View.Begin);
            yield return new WaitForSecondsRealtime(0.15f);
            Check("Replay can begin First Steps again", flow.Mode == ScreenMode.Playing && missions.Stage == MissionStage.FirstSteps);
            flow.Pause();
            yield return Click(hud.View.PauseMenu);
            yield return new WaitForSecondsRealtime(0.6f);
            FindScene();
            Check("Pause-menu return resets the session", flow.Mode == ScreenMode.Menu && hud.View.Menu.activeInHierarchy
                && state.Snapshot.Seedlings == 0 && missions.Stage == MissionStage.FirstSteps && Time.timeScale == 1f);
            Check("Runtime produced no error or exception logs", errors.Count == 0, string.Join("\n", errors));
        }

        private void FindScene()
        {
            state = FindFirstObjectByType<GameState>();
            missions = FindFirstObjectByType<MissionSystem>();
            flow = FindFirstObjectByType<GameFlow>();
            rig = FindFirstObjectByType<VolunteerRig>();
            hud = FindFirstObjectByType<RimbaraHud>();
            guide = FindFirstObjectByType<WorldGuide>();
            audioSystem = FindFirstObjectByType<GameAudio>();
            if (state == null || missions == null || flow == null || rig == null || hud == null || guide == null || audioSystem == null)
                throw new InvalidOperationException("The authored Main scene is missing runtime systems.");
            input = rig.GetComponent<VolunteerInput>();
            actor = rig.GetComponent<VolunteerActor>();
            detector = rig.GetComponent<InteractionDetector>();
            water = rig.GetComponent<WaterSprayer>();
            seeds = FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(s => s.Slot).ToArray();
            beds = FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(b => b.Slot).ToArray();
            fires = FindObjectsByType<FirePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(f => f.Slot).ToArray();
        }

        private IEnumerator RunDisaster()
        {
            DisasterChapter chapter = missions.DisasterChapter;
            DisasterHud storyHud = FindFirstObjectByType<DisasterHud>();
            CoordinatorDialogue coordinator = FindFirstObjectByType<CoordinatorDialogue>();
            EquipmentPickup equipment = FindFirstObjectByType<EquipmentPickup>();
            DebrisPickup[] piles = FindObjectsByType<DebrisPickup>(FindObjectsSortMode.None).OrderBy(p => p.ObjectId).ToArray();
            profileName = "From Ashes / Disaster chapter / local interaction fixtures";
            Check("Story starts in menu with chapter briefing pending", flow.Mode == ScreenMode.Menu
                && chapter.Progress.Stage == DisasterStage.Briefing && !state.SessionRunning);
            Check("Authored Disaster object supply", fires.Length == 10 && piles.Length == 6);
            HumanNpcVisual[] humanNpcs = FindObjectsByType<HumanNpcVisual>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Check("Ten NPCs use eight distinct human characters", humanNpcs.Length == 10 && humanNpcs.Select(n => n.CharacterId).Distinct().Count() == 8);
            foreach (HumanNpcVisual npc in humanNpcs)
            {
                Animator animator = npc.GetComponentInChildren<Animator>(true);
                Check("NPC Humanoid avatar and controller: " + npc.name, animator != null && animator.isHuman
                    && animator.avatar.isValid && animator.runtimeAnimatorController != null && !animator.applyRootMotion);
                Check("NPC visual preserves player and physics: " + npc.name, npc.GetComponentInParent<VolunteerRig>() == null
                    && npc.GetComponentsInChildren<Collider>(true).Length == 0
                    && npc.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(r => r.sharedMesh != null && r.bones.Length > 0));
            }
            Check("Original Disaster map uses kit resources, not demo village layout", UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
                .Any(r => r.name == "Environment - RIMBARA Disaster Zone (EmaceArt resources)" || r.name == "Environment - RIMBARA")
                && !UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Any(r => r.name == "Environment - EmaceArt Slavic Village"));
            Transform postBoard = GameObject.Find("Mission map board").transform;
            TMPro.TextMeshPro postSign = postBoard.GetComponentInChildren<TMPro.TextMeshPro>();
            Check("Emergency post inscription is attached to board, not a billboard", postSign != null
                && postSign.text == "POSKO DARURAT" && postSign.transform.parent == postBoard
                && postSign.GetComponent<Rimbara.World.Billboard>() == null);
            Quaternion signRotation = postSign.transform.rotation;
            yield return CapturePostBoard(postBoard);
            Check("Emergency post inscription stays aligned and fits board", Quaternion.Angle(signRotation, postSign.transform.rotation) < 0.1f
                && Quaternion.Angle(postSign.transform.rotation, postBoard.rotation) < 0.1f && !postSign.isTextOverflowing);
            yield return Capture("01-story-menu.png");
            Check("Menu is Indonesian", hud.View.Start.GetComponentInChildren<TMPro.TMP_Text>().text == "MULAI PEMULIHAN");
            yield return Click(hud.View.OpenControls);
            Check("Indonesian controls screen opens", flow.Mode == ScreenMode.Controls
                && hud.View.Controls.GetComponentsInChildren<TMPro.TMP_Text>().Any(t => t.text.Contains("SPASI / Melompat")));
            yield return Capture("01a-controls-id.png");
            yield return Click(hud.View.ControlsBack);
            yield return Click(hud.View.Start);
            Check("Pointer opens storyline briefing", flow.Mode == ScreenMode.Briefing);
            yield return Capture("01b-briefing-id.png");
            yield return Click(hud.View.Begin);
            Check("Pointer begins Disaster chapter", flow.Mode == ScreenMode.Playing && state.SessionRunning);
            yield return CaptureWorldDetail(humanNpcs.Single(n => n.CharacterId == "Coordinator").transform, "npc-coordinator.png", 3f, human: true);
            yield return CaptureWorldDetail(humanNpcs.First(n => n.CharacterId == "ResidentWoman").transform, "npc-resident.png", 3f, human: true);
            Check("Early planting is locked", !missions.CanCollectOrPlant);
            if (FindFirstObjectByType<RestoredChapter>() != null)
                yield return CaptureWorldDetail(chapter.transform, "00-disaster-same-view.png", 35f);
            Check("Water locked before equipment", !missions.CanExtinguish);
            // Our original map reserves a central arrival road. Traverse it with
            // the actual motor instead of teleporting the prologue approach.
            yield return MoveTo(Ground(new Vector3(0f, 0f, -15f)), 0.65f);
            yield return MoveTo(Ground(new Vector3(1.5f, 0f, -12.2f)), 0.45f);
            Check("Original arrival road reaches emergency post without teleport", HorizontalDistance(rig.transform.position, coordinator.transform.position) < 2.5f);
            // Teleport only to isolate each interaction fixture; final approach uses
            // the real motor. This does not certify an unobstructed full-map route.
            yield return Approach(coordinator.transform);
            yield return PressE();
            Check("E opens coordinator dialogue and pauses gameplay", storyHud.DialogueOpen && flow.Mode == ScreenMode.Paused
                && !state.SessionRunning && Time.timeScale == 0f);
            Check("Paused dialogue cannot mutate chapter", !chapter.AcceptBriefing());
            Check("Coordinator dialogue uses Indonesian", storyHud.GetComponentsInChildren<TMPro.TMP_Text>(true)
                .Any(t => t.text.Contains("Selamat datang di posko darurat")));
            yield return Capture("02-coordinator.png");
            yield return AdvanceDialogue(storyHud);
            Check("First pointer advances dialogue without accepting early", chapter.Progress.Stage == DisasterStage.Briefing && storyHud.DialogueOpen);
            yield return AdvanceDialogue(storyHud);
            yield return AdvanceDialogue(storyHud);
            Check("Final pointer accepts emergency briefing", chapter.Progress.Stage == DisasterStage.Equipment
                && !storyHud.DialogueOpen && state.SessionRunning);
            yield return Approach(equipment.transform);
            yield return PressE();
            Check("E collects equipment once and enables water", chapter.Progress.HasEquipment && missions.CanExtinguish && !equipment.Available);
            Check("HUD and action feedback use Indonesian", hud.View.Seedlings.text.Contains("PERLENGKAPAN")
                && hud.View.Toast.text.Contains("Perlengkapan siap"));
            flow.Pause();
            yield return Capture("02a-pause-id.png");
            yield return Click(hud.View.Resume);
            Check("Equipment cannot be collected twice", !chapter.CollectEquipment());
            Check("Cleanup blocked before fires", !chapter.ClearDebris(piles[0].ObjectId));
            yield return Approach(coordinator.transform);
            yield return PressE();
            Check("Coordinator can repeat current objective", storyHud.DialogueOpen && flow.Mode == ScreenMode.Paused);
            input.EnableTestDriver(false);
            QueueKeys(Key.Escape);
            yield return new WaitForSecondsRealtime(0.15f);
            QueueKeys();
            yield return new WaitForSecondsRealtime(0.15f);
            Check("Escape closes dialogue without losing equipment", !storyHud.DialogueOpen && flow.Mode == ScreenMode.Playing && chapter.Progress.HasEquipment);
            yield return MoveTo(Ground(new Vector3(0f, 0f, -5f)), 0.6f);
            yield return MoveTo(Ground(new Vector3(0f, 0f, 16f)), 0.6f);
            yield return MoveTo(Ground(new Vector3(-9f, 0f, 16f)), 0.6f);
            yield return MoveTo(Ground(new Vector3(-14f, 0f, 15f)), 0.6f);
            yield return MoveTo(Ground(new Vector3(-18f, 0f, 12f)), 0.6f);
            yield return MoveTo(Ground(new Vector3(-24f, 0f, 11f)), 0.6f);
            yield return MoveTo(Ground(new Vector3(-28f, 0f, 11f)), 0.6f);
            yield return MoveTo(Ground(fires[0].transform.position + new Vector3(3f, 0f, 1f)), 0.6f);
            float corridorDistance = HorizontalDistance(rig.transform.position, fires[0].transform.position);
            Check("Authored forest corridor reaches first fire without disabling collision", corridorDistance < 4f,
                "player=" + rig.transform.position + " fire=" + fires[0].transform.position + " distance=" + corridorDistance);
            foreach (FirePoint fire in fires)
            {
                yield return ApproachFire(fire);
                Check("Crosshair selects chapter fire " + fire.ObjectId, water.AimedFire == fire);
                yield return Extinguish(fire);
            }
            Check("Containment unlocks cleanup without advancing legacy missions", chapter.Progress.Stage == DisasterStage.CleanTheWasteland
                && state.Snapshot.Fires == 10 && missions.Stage == MissionStage.FirstSteps && !missions.CanCollectOrPlant);
            yield return Capture("03-fires-contained.png");
            foreach (DebrisPickup pile in piles)
            {
                if (pile == piles[0])
                {
                    rig.Teleport(Ground(pile.transform.position + Vector3.back * 6f));
                    yield return new WaitForSecondsRealtime(0.15f);
                    Check("Debris cannot be cleared out of range", !pile.Interact(actor).Success && !pile.Cleared);
                    flow.Pause();
                    Check("Pause blocks cleanup progress", !chapter.ClearDebris(pile.ObjectId));
                    flow.Resume();
                }
                yield return Approach(pile.transform);
                yield return PressE();
                Check("E clears " + pile.ObjectId, pile.Cleared && !pile.Available);
                Check("Cleared debris cannot earn duplicate progress " + pile.ObjectId, !chapter.ClearDebris(pile.ObjectId));
            }
            Check("Disaster percentage reaches 100 without final-story victory", chapter.Progress.Stage == DisasterStage.RecoveryReady
                && flow.Mode == ScreenMode.Playing && chapter.Progress.ChapterFraction == 1f);
            yield return Capture("04-clean-start.png");
            yield return Approach(coordinator.transform);
            yield return PressE();
            yield return AdvanceDialogue(storyHud);
            yield return AdvanceDialogue(storyHud);
            if (chapter.Recovery != null) yield return RunRecovery(chapter.Recovery, storyHud);
            else
            {
                Check("Coordinator closes chapter with honest milestone screen", flow.Mode == ScreenMode.Victory
                    && hud.View.VictoryResult.text.Contains(IndonesianText.ChapterPending) && !state.SessionRunning);
                yield return Capture("05-chapter-complete.png");
            }
            int completedScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;
            yield return Click(hud.View.Replay);
            float reloadDeadline = Time.realtimeSinceStartup + 12f;
            while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle == completedScene && Time.realtimeSinceStartup < reloadDeadline)
                yield return null;
            Check("Replay pointer reloads the authored scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle != completedScene);
            // Scene import/instantiation now includes eight human avatars. Give Start
            // two actual update frames instead of assuming a 0.6 s wall-clock wait.
            yield return null; yield return null;
            FindScene();
            Check("Replay restores chapter equipment, fires and debris", flow.Mode == ScreenMode.Briefing
                && missions.DisasterChapter.Progress.Stage == DisasterStage.Briefing && !missions.DisasterChapter.Progress.HasEquipment
                && fires.All(f => !f.Extinguished) && FindObjectsByType<DebrisPickup>(FindObjectsSortMode.None).All(p => !p.Cleared),
                "mode=" + flow.Mode + " stage=" + missions.DisasterChapter.Progress.Stage + " equipment=" + missions.DisasterChapter.Progress.HasEquipment);
            RecoveryChapter resetRecovery = missions.DisasterChapter.Recovery;
            if (resetRecovery != null)
                Check("Replay clears Recovery inventory, digging, watering and ecosystem", resetRecovery.Progress.Stage == RecoveryStage.Dormant
                    && resetRecovery.Progress.Fraction == 0f && resetRecovery.Progress.Carried == 0 && resetRecovery.Progress.Dug == 0
                    && !FindFirstObjectByType<RecoveryEnvironment>().WildlifeReturned);
            RecoveryRiver resetRiver = FindFirstObjectByType<RecoveryRiver>();
            if (resetRiver != null)
            {
                yield return null;
                Check("Replay resets all ten river trash targets and recovered banks", resetRecovery.Progress.WasteGoal == 10
                    && FindObjectsByType<RecoveryTask>(FindObjectsSortMode.None).Where(t => t.Kind == RecoveryTaskKind.Waste).All(t => !t.Done)
                    && resetRiver.CleanupFraction == 0f && !resetRiver.BanksRecovered);
            }
            RestoredChapter resetRestored = FindFirstObjectByType<RestoredChapter>();
            if (resetRestored != null) Check("Replay resets final mission and restored world", resetRestored.Progress.Stage == RestoredStage.Dormant
                && !resetRestored.Progress.HasSeedling && !FindFirstObjectByType<RestoredEnvironment>().Applied);
        }

        private IEnumerator RunRecovery(RecoveryChapter recovery, DisasterHud storyHud)
        {
            profileName = "From Ashes / Disaster + Recovery / local fixtures and physical corridors";
            RecoveryProgress p = recovery.Progress;
            RangerDialogue ranger = FindFirstObjectByType<RangerDialogue>();
            yield return CaptureWorldDetail(ranger.GetComponentInChildren<HumanNpcVisual>(true).transform, "npc-ranger.png", 3f, human: true);
            RecoverySeedling[] supply = FindObjectsByType<RecoverySeedling>(FindObjectsSortMode.None).OrderBy(s => s.ObjectId).ToArray();
            RecoveryPlantingPoint[] targets = FindObjectsByType<RecoveryPlantingPoint>(FindObjectsSortMode.None).OrderBy(b => b.Slot).ToArray();
            RecoveryTask[] tasks = FindObjectsByType<RecoveryTask>(FindObjectsSortMode.None).OrderBy(t => t.ObjectId).ToArray();
            Check("Zone 1 dialogue transitions to Recovery without victory or reset", p.Stage == RecoveryStage.Briefing && state.SessionRunning
                && flow.Mode == ScreenMode.Playing && missions.DisasterChapter.Progress.Fires == 10 && missions.DisasterChapter.Progress.Debris == 6);
            RecoveryRiver river = FindFirstObjectByType<RecoveryRiver>();
            int wasteGoal = river == null ? 3 : RecoveryRiverLayout.TrashCount;
            Check("Recovery supply and initially unplanted trees", supply.Length == 5 && targets.Length == 5 && tasks.Length == wasteGoal + 2
                && targets.All(b => !b.Planted && !b.Watered) && !FindFirstObjectByType<RecoveryEnvironment>().WildlifeReturned);
            yield return null;
            if (recovery.Restored != null) yield return CaptureWorldDetail(recovery.transform, "00-recovery-same-view.png", 35f);
            Check("Recovery atmosphere changes without mature forest", FindFirstObjectByType<RecoveryEnvironment>().RecoverySkyActive
                && RenderSettings.fogDensity < 0.008f && targets.All(b => !b.transform.Find("Young planted tree").gameObject.activeSelf));
            Check("Planting and ecosystem locked before ranger briefing", !recovery.CanPlant && !recovery.CanRestore
                && !p.Collect(supply[0].ObjectId, true) && !p.RestoreHabitat("recovery-habitat-0", true));
            yield return MoveTo(Ground(new Vector3(0f, 0f, -15f)), 0.6f);
            yield return MoveTo(Ground(new Vector3(-6f, 0f, -13f)), 0.6f);
            yield return MoveTo(Ground(ranger.transform.position + Vector3.back * 1.7f), 0.5f);
            detector.Scan();
            Check("Physical route from post reaches ranger without teleport", detector.Focused == ranger);
            yield return PressE();
            Check("Ranger opens Indonesian ecological briefing", storyHud.DialogueOpen && !state.SessionRunning
                && storyHud.GetComponentsInChildren<TMPro.TMP_Text>(true).Any(t => t.text.Contains("hutan adalah rumah")));
            Check("Paused ranger cannot accept briefing", !recovery.AcceptBriefing());
            yield return Capture("06-ranger-briefing.png");
            input.EnableTestDriver(false); QueueKeys(Key.Escape); yield return new WaitForSecondsRealtime(0.15f); QueueKeys(); yield return new WaitForSecondsRealtime(0.15f);
            Check("Escape dismisses ranger without accepting mission", !storyHud.DialogueOpen && p.Stage == RecoveryStage.Briefing && state.SessionRunning);
            yield return PressE(); yield return AdvanceDialogue(storyHud); yield return AdvanceDialogue(storyHud);
            Check("Ranger enables planting after complete dialogue", p.Stage == RecoveryStage.Planting && recovery.CanPlant && !recovery.CanRestore);
            foreach (RecoverySeedling seed in supply)
            {
                yield return Approach(seed.transform); yield return PressE();
                Check("E picks up Recovery seed " + seed.ObjectId, seed.Collected && !seed.Available);
                Check("Seed pickup cannot duplicate inventory " + seed.ObjectId, !p.Collect(seed.ObjectId, true));
            }
            Check("Recovery inventory does not advance legacy MVP", p.Carried == 5 && state.Snapshot.Seedlings == 0 && missions.Stage == MissionStage.FirstSteps);
            yield return Capture("07-nursery.png");
            foreach (RecoveryPlantingPoint bed in targets)
            {
                yield return Approach(bed.transform);
                Check("Cannot water before specific bed is planted " + bed.ObjectId, !bed.ReceiveWater(1f));
                yield return PressE();
                Check("E digs bed without consuming seed " + bed.ObjectId, bed.Dug && !bed.Planted);
                yield return PressE();
                Check("Second E plants young tree " + bed.ObjectId, bed.Planted && !bed.Watered);
                Check("No duplicate planting " + bed.ObjectId, !p.Plant(bed.ObjectId, true));
            }
            Check("All planting still requires all watering", p.Planted == 5 && p.Carried == 0 && p.Stage == RecoveryStage.Planting && !p.AcceptPlantingDebrief(true));
            foreach (RecoveryPlantingPoint bed in targets)
            {
                yield return ApproachSeedlingForWater(bed);
                Check("Crosshair identifies planted seedling " + bed.ObjectId, water.AimedSeedling == bed && water.AimHint == IndonesianText.SeedlingWaterAction);
                if (bed == targets[0])
                {
                    Vector3 nearPosition = rig.transform.position;
                    rig.Teleport(Ground(bed.transform.position + Vector3.back * 11f));
                    yield return new WaitForSecondsRealtime(0.2f);
                    yield return AimAtPoint(bed.AimPosition);
                    Check("Range prevents distant seedling watering", water.AimedSeedling == bed && !water.ApplySpray(0.5f) && bed.WaterFraction == 0f);
                    rig.Teleport(nearPosition); yield return new WaitForSecondsRealtime(0.2f);
                    yield return AimAtPoint(bed.AimPosition);
                    Transform nozzle = rig.transform.Find("Water nozzle");
                    GameObject blocker = Blocker((nozzle.position + bed.AimPosition) * 0.5f, Vector3.one * 0.35f);
                    Check("Obstruction blocks seedling water", !water.ApplySpray(0.5f) && bed.WaterFraction == 0f);
                    Destroy(blocker); yield return new WaitForSecondsRealtime(0.2f);
                    yield return AimAtPoint(bed.AimPosition);
                    flow.Pause(); Check("Pause blocks seedling watering", !bed.ReceiveWater(0.5f) && bed.WaterFraction == 0f); flow.Resume();
                    yield return AimAtPoint(bed.AimPosition);
                    yield return SprayFor(0.35f);
                    Check("Partial water does not complete tree", bed.WaterFraction > 0f && bed.WaterFraction < 1f && p.Watered == 0);
                    yield return Capture("08-seedling-watering.png");
                }
                input.EnableTestDriver(false);
                float deadline = Time.realtimeSinceStartup + 8f;
                while (!bed.Watered && Time.realtimeSinceStartup < deadline)
                {
                    SetCameraAtPoint(bed.AimPosition); QueueMouse(default, true); yield return null;
                }
                QueueMouse(); input.EnableTestDriver(true); yield return new WaitForSecondsRealtime(0.15f);
                Check("Held mouse waters registered bed " + bed.ObjectId, bed.Watered);
                Check("Watering completed bed cannot duplicate progress " + bed.ObjectId, !bed.ReceiveWater(0.5f));
                Check("Tree remains a young sapling " + bed.ObjectId, bed.transform.Find("Young planted tree").GetComponentsInChildren<Renderer>()
                    .Max(r => r.bounds.max.y) - bed.transform.position.y < 1.6f);
            }
            Check("All watered trees require ranger debrief before ecosystem", p.Stage == RecoveryStage.PlantingDebrief && !recovery.CanRestore);
            yield return Approach(ranger.transform); yield return PressE();
            yield return AdvanceDialogue(storyHud); yield return AdvanceDialogue(storyHud);
            Check("Debrief activates ecosystem recovery", p.Stage == RecoveryStage.Ecosystem && recovery.CanRestore && !recovery.CanPlant);
            RecoveryTask[] waste = tasks.Where(t => t.Kind == RecoveryTaskKind.Waste).ToArray();
            if (river != null)
            {
                Check("Winding river has ten registered trash clusters and flow audio", waste.Length == 10 && p.WasteGoal == 10
                    && river.FlowAudible && river.transform.Find("River water surface").GetComponent<MeshFilter>().sharedMesh.vertexCount > 100);
                bool channelVisible = true;
                for (float z = RecoveryRiverLayout.StartZ + 1f; z < RecoveryRiverLayout.EndZ - 1f; z += 1f)
                {
                    Vector3 point = new Vector3(RecoveryRiverLayout.CenterX(z), 0f, z);
                    channelVisible &= Terrain.activeTerrain.SampleHeight(point) + Terrain.activeTerrain.transform.position.y < RecoveryRiverLayout.WaterY - 0.02f;
                }
                Check("River surface remains above its carved channel throughout", channelVisible);
                yield return CaptureWorldDetail(river.transform.Find("River water surface"), "09b-river-before-cleanup.png", 22f);
                yield return MoveTo(Ground(new Vector3(-6f, 0f, -13f)), 0.7f);
                yield return MoveTo(Ground(new Vector3(-4.3f, 0f, 14f)), 0.65f);
                yield return MoveTo(Ground(RecoveryRiverLayout.BankPoint(20f, -1, 3.15f)), 0.5f);
            }
            foreach (RecoveryTask task in waste)
            {
                if (river == null) yield return Approach(task.transform);
                else
                {
                    int index = Array.IndexOf(waste, task);
                    if (index == 5)
                    {
                        yield return MoveTo(Ground(RecoveryRiverLayout.BankPoint(RecoveryRiverLayout.BridgeZ, -1)), 0.6f);
                        yield return MoveTo(Ground(new Vector3(RecoveryRiverLayout.CenterX(RecoveryRiverLayout.BridgeZ), 0f, RecoveryRiverLayout.BridgeZ)), 0.6f);
                        Check("Physical river crossing uses solid bridge deck", rig.transform.position.y > 0.12f);
                        yield return MoveTo(Ground(RecoveryRiverLayout.BankPoint(RecoveryRiverLayout.BridgeZ, 1)), 0.6f);
                    }
                    yield return MoveTo(Ground(task.transform.position + Vector3.back * 1.3f), 0.4f);
                    detector.Scan();
                    Check("Physical bank route selects river trash " + task.ObjectId, detector.Focused == task);
                }
                flow.Pause(); Check("Pause blocks ecosystem task " + task.ObjectId, !task.Interact(actor).Success); flow.Resume();
                yield return PressE(); Check("E cleans waste " + task.ObjectId, task.Done);
                Check("Waste cannot be cleaned twice " + task.ObjectId, !p.CleanWaste(task.ObjectId, true));
                if (river != null && p.Waste < wasteGoal)
                    Check("River stays unfinished until final trash " + task.ObjectId, !p.WaterClean && !river.BanksRecovered);
            }
            Check("Water cleaner does not mean habitats restored", p.WaterClean && p.Habitats == 0 && p.Stage == RecoveryStage.Ecosystem
                && !FindFirstObjectByType<RecoveryEnvironment>().WildlifeReturned);
            yield return null;
            Renderer sourceWater = river == null ? GameObject.Find("Recovery water source").GetComponent<Renderer>() : river.transform.Find("River water surface").GetComponent<Renderer>();
            Check("Water material follows cleaned source targets", sourceWater.sharedMaterial.name.Contains("Cleaner recovery water"));
            if (river != null) Check("Ten cleanups restore riverbanks without completing habitats", river.CleanupFraction == 1f && river.BanksRecovered && p.Habitats == 0);
            yield return Capture("09-water-cleanup.png");
            yield return CaptureWorldDetail(sourceWater.transform, "09a-water-source-detail.png", river == null ? 7f : 22f);
            foreach (RecoveryTask task in tasks.Where(t => t.Kind == RecoveryTaskKind.Habitat))
            {
                yield return Approach(task.transform); yield return PressE(); Check("E restores habitat " + task.ObjectId, task.Done);
                Check("Habitat cannot be restored twice " + task.ObjectId, !p.RestoreHabitat(task.ObjectId, true));
            }
            yield return new WaitForSecondsRealtime(0.2f);
            Check("Wildlife signs only return after water and habitats", p.Stage == RecoveryStage.EcosystemDebrief && p.Fraction == 1f
                && FindFirstObjectByType<RecoveryEnvironment>().WildlifeReturned && flow.Mode == ScreenMode.Playing);
            yield return Capture("10-recovery-habitat.png");
            yield return CaptureWorldDetail(tasks.First(t => t.Kind == RecoveryTaskKind.Habitat).transform, "10a-habitat-detail.png", 4.5f);
            yield return Approach(ranger.transform); yield return PressE();
            yield return AdvanceDialogue(storyHud); yield return AdvanceDialogue(storyHud);
            if (recovery.Restored != null) yield return RunRestored(recovery.Restored, storyHud);
            else
            {
                Check("Recovery closes with milestone not final-story ending", p.Stage == RecoveryStage.Complete && flow.Mode == ScreenMode.Victory
                    && !state.SessionRunning && hud.View.VictoryResult.text.Contains("Zona 1 dan Zona 2 selesai") && hud.View.VictoryResult.text.Contains(IndonesianText.ChapterPending));
                yield return Capture("11-recovery-complete.png");
            }
        }

        private IEnumerator RunRestored(RestoredChapter restored, DisasterHud storyHud)
        {
            profileName = "From Ashes / Three zones / physical final route and riverbanks";
            RestoredProgress p = restored.Progress;
            EndingDirector director = restored.Director;
            RestoredEnvironment world = FindFirstObjectByType<RestoredEnvironment>();
            Check("Recovery routes to time transition not victory", p.Stage == RestoredStage.TimeTransition && flow.Mode == ScreenMode.Transition
                && !state.SessionRunning && !p.HasSeedling && missions.DisasterChapter.Progress.Fires == 10);
            float before = director.SequenceSeconds;
            flow.Pause(); yield return new WaitForSecondsRealtime(0.4f);
            Check("Time transition pauses without gameplay mutation", director.SequenceSeconds <= before + 0.02f && !restored.AcceptBriefing());
            yield return Click(hud.View.Resume);
            Check("Resume returns to transition rather than playing", flow.Mode == ScreenMode.Transition);
            yield return new WaitForSecondsRealtime(3f); yield return Capture("12-time-transition.png");
            float deadline = Time.realtimeSinceStartup + 12f;
            while (!restored.EnvironmentReady && Time.realtimeSinceStartup < deadline) yield return null;
            Check("Time jump preserves prior missions and applies restored world", restored.EnvironmentReady && world.Applied && flow.Mode == ScreenMode.Playing
                && restored.Recovery.Progress.Watered == 5 && restored.Recovery.Progress.Waste == 10 && restored.Recovery.Progress.Habitats == 2);
            Check("Recovered environment owns sun fog and river", RenderSettings.fogDensity < 0.002f && RenderSettings.sun.intensity > 1.4f
                && FindFirstObjectByType<RecoveryRiver>().transform.Find("River water surface").GetComponent<Renderer>().sharedMaterial.name.Contains("RestoredRiver"));
            yield return CaptureWorldDetail(restored.transform, "13-restored-overview.png", 35f);
            ElderDialogue elder = FindFirstObjectByType<ElderDialogue>();
            HumanNpcVisual elderVisual = elder.GetComponentInChildren<HumanNpcVisual>(true);
            Check("Human elder becomes visible only after time transition", elderVisual != null && elderVisual.gameObject.activeInHierarchy
                && elderVisual.GetComponentsInChildren<Renderer>().All(r => r.enabled));
            yield return CaptureWorldDetail(elderVisual.transform, "npc-elder.png", 3f, human: true);
            HumanNpcVisual villager = FindObjectsByType<HumanNpcVisual>(FindObjectsInactive.Include, FindObjectsSortMode.None).First(n => n.CharacterId == "VillagerWoman");
            yield return CaptureWorldDetail(villager.transform, "npc-villager-woman.png", 3f, human: true);
            foreach (HumanNpcVisual npc in FindObjectsByType<HumanNpcVisual>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .GroupBy(n => n.CharacterId).Select(g => g.First()))
                yield return CaptureNpcPortrait(npc);
            FinalPlantingPoint finalBed = restored.FinalBed;
            RestoredViewpoint panorama = restored.Viewpoint;
            Check("Early final actions are locked", !finalBed.Interact(actor).Success && !finalBed.ReceiveWater(3f) && !restored.VisitViewpoint(panorama.ObjectId));
            Check("Final viewpoint rejects out of range", !panorama.Interact(actor).Success);
            yield return MoveTo(Ground(new Vector3(0f, 0f, -33f)), 0.7f);
            yield return MoveTo(Ground(elder.transform.position + Vector3.back * 1.7f), 0.4f);
            detector.Scan(); Check("Physical route finds village elder", detector.Focused == elder);
            yield return PressE();
            Check("Elder has Indonesian recognition dialogue", storyHud.DialogueOpen && storyHud.GetComponentsInChildren<TMPro.TMP_Text>(true).Any(t => t.text.Contains("hutan ini dipenuhi api dan abu")));
            yield return Capture("14-elder-dialogue.png");
            input.EnableTestDriver(false); QueueKeys(Key.Escape); yield return new WaitForSecondsRealtime(0.15f); QueueKeys(); yield return new WaitForSecondsRealtime(0.15f);
            Check("Dismissing elder does not grant final seedling", p.Stage == RestoredStage.ElderBriefing && !p.HasSeedling);
            yield return PressE(); yield return AdvanceDialogue(storyHud); yield return AdvanceDialogue(storyHud); yield return AdvanceDialogue(storyHud);
            Check("Elder grants one dedicated final seedling", p.Stage == RestoredStage.FinalPlanting && p.HasSeedling && restored.Recovery.Progress.Carried == 0
                && !restored.AcceptBriefing());
            yield return MoveTo(Ground(new Vector3(-4f, 0f, -24f)), 0.45f);
            detector.Scan(); Check("Physical final bed approach uses E target", detector.Focused == finalBed);
            yield return PressE(); Check("Final planting consumes the dedicated seed only", p.Planted && !p.HasSeedling && p.Stage == RestoredStage.FinalWatering);
            Check("Viewpoint cannot end story before final watering", !restored.VisitViewpoint(panorama.ObjectId));
            yield return MoveTo(Ground(new Vector3(-4f, 0f, -25.2f)), 0.4f);
            yield return AimAtPoint(finalBed.AimPosition);
            Check("Final sapling is camera aimable", water.AimedFinalTree == finalBed);
            Vector3 nearFinalTree = rig.transform.position;
            rig.Teleport(Ground(finalBed.transform.position + Vector3.back * 11f));
            yield return new WaitForSecondsRealtime(0.2f); yield return AimAtPoint(finalBed.AimPosition);
            Check("Range blocks final tree watering", !water.ApplySpray(0.5f) && finalBed.WaterFraction == 0f);
            rig.Teleport(nearFinalTree); yield return new WaitForSecondsRealtime(0.2f); yield return AimAtPoint(finalBed.AimPosition);
            Transform nozzle = rig.transform.Find("Water nozzle");
            GameObject waterBlock = Blocker((nozzle.position + finalBed.AimPosition) * 0.5f, Vector3.one * 0.35f);
            Check("Obstacle blocks final tree watering", !water.ApplySpray(0.5f) && finalBed.WaterFraction == 0f);
            Destroy(waterBlock); yield return new WaitForSecondsRealtime(0.15f); yield return AimAtPoint(finalBed.AimPosition);
            flow.Pause(); Check("Pause blocks final water", !finalBed.ReceiveWater(0.5f) && finalBed.WaterFraction == 0f); flow.Resume();
            yield return AimAtPoint(finalBed.AimPosition);
            input.EnableTestDriver(false);
            deadline = Time.realtimeSinceStartup + 8f;
            while (!p.Watered && Time.realtimeSinceStartup < deadline) { SetCameraAtPoint(finalBed.AimPosition); QueueMouse(default, true); yield return null; }
            QueueMouse(); input.EnableTestDriver(true); yield return new WaitForSecondsRealtime(0.15f);
            Check("Held mouse waters final sapling once", p.Watered && p.Stage == RestoredStage.Viewpoint && !finalBed.ReceiveWater(1f));
            Check("Final planting does not instantly grow a mature tree", finalBed.transform.Find("Final young sapling").GetComponentsInChildren<Renderer>()
                .Max(r => r.bounds.max.y) - finalBed.transform.position.y < 1.6f);
            yield return Capture("15-final-tree-watered.png");
            yield return MoveTo(Ground(new Vector3(0f, 0f, -20f)), 0.6f);
            yield return MoveTo(Ground(panorama.transform.position + Vector3.back * 1.3f), 0.4f);
            detector.Scan(); Check("Physical route reaches viewpoint after final tree", detector.Focused == panorama);
            yield return PressE();
            Check("Viewpoint starts cinematic with gameplay locked", p.Stage == RestoredStage.Ending && flow.Mode == ScreenMode.Cinematic
                && !state.SessionRunning && !rig.ControlsEnabled && !water.ApplySpray(1f));
            yield return new WaitForSecondsRealtime(2f); yield return Capture("16-ending-first-shot.png");
            before = director.SequenceSeconds;
            flow.Pause(); yield return new WaitForSecondsRealtime(0.5f);
            Check("Ending pause freezes camera sequence", director.SequenceSeconds <= before + 0.02f && flow.Mode == ScreenMode.Paused);
            if (CommandLine.Value("--rimbara-ending-skip") != null)
            {
                yield return Click(director.SkipButton);
                Check("Confirmed cinematic skip gives same final result", p.Stage == RestoredStage.Complete && p.EndingSkipped && flow.Mode == ScreenMode.Victory);
            }
            else
            {
                yield return Click(hud.View.Resume);
                Check("Cinematic resume does not enable player movement", flow.Mode == ScreenMode.Cinematic && !rig.ControlsEnabled);
                yield return new WaitForSecondsRealtime(10f); yield return Capture("17-ending-river-forest.png");
                deadline = Time.realtimeSinceStartup + 40f;
                while (flow.Mode != ScreenMode.Victory && Time.realtimeSinceStartup < deadline) yield return null;
                Check("Normal ending completes only after cinematic", p.Stage == RestoredStage.Complete && !p.EndingSkipped && flow.Mode == ScreenMode.Victory);
            }
            Check("Final result recognizes five missions not chapter-only completion", hud.View.VictoryResult.text.Contains("MISI 01–05 SELESAI")
                && !hud.View.VictoryResult.text.Contains(IndonesianText.ChapterPending) && !state.SessionRunning);
            yield return Capture("18-story-complete.png");
        }

        private void SetCameraAtPoint(Vector3 point)
        {
            Quaternion rotation = Quaternion.LookRotation(point - rig.ViewCamera.transform.position);
            float pitch = Mathf.DeltaAngle(0f, rotation.eulerAngles.x) - rig.OfficialController.CameraAngleOverride;
            rig.SetCameraOrbit(rotation.eulerAngles.y, pitch);
        }

        private IEnumerator AimAtPoint(Vector3 point)
        {
            ClearDriver();
            for (int i = 0; i < 7; i++) { SetCameraAtPoint(point); yield return new WaitForSecondsRealtime(0.12f); }
            water.UpdateAim();
        }

        private IEnumerator ApproachSeedlingForWater(RecoveryPlantingPoint bed)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f;
                Vector3 candidate = Ground(bed.transform.position + new Vector3(Mathf.Sin(angle), 0f, -Mathf.Cos(angle)) * 3f);
                if (!ClearCapsule(candidate)) continue;
                rig.Teleport(candidate); yield return new WaitForSecondsRealtime(0.2f);
                rig.transform.rotation = Quaternion.LookRotation(bed.transform.position - rig.transform.position);
                yield return AimAtPoint(bed.AimPosition);
                if (water.AimedSeedling == bed && water.AimHint == IndonesianText.SeedlingWaterAction) yield break;
            }
            throw new InvalidOperationException("No clear water approach to seedling " + bed.ObjectId + " / " + water.AimHint);
        }

        private IEnumerator Approach(Transform target)
        {
            Vector3 approach = Vector3.zero;
            Vector3 start = Vector3.zero;
            bool found = false;
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f;
                Vector3 direction = new Vector3(Mathf.Sin(angle), 0f, -Mathf.Cos(angle));
                approach = Ground(target.position + direction * 1.4f);
                start = Ground(target.position + direction * 2.2f);
                if (!ClearCapsule(approach) || !ClearCapsule(start)) continue;
                if (FindObjectsByType<InteractableBehaviour>(FindObjectsSortMode.None).Any(other => other.Available && other.transform != target
                    && HorizontalDistance(other.transform.position, start) < HorizontalDistance(target.position, start) + 0.2f)) continue;
                if (Physics.Linecast(approach + Vector3.up * 0.8f, target.position + Vector3.up * 0.55f, 1 << 8, QueryTriggerInteraction.Ignore)) continue;
                found = true;
                break;
            }
            if (!found) throw new InvalidOperationException("No clear motor approach around " + target.name);
            rig.Teleport(start);
            yield return new WaitForSecondsRealtime(0.2f);
            yield return MoveTo(approach, 0.55f);
            detector.Scan();
            Check("Nearby unobstructed interaction selects " + target.name, detector.Focused != null && detector.Focused.transform == target,
                "player=" + rig.transform.position + " target=" + target.position + " focus=" + (detector.Focused == null ? "none" : detector.Focused.name));
        }

        private IEnumerator AdvanceDialogue(DisasterHud storyHud)
        {
            int before = storyHud.DialoguePage;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                yield return Click(storyHud.DialogueNext);
                if (!storyHud.DialogueOpen || storyHud.DialoguePage != before) yield break;
                yield return new WaitForSecondsRealtime(0.2f);
            }
            throw new InvalidOperationException("UI pointer did not advance dialogue page " + before);
        }

        private IEnumerator ApproachFire(FirePoint fire)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f;
                Vector3 candidate = fire.transform.position + new Vector3(Mathf.Sin(angle), 0f, -Mathf.Cos(angle)) * 3.5f;
                RaycastHit surface;
                if (!Physics.Raycast(candidate + Vector3.up * 0.6f, Vector3.down, out surface, 12f, 1 << 8, QueryTriggerInteraction.Ignore)) continue;
                candidate.y = surface.point.y + 0.06f;
                if (!ClearCapsule(candidate)) continue;
                rig.Teleport(candidate);
                yield return new WaitForSecondsRealtime(0.25f);
                yield return AimAt(fire);
                rig.transform.rotation = Quaternion.Euler(0f, rig.ViewCamera.transform.eulerAngles.y, 0f);
                yield return new WaitForSecondsRealtime(0.4f);
                yield return AimAt(fire);
                if (water.AimedFire == fire && water.AimHint == IndonesianText.WaterAction) yield break;
            }
            throw new InvalidOperationException("No clear Slavic water approach to " + fire.ObjectId + " / " + water.AimHint
                + " / player " + rig.transform.position + " / fire " + fire.transform.position);
        }

        private static bool ClearCapsule(Vector3 point)
        {
            // Some team scenery uses Default rather than World. Include both;
            // exclude only the player, and keep solid obstacle collision intact.
            return Physics.OverlapCapsule(point + Vector3.up * 0.65f, point + Vector3.up * 1.5f, 0.4f,
                ~(1 << 9), QueryTriggerInteraction.Ignore).All(c => c is TerrainCollider);
        }

        private IEnumerator MoveTo(Vector3 destination, float radius = 0.5f, bool stopAtVictory = false)
        {
            input.EnableTestDriver(true);
            ClearDriver();
            float deadline = Time.realtimeSinceStartup + 35f;
            while (HorizontalDistance(rig.transform.position, destination) > radius)
            {
                if (stopAtVictory && flow.Mode == ScreenMode.Victory) break;
                if (!state.SessionRunning || Time.realtimeSinceStartup > deadline)
                    throw new InvalidOperationException("Movement route could not reach " + destination + " from " + rig.transform.position);
                Vector3 difference = destination - rig.transform.position;
                difference.y = 0f;
                Vector3 local = Quaternion.Inverse(Quaternion.Euler(0f, rig.ViewCamera.transform.eulerAngles.y, 0f)) * difference.normalized;
                bool run = difference.magnitude > 2f;
                input.SetTestInput(new Vector2(local.x, local.z), Vector2.zero, run, false, false);
                yield return null;
            }
            ClearDriver();
            yield return new WaitForSecondsRealtime(0.18f);
        }

        private IEnumerator PressE()
        {
            ClearDriver();
            input.EnableTestDriver(false);
            QueueKeys(Key.E);
            yield return new WaitForSecondsRealtime(0.12f);
            QueueKeys();
            yield return new WaitForSecondsRealtime(0.12f);
            input.EnableTestDriver(true);
        }

        private IEnumerator Click(Button button)
        {
            Canvas.ForceUpdateCanvases();
            RectTransform rect = (RectTransform)button.transform;
            Vector3 center = rect.TransformPoint(rect.rect.center);
            yield return ClickAt(RectTransformUtility.WorldToScreenPoint(null, center));
        }

        private IEnumerator ClickAt(Vector2 position)
        {
            input.EnableTestDriver(false);
            // Keep the pointer stable across actual update frames. A one-shot
            // event followed by a timed wait can race UI layout and real mice.
            yield return PointerFrames(position, false, 0.2f);
            yield return PointerFrames(position, true, 0.2f);
            yield return PointerFrames(position, false, 0.2f);
        }

        private IEnumerator PointerFrames(Vector2 position, bool pressed, float seconds)
        {
            float until = Time.realtimeSinceStartup + seconds;
            do { QueueMouse(left: pressed, position: position); yield return null; }
            while (Time.realtimeSinceStartup < until);
        }

        private IEnumerator AimAt(FirePoint target)
        {
            input.EnableTestDriver(true);
            ClearDriver();
            for (int i = 0; i < 7; i++)
            {
                Quaternion rotation = Quaternion.LookRotation(target.AimPosition - rig.ViewCamera.transform.position);
                float pitch = Mathf.DeltaAngle(0f, rotation.eulerAngles.x) - rig.OfficialController.CameraAngleOverride;
                rig.SetCameraOrbit(rotation.eulerAngles.y, pitch);
                yield return new WaitForSecondsRealtime(0.12f);
            }
            water.UpdateAim();
        }

        private IEnumerator SprayFor(float seconds)
        {
            ClearDriver();
            input.EnableTestDriver(false);
            QueueMouse(default, true);
            yield return new WaitForSecondsRealtime(seconds);
            QueueMouse();
            yield return new WaitForSecondsRealtime(0.1f);
            input.EnableTestDriver(true);
        }

        private IEnumerator Extinguish(FirePoint target)
        {
            input.EnableTestDriver(false);
            QueueKeys();
            QueueMouse(default, true);
            float deadline = Time.realtimeSinceStartup + state.Profile.ExtinguishSeconds + 5f;
            while (!target.Extinguished && Time.realtimeSinceStartup < deadline)
            {
                if (missions.DisasterChapter != null)
                {
                    // Holding water turns the volunteer/nozzle toward the camera.
                    // Track the target through the resulting Cinemachine movement
                    // rather than assuming a fixed camera on uneven kit ground.
                    Quaternion rotation = Quaternion.LookRotation(target.AimPosition - rig.ViewCamera.transform.position);
                    float pitch = Mathf.DeltaAngle(0f, rotation.eulerAngles.x) - rig.OfficialController.CameraAngleOverride;
                    rig.SetCameraOrbit(rotation.eulerAngles.y, pitch);
                    QueueMouse(default, true);
                }
                yield return null;
            }
            QueueMouse();
            yield return new WaitForSecondsRealtime(0.15f);
            if (!target.Extinguished && missions.DisasterChapter != null) yield return Capture("water-failure.png");
            Check("Water contact completes " + target.ObjectId, target.Extinguished, "Contact " + target.SuppressionFraction.ToString("0.000")
                + " / " + water.AimHint + " / player=" + rig.transform.position + " / target=" + target.transform.position);
            input.EnableTestDriver(true);
        }

        private IEnumerator CapturePostBoard(Transform board)
        {
            yield return CaptureWorldDetail(board, "01c-post-board.png", 3.5f, true);
        }

        private IEnumerator CaptureWorldDetail(Transform subject, string filename, float distance, bool board = false, bool human = false)
        {
            // Diagnostic-only close-up. No changes to player/camera/gameplay state.
            yield return new WaitForEndOfFrame();
            var cameraObject = new GameObject("Smoke-only board evidence camera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = 48f;
            camera.transform.position = subject.position - subject.forward * distance + subject.up * (board ? 0.15f : Mathf.Max(2.8f, distance * 0.55f));
            camera.transform.LookAt(subject.position + subject.up * (board ? -0.1f : 0.15f), subject.up);
            if (human)
            {
                camera.fieldOfView = 40f;
                camera.transform.position = subject.position + subject.forward * distance + Vector3.up * 1.2f;
                camera.transform.LookAt(subject.position + Vector3.up * 1f);
            }
            RenderTexture target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                // Let URP render this auxiliary camera through its normal frame loop.
                camera.enabled = true;
                yield return new WaitForEndOfFrame();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.Combine(output, filename), image.EncodeToPNG());
                captures.Add(filename);
            }
            finally
            {
                camera.enabled = false;
                camera.targetTexture = null;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Destroy(image);
                Destroy(cameraObject);
            }
        }

        private IEnumerator Capture(string filename)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            try { File.WriteAllBytes(Path.Combine(output, filename), image.EncodeToPNG()); }
            finally { Destroy(image); }
            captures.Add(filename);
        }

        private IEnumerator CaptureNpcPortrait(HumanNpcVisual subject)
        {
            // Opt-in evidence clone in an isolated, lit location. No map/player edits.
            Animator source = subject.GetComponentInChildren<Animator>(true);
            GameObject clone = Instantiate(source.gameObject, new Vector3(0f, 500f, 0f), Quaternion.identity);
            clone.SetActive(true);
            foreach (Transform child in clone.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
            Animator animator = clone.GetComponent<Animator>();
            animator.SetFloat("Speed", 0f); animator.SetFloat("MotionSpeed", 1f);
            animator.Update(0.2f);
            var cameraObject = new GameObject("Smoke-only NPC portrait camera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.cullingMask = 1 << 31; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.19f, 0.22f, 0.2f); camera.fieldOfView = 39f;
            camera.transform.position = clone.transform.position + new Vector3(0.4f, 1.25f, 3f);
            camera.transform.LookAt(clone.transform.position + Vector3.up * 0.95f);
            var lightObject = new GameObject("Smoke-only NPC portrait light", typeof(Light));
            Light light = lightObject.GetComponent<Light>(); light.type = LightType.Point;
            light.cullingMask = 1 << 31; light.intensity = 8f; light.range = 10f;
            light.transform.position = clone.transform.position + new Vector3(-1.4f, 2.4f, 2.2f);
            RenderTexture target = RenderTexture.GetTemporary(900, 900, 24, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            var image = new Texture2D(900, 900, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                yield return new WaitForEndOfFrame();
                RenderTexture.active = target; image.ReadPixels(new Rect(0f, 0f, 900f, 900f), 0, 0); image.Apply();
                string filename = "npc-portrait-" + subject.CharacterId + ".png";
                File.WriteAllBytes(Path.Combine(output, filename), image.EncodeToPNG()); captures.Add(filename);
                Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                Check("Animated NPC human proportions: " + subject.CharacterId, head != null && hips != null
                    && head.position.y - hips.position.y > 0.4f && head.position.y - clone.transform.position.y > 1.2f);
            }
            finally
            {
                camera.targetTexture = null; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target);
                Destroy(image); Destroy(cameraObject); Destroy(lightObject); Destroy(clone);
            }
        }

        private static Vector3 Ground(Vector3 position)
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                RaycastHit hit;
                // The kit has stacked mesh surfaces: roads beneath roofs/bridges.
                // Ground the fixture near the authored target's elevation, not
                // onto the highest rooftop hit by a ray from the sky.
                if (!Physics.Raycast(position + Vector3.up * 0.6f, Vector3.down, out hit, 12f, 1 << 8, QueryTriggerInteraction.Ignore))
                    throw new InvalidOperationException("No Slavic world surface beneath runtime fixture: " + position);
                position.y = hit.point.y + 0.06f;
                return position;
            }
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y + 0.06f;
            return position;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private static GameObject Blocker(Vector3 center, Vector3 size)
        {
            var root = new GameObject("Smoke-only obstruction fixture");
            root.layer = 8;
            root.transform.position = center;
            root.AddComponent<BoxCollider>().size = size;
            Physics.SyncTransforms();
            return root;
        }

        private void ClearDriver() { input.SetTestInput(Vector2.zero, Vector2.zero, false, false, false); }
        private void QueueKeys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); }
        private void QueueMouse(Vector2 delta = default, bool left = false, Vector2? position = null)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position ?? new Vector2(40f, 40f), delta = delta }.WithButton(MouseButton.Left, left));
        }

        private void Check(string name, bool passed, string detail = "")
        {
            checks.Add(new CheckResult { Name = name, Passed = passed, Detail = detail });
            if (!passed) throw new InvalidOperationException("Runtime smoke check failed: " + name + " / " + detail);
        }

        private void RecordLog(string condition, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                errors.Add(condition + "\n" + trace);
        }

        private void Finish()
        {
            finished = true;
            if (volumeCaptured && audioSystem != null) audioSystem.SetVolume(originalVolume);
            Application.logMessageReceived -= RecordLog;
            Time.timeScale = 1f;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            foreach (InputDevice device in temporarilyDisabledDevices)
                if (device.added) InputSystem.EnableDevice(device);
            InputSystem.settings.backgroundBehavior = originalBackgroundBehavior;
            float[] sorted = frameTimes.OrderBy(t => t).ToArray();
            bool passed = checks.Count > 0 && checks.All(c => c.Passed) && errors.Count == 0;
            var report = new Report
            {
                Passed = passed, Unity = Application.unityVersion, Profile = profileName, Utc = DateTime.UtcNow.ToString("O"),
                GraphicsDevice = SystemInfo.graphicsDeviceName + " / " + SystemInfo.graphicsDeviceType,
                Width = Screen.width, Height = Screen.height,
                MeanFrameMilliseconds = sorted.Length == 0 ? 0f : sorted.Average(),
                P95FrameMilliseconds = sorted.Length == 0 ? 0f : sorted[Mathf.Min(sorted.Length - 1, Mathf.FloorToInt(sorted.Length * 0.95f))],
                Checks = checks.ToArray(), Captures = captures.ToArray(), Errors = errors.ToArray()
            };
            File.WriteAllText(Path.Combine(output, "report.json"), JsonUtility.ToJson(report, true));
            Debug.Log("RIMBARA runtime smoke " + (passed ? "PASSED" : "FAILED") + ": " + checks.Count + " checks; " + output);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(passed ? 0 : 1);
#endif
        }
    }
}
