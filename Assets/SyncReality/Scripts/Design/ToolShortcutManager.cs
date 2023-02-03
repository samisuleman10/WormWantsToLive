using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

#if UNITY_EDITOR

using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;

// NOTE: Currently there is no (documented) way to use two modifiers shortcuts when creating the action with
// with the [Shortcut] attribute. On the documentation there is nothing specific.
// There may be a workaround to find, if we debug the system shortcuts, and we recreate them
// swapping the old keycode with a new one

/// <summary>
/// Struct wrapper for Unity's ShortcutArguments
/// </summary>
[System.Serializable]
public struct CustomShortcutArguments
{
    private ShortcutArguments shortcutArgs;
    public object context
    {
        get {return shortcutArgs.context; }
    }
    public ShortcutStage stage
    {
        get { return shortcutArgs.stage; }
    }
    public KeyCode keyCode;
    public CustomShortcutArguments (ShortcutArguments _shortcutArgs, UnityEngine.KeyCode _keyCode)
    {
        shortcutArgs = _shortcutArgs;
        keyCode = _keyCode;
    }
}
 
 
/// <summary>
/// This class extends unity events with shortcut args
/// </summary>
[System.Serializable]
public class UnityEventArgs : UnityEvent<CustomShortcutArguments> {}


/// <summary>
/// Read instruction in the Shortcut #region at the end of the file
/// </summary>
[ExecuteInEditMode, UnityEditor.InitializeOnLoad]
public class ToolShortcutManager : MonoBehaviour
{
#region Initialization
    // ------------ KeyCode as key ------ Context as key -- UnityEventArgs as value ----------
    private Dictionary <string, Dictionary <string, UnityEventArgs>> m_shortcutArgsDictionary;

    private Dictionary <System.Reflection.MethodInfo, String> m_reverseSearchDictionary;
    private static ToolShortcutManager m_instance;

    private const string m_globalContextString = "UnityEditor.ShortcutManagement.ContextManager+GlobalContext";

    public static ToolShortcutManager Instance
    {
        get
        {
            if (!m_instance)
            {
                m_instance = FindObjectOfType (typeof (ToolShortcutManager)) as ToolShortcutManager;
                if (!m_instance)
                {
                    //    Debug.LogError ("There needs to be one active ToolShortcutManager script on a GameObject in your scene.");
                }
                else
                {
                    m_instance.Init (); 
                }
            }
            return m_instance;
        }
    }

    /// <summary>
    /// Initialize a new keyboard profile for SRPreviewWindow
    /// </summary>
    /// <param name="profileName"></param>
    public static void InitKeyboardProfile (string profileName = "SRDefault")
    {
        if (!ShortcutManager.instance.GetAvailableProfileIds().Contains(profileName)) {
            var currentProfile = ShortcutManager.instance.activeProfileId;
            ShortcutManager.instance.CreateProfile(profileName);
            ShortcutManager.instance.activeProfileId = profileName;

            string tempStringKey;
            foreach (string shortcutId in ShortcutManager.instance.GetAvailableShortcutIds()) {
                tempStringKey = ShortcutManager.instance.GetShortcutBinding(shortcutId).ToString();
                if (!shortcutId.Contains("Syncreality") && !(tempStringKey.Contains("CTRL") || tempStringKey.Contains("⌘"))) 
                {
                    ShortcutManager.instance.RebindShortcut(shortcutId, ShortcutBinding.empty);
                }
            }

            RevertKeyboardProfileToUnityDefault();
        }
    }
    public static void SelectKeyboardProfile (string profileName = "SRDefault")
    {
        if (!ShortcutManager.instance.GetAvailableProfileIds().Contains(profileName)) {
            InitKeyboardProfile (profileName);
        }
        ShortcutManager.instance.activeProfileId = profileName;
    }
    public static void RevertKeyboardProfileToUnityDefault ()
    {
        SelectKeyboardProfile("Default");
    }

