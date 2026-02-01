using TMPro;
using UnityEngine;
using Oculus.Interaction;
using static ElectronicPipette;

public class ElectronicPipetteBoard : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI currentVolumeText;
    [SerializeField] private TextMeshProUGUI setVolumeText;
    [SerializeField] private TextMeshPro stepModeText;

    [Header("Buttons")]
    public PointableUnityEventWrapper stepModeButton;
    public PointableUnityEventWrapper increaseVolumeButton;
    public PointableUnityEventWrapper decreaseVolumeButton;
    public PointableUnityEventWrapper aspirateButton;
    public PointableUnityEventWrapper dispenseButton;

    public void SetCurrentVolumeText(float currentVolume, float maxVolume)
    {
        if (currentVolumeText != null)
        {
            string txt = currentVolume.ToString("0.####") + " / " + maxVolume.ToString("0.####");
            currentVolumeText.text = txt;
        }
    }

    public void SetSetVolumeText(float setVolume)
    {
        if (setVolumeText != null)
        {
            setVolumeText.text = setVolume.ToString("0.####");
        }
    }

    public void SetStepModeText(VolumeStep step)
    {
        if (stepModeText != null)
        {
            string stepMode = step switch
            {
                VolumeStep.ML_1     => "1",
                VolumeStep.ML_0_1   => "0.1",
                VolumeStep.ML_0_01  => "0.01",
                VolumeStep.ML_0_001 => "0.001",
                _ => "Unknown"
            };
            stepModeText.text = stepMode;
        }
    }
}
