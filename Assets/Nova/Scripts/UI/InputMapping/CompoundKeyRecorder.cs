using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nova
{
    public class CompoundKeyRecorder : MonoBehaviour, IPointerClickHandler
    {
        private InputMapper inputMapper;
        private InputMappingController controller;
        private readonly HashSet<KeyCode> prefixKeys = new HashSet<KeyCode>(CompoundKey.PrefixKeys);

        public RecordPopupController popupController;

        private void Awake()
        {
            inputMapper = Utils.FindNovaGameController().InputMapper;
        }

        private void OnEnable()
        {
            inputMapper.SetEnableAll(false);
            popupController.entry = entry;
            popupController.Show();
        }

        private void OnDisable()
        {
            popupController.Hide();
            inputMapper.SetEnableAll(true);
            if (entry != null)
            {
                entry.FinishModify();
                entry.RefreshDisplay();
            }

            entry = null;
        }

        public void Init(InputMappingController controller)
        {
            this.controller = controller;
            gameObject.SetActive(false);
        }

        private bool isPressing = false;
        private InputMappingListEntry entry;

        public void BeginRecording(InputMappingListEntry entry)
        {
            isPressing = false;
            this.entry = entry;
            gameObject.SetActive(true);
        }

        private static bool AnyKeyPressing => CompoundKey.KeyboardKeys.Any(Input.GetKey);

        private static IEnumerable<KeyCode> PressedKey =>
            CompoundKey.KeyboardKeys.Where(Input.GetKey);

        private void WaitPress()
        {
            if (!AnyKeyPressing) return;
            entry.key.Clear();
            isPressing = true;
            controller.MarkDataDirty();
            HandlePress();
        }

        private void HandlePress()
        {
            if (!AnyKeyPressing)
            {
                gameObject.SetActive(false);
                return;
            }

            var compoundKey = entry.key;
            var dirty = false;

            if (CompoundKey.AltIsHolding)
            {
                compoundKey.Alt = true;
                dirty = true;
            }

            if (CompoundKey.WinIsHolding)
            {
                compoundKey.Win = true;
                dirty = true;
            }

            if (CompoundKey.ShiftIsHolding)
            {
                compoundKey.Shift = true;
                dirty = true;
            }

            if (CompoundKey.CtrlIsHolding)
            {
                compoundKey.Ctrl = true;
                dirty = true;
            }

            foreach (var key in PressedKey)
            {
                if (!prefixKeys.Contains(key))
                {
                    compoundKey.Key = key;
                    dirty = true;
                }
            }

            if (dirty)
            {
                entry.RefreshDisplay();
            }
        }

        private void Update()
        {
            if (entry == null) return;
            if (!isPressing)
            {
                WaitPress();
            }
            else
            {
                HandlePress();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            gameObject.SetActive(false);
        }
    }
}