    void Start()
    {
        // workaround for restoring the default profile when loading a scene in editor
        RevertKeyboardProfileToUnityDefault();
    }

#endregion

#region Private Methods

    /// <summary>
    /// Initialize event dictionaries
    /// </summary>
    private void Init ()
    {
        if (m_shortcutArgsDictionary == null)
        {
            m_shortcutArgsDictionary = new Dictionary<string, Dictionary <string, UnityEventArgs>>();
            m_reverseSearchDictionary = new Dictionary<System.Reflection.MethodInfo, String>();
        }
    }

    private void OnDestroy ()
    {
        RevertKeyboardProfileToUnityDefault ();
    }

    private void OnDisable ()
    {
        RevertKeyboardProfileToUnityDefault ();
    }

    /// <summary>
    /// Registering the event
    /// </summary>
    /// <param name="keyCode"></param>
    /// <param name="listener"></param>
    private static void StartListening (string keyCode, UnityAction<CustomShortcutArguments> listener, string context)
    {
        if (Instance != null)
        {
            UnityEventArgs thisEvent = null;
            Dictionary <string, UnityEventArgs> subDictionary = null;
            if (Instance.m_shortcutArgsDictionary.TryGetValue (keyCode, out subDictionary))
            {
                if (subDictionary.TryGetValue(context, out thisEvent))
                {
                    thisEvent.AddListener (listener);
                }
                else 
                {
                    thisEvent = new UnityEventArgs ();
                    thisEvent.AddListener (listener);
                    subDictionary.Add (context, thisEvent);
                }
            } 
            else
            {
                subDictionary = new Dictionary<string, UnityEventArgs> ();
                thisEvent = new UnityEventArgs ();
                thisEvent.AddListener (listener);
                subDictionary.Add (context, thisEvent);
                Instance.m_shortcutArgsDictionary.Add (keyCode,subDictionary);   
            }

            if (!Instance.m_reverseSearchDictionary.ContainsKey(listener.Method))
            {
                Instance.m_reverseSearchDictionary.Add(listener.Method, keyCode);
            }
            
        } 
    }

    private static void DebugListeners ()
    {
        var testString = "";
        foreach (KeyValuePair<string, Dictionary<string, UnityEventArgs>> pair in Instance.m_shortcutArgsDictionary)
        {
            testString += "-----" + pair.Key.ToString() + "-----\n";
            foreach (KeyValuePair<string, UnityEventArgs> subPair in pair.Value)
            {
                testString += "| " + subPair.Key.ToString() + " | ";
                for (int i = 0; i < subPair.Value.GetPersistentEventCount(); i++)
                {
                    testString += subPair.Value.GetPersistentMethodName(i) + " | ";
                }
                testString += "\n";
            }
        }
        Debug.Log (testString);
    }

    /// <summary>
    /// Stop listening for events from 
    /// </summary>
    /// <param name="keyCode"></param>
    /// <param name="listener"></param>
    /// <param name="context"></param>
    private static void StopListening (string keyCode, UnityAction<CustomShortcutArguments> listener, string context)
    {
        if (m_instance == null) return;
        UnityEventArgs thisEvent = null;
        Dictionary <string, UnityEventArgs> subDictionary = null;
        if (Instance.m_shortcutArgsDictionary.TryGetValue (keyCode, out subDictionary))
        {
            if (subDictionary.TryGetValue (keyCode, out thisEvent))
            {
                thisEvent.RemoveListener (listener);
            }
        }
    }

