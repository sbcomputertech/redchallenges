using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
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
        public const string ModVersion = "1.0.0";
        public static bool menuEnabled = false;
        internal Harmony? Harmony;
        public static ManualLogSource? mLog;
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
    /* Harmony patches modify the game code at runtime
     * Official website: https://harmony.pardeike.net/
     * Introduction: https://harmony.pardeike.net/articles/intro.html
     * API Documentation: https://harmony.pardeike.net/api/index.html
     */

    // Here's the example of harmony patch
    [HarmonyPatch(typeof(VersionNumberTextMesh), nameof(VersionNumberTextMesh.Start))]
    /* We're patching the method "Start" of class VersionNumberTextMesh
    * The first argument can typeof(class) or class name (string). Warning: it's case-sensitive
    * The second argument is our method. It can be a nameof(class.method) or method name (string). Also case-sensitive
    * So, for example, patch can look like this:
    * [HarmonyPatch("VersionNumberTextMesh", "Start")]
    * Or like this:
    * [HarmonyPatch(typeof(VersionNumberTextMesh), nameof(VersionNumberTextMesh.Start))
    */
    public class VersionNumberTextMeshPatch
    {
        // Postfix is called after executing target method's code.
        [HarmonyPostfix]
        public static void VersionNumberStuff(VersionNumberTextMesh __instance)
        {
            // We're adding new line to version text.
            __instance.textMesh.text += $"\n<color=red>{Main.ModName} v{Main.ModVersion} by {Main.ModAuthor}</color>";
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
                    if (r.Next(5) == 0) amountToSpawn++;
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
                manager.lastWeapon = null;

                Random rand = new Random();
                bool rareWeapon = rand.Next(5) == 0;

                GameObject spawnedWeapon = Object.Instantiate(SurvivalMode.instance.GetRandomWeapon(rareWeapon),
                    __instance.transform.position, __instance.transform.rotation);

                NetworkObject component = spawnedWeapon.GetComponent<NetworkObject>();
                component.Spawn(true);
                component.DestroyWithScene = true;

                Weapon weapon = spawnedWeapon.GetComponent<Weapon>();
                manager.EquipWeapon(weapon);
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
        }
    }
}
