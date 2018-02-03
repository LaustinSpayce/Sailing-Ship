using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FMODUnity
{
    [AddComponentMenu("")]
    public class RuntimeManager : MonoBehaviour
    {
        static SystemNotInitializedException initException = null;
        static RuntimeManager instance;
        static bool isQuitting = false;
        [SerializeField]
        FMODPlatform fmodPlatform;
        static RuntimeManager Instance
        {
            get
            {
                if (initException != null)
                {
                    throw initException;
                }
                if (isQuitting)
                {
                    throw new Exception("FMOD Studio attempted access by script to RuntimeManager while application is quitting");
                }

                if (instance == null)
                {
                    FMOD.RESULT initResult = FMOD.RESULT.OK; // Initialize can return an error code if it falls back to NO_SOUND, throw it as a non-cached exception

                    var existing = FindObjectOfType(typeof(RuntimeManager)) as RuntimeManager;
                    if (existing != null)
                    {
                        // Older versions of the integration may have leaked the runtime manager game object into the scene,
                        // which was then serialized. It won't have valid pointers so don't use it.
                        if (existing.cachedPointers[0] != 0)
                        {
                            instance = existing;
                            instance.studioSystem.handle = ((IntPtr)instance.cachedPointers[0]);
                            instance.lowlevelSystem.handle = ((IntPtr)instance.cachedPointers[1]);
                            return instance;
                        }
                    }

                    var gameObject = new GameObject("FMOD.UnityIntegration.RuntimeManager");
                    instance = gameObject.AddComponent<RuntimeManager>();
                    DontDestroyOnLoad(gameObject);
                    gameObject.hideFlags = HideFlags.HideInHierarchy;

                    try
                    {
                        #if UNITY_ANDROID && !UNITY_EDITOR
            
                        // First, obtain the current activity context
                        AndroidJavaObject activity = null;
                        using (var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                        {
                            activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                        }

                        using (var fmodJava = new AndroidJavaClass("org.fmod.FMOD"))
                        {
                            if (fmodJava != null)
                            {
                                fmodJava.CallStatic("init", activity);
                            }
                            else
                            {
                                UnityEngine.Debug.LogWarning("FMOD Studio: Cannot initialize Java wrapper");
                            }
                        }
                        
                        #endif

                        RuntimeUtils.EnforceLibraryOrder();
                        initResult = instance.Initialize();
                    }
                    catch (Exception e)
                    {
                        initException = e as SystemNotInitializedException;
                        if (initException == null)
                        {
                            initException = new SystemNotInitializedException(e);
                        }
                        throw initException;
                    }

                    if (initResult != FMOD.RESULT.OK)
                    {
                        throw new SystemNotInitializedException(initResult, "Output forced to NO SOUND mode");
                    }
                }

                return instance;
            }
       
        }

        public static FMOD.Studio.System StudioSystem
        {
            get { return Instance.studioSystem; }
        }


        public static FMOD.System LowlevelSystem
        {
            get { return Instance.lowlevelSystem; }
        }

        FMOD.Studio.System studioSystem;
        FMOD.System lowlevelSystem;
        FMOD.DSP mixerHead;

        [SerializeField]
        private long[] cachedPointers = new long[2];

        struct LoadedBank
        {
            public FMOD.Studio.Bank Bank;
            public int RefCount;
        }
        
        Dictionary<string, LoadedBank> loadedBanks = new Dictionary<string, LoadedBank>();
        Dictionary<string, uint> loadedPlugins = new Dictionary<string, uint>();

        // Explicit comparer to avoid issues on platforms that don't support JIT compilation
        class GuidComparer : IEqualityComparer<Guid>
        {
            bool IEqualityComparer<Guid>.Equals(Guid x, Guid y)
            {
                return x.Equals(y);
            }

            int IEqualityComparer<Guid>.GetHashCode(Guid obj)
            {
                return obj.GetHashCode();
            }
        }
        Dictionary<Guid, FMOD.Studio.EventDescription> cachedDescriptions = new Dictionary<Guid, FMOD.Studio.EventDescription>(new GuidComparer());

        void CheckInitResult(FMOD.RESULT result, string cause)
        {
            if (result != FMOD.RESULT.OK)
            {
                if (studioSystem.isValid())
                {
                    studioSystem.release();
					studioSystem.clearHandle();
                }
                throw new SystemNotInitializedException(result, cause);
            }
        }

        FMOD.RESULT Initialize()
        {
            FMOD.RESULT result = FMOD.RESULT.OK;
            FMOD.RESULT initResult = FMOD.RESULT.OK;
            Settings fmodSettings = Settings.Instance;
            fmodPlatform = RuntimeUtils.GetCurrentPlatform();

            int sampleRate = fmodSettings.GetSampleRate(fmodPlatform);
            int realChannels = Math.Min(fmodSettings.GetRealChannels(fmodPlatform), 256); // Prior to 1.08.10 we didn't clamp this properly in the settings screen
            int virtualChannels = fmodSettings.GetVirtualChannels(fmodPlatform);
            FMOD.SPEAKERMODE speakerMode = (FMOD.SPEAKERMODE)fmodSettings.GetSpeakerMode(fmodPlatform);
            FMOD.OUTPUTTYPE outputType = FMOD.OUTPUTTYPE.AUTODETECT;

            FMOD.ADVANCEDSETTINGS advancedSettings = new FMOD.ADVANCEDSETTINGS();
            advancedSettings.randomSeed = (uint)DateTime.Now.Ticks;
            #if UNITY_EDITOR || UNITY_STANDALONE
            advancedSettings.maxVorbisCodecs = realChannels;
            #elif UNITY_XBOXONE
            advancedSettings.maxXMACodecs = realChannels;
            #elif UNITY_PS4
            advancedSettings.maxAT9Codecs = realChannels;
            #else
            advancedSettings.maxFADPCMCodecs = realChannels;
            #endif

            #if UNITY_EDITOR || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX) && DEVELOPMENT_BUILD)
            result = FMOD.Debug.Initialize(FMOD.DEBUG_FLAGS.LOG, FMOD.DEBUG_MODE.FILE, null, RuntimeUtils.LogFileName);
            if (result == FMOD.RESULT.ERR_FILE_NOTFOUND)
            {
                UnityEngine.Debug.LogWarningFormat("FMOD Studio: Cannot open FMOD debug log file '{0}', logs will be missing for this session.", System.IO.Path.Combine(Application.dataPath, RuntimeUtils.LogFileName));
            }
            else
            {
                CheckInitResult(result, "FMOD.Debug.Initialize");
            }
            #endif

            FMOD.Studio.INITFLAGS studioInitFlags = FMOD.Studio.INITFLAGS.NORMAL | FMOD.Studio.INITFLAGS.DEFERRED_CALLBACKS;
            if (fmodSettings.IsLiveUpdateEnabled(fmodPlatform))
            {
                studioInitFlags |= FMOD.Studio.INITFLAGS.LIVEUPDATE;

                #if UNITY_5_0 || UNITY_5_1 // These versions of Unity shipped with FMOD4 profiling enabled consuming our port number.
                UnityEngine.Debug.LogWarning("FMOD Studio: Live Update port in-use by Unity, switching to port 9265");
                advancedSettings.profilePort = 9265;
                #endif
            }

retry:
            result = FMOD.Studio.System.create(out studioSystem);
            CheckInitResult(result, "FMOD.Studio.System.create");

            result = studioSystem.getLowLevelSystem(out lowlevelSystem);
            CheckInitResult(result, "FMOD.Studio.System.getLowLevelSystem");

            result = lowlevelSystem.setOutput(outputType);
            CheckInitResult(result, "FMOD.System.setOutput");

            result = lowlevelSystem.setSoftwareChannels(realChannels);
            CheckInitResult(result, "FMOD.System.setSoftwareChannels");

            result = lowlevelSystem.setSoftwareFormat(sampleRate, speakerMode, 0);
            CheckInitResult(result, "FMOD.System.setSoftwareFormat");

            result = lowlevelSystem.setAdvancedSettings(ref advancedSettings);
            CheckInitResult(result, "FMOD.System.setAdvancedSettings");

            result = studioSystem.initialize(virtualChannels, studioInitFlags, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
            if (result != FMOD.RESULT.OK && initResult == FMOD.RESULT.OK)
            {
                initResult = result; // Save this to throw at the end (we'll attempt NO SOUND to shield ourselves from unexpected device failures)
                outputType = FMOD.OUTPUTTYPE.NOSOUND;
                UnityEngine.Debug.LogErrorFormat("FMOD Studio: Studio::System::initialize returned {0}, defaulting to no-sound mode.", result.ToString());

                goto retry;
            }
            CheckInitResult(result, "Studio::System::initialize");

            // Test network functionality triggered during System::update
            if ((studioInitFlags & FMOD.Studio.INITFLAGS.LIVEUPDATE) != 0)
            {
                studioSystem.flushCommands(); // Any error will be returned through Studio.System.update

                result = studioSystem.update();
                if (result == FMOD.RESULT.ERR_NET_SOCKET_ERROR)
                {
                    studioInitFlags &= ~FMOD.Studio.INITFLAGS.LIVEUPDATE;
                    UnityEngine.Debug.LogWarning("FMOD Studio: Cannot open network port for Live Update (in-use), restarting with Live Update disabled.");

                    result = studioSystem.release();
                    CheckInitResult(result, "FMOD.Studio.System.Release");

                    goto retry;
                }
            }

            LoadPlugins(fmodSettings);
            LoadBanks(fmodSettings);

            return initResult;
        }
        
        class AttachedInstance
        {
            public FMOD.Studio.EventInstance instance;
            public Transform transform;
            public Rigidbody rigidBody;
            public Rigidbody2D rigidBody2D;
        }

        List<AttachedInstance> attachedInstances = new List<AttachedInstance>(128);

        #if UNITY_EDITOR
        List<FMOD.Studio.EventInstance> eventPositionWarnings = new List<FMOD.Studio.EventInstance>();
        #endif

        bool listenerWarningIssued = false;
        void Update()
        {
            if (studioSystem.isValid())
            {
                studioSystem.update();

                bool foundListener = false;
                bool hasAllListeners = false;
                int numListeners = 0;
                for (int i = FMOD.CONSTANTS.MAX_LISTENERS - 1; i >= 0; i--)
                {
                    if (!foundListener && HasListener[i])
                    {
                        numListeners = i + 1;
                        foundListener = true;
                        hasAllListeners = true;
                    }

                    if (!HasListener[i] && foundListener)
                    {
                        hasAllListeners = false;
                    }
                }

                if (foundListener)
                {
                    studioSystem.setNumListeners(numListeners);
                }

                if (!hasAllListeners && !listenerWarningIssued)
                {
                    listenerWarningIssued = true;
                    UnityEngine.Debug.LogWarning("FMOD Studio Integration: Please add an 'FMOD Studio Listener' component to your a camera in the scene for correct 3D positioning of sounds");
                }

                for (int i = 0; i < attachedInstances.Count; i++)
                {
                    FMOD.Studio.PLAYBACK_STATE playbackState = FMOD.Studio.PLAYBACK_STATE.STOPPED;
                    attachedInstances[i].instance.getPlaybackState(out playbackState);
                    if (!attachedInstances[i].instance.isValid() || 
                        playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED ||
                        attachedInstances[i].transform == null // destroyed game object
                        )
                    {
                        attachedInstances.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (attachedInstances[i].rigidBody)
                    {
                        attachedInstances[i].instance.set3DAttributes(RuntimeUtils.To3DAttributes(attachedInstances[i].transform, attachedInstances[i].rigidBody));
                    }
                    else
                    {
                        attachedInstances[i].instance.set3DAttributes(RuntimeUtils.To3DAttributes(attachedInstances[i].transform, attachedInstances[i].rigidBody2D));
                    }
                }

                #if UNITY_EDITOR
                for (int i = eventPositionWarnings.Count - 1; i >= 0; i--)
                {
                    if (eventPositionWarnings[i].isValid())
                    {
                        FMOD.ATTRIBUTES_3D attribs;
                        eventPositionWarnings[i].get3DAttributes(out attribs);
                        if (attribs.position.x == 1e+18F &&
                            attribs.position.y == 1e+18F &&
                            attribs.position.z == 1e+18F)
                        {
                            string path;
                            FMOD.Studio.EventDescription desc;
                            eventPositionWarnings[i].getDescription(out desc);
                            desc.getPath(out path);
                            Debug.LogWarningFormat("FMOD Studio: Instance of Event {0} has not had EventInstance.set3DAttributes() called on it yet!", path);
                        }
                    }
                    eventPositionWarnings.RemoveAt(i);
                }
                #endif
            }
        }

        public static void AttachInstanceToGameObject(FMOD.Studio.EventInstance instance, Transform transform, Rigidbody rigidBody)
        {
            var attachedInstance = new AttachedInstance();
            attachedInstance.transform = transform;
            attachedInstance.instance = instance;
            attachedInstance.rigidBody = rigidBody;
            Instance.attachedInstances.Add(attachedInstance);
        }

        public static void AttachInstanceToGameObject(FMOD.Studio.EventInstance instance, Transform transform, Rigidbody2D rigidBody2D)
        {
            var attachedInstance = new AttachedInstance();
            attachedInstance.transform = transform;
            attachedInstance.instance = instance;
            attachedInstance.rigidBody2D = rigidBody2D;
            attachedInstance.rigidBody = null;
            Instance.attachedInstances.Add(attachedInstance);
        }

        public static void DetachInstanceFromGameObject(FMOD.Studio.EventInstance instance)
        {
            var manager = Instance;
            for (int i = 0; i < manager.attachedInstances.Count; i++)
            {
                if (manager.attachedInstances[i].instance.handle == instance.handle)
                {
                    manager.attachedInstances.RemoveAt(i);
                    return;
                }
            }
        }

        Rect windowRect = new Rect(10, 10, 300, 100);
        void OnGUI()
        {
            if (studioSystem.isValid() && Settings.Instance.IsOverlayEnabled(fmodPlatform))
            {
                windowRect = GUI.Window(0, windowRect, DrawDebugOverlay, "FMOD Studio Debug");
            }
        }

        string lastDebugText;
        float lastDebugUpdate = 0;
        void DrawDebugOverlay(int windowID)
        {
            if (lastDebugUpdate + 0.25f < Time.unscaledTime)
            {
                if (initException != null)
                {
                    lastDebugText = initException.Message;
                }
                else
                {
                    if (!mixerHead.hasHandle())
                    {
                        FMOD.ChannelGroup master;
                        lowlevelSystem.getMasterChannelGroup(out master);
                        master.getDSP(0, out mixerHead);
                        mixerHead.setMeteringEnabled(false, true);
                    }

                    StringBuilder debug = new StringBuilder();

                    FMOD.Studio.CPU_USAGE cpuUsage;
                    studioSystem.getCPUUsage(out cpuUsage);
                    debug.AppendFormat("CPU: dsp = {0:F1}%, studio = {1:F1}%\n", cpuUsage.dspusage, cpuUsage.studiousage);

                    int currentAlloc, maxAlloc;
                    FMOD.Memory.GetStats(out currentAlloc, out maxAlloc);
                    debug.AppendFormat("MEMORY: cur = {0}MB, max = {1}MB\n", currentAlloc >> 20, maxAlloc >> 20);

                    int realchannels, channels;
                    lowlevelSystem.getChannelsPlaying(out channels, out realchannels);
                    debug.AppendFormat("CHANNELS: real = {0}, total = {1}\n", realchannels, channels);

                    FMOD.DSP_METERING_INFO outputMetering;
                    mixerHead.getMeteringInfo(IntPtr.Zero, out outputMetering);
                    float rms = 0;
                    for (int i = 0; i < outputMetering.numchannels; i++)
                    {
                        rms += outputMetering.rmslevel[i] * outputMetering.rmslevel[i];
                    }
                    rms = Mathf.Sqrt(rms / (float)outputMetering.numchannels);

                    float db = rms > 0 ? 20.0f * Mathf.Log10(rms * Mathf.Sqrt(2.0f)) : -80.0f;
                    if (db > 10.0f) db = 10.0f;

                    debug.AppendFormat("VOLUME: RMS = {0:f2}db\n", db);
                    lastDebugText = debug.ToString();
                    lastDebugUpdate = Time.unscaledTime;
                }
            }

            GUI.Label(new Rect(10, 20, 290, 100), lastDebugText);
            GUI.DragWindow();
        }

        void OnDisable()
        {
            // If we're being torn down for a script reload - cache the native pointers in something unity can serialize
            cachedPointers[0] = (long)studioSystem.handle;
            cachedPointers[1] = (long)lowlevelSystem.handle;
        }

        void OnDestroy()
        {
            if (studioSystem.isValid())
            {
                studioSystem.release();
                studioSystem.clearHandle();
            }
            initException = null;
            instance = null;
            isQuitting = true;
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (studioSystem.isValid())
            {
                // Strings bank is always loaded
                if (loadedBanks.Count > 1)
                    PauseAllEvents(pauseStatus);

                if (pauseStatus)
                {
                    lowlevelSystem.mixerSuspend();
                }
                else
                {
                    lowlevelSystem.mixerResume();
                }
            }
        }

        public static void LoadBank(string bankName, bool loadSamples = false)
        {
            if (Instance.loadedBanks.ContainsKey(bankName))
            {
                LoadedBank loadedBank = Instance.loadedBanks[bankName];
                loadedBank.RefCount++;

                if (loadSamples)
                {
                    loadedBank.Bank.loadSampleData();
                }
                Instance.loadedBanks[bankName] = loadedBank;
            }
            else
            {
                LoadedBank loadedBank = new LoadedBank();
                string bankPath = RuntimeUtils.GetBankPath(bankName);
                FMOD.RESULT loadResult;
                #if UNITY_ANDROID && !UNITY_EDITOR
                if (!bankPath.StartsWith("file:///android_asset"))
                {
                    using (var www = new WWW(bankPath))
                    {
                        while (!www.isDone) { }
                        if (!String.IsNullOrEmpty(www.error))
                        {
                            throw new BankLoadException(bankPath, www.error);
                        }
                        else
                        {
                            loadResult = Instance.studioSystem.loadBankMemory(www.bytes, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);
                        }
                    }
                }
                else
                #endif
                {
                    loadResult = Instance.studioSystem.loadBankFile(bankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);
                }

                if (loadResult == FMOD.RESULT.OK)
                {
                    loadedBank.RefCount = 1;
                    Instance.loadedBanks.Add(bankName, loadedBank);

                    if (loadSamples)
                    {
                        loadedBank.Bank.loadSampleData();
                    }
                }
                else if (loadResult == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
                {
                    // someone loaded this bank directly using the studio API
                    // TODO: will the null bank handle be an issue
                    loadedBank.RefCount = 2;
                    Instance.loadedBanks.Add(bankName, loadedBank);
                }
                else
                {
                    throw new BankLoadException(bankPath, loadResult);
                }
            }
        }

        public static void LoadBank(TextAsset asset, bool loadSamples = false)
        {
            string bankName = asset.name;
            if (Instance.loadedBanks.ContainsKey(bankName))
            {
                LoadedBank loadedBank = Instance.loadedBanks[bankName];
                loadedBank.RefCount++;

                if (loadSamples)
                {
                    loadedBank.Bank.loadSampleData();
                }
            }
            else
            {
                LoadedBank loadedBank = new LoadedBank();
                FMOD.RESULT loadResult = Instance.studioSystem.loadBankMemory(asset.bytes, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);

                if (loadResult == FMOD.RESULT.OK)
                {
                    loadedBank.RefCount = 1;
                    Instance.loadedBanks.Add(bankName, loadedBank);

                    if (loadSamples)
                    {
                        loadedBank.Bank.loadSampleData();
                    }
                }
                else if (loadResult == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
                {
                    // someone loaded this bank directly using the studio API
                    // TODO: will the null bank handle be an issue
                    loadedBank.RefCount = 2;
                    Instance.loadedBanks.Add(bankName, loadedBank);
                }
                else
                {
                    throw new BankLoadException(bankName, loadResult);
                }
            }
        }

        private void LoadBanks(Settings fmodSettings)
        {
            if (fmodSettings.ImportType == ImportType.StreamingAssets)
            {
                // Always load strings bank
                try
                {
                    LoadBank(fmodSettings.MasterBank + ".strings", fmodSettings.AutomaticSampleLoading);

                    if (fmodSettings.AutomaticEventLoading)
                    {
                        LoadBank(fmodSettings.MasterBank, fmodSettings.AutomaticSampleLoading);

                        foreach (var bank in fmodSettings.Banks)
                        {
                            LoadBank(bank, fmodSettings.AutomaticSampleLoading);
                        }

                        WaitForAllLoads();
                    }
                }
                catch (BankLoadException e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        public static void UnloadBank(string bankName)
        {
            LoadedBank loadedBank;
            if (Instance.loadedBanks.TryGetValue(bankName, out loadedBank))
            {
                loadedBank.RefCount--;
                if (loadedBank.RefCount == 0)
                {
                    loadedBank.Bank.unload();
                    Instance.loadedBanks.Remove(bankName);
                    return;
                }
                Instance.loadedBanks[bankName] = loadedBank;
            }
        }

        public static bool AnyBankLoading()
        {
            bool loading = false;
            foreach (LoadedBank bank in Instance.loadedBanks.Values)
            {
                FMOD.Studio.LOADING_STATE loadingState;
                bank.Bank.getSampleLoadingState(out loadingState);
                loading |= (loadingState == FMOD.Studio.LOADING_STATE.LOADING);
            }
            return loading;
        }

        public static void WaitForAllLoads()
        {
            Instance.studioSystem.flushSampleLoading();
        }

        public static Guid PathToGUID(string path)
        {
            Guid guid = Guid.Empty;
            if (path.StartsWith("{"))
            {
                FMOD.Studio.Util.ParseID(path, out guid);
            }
            else
            {
                var result = Instance.studioSystem.lookupID(path, out guid);
                if (result == FMOD.RESULT.ERR_EVENT_NOTFOUND)
                {
                    throw new EventNotFoundException(path);
                }
            }
            return guid;
        }

        public static FMOD.Studio.EventInstance CreateInstance(string path)
        {
            try
            {
                return CreateInstance(PathToGUID(path));
            }
            catch(EventNotFoundException)
            {
                // Switch from exception with GUID to exception with path
                throw new EventNotFoundException(path);
            }
        }

        public static FMOD.Studio.EventInstance CreateInstance(Guid guid)
        {
            FMOD.Studio.EventDescription eventDesc = GetEventDescription(guid);
            FMOD.Studio.EventInstance newInstance;
            eventDesc.createInstance(out newInstance);

#if UNITY_EDITOR
            bool is3D = false;
            eventDesc.is3D(out is3D);
            if (is3D)
            {
                // Set position to 1e+18F, set3DAttributes should be called by the dev after this.
                newInstance.set3DAttributes(RuntimeUtils.To3DAttributes(new Vector3(1e+18F, 1e+18F, 1e+18F)));
                instance.eventPositionWarnings.Add(newInstance);
            }
#endif

            return newInstance;
        }

        public static void PlayOneShot(string path, Vector3 position = new Vector3())
        {
            try
            {
                PlayOneShot(PathToGUID(path), position);
            }
            catch (EventNotFoundException)
            {
                Debug.LogWarning("FMOD Event not found: " + path);
            }
        }

        public static void PlayOneShot(Guid guid, Vector3 position = new Vector3())
        {
            var instance = CreateInstance(guid);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            instance.start();
            instance.release();
        }

        public static void PlayOneShotAttached(string path, GameObject gameObject)
        {
            try
            {
                PlayOneShotAttached(PathToGUID(path), gameObject);
            }
            catch (EventNotFoundException)
            {
                Debug.LogWarning("FMOD Event not found: " + path);
            }
        }

        public static void PlayOneShotAttached(Guid guid, GameObject gameObject)
        {
            var instance = CreateInstance(guid);
            AttachInstanceToGameObject(instance, gameObject.transform, gameObject.GetComponent<Rigidbody>());
            instance.start();
            instance.release();
        }

        public static FMOD.Studio.EventDescription GetEventDescription(string path)
        {
            try
            {
                return GetEventDescription(PathToGUID(path));
            }
            catch (EventNotFoundException)
            {
                throw new EventNotFoundException(path);
            }
        }

        public static FMOD.Studio.EventDescription GetEventDescription(Guid guid)
        {
            FMOD.Studio.EventDescription eventDesc;
            if (Instance.cachedDescriptions.ContainsKey(guid) && Instance.cachedDescriptions[guid].isValid())
            {
                eventDesc = Instance.cachedDescriptions[guid];
            }
            else
            {
                var result = Instance.studioSystem.getEventByID(guid, out eventDesc);

                if (result != FMOD.RESULT.OK)
                {
                    throw new EventNotFoundException(guid);
                }

                if (eventDesc.isValid())
                {
                    Instance.cachedDescriptions[guid] = eventDesc;
                }
            }
            return eventDesc;
        }

        public static bool[] HasListener = new bool[FMOD.CONSTANTS.MAX_LISTENERS];

        public static void SetListenerLocation(GameObject gameObject, Rigidbody rigidBody = null)
        {
            Instance.studioSystem.setListenerAttributes(0, RuntimeUtils.To3DAttributes(gameObject, rigidBody));
        }
        
        public static void SetListenerLocation(GameObject gameObject, Rigidbody2D rigidBody2D)
        {
            Instance.studioSystem.setListenerAttributes(0, RuntimeUtils.To3DAttributes(gameObject, rigidBody2D));
        }

        public static void SetListenerLocation(Transform transform)
        {
            Instance.studioSystem.setListenerAttributes(0, transform.To3DAttributes());
        }

        public static void SetListenerLocation(int listenerIndex, GameObject gameObject, Rigidbody rigidBody = null)
        {
            Instance.studioSystem.setListenerAttributes(listenerIndex, RuntimeUtils.To3DAttributes(gameObject, rigidBody));
        }
        
        public static void SetListenerLocation(int listenerIndex, GameObject gameObject, Rigidbody2D rigidBody2D)
        {
            Instance.studioSystem.setListenerAttributes(listenerIndex, RuntimeUtils.To3DAttributes(gameObject, rigidBody2D));
        }

        public static void SetListenerLocation(int listenerIndex, Transform transform)
        {
            Instance.studioSystem.setListenerAttributes(listenerIndex, transform.To3DAttributes());
        }

        public static FMOD.Studio.Bus GetBus(string path)
        {
            FMOD.Studio.Bus bus;
            if (StudioSystem.getBus(path, out bus) != FMOD.RESULT.OK)
            {
                throw new BusNotFoundException(path);
            }
            return bus;
        }

        public static FMOD.Studio.VCA GetVCA(string path)
        {
            FMOD.Studio.VCA vca;
            if (StudioSystem.getVCA(path, out vca) != FMOD.RESULT.OK)
            {
                throw new VCANotFoundException(path);
            }
            return vca;
        }

        public static void PauseAllEvents(bool paused)
        {
            GetBus("bus:/").setPaused(paused);
        }

        public static void MuteAllEvents(bool muted)
        {
            GetBus("bus:/").setMute(muted);
        }

        public static bool IsInitialized
        {
            get
            {
                return instance != null && instance.studioSystem.isValid();
            }
        }

        public static bool HasBanksLoaded
        {
            get
            {
                return instance.loadedBanks.Count > 1;
            }
        }

        private void LoadPlugins(Settings fmodSettings)
        {
            #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            FmodUnityNativePluginInit(lowlevelSystem.handle);
            #else

            FMOD.RESULT result;
            foreach (var pluginName in fmodSettings.Plugins)
            {
                if (string.IsNullOrEmpty(pluginName))
                    continue;
                string pluginPath = RuntimeUtils.GetPluginPath(pluginName);
                uint handle;
                result = lowlevelSystem.loadPlugin(pluginPath, out handle);
                #if UNITY_64 || UNITY_EDITOR_64
                // Add a "64" suffix and try again
                if (result == FMOD.RESULT.ERR_FILE_BAD || result == FMOD.RESULT.ERR_FILE_NOTFOUND)
                {
                    string pluginPath64 = RuntimeUtils.GetPluginPath(pluginName + "64");
                    result = lowlevelSystem.loadPlugin(pluginPath64, out handle);
                }
                #endif
                CheckInitResult(result, String.Format("Loading plugin '{0}' from '{1}'", pluginName, pluginPath));
                loadedPlugins.Add(pluginName, handle);
            }
            #endif
        }

        #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern FMOD.RESULT FmodUnityNativePluginInit(IntPtr system);
        #endif
    }
}
