using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace ChallengeMod
{
    [BepInPlugin(ModName, ModGUID, ModVersion)]
    [BepInProcess("SPIDERHECK.exe")]
    public class Main : BaseUnityPlugin
    {
        public const string ModName = "Challenges";
        public const string ModAuthor  = "reddust9";
        public const string ModGUID = "me.rd9.challengemod";
        public const string ModVersion = "1.1.0";
        public static bool menuEnabled = false;
        public static bool enemyAiDisabled = false;
        public static bool scrMode = false;
        internal Harmony? Harmony;
        public static ManualLogSource mLog = new ManualLogSource("error: mod uninitialized");
        private int timeToSwitchWeapon = 500;
        internal void Awake()
        {
            // Creating new harmony instance
            Harmony = new Harmony(ModGUID);

            // Applying patches
            Harmony.PatchAll();
            mLog = Logger;
            mLog.LogInfo($"{ModName} successfully loaded! Made by {ModAuthor}");

            DataStore.ChallengeType = ChallengeType.ENEMY_ON_JUMP;
            Config.Bind(
                new ConfigDefinition(
                    "Challenge settings", "Challenge type (LEGACY):"
                ),
                DataStore.ChallengeType
            ).SettingChanged += new EventHandler(DoDataStoreChange);
            Config.Bind<KeyboardShortcut>(
                new ConfigDefinition(
                        "Challenge settings", "Menu keybind:"
                    ),
                    new KeyboardShortcut(KeyCode.T, new KeyCode[0])
                ).SettingChanged += new EventHandler(DoHotKeyChange);
            Config.Bind<bool>(
                new ConfigDefinition(
                    "Challenge settings", "Scribblzz mode:"
                ),
                false
            ).SettingChanged += new EventHandler(ScribblzzModeToggle);
        }
        internal void FixedUpdate()
        {
            if(timeToSwitchWeapon == 0)
            {
                DataStore.shouldSwitchWeapon = true;
                timeToSwitchWeapon = 500;
            } else
            {
                DataStore.shouldSwitchWeapon = false;
                timeToSwitchWeapon--;
            }
            if (Utils.GetControl(DataStore.MenuHotkey) != null && Utils.GetControl(DataStore.MenuHotkey).wasPressedThisFrame)
            {
                base.Logger.LogInfo(string.Format("Menu Toggled: {0}", menuEnabled));
                menuEnabled = !menuEnabled;
            }
            if(Utils.GetControl(KeyCode.F9).wasPressedThisFrame)
            {
                // nerf enemy ai
                enemyAiDisabled = !enemyAiDisabled;
            }
        }
        public static void DoDataStoreChange(object sender, EventArgs eargs)
        {
            SettingChangedEventArgs args = (SettingChangedEventArgs)eargs;
            DataStore.ChallengeType = (ChallengeType)args.ChangedSetting.BoxedValue;
            if(DataStore.ChallengeType == ChallengeType.RANDOM)
            {
                DataStore.ChallengeType = CHHelper.GetRandom();
            }
        }
        public static void ScribblzzModeToggle(object sender, EventArgs eargs)
        {
            SettingChangedEventArgs args = (SettingChangedEventArgs)eargs;
            if((bool)args.ChangedSetting.BoxedValue)
            {
                Main.mLog.LogInfo("Scribblzz mode on!");
                Main.scrMode = true;
            } else
            {
                Main.mLog.LogInfo("Scribblzz mode off!");
                Main.scrMode = false;
            }
        }
        public static void DoDataStoreChange(ChallengeType type)
        {
            DataStore.ChallengeType = type;
            if (DataStore.ChallengeType == ChallengeType.RANDOM)
            {
                DataStore.ChallengeType = CHHelper.GetRandom();
            }
        }
        public static void DoHotKeyChange(object sender, EventArgs eargs)
        {
            mLog.LogWarning("HOTKEY CHANGE!!!");
            SettingChangedEventArgs args = (SettingChangedEventArgs)eargs;
            DataStore.MenuHotkey = ((KeyboardShortcut)args.ChangedSetting.BoxedValue).MainKey;
            mLog.LogInfo("<Keyboard>/#(" + DataStore.MenuHotkey.ToString() + ")");
        }
    }
    [HarmonyPatch(typeof(SurvivalModeHud), nameof(SurvivalModeHud.UpdateLivesCounter))]
    public class PatchLivesText
    {
        public static float baseFS;

        [HarmonyPrefix]
        public static bool LivesNumberStuff(ref SurvivalModeHud __instance)
        {
            if (SurvivalMode.instance.EndlessSurvivalActive() && DataStore.ChallengeType == ChallengeType.BULLET_HELL)
            {
                __instance.livesCounter.transform.parent.gameObject.SetActive(true);
                __instance.livesCounter.text = "One";
                __instance.livesCounter.fontSize = baseFS - 6;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(SurvivalModeHud), nameof(SurvivalModeHud.Awake))]
    public class SMHAwakePatch
    {
        [HarmonyPostfix]
        public static void SetBaseFS(ref SurvivalModeHud __instance)
        {
            PatchLivesText.baseFS = __instance.livesCounter.fontSize;
        }
    }

    [HarmonyPatch(typeof(FlashLight), nameof(FlashLight.Use))]
    public class FlashLightUsePatch
    {
        [HarmonyPrefix]
        public static bool F_lashlight(bool use, ref FlashLight __instance)
        {
            if(use)
            {
                Log.print("F in the lashlight");
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SpiderController), nameof(SpiderController.FixedUpdate))]
    public class SCFU
    {
        public static void Postfix(ref SpiderController __instance)
        {
            // ok i added this by mistake but i might need it later so it'll stay
        }
    }

    [HarmonyPatch(typeof(SpiderController), nameof(SpiderController.Jump))]
    public class SpiderControllerJumpPatch
    {
        [HarmonyPrefix]
        public static void EditJumpForce(ref SpiderController __instance)
        {
            //Log.print("Jumpforce: " + __instance.jumpForce);
            if (DataStore.ChallengeType == ChallengeType.JUMP_PLUS_50_PERCENT)
            {
                if (__instance.jumpForce <= 99000)
                {
                    __instance.jumpForce *= 1.5f;
                }
            } else
            {
                __instance.jumpForce = 4500; // default spiderheck jump force
            }
        }
    }
    [HarmonyPatch(typeof(SpiderController), nameof(SpiderController.OnJump))]
    public class SpiderControllerOnJumpPatch
    {
        public static int amountToSpawn = 1;

        [HarmonyPostfix]
        public static void SpawnEnemiesOnJump(InputValue input, ref SpiderController __instance)
        {
            if (input.isPressed && DataStore.ChallengeType == ChallengeType.ENEMY_ON_JUMP)
            {
                try
                {
                    for(int i = 0; i < amountToSpawn; i++) {
                        EnemySpawner.instance.SpawnRandomEnemy();
                    }
                    Random r = new();
                    if (!Main.scrMode && r.Next(5) == 0) amountToSpawn++;
                }
                catch { }
            }
        }
    }
    [HarmonyPatch(typeof(ParticleBlade), nameof(ParticleBlade.TryResizeSaber))]
    public class SuperLongBlade
    {
        [HarmonyPostfix]
        public static void Override(Vector2 newSize, ref ParticleBlade __instance)
        {
            if (DataStore.ChallengeType == ChallengeType.BIG_PARTICLE_BLADES)
            {
                Vector2 vector = new(50, 1000);
                __instance.particleField.Resize(vector);
                __instance._currentSize = vector;
            }
        }
    }
    [HarmonyPatch(typeof(PickUpVacuum),nameof(PickUpVacuum.FixedUpdate))]
    public class PickUpVacuumFixedUpdatePatch
    {
        [HarmonyPostfix]
        public static void AddDropWeaponCheck(ref PickUpVacuum __instance)
        {
            SpiderWeaponManager manager = __instance.spiderWeaponManager;
            if(manager.equippedWeapon && DataStore.shouldSwitchWeapon && DataStore.ChallengeType == ChallengeType.WEAPON_SWITCHING)
            {
                manager.ThrowWeapon();
                manager.lastWeapon.Delete();

                Random rand = new();
                bool rareWeapon = rand.Next(5) == 0;

                GameObject spawnedWeapon = Object.Instantiate(SurvivalMode.instance.GetRandomWeapon(rareWeapon),
                    __instance.transform.position, __instance.transform.rotation);

                Main.mLog.LogInfo("Weapon to spawn: " + spawnedWeapon.name);

                NetworkObject component = spawnedWeapon.GetComponent<NetworkObject>();
                component.Spawn(true);
                component.DestroyWithScene = true;

                Main.mLog.LogInfo("Ran network spawn");

                Weapon weapon = spawnedWeapon.GetComponent<Weapon>();
                manager.EquipWeapon(weapon);

                Main.mLog.LogInfo("Equipped weapon");
            }
        }
    }
    [HarmonyPatch(typeof(GameController), nameof(GameController.Awake))]
    public class GCAwakepatch
    {
        [HarmonyPrefix]
        public static void AddGuiScript(ref GameController __instance)
        {
            __instance.gameObject.AddComponent(typeof(GuiMonoBehaviour));
            __instance.gameObject.AddComponent(typeof(BulletHellController));
        }
    }
    [HarmonyPatch(typeof(EnemyBrain), nameof(EnemyBrain.FixedUpdate))]
    public class NerfEnemies
    {
        public static void Postfix(ref EnemyBrain __instance)
        {
            __instance.enabled = !Main.enemyAiDisabled;
        }
    }
    public class BulletHellController : MonoBehaviour
    {
        public int enemyTimer = 0;
        public int enTime;
        public void Awake()
        {
            Main.mLog.LogInfo("BulletHellController active!");
        }
        public void FixedUpdate()
        {
            if (DataStore.ChallengeType != ChallengeType.BULLET_HELL) return;

            enTime = Main.scrMode ? 500 : 400;

            if (enemyTimer > enTime)
            {
                EnemySpawner.instance.SpawnRandomEnemy();
                enemyTimer = 0;
            } else
            {
                enemyTimer++;
            }

            Grenade[] bullets = FindObjectsOfType<Grenade>();
            foreach (Grenade bullet in bullets)
            {
                bullet._timeToExplode = 10;
            }



            SurvivalMode.instance.Lives = 1;
        }
    }
    [HarmonyPatch(typeof(SpiderController), nameof(SpiderController.HandleGroundMovement))]
    public class SCHGM
    {
        [HarmonyPrefix]
        public static void Patch(ref float x, ref float y)
        {
            if(DataStore.ChallengeType == ChallengeType.INVERTED_CTRL)
            {
                if(!Main.scrMode) y = -y;
                x = -x;
            }
        }
    }
}
