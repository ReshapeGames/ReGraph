using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using Reshape.Unity;
using TMPro;

namespace Reshape.ReFramework
{
    [HideMonoScript]
    public class DialogCanvas : ReSinglonBehaviour<DialogCanvas>
    {
        public GameObject messagePanel;
        public GameObject actorPanel;
        public GameObject textPanel;
        public TMP_Text actorLabel;
        public TMP_Text textLabel;
        public GameObject nextIndicator;
#if ENABLE_INPUT_SYSTEM
        public InputActionAsset inputAction;
#endif
        [ValueDropdown("ParamProceedKeyChoice", ExpandAllMenuItems = false, AppendNextDrawer = true)]
        public string dialogProceedKey;

        public GameObject choicePanel;
        public GameObject[] choiceButtons;
        public TMP_Text[] choiceLabels;
        
        public delegate void CommonDelegate ();
        public event CommonDelegate onKeyDialogProceed;

        private int chosenChoice;
        private InputAction dialogProceedAction;

        public static string GetChosenChoice ()
        {
            if (instance == null || instance.chosenChoice < 0)
                return string.Empty;
            return instance.choiceLabels[instance.chosenChoice].text;
        }

        public static void HidePanel ()
        {
            if (instance == null)
                return;
            instance.messagePanel.SetActive(false);
            instance.choicePanel.SetActive(false);
        }
        
        public static bool IsPanelHide ()
        {
            if (instance == null)
                return false;
            return !instance.messagePanel.activeSelf;
        }


        public static void ShowMessagePanel (string actor, string message, bool haveContinue)
        {
            if (instance == null)
                return;
            instance.messagePanel.SetActive(true);
            instance.actorLabel.text = actor;
            instance.textLabel.text = message;
            instance.actorPanel.SetActive(false);
            if (!string.IsNullOrEmpty(instance.actorLabel.text))
                instance.actorPanel.SetActive(true);
            instance.textPanel.SetActive(true);
            instance.nextIndicator.SetActive(haveContinue);
        }

        public static void ShowChoicePanel (List<string> choices)
        {
            instance.chosenChoice = -1;
            for (int i = 0; i < instance.choiceButtons.Length; i++)
                instance.choiceButtons[i].SetActive(false);
            for (int i = 0; i < choices.Count; i++)
            {
                if (i < instance.choiceButtons.Length)
                {
                    instance.choiceButtons[i].SetActive(true);
                    instance.choiceLabels[i].text = choices[i];
                }
                else
                {
                    break;
                }
            }

            instance.choicePanel.SetActive(true);
            instance.nextIndicator.SetActive(false);
        }

        public static void HideChoicePanel ()
        {
            instance.choicePanel.SetActive(false);
        }

        public void OnClickChoice (int choiceIndex)
        {
            chosenChoice = choiceIndex;
        }

        protected override void Awake ()
        {
            messagePanel.SetActive(false);
            choicePanel.SetActive(false);
            base.Awake();
        }

        protected void Start ()
        {
#if ENABLE_INPUT_SYSTEM
            if (inputAction != null)
            {
                dialogProceedAction = inputAction.FindAction(dialogProceedKey);
                if (dialogProceedAction != null)
                    dialogProceedAction.performed += OnKeyDialogProceed;
            }
#endif
        }
        
        public void OnKeyDialogProceed (InputAction.CallbackContext content)
        {
            onKeyDialogProceed?.Invoke();
        }

        protected void OnDestroy ()
        {
#if ENABLE_INPUT_SYSTEM
            if (inputAction != null && dialogProceedAction != null)
                 dialogProceedAction.performed -= OnKeyDialogProceed;
            ClearInstance();
#endif
        }

#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        private IEnumerable ParamProceedKeyChoice ()
        {
            ValueDropdownList<string> menu = new ValueDropdownList<string>();
            if (inputAction != null)
            {
                for (int i = 0; i < inputAction.actionMaps.Count; i++)
                {
                    string mapName = inputAction.actionMaps[i].name;
                    for (int j = 0; j < inputAction.actionMaps[i].actions.Count; j++)
                    {
                        menu.Add(mapName + "//" + inputAction.actionMaps[i].actions[j].name, inputAction.actionMaps[i].actions[j].name);
                    }
                }
            }

            return menu;
        }
#endif
    }
}