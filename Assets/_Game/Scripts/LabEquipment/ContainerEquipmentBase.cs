using UnityEngine;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.mixture;
using com.ethnicthv.chemlab.engine.molecule;
using System.Collections.Generic;
using Environment = com.ethnicthv.chemlab.client.core.game.Environment;

public abstract class ContainerEquipmentBase : LabEquipmentBase,
    IMixtureContainer, IChemicalTicker,
    IHeatable, ISolidContainer, IGasContainer
{
    [Header("Settings")]
    public float maxVolume = 100f;
    public float currentVolume = 0;

    public Transform spoutCenter;
    public float spoutRadius;

    [SerializeField] private float heatConductivity = 1000f;

    [Header("=== DEBUG MIXTURE ===")]
    [SerializeField] private List<string> debugMolecules = new();
    [SerializeField] private List<float> debugMoles = new();
    [SerializeField] private float debugTemperature;


    private float _heatPower;
    private IHeater _heater;

    private Mixture _contents;

    private Mixture _tickGasMixture;
    private float _tickGasVolume;
    
    // =========================
    // ===== MIXTURE CORE ======
    // =========================

    public float GetMaxVolume() => maxVolume;

    public float GetVolume() => currentVolume;

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp(volume, 0, maxVolume);
    }

    public Mixture GetMixture() => _contents;

    public void SetMixture(Mixture mixture)
    {
        _contents = mixture;
    }

    public void SetMixtureAndVolume(Mixture mixture, float volume)
    {
        _contents = mixture;
        SetVolume(volume);
    }

    public (Mixture mixture, float volume) GetMixtureAndVolume()
    {
        return (_contents, currentVolume);
    }

    public bool IsEmpty()
    {
        return _contents == null || currentVolume <= 0;
    }

    public void Clear()
    {
        _contents = null;
        currentVolume = 0;
    }

    // =========================
    // ===== ADD / MIX =========
    // =========================

    /// 🔥 HÀM QUAN TRỌNG NHẤT
    public void ReceiveMixture(Mixture incoming, float volume)
    {
        if (incoming == null || volume <= 0) return;

        // Clamp theo sức chứa còn lại
        float spaceLeft = maxVolume - currentVolume;
        float actualVolume = Mathf.Min(volume, spaceLeft);

        if (actualVolume <= 0) return;

        // Nếu container rỗng → set trực tiếp
        if (_contents == null || currentVolume <= 0)
        {
            _contents = incoming;
            currentVolume = actualVolume;
            return;
        }

        var mixtures = new Dictionary<Mixture, float>
        {
            { _contents, currentVolume },
            { incoming, actualVolume }
        };

        var (mixed, newVolume) = Mixture.Mix(mixtures);

        _contents = mixed;

        if (newVolume > 0)
            _contents.Scale(1f / newVolume);

        currentVolume = newVolume;
    }

    public Mixture ExtractMixture(float volume)
    {
        if (_contents == null || currentVolume <= 0) return null;

        float ratio = volume / currentVolume;

        var extracted = Mixture.CreateMixture();

        foreach (var molecule in _contents.GetMolecules())
        {
            float moles = _contents.GetMoles(molecule);
            extracted.AddMoles(molecule, moles, out _);
        }

        extracted.Scale(1f / ratio);

        currentVolume -= volume;

        return extracted;
    }

    public void AddMixture(Mixture mixture, float volume)
    {
        ReceiveMixture(mixture, volume);
    }

    // =========================
    // ===== HEATING ===========
    // =========================

    public void SetHeatPower(float heatPower)
    {
        _heatPower = heatPower;
    }

    public bool IsHeating() => _heatPower > 0;

    public void SetHeater(IHeater heater)
    {
        _heater = heater;
    }

    public bool IsAttachedToHeater(out IHeater heater)
    {
        heater = _heater;
        return heater != null;
    }

    // =========================
    // ===== CHEM TICK =========
    // =========================

    public void Tick()
    {
        if (_contents == null) return;

        float heat = _heatPower;

        heat += (Environment.Instance.Temperature - _contents.GetTemperature()) * heatConductivity;
        heat /= 20f;

        if (currentVolume > 0)
        {
            _contents.Heat(heat / currentVolume);
            _contents.DisturbEquilibrium();
        }

        _contents.Tick(out var shouldUpdate);

        if (shouldUpdate)
        {
            var phases = _contents.SeparatePhases(currentVolume, true);

            _contents = phases.LiquidMixture;
            currentVolume = phases.LiquidVolume;

            _tickGasMixture = phases.GasMixture;
            _tickGasVolume = phases.GasVolume;
        }

        UpdateDebug();
    }

    // =========================
    // ===== GAS ===============
    // =========================

    public void SetGasMixture(Mixture mixture)
    {
        _tickGasMixture = mixture;
    }

    public Mixture GetGasMixture() => _tickGasMixture;

    public float GetGasVolume() => _tickGasVolume;

    public bool HasGasMixture()
    {
        return _tickGasMixture != null && _tickGasVolume > 0;
    }

    public bool TryGetGasMixture(out Mixture mixture)
    {
        if (!HasGasMixture())
        {
            mixture = null;
            return false;
        }

        mixture = _tickGasMixture;
        return true;
    }

    public void AddGasMixture(Mixture mixture, float volume)
    {
        if (_tickGasMixture == null)
        {
            _tickGasMixture = mixture;
            _tickGasVolume = volume;
            return;
        }

        var mixtures = new Dictionary<Mixture, float>
        {
            { _tickGasMixture, _tickGasVolume },
            { mixture, volume }
        };

        var (mixed, newVolume) = Mixture.Mix(mixtures);

        _tickGasMixture = mixed;
        _tickGasMixture.Scale(1f / newVolume);
        _tickGasVolume = newVolume;
    }

    // =========================
    // ===== SOLID =============
    // =========================

    public void AddSolidMolecule(Molecule solidMolecule, float moles)
    {
        if (!solidMolecule.IsSolid()) return;
        if (moles <= 0) return;

        if (_contents == null)
            _contents = Mixture.CreateMixture();

        if (currentVolume <= 0)
            currentVolume = 0.001f;

        _contents.AddMoles(solidMolecule, moles / currentVolume, out _);
    }

    public void RemoveSolidMolecule(Molecule solidMolecule)
    {
        Debug.LogError("Cannot remove solid directly from mixture");
    }

    public bool ContainsSolidMolecule(Molecule solidMolecule)
    {
        return _contents != null && _contents.ContainMolecule(solidMolecule);
    }

    public float GetSolidMoleculeAmount(Molecule solidMolecule)
    {
        return _contents.GetMoles(solidMolecule);
    }

    public bool TryGetSolidMoleculeAmount(Molecule solidMolecule, out float amount)
    {
        if (!ContainsSolidMolecule(solidMolecule))
        {
            amount = 0;
            return false;
        }

        amount = _contents.GetMoles(solidMolecule);
        return true;
    }

    public bool IsSolidEmpty()
    {
        return _contents == null;
    }

    public void ClearSolid()
    {
        Debug.LogError("Cannot clear solid separately from mixture");
    }

    protected void UpdateDebug()
    {
        debugMolecules.Clear();
        debugMoles.Clear();

        if (_contents == null) return;

        debugTemperature = _contents.GetTemperature();

        foreach (var molecule in _contents.GetMolecules())
        {
            float moles = _contents.GetMoles(molecule);

            if (moles <= 0) continue;

            debugMolecules.Add(molecule.GetFullID());
            debugMoles.Add(moles);
        }
    }
}