    /// <summary>
    /// Triggers the event when the shortcut is pressed
    /// </summary>
    /// <param name="args"></param>
    /// <param name="keyCode"></param>
    /// <param name="keyCode2"></param>
    /// <param name="keyCode3"></param>
    private static void TriggerEvent (ShortcutArguments args, KeyCode keyCode, KeyCode? keyCode2 = null, KeyCode? keyCode3 = null)
    {
        if (m_instance == null) return;
        // Composing the key combination string
        var tempStringKey = keyCode.ToString () + keyCode2 ?? "" + keyCode3 ?? "";
        // Filtering the context string with regular expression
        var tempContext = ProcessContextString(args.context.ToString());
        
        UnityEventArgs thisEvent = null;
        Dictionary <string, UnityEventArgs> subDictionary = null;
        if (Instance.m_shortcutArgsDictionary.TryGetValue (tempStringKey, out subDictionary))
        {
            if (subDictionary.TryGetValue (tempContext, out thisEvent))
            {
                thisEvent.Invoke (new CustomShortcutArguments(args, keyCode));
            }
        }
    }

    /// <summary>
    /// Tool to sanitize the context string
    /// </summary>
    /// <param name="contextString"></param>
    /// <returns></returns>
    private static string ProcessContextString (string contextString)
    {
        Regex rgx = new Regex("[^a-zA-Z0-9 -]");
        var tempContext = "" + contextString.Trim();
        tempContext = rgx.Replace(tempContext, "");
        tempContext = tempContext.Replace("UnityEditor", "");
        return tempContext;
    }
#endregion

#region Public Methods
    /// <summary>
    /// Start Listening for shortcut events. Specify modifiers keys before normal keycode
    /// </summary>
    /// <param name="methodToRegister">method to call</param>
    /// <param name="keyCode">at least one keycode must be specified</param>
    /// <param name="keyCode2">optional keycode</param>
    /// <param name="keyCode3"></param>
    public static void StartListening (UnityAction<CustomShortcutArguments> methodToRegister, KeyCode keyCode, KeyCode? keyCode2 = null, KeyCode? keyCode3 = null)
    {
        var tempStringKey = keyCode.ToString () + keyCode2 ?? "" + keyCode3 ?? "";
        StartListening (tempStringKey, methodToRegister, context: ProcessContextString(m_globalContextString));
    }

    /// <summary>
    /// Start Listening for shortcut events. Specify context and modifiers keys before normal keycode
    /// </summary>
    /// <param name="methodToRegister">method to call</param>
    /// <param name="context">context where the shortcut is valid. Example "SceneView"</param>
    /// <param name="keyCode">at least one keycode must be specified</param>
    /// <param name="keyCode2">optional keycode</param>
    /// <param name="keyCode3"></param>
    public static void StartListening (UnityAction<CustomShortcutArguments> methodToRegister, string context, KeyCode keyCode, KeyCode? keyCode2 = null, KeyCode? keyCode3 = null)
    {
        var tempStringKey = keyCode.ToString () + keyCode2 ?? "" + keyCode3 ?? "";
        StartListening (tempStringKey, methodToRegister, context);
    }

    /// <summary>
    /// Gets the registerd key registered for the specified method
    /// </summary>
    /// <param name="methodToSearch"></param>
    /// <returns>Key in string format</returns>
    public static string GetListenedKeycode (UnityAction<CustomShortcutArguments> methodToSearch)
    {
        string returnValue = "null";

        if (Instance != null)
            Instance.m_reverseSearchDictionary.TryGetValue (methodToSearch.Method, out returnValue);

        return returnValue;
    }

    /// <summary>
    /// Stop Listening for shortcut events. Specify modifiers keys before normal keycode
    /// </summary>
    /// <param name="methodToRegister"></param>
    /// <param name="context">context where the shortcut is valid. Example "SceneView"</param>
    /// <param name="keyCode"></param>
    /// <param name="keyCode2"></param>
    /// <param name="keyCode3"></param>
    public static void StopListening (UnityAction<CustomShortcutArguments> methodToRegister, KeyCode keyCode, KeyCode? keyCode2 = null, KeyCode? keyCode3 = null)
    {
        var tempStringKey = keyCode.ToString () + keyCode2 ?? "" + keyCode3 ?? "";
        StopListening (tempStringKey, methodToRegister, context: ProcessContextString(m_globalContextString));
    }

