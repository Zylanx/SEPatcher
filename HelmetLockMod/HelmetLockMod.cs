using System;
using System.Linq;
using System.Reflection;
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Harmony;
using SEModLoader;
using UnityEngine;

namespace HelmetLockMod
{
    // HelmetLockMod: A simple mod that adds a button to the controls menu
    //                to lock or unlock a helmet with a single keypress
    public class HelmetLockMod : MonoBehaviour, IMod // The IMod just makes it easy to find a valid class
    {
        // For now, just make the mod a Singleton. This may change in future versions
        
        // Instance variable and lock for (Thread-safe) Singleton pattern
        public static HelmetLockMod Instance;
        private static object _instanceLock = new object();

        // The name for the mod
        public static string ModName = "HelmetLockMod";

        public static KeyCode DefaultLockKey = KeyCode.U;

        // Stores the last state of the key for debouncing
        public bool _lastButtonState;

        // Init: The init function handles instantiating a gameobject
        //       to contain the mod and to also register it with Unity
        public static void Init()
        {
            lock (_instanceLock)
            {
                if (Instance == null)
                {
                    // Create the GameObject and keeps it alive through scene changes
                    Debug.Log("HelmetLockMod: HelmetLockMod Loaded");
                    GameObject go = new GameObject();
                    go.name = ModName;
                    Instance = go.AddComponent<HelmetLockMod>();
                    DontDestroyOnLoad(go);

                    // Activate the Harmony Patcher to register the key
                    var harmony = HarmonyInstance.Create("com.zylanx.helmetlockmod");
                    harmony.PatchAll(Assembly.GetExecutingAssembly());

                    // Hacky way to get the game to load in our new key
                    // NOTE: it is possible that this could override settings
                    //       if the mod is loaded after the user has changed settings.
                    // Once the modloader is more developed, it will have a proper way to
                    // register keys as well as manage mod settings
                    Settings.LoadSettings();
                }
            }
        }

        // Unity has initialised the object. Set the initial values
        public void Awake()
        {
            _lastButtonState = false;
        }

        // Unity controlled.
        // Update: Each update, check if the game is running,
        //         if the key is pressed, and then toggles the lock state of the helmet
        //         as needed
        public void Update()
        {
            if (GameManager.GameState == GameState.Running)
            {
                // If the button is being pressed but was not pressed previously
                if (KeyManager.GetButtonDown(KeyManager.GetKey("Lock Helmet")))
                {
                    if (_lastButtonState == false)
                    {
                        // Find the local player
                        var player = Human.AllHumans.FirstOrDefault(human => human.IsLocalPlayer);

                        if (player == null)
                        {
                            Debug.LogError("HelmetLockMod: Could not find local player");
                        }
                        else
                        {
                            // If the helmet slot has a helmet
                            if (player.HelmetSlot.Occupant)
                            {
                                var helmet = player.HelmetSlot.Occupant;

                                // Check the helmet can be locked
                                if (helmet.HasLockState)
                                {
                                    // Print a string to the console telling the player what is being done
                                    var consoleString = String.Format("{0}ing {1}...",
                                        helmet.IsLocked ? ActionStrings.Unlock : ActionStrings.Lock,
                                        helmet.DisplayName);

                                    ConsoleDebug.AddText(String.Format("<color=yellow>{0}</color>", consoleString));

                                    // Get the index of the "Lock Item" action for the helmet
                                    // Then tell the player to send a command to the helmet to toggle its lock
                                    var helmetLockIndex = helmet.InteractLock.InteractableId;
                                    player.CallCmdInteractWith(helmetLockIndex, helmet.netId, player.netId, player.HelmetSlot.SlotId, false);
                                }
                            }
                        }
                    }
                }

                // Update the buttons last state
                _lastButtonState = KeyManager.GetButton(KeyManager.GetKey("Lock Helmet"));
            }
        }
    }

    // This is a Harmony Patch Class.
    // See the Harmony Github Wiki for more information
    [HarmonyPatch(typeof(KeyManager))]
    [HarmonyPatch("SetDefaultKeyboard")]
    class Patch_KeyManager_SetDefaultKeyboard
    {
        static void Postfix()
        {
            // Call the private method KeyManager.AddKey then refresh the input screen
            typeof(KeyManager).GetMethod("AddKey", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { "Lock Helmet", HelmetLockMod.DefaultLockKey });
            KeyManager.RefreshStatusKeys();
        }
    }
}
