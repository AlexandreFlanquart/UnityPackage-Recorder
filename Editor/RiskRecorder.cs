using UnityEngine;

using TMPro;


namespace MyUnityPackage.Recorder
{/*
    public class RiskRecorder : MonoBehaviour
    {   
        [TabGroup("Tab", "Settings")]
        [SerializeField] private CinemachineCamera virtualCamera;   
        [TabGroup("Tab", "Settings")]
        [SerializeField] public GameObject dialogueSystemManager;
        [TabGroup("Tab", "Settings")]
        [SerializeField] private UI_Feedback uiFeedback;
        [TabGroup("Tab", "Settings")]
        [SerializeField] private GameObject responseButtonsContainer;
        [Space(10)]
        [TabGroup("Tab", "Settings")]
        [SerializeField] private List<GameObject> objectsToDisableOnStart = new List<GameObject>();

        [TabGroup("Tab", "Risks Recording")]
        [TabGroup("Tab/Risks Recording/SubTabGroup", "Risks")]
        [SerializeField] private RiskListSO riskListSO;
        [TabGroup("Tab/Risks Recording/SubTabGroup", "Output")]
        [SerializeField] public string OutputPath = "Assets/Recordings";

        [TabGroup("Tab", "Manual Recording")]
        [SerializeField] public string manualRecordingOutputName = "Manual_Recording";

        [Space(20)]
        [TabGroup("Tab/Risks Recording/SubTabGroup", "Risks")]
        [LabelText("List of risks")]
        [TableList]
        [SerializeField] private List<RiskDisplay> riskDisplayList = new List<RiskDisplay>();

        private List<Risk> riskList = new List<Risk>();
        private RecorderController m_RecorderController;
        private RecorderControllerSettings m_ControllerSettings;
        private MovieRecorderSettings m_RecorderSettings;
        private DialogueSystemController dialogueSystemController;
        private DialogueHelper dialogueHelper;

        [System.Serializable]
        public class RiskDisplay
        {
            [ReadOnly]
            public RiskDataSO risk;
            
            [TableColumnWidth(75, Resizable = false)]
            [LabelText("")]
            public bool isSelected;

            public RiskDisplay(RiskDataSO risk)
            {
                this.risk = risk;
                this.isSelected = false;
            }
        }

        private void Start() {
            // Disable all gameobjects not needed
            foreach (var obj in objectsToDisableOnStart)
            {
                obj.SetActive(false);
            }

            dialogueSystemController = dialogueSystemManager.GetComponent<DialogueSystemController>();
            dialogueHelper = dialogueSystemManager.GetComponent<DialogueHelper>();

            // disable random answer in dialogue system
            dialogueHelper.SetRandomStatus(false);
        }

        #region Buttons
        [TabGroup("Tab/Risks Recording/SubTabGroup", "Output")]
        [Button("Open Output Folder", ButtonSizes.Large)]
        private void OpenOutputFolder()
        {
            EditorUtility.RevealInFinder(OutputPath);
        }

        // Get all risk in riskListSO to add in checklist
        [TabGroup("Tab/Risks Recording/SubTabGroup", "Risks")]
        [GUIColor(0.3f, 0.8f, 0.3f)]
        [Button("GetAllRisks", ButtonSizes.Large)]
        private void GetAllRisks()
        {
            if (riskListSO != null)
            {
                Debug.Log("GetAllRisks");
                RiskDataSO[] riskDataList = riskListSO.GetAllRisks();
                riskDisplayList.Clear();
                for (int i = 0; i < riskDataList.Length; i++)
                {
                    riskDisplayList.Add(new RiskDisplay(riskDataList[i]));
                }
            }
            else
            {
                Debug.LogWarning("[RiskSimulator] Please assign a RiskListSO in the Inspector");
            }
        }

        [TabGroup("Tab/Risks Recording/SubTabGroup", "Risks")]
        [Button("Toggle All Risks", ButtonSizes.Large)]
        private void ToggleAllRisks()
        {
            if (riskDisplayList == null || riskDisplayList.Count == 0)
            {
                Debug.LogWarning("[RiskSimulator] No risks available to toggle");
                return;
            }

            // Vérifie si tous les risques sont sélectionnés
            bool allSelected = riskDisplayList.All(rd => rd.isSelected);
            
            // Inverse la slection pour tous les risques
            foreach (var riskDisplay in riskDisplayList)
            {
                riskDisplay.isSelected = !allSelected;
            }
        }

        [TabGroup("Tab/Risks Recording/SubTabGroup", "Simulate")]
        [Button("Simulate Risks", ButtonSizes.Large)]
        private void SimulateRisks()
        {
            if (riskListSO == null)
            {
                Debug.LogWarning("[RiskSimulator] Please assign a RiskListSO in the Inspector");
                return;
            }

            StartCoroutine(SimulateRisksForearchAnswer());
        }

        [TabGroup("Tab/Risks Recording/SubTabGroup", "Simulate")]
        [Button("Stop All", ButtonSizes.Large)]
        private void StopAllSimulation(){
            Debug.Log("stop all");
            StopAllCoroutines();

            foreach (var risk in riskList)
            {
                Destroy(risk.gameObject);
            }
            riskList.Clear();

            StopRecording();
        }
        #endregion
        
        // instantiate all risks good then risks bad
        private IEnumerator SimulateRisksForearchAnswer(){
            for(int i=0; i< 3;i++)  {
                yield return StartCoroutine(InstantiateRisksCoroutine(i));   
                yield return new WaitForSeconds(1);                
            }                
        }

        // Instantiate all GOOD or BAD risks
        private IEnumerator InstantiateRisksCoroutine(int index)
        {
            // Clear and get only risks wanted
            riskList.Clear();
            var selectedRisks = riskDisplayList.Where(rd => rd.isSelected).Select(rd => rd.risk).ToList();
            int nbRiskToInstantiate = selectedRisks.Count;
            Debug.Log($"nbRiskToInstantiate : {nbRiskToInstantiate}");

            if (selectedRisks.Count == 0)
            {
                Debug.LogWarning("[RiskSimulator] No risk selected");
                yield break;
            }

            foreach (var riskData in selectedRisks)
            {
                Debug.Log("InstantiateAsync");
                // Charger d'abord le GameObject, puis obtenir le composant Risk
                AddressablesManager.InstantiateAsync(riskData.riskPrefabAssetReference.AssetGUID, Vector3.zero, Quaternion.identity, null, (gameObject) =>
                {
                    var risk = gameObject.GetComponent<Risk>();
                    if (risk != null)
                    {
                        Debug.Log("Risk loaded");
                        riskList.Add(risk);
                        risk.SetRiskDataSO(riskData);
                        gameObject.SetActive(false);
                        nbRiskToInstantiate--;
                    }
                    else
                    {
                        Debug.LogError($"No Risk component found on loaded GameObject: {riskData.riskPrefabAssetReference.AssetGUID}");
                    }
                });
            }
            Debug.Log($"nbRiskToInstantiate : {nbRiskToInstantiate}");
            yield return new WaitUntil(() => nbRiskToInstantiate == 0);
                
        yield return StartCoroutine(SimulateRisksCoroutine(index));
        }
        
        // Simulate all GOOD or BAD risks  
        public IEnumerator SimulateRisksCoroutine(int index){
            foreach (var risk in riskList)
            {
                if(index < risk.specifics.Length)  {
                    yield return SimulateRiskCoroutine(risk, index, risk.specifics[index].name.Contains("good")); 
                    yield return new WaitForSeconds(0.2f);       
                }
                Destroy(risk.gameObject);
            }  
            Debug.Log("All simulation ended");
        }
        
        // Simulate one risk
        private IEnumerator SimulateRiskCoroutine(Risk risk, int index, bool isGood)
        {    
            Debug.Log("SimulateRiskCoroutine : " + index);

            risk.gameObject.SetActive(true);
            // Position the camera
            FocusOnRiskViewPosition(risk);
            
            // Démarrer l'enregistrement
            string status = isGood ? "good" : "bad";
            StartRecording($"{risk.name}_{status}{index+1}");
            yield return new WaitForSeconds(1f);
            
            // Start the risk
            risk.StartRisk();

            Debug.Log("duration : " + risk.intro.duration);
            yield return new WaitForSeconds((float)risk.intro.duration);
            yield return new WaitForSeconds(3f);
            Debug.Log("nb response : " + responseButtonsContainer.transform.childCount);
            StandardUIResponseButton responseButton = responseButtonsContainer.transform.GetChild(index+1).GetComponent<StandardUIResponseButton>();
            
            // Simulate button click
            if (responseButton != null)
            {
                // Simulate button "pressed" state
                var buttonComponent = responseButton.GetComponent<UnityEngine.UI.Button>();
                if (buttonComponent != null)
                {
                    //Debug.Log("press on button : " + responseButton.label.gameObject.GetComponent<TextMeshProUGUI>().text);
                    buttonComponent.OnPointerEnter(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
                    buttonComponent.OnPointerDown(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
                    yield return new WaitForSeconds(0.5f); // Time to see the button pressed
                    buttonComponent.OnPointerUp(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
                }
            }
            
            risk.SetAnswer(index);
            yield return new WaitForSeconds(1f);

            dialogueSystemController.StopConversation();

            yield return new WaitForSeconds((float)risk.specifics[index].duration);
            //yield return new WaitUntil(() => uiFeedback.isActive);
            yield return new WaitForSeconds(2f);

            Debug.Log("PopUpHide");
            uiFeedback.PopUpHide();
            
            // Stop recording
            StopRecording();   
        }

        // TODO : position the vcam
        public void FocusOnRiskViewPosition(Risk risk)
        {
            Debug.Log("FocusOnRiskViewPosition");
        }

        private void StartRecording(string riskName = "")
        {
            PrepareRecorder();
            
            riskName =  riskName.Substring(1).Replace("(Clone)", "");
            m_RecorderSettings.OutputFile = $"{OutputPath}/{riskName}";
            
            m_RecorderController.StartRecording();
            Debug.Log($"Recording started: {riskName}");
        }

        [TabGroup("Tab", "Manual Recording")]
        [Button(ButtonSizes.Large)]
        private void StartRecording()
        {
            if(string.IsNullOrEmpty(manualRecordingOutputName)) 
            {
                manualRecordingOutputName = "Manual Recording";
            }
            StartRecording(manualRecordingOutputName);
        }

        [TabGroup("Tab", "Manual Recording")]
        [Button(ButtonSizes.Large)]
        private void StopRecording()
        {

            if (m_RecorderController != null && m_RecorderController.IsRecording())
            {
                m_RecorderController.StopRecording();
                Debug.Log("Recording stopped");
            }
            m_RecorderController = null;
            Debug.Log("Recording finished");
        }

        private void PrepareRecorder()
        {
            if (m_RecorderController == null)
            {
                m_ControllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
                m_RecorderController = new RecorderController(m_ControllerSettings);

                m_RecorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
                m_RecorderSettings.name = "My Movie Recorder";
                m_RecorderSettings.Enabled = true;

                var gameViewInput = new GameViewInputSettings();
                m_RecorderSettings.ImageInputSettings = gameViewInput;

                m_ControllerSettings.AddRecorderSettings(m_RecorderSettings);
                m_ControllerSettings.SetRecordModeToManual();
                m_ControllerSettings.FrameRate = 30;

                m_RecorderController.PrepareRecording();
            }
        }
    }
    */
}