    /// <summary>
    /// Stop Listening for shortcut events. Specify context and modifiers keys before normal keycode
    /// </summary>
    /// <param name="methodToRegister"></param>
    /// <param name="keyCode"></param>
    /// <param name="keyCode2"></param>
    /// <param name="keyCode3"></param>
    public static void StopListening (UnityAction<CustomShortcutArguments> methodToRegister, string context, KeyCode keyCode, KeyCode? keyCode2 = null, KeyCode? keyCode3 = null)
    {
        var tempStringKey = keyCode.ToString () + keyCode2 ?? "" + keyCode3 ?? "";
        StopListening (tempStringKey, methodToRegister, context);
    }
#endregion


#region Shortcut Methods definitions

    /// <summary>
    /// Add new shortcuts at the end of the #region Shortcut Methods definitions
    /// If you add no context, shortcuts become Global and appear as Dust-Gray-Pink color in Unity Shortcut window
    /// If you add a context, shortcuts become Contextual and appear as Cyan-Gray color in Unity Shortcut window
    /// To add a context, you need to use for example  typeof(SRPreviewWindow), or  typeof(SceneView)
    /// </summary>
    
    [Shortcut("Syncreality/ToggleSynclinkMode", typeof(SceneView), KeyCode.G)]
    public static void ToolWindowToggleAlternateMode (ShortcutArguments args)
    {
        TriggerEvent (args, KeyCode.G);
    }
    
    [Shortcut("Syncreality/LayoutAreaUseX", typeof(SceneView),KeyCode.B)]
    public static void LayoutAreaUseX (ShortcutArguments args)
    {
        TriggerEvent (args, KeyCode.B);
    }

   // [Shortcut("Syncreality/OpenToolWindow", KeyCode.T, UnityEditor.ShortcutManagement.ShortcutModifiers.Shift)]
    public static void OpenToolWindow (ShortcutArguments args)
    {
        TriggerEvent (args, KeyCode.LeftShift, KeyCode.T);
    }

    [Shortcut("Syncreality/SaveSyncSetup", KeyCode.S, UnityEditor.ShortcutManagement.ShortcutModifiers.Shift)]
    public static void SaveSyncSetup (ShortcutArguments args)
    {
        TriggerEvent (args, KeyCode.LeftShift, KeyCode.S);
    }

    [Shortcut("Syncreality/Toggle snap with mouse", typeof(SceneView), KeyCode.C)]
    public static void SnapOrLayoutMode (ShortcutArguments args)
    {
        TriggerEvent (args, KeyCode.C);
    }

    [ClutchShortcut("Syncreality/SRPreviewWindow - Forward", typeof(CustomEditorWindow), KeyCode.W)]
    public static void SRPreviewWindowForward (ShortcutArguments args)
    {
        TriggerEvent (args, KeyCode.W);
    }
    [ClutchShortcut("Syncreality/SRPreviewWindow - Left", typeof(CustomEditorWindow), KeyCode.A)]
    public static void SRPreviewWindowLeft (ShortcutArguments args)
    {
        TriggerEvent (args, KeyCode.A);
    }
    [ClutchShortcut("Syncreality/SRPreviewWindow - Backward", typeof(CustomEditorWindow), KeyCode.S)]
    public static void SRPreviewWindowBackward (ShortcutArguments args)
    {
        TriggerEvent (args, KeyCode.S);
    }
    [ClutchShortcut("Syncreality/SRPreviewWindow - Right", typeof(CustomEditorWindow), KeyCode.D)]
    public static void SRPreviewWindowRight (ShortcutArguments args)
    {
        TriggerEvent (args, KeyCode.D);
    }
    [ClutchShortcut("Syncreality/SRPreviewWindow - Rotate Lights", typeof(CustomEditorWindow), KeyCode.Q)]
    public static void SRPreviewWindowRotateLights (ShortcutArguments args)
    {
        TriggerEvent (args, KeyCode.Q);
    }

#endregion



}

#endif