using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.mixture;

public class ToolSetting : MonoBehaviour
{
    public TMP_Dropdown dropdownChemical;
    public Slider sliderVolume;
    public TextMeshProUGUI textVolumeValue;
    public Button btnConfirm;
    public Button btnCancel;

    private WristUI wristUI;
    private int volume = 0;

    private List<MoleculeType> _types = new List<MoleculeType>();

    void Awake()
    {
        wristUI = GetComponentInParent<WristUI>();
        InitDropdown();
        InitSlider();
        btnConfirm.onClick.AddListener(OnConfirm);
        btnCancel.onClick.AddListener(() => wristUI.pageManager.ShowPage(EnumPage.ChemicalTool));
    }

    void OnEnable()
    {
        volume = 0;
        textVolumeValue.text = volume.ToString();
        sliderVolume.minValue = 0;
        if (ChemicalToolsSO._cache_chemicalTool != null)
            sliderVolume.maxValue = ChemicalToolsSO._cache_chemicalTool.maxVolume;
        sliderVolume.value = 0;
    }

    void InitDropdown()
    {
        dropdownChemical.ClearOptions();
        _types.Clear();

        var options = new List<TMP_Dropdown.OptionData>();

        foreach (MoleculeType type in Enum.GetValues(typeof(MoleculeType)))
        {
            _types.Add(type);

            // format đẹp hơn
            string display = type.ToString();
            display = System.Text.RegularExpressions.Regex
                .Replace(display, "(\\B[A-Z])", " $1"); // thêm space trước chữ hoa

            options.Add(new TMP_Dropdown.OptionData(display));
        }

        dropdownChemical.AddOptions(options);
    }

    void InitSlider()
    {
        UpdateVolumeText(sliderVolume.value);
        sliderVolume.onValueChanged.AddListener(UpdateVolumeText);
    }

    void UpdateVolumeText(float value)
    {
        volume = (int)value;
        textVolumeValue.text = volume.ToString();
    }

    public void OnConfirm()
    {
        MoleculeType selectedType = _types[dropdownChemical.value];
        Molecule molecule = MoleculeRegistry.Get(selectedType);

        Transform cam = Camera.main.transform;
        Vector3 spawnPos = cam.position + cam.forward * 0.2f;
        LiquidContainer liquidContainer = Instantiate(ChemicalToolsSO._cache_chemicalTool.prefab, spawnPos, Quaternion.identity).GetComponent<LiquidContainer>();
        if (liquidContainer != null)
        {
            if (selectedType == MoleculeType.None || volume <= 0)
                return;
            else
            {
                var mixture = Mixture.Pure(molecule);
                liquidContainer.SetMixtureAndVolume(mixture, volume);
            }
        }
    }
}