using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.client.core.game;
using com.ethnicthv.chemlab.client.core.renderer;
using com.ethnicthv.chemlab.engine.api.molecule;
using com.ethnicthv.chemlab.engine.formula;
using com.ethnicthv.chemlab.engine.molecule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.menu.newcompound
{
    public class NewCompoundController : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private TMP_InputField idInput;
        [SerializeField] private TMP_InputField nameInput;
        
        [Header("Properties")] [SerializeField]
        private TMP_InputField formulaInput;

        [SerializeField] private TMP_InputField densityInput;
        [SerializeField] private TMP_InputField blInput;
        [SerializeField] private TMP_Dropdown blUnitInput;
        [SerializeField] private TMP_InputField dmInput;
        [SerializeField] private TMP_InputField shcInput;
        [SerializeField] private TMP_InputField mhcInput;
        [SerializeField] private TMP_InputField lhInput;
        [SerializeField] private TMP_InputField liquidColorInput;
        [SerializeField] private TMP_InputField burningColorInput;
        [SerializeField] private TMP_InputField burnIntensityInput;
        [SerializeField] private TMP_InputField gasColorInput;
        [SerializeField] private TMP_InputField solidColorInput;
        [Space(5)] [SerializeField] private Transform propertiesContainer;
        [SerializeField] private List<GameObject> disableObject;
        [SerializeField] private int shcDisableIndex;
        [SerializeField] private int mhcDisableIndex;

        [Header("View Control")] [SerializeField]
        private GameObject structureView;
        [SerializeField] private GameObject tagEditorView;
        private int _curViewIndex;

        [Header("Tag Editor")] [SerializeField]
        private Toggle tagPrefab;

        [SerializeField] private Transform tagContainer;
        private readonly Dictionary<MoleculeTag, Toggle> _tagToggles = new();
        private readonly Queue<MoleculeTag> _selectedTags = new();

        [Header("Structure")] [SerializeField] private TextMeshProUGUI massText;
        [SerializeField] private TextMeshProUGUI chargeText;
        [SerializeField] private Transform cameraBox;
        private Formula _f;

        private Formula Formula
        {
            get => _f;
            set => SetDisplayedFormula(value);
        }

        private Coroutine _centerRenderedMoleculeCoroutine;

        private void Awake()
        {
            massText.text = "0.00";
            chargeText.text = "0";

            SetupDisableObject();
            SetupTags();
            OpenView(0);
            SetDisplayedFormula(null);

            formulaInput.onSubmit.AddListener(UpdateDisableStateFormula);
            shcInput.onSubmit.AddListener(arg0 => UpdateDisableStateHeatCapacity(shcDisableIndex, arg0));
            mhcInput.onSubmit.AddListener(arg0 => UpdateDisableStateHeatCapacity(mhcDisableIndex, arg0));
        }

        private void UpdateDisableStateHeatCapacity(int o, string heatCapacity)
        {
            var disableIndex = (o - shcDisableIndex + 1) % 2 + shcDisableIndex;
            var disableInput = shcInput;
            if (disableIndex == mhcDisableIndex)
            {
                disableInput = mhcInput;
            }

            disableInput.text = "";
            
            if (string.IsNullOrEmpty(heatCapacity) || !float.TryParse(heatCapacity, out _))
            {
                disableObject[disableIndex].SetActive(false);
                return;
            }

            disableObject[disableIndex].SetActive(true);
        }

        private void UpdateDisableStateFormula(string formula)
        {
            if (string.IsNullOrEmpty(formula)) return;
            var s = false;
            try
            {
                Formula = Formula.Deserialize(formula);
            }
            catch (Exception)
            {
                s = true;
            }

            for (var i = 1; i < disableObject.Count; i++)
            {
                var disable = disableObject[i];
                disable.SetActive(s);
            }
        }

        private void SetupDisableObject()
        {
            disableObject = new List<GameObject>();
            for (var i = 0; i < propertiesContainer.childCount; i++)
            {
                var child = propertiesContainer.GetChild(i).gameObject;
                var disable = child.transform.Find("Disable");

                disableObject.Add(disable.gameObject);
                switch (child.name)
                {
                    case "SpecificHeatCapacity":
                        shcDisableIndex = i;
                        break;
                    case "MolarHeatCapacity":
                        mhcDisableIndex = i;
                        break;
                }
            }
        }

        private void OpenView(int view)
        {
            if (_curViewIndex == view) return;
            _curViewIndex = view;
            structureView.SetActive(view == 0);
            tagEditorView.SetActive(view == 1);
        }

        private void SetupTags()
        {
            var tags = Enum.GetValues(typeof(MoleculeTag));
            Array.Sort(tags);
            foreach (var moleculeTag in tags)
            {
                var toggle = Instantiate(tagPrefab, tagContainer);
                toggle.gameObject.SetActive(true);
                toggle.GetComponentInChildren<TextMeshProUGUI>().text = moleculeTag.ToString();
                toggle.isOn = false;
                _tagToggles.Add((MoleculeTag)moleculeTag, toggle);
            }
        }

        private void SetDisplayedFormula(Formula formula)
        {
            //Note: unregister previous molecule
            RenderProgram.Instance.ClearRenderEntity();

            //Note: update molecule displayed
            _f = formula;
            if (_f == null)
            {
                massText.text = "0.00";
                chargeText.text = "0";
                return;
            }
            RenderProgram.Instance.RegisterRenderEntity(_f, Vector3.zero);

            //Note: update mass and charge
            massText.text = _f.GetAtoms().Aggregate(0f, (f, atom) => f + atom.GetMass()).ToString("F2");
            chargeText.text = GetChargeText();

            if (_centerRenderedMoleculeCoroutine != null)
            {
                StopCoroutine(_centerRenderedMoleculeCoroutine);
            }

            _centerRenderedMoleculeCoroutine = StartCoroutine(CenterRenderedMoleculeCoroutine());
        }

        private string GetChargeText()
        {
            var charge = (int)_f.GetAtoms().Aggregate(0f, (f, atom) => f + atom.FormalCharge);
            return charge switch
            {
                > 0 => charge + "+",
                < 0 => Math.Abs(charge) + "-",
                _ => "0"
            };
        }

        private IEnumerator CenterRenderedMoleculeCoroutine()
        {
            yield return new WaitForSeconds(0.1f);

            if (!RenderProgram.Instance.HasAnyRenderEntity()) yield break;

            var (lower, higher) = RenderProgram.Instance.GetBound(0);

            var center = (lower + higher) / 2;

            cameraBox.transform.position = center;
        }

        public void SubmitTag()
        {
            _selectedTags.Clear();
            foreach (var (moleculeTag, toggle) in _tagToggles)
            {
                if (toggle.isOn)
                {
                    _selectedTags.Enqueue(moleculeTag);
                }
            }

            OpenView(0);
        }

        public void OpenTagEditor()
        {
            if (_curViewIndex == 1) return;
            OpenView(1);
        }

        public void CreateCompound()
        {
            if (_curViewIndex != 0) return;
            
            var builder = Molecule.Builder.Create(false);

            if (string.IsNullOrEmpty(idInput.text) || string.IsNullOrEmpty(nameInput.text))
            {
                Debug.LogError("Invalid id or name");
                return;
            }

            builder.ID(idInput.text);
            builder.TranslationKey(idInput.text);
            Translator.Instance.AddTranslation(idInput.text, nameInput.text);

            if (Formula == null)
            {
                Debug.LogError("Invalid formula");
                return;
            }

            builder.Structure(Formula);

            if (string.IsNullOrEmpty(densityInput.text) && float.TryParse(densityInput.text, out var density))
            {
                builder.Density(density);
            }

            if (string.IsNullOrEmpty(blInput.text) && float.TryParse(blInput.text, out var bl))
            {
                if (blUnitInput.value == 0)
                {
                    builder.BoilingPoint(bl);
                }
                else
                {
                    builder.BoilingPointInKelvins(bl);
                }
            }

            if (string.IsNullOrEmpty(dmInput.text) && int.TryParse(dmInput.text, out var dm))
            {
                builder.DipoleMoment(dm);
            }

            if (string.IsNullOrEmpty(shcInput.text) && float.TryParse(shcInput.text, out var shc))
            {
                builder.SpecificHeatCapacity(shc);
            }

            if (string.IsNullOrEmpty(mhcInput.text) && float.TryParse(mhcInput.text, out var mhc))
            {
                builder.MolarHeatCapacity(mhc);
            }

            if (string.IsNullOrEmpty(lhInput.text) && float.TryParse(lhInput.text, out var lh))
            {
                builder.LatentHeat(lh);
            }

            if (string.IsNullOrEmpty(liquidColorInput.text) &&
                ColorUtility.TryParseHtmlString(liquidColorInput.text, out var liquidColor))
            {
                //Note: convert Unity Color to int
                var liquidColorInt = (int)(liquidColor.r * 255) << 16 | (int)(liquidColor.g * 255) << 8 |
                                     (int)(liquidColor.b * 255);
                builder.Color(liquidColorInt);
            }

            if (string.IsNullOrEmpty(burningColorInput.text)
                && ColorUtility.TryParseHtmlString(burningColorInput.text, out var burningColor)
                && string.IsNullOrEmpty(burnIntensityInput.text)
                && float.TryParse(burnIntensityInput.text, out var burnIntensity))
            {
                builder.BurnColor(burningColor, burnIntensity);
            }

            if (string.IsNullOrEmpty(gasColorInput.text) &&
                ColorUtility.TryParseHtmlString(gasColorInput.text, out var gasColor))
            {
                //Note: convert Unity Color to int
                builder.GasColor(gasColor);
            }

            if (string.IsNullOrEmpty(solidColorInput.text) &&
                ColorUtility.TryParseHtmlString(solidColorInput.text, out var solidColor))
            {
                //Note: convert Unity Color to int
                builder.SolidColor(solidColor);
            }

            builder.Tag(_selectedTags.ToArray());
            //Note: create the molecule

            builder.Build();
            
            //Note: Reset all
            Formula = null;
            
            idInput.text = "";
            nameInput.text = "";
            formulaInput.text = "";
            densityInput.text = "";
            blInput.text = "";
            dmInput.text = "";
            shcInput.text = "";
            mhcInput.text = "";
            lhInput.text = "";
            liquidColorInput.text = "";
            burningColorInput.text = "";
            burnIntensityInput.text = "";
            gasColorInput.text = "";
            solidColorInput.text = "";
            
            foreach (var toggle in _tagToggles)
            {
                toggle.Value.isOn = false;
            }
        }
    }
}