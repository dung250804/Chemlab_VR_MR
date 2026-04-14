using UnityEngine;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.mixture;
using com.ethnicthv.chemlab.engine.molecule;
using System.Collections.Generic;
using Environment = com.ethnicthv.chemlab.client.core.game.Environment;
using com.ethnicthv.chemlab.engine;

public abstract class ContainerEquipmentBase : LabEquipmentBase,
    IMixtureContainer, IChemicalTicker,
    IHeatable, ISolidContainer, IGasContainer
{
    [Header("Settings")]
    public float maxVolume = 100f;
    public float currentVolume = 0;

    public Transform spoutCenter;
    public float spoutRadius;

    [SerializeField] private ParticleSystem smokeParticle;
    [SerializeField] private bool heatAffect = true;

    [Header("=== DEBUG MIXTURE ===")]
    [SerializeField] private List<string> debugMolecules = new();
    [SerializeField] private List<float> debugMoles = new();
    [SerializeField] private float debugTemperature;


    private float _heatPower;
    private IHeater _heater;

    private Mixture _contents;

    private Mixture _tickGasMixture;
    private float _tickGasVolume;

    private void OnEnable()
    {
        if (smokeParticle != null)
        {
            var emission = smokeParticle.emission;
            emission.rateOverTime = 0;
        }
        ChemicalTickerHandler.AddTicker(this);
    }

    private void OnDisable()
    {
        ChemicalTickerHandler.RemoveTicker(this);
    }

    
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

        // if (newVolume > 0)
        //     _contents.Scale(1000f / newVolume);

        currentVolume = newVolume;

        NormalizeState();
    }

    public void AddMixture(Mixture mixture, float volume)
    {
        if (mixture == null || volume <= 0 || currentVolume >= maxVolume) return;
        float realVolume = Mathf.Min(volume, maxVolume - currentVolume);
        var (newMixture, newVolume) = Mixture.Mix(new Dictionary<Mixture, float>
        {
            { GetMixture(), GetVolume() },
            { mixture, realVolume }
        });
        
        SetMixtureAndVolume(newMixture, newVolume);
        // float volumeInLiters = newVolume / 1000f;
        // _contents.Scale(1f / volumeInLiters);
        NormalizeState();
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
        
        // Newton cooling
        float k = 0.1f; // hệ số trao đổi nhiệt
        float envCooling = k * (_contents.GetTemperatureKelvin() - Environment.Instance.Temperature);

        float heat = _heatPower - envCooling;

        if (currentVolume > 0 && heatAffect)
        {
            float volumeInLiters = currentVolume / 1000f;
            _contents.Heat(heat / volumeInLiters);
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
            _tickGasMixture?.UpdateColor();
            UpdateSmoke();
        }
        _contents.UpdateColor();
        NormalizeState();
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

    protected void NormalizeState()
    {
        if (_contents == null || currentVolume <= 0)
        {
            _contents = null;
            currentVolume = 0;
            return;
        }

        if (_contents.GetMolecules().Count == 0)
        {
            _contents = null;
            currentVolume = 0;
        }
    }

    public float GetPH()
    {
        var mixture = GetMixture();
        if (mixture == null || currentVolume <= 0f)
            return 7f;

        // Đổi thể tích sang L (giả sử currentVolume là mL)
        float volumeInLiters = currentVolume / 1000f;

        // Lấy số mol
        float hMoles = mixture.GetMoles(Molecules.Proton);
        float ohMoles = mixture.GetMoles(Molecules.Hydroxide);

        // Đổi sang nồng độ (mol/L)
        float h = hMoles / volumeInLiters;
        float oh = ohMoles / volumeInLiters;

        const float Kw = 1e-14f;
        const float epsilon = 1e-7f;

        // Nếu có OH- mà không có H+ → tính ngược lại từ Kw
        if (h <= 0f && oh > 0f)
        {
            h = Kw / oh;
        }

        // Nếu không có gì → trung tính
        if (h <= 0f)
        {
            h = epsilon;
        }

        float pH = -Mathf.Log10(h);

        // Clamp cho đẹp (tránh bug số)
        return Mathf.Clamp(pH, 0f, 14f);
    }

    protected void UpdateDebug()
    {
        debugMolecules.Clear();
        debugMoles.Clear();

        if (_contents == null) return;

        debugTemperature = _contents.GetTemperatureKelvin();

        foreach (var molecule in _contents.GetMolecules())
        {
            // Lấy nồng độ (mol/L) nhân với thể tích (Lít) để ra số mol thực tế
            float concentration = _contents.GetMoles(molecule);
            float actualMoles = concentration * (currentVolume / 1000f); 

            if (actualMoles <= 0) continue;

            debugMolecules.Add(molecule.GetFullID());
            debugMoles.Add(actualMoles); 
        }
    }

    private void UpdateSmoke()
    {
        if (smokeParticle == null) return;
        
        var emission = smokeParticle.emission;
        var main = smokeParticle.main;

        // Không có khí → tắt khói
        if (_tickGasMixture == null || _tickGasVolume <= 0 || _contents == null || currentVolume <= 0)
        {
            emission.rateOverTime = 0;
            return;
        }

        main.startColor = _tickGasMixture.GetColor();

        // =========================
        // 1. INTENSITY
        // =========================

        float gasBuffer = 0f;
        gasBuffer += _tickGasVolume;
        gasBuffer = Mathf.Lerp(gasBuffer, 0f, Time.deltaTime * 2f);
        // normalize lượng khí (tự chỉnh maxGas tùy game)
        float maxGas = 0.1f; // mL/tick (tweak)
        float intensity = Mathf.Clamp01(gasBuffer / maxGas);

        // =========================
        // 2. TEMPERATURE
        // =========================
        float temp = _contents.GetTemperatureKelvin();
        float tempNorm = Mathf.Clamp01((temp - 300f) / 200f); // 300K → 500K

        // =========================
        // 3. VOLATILITY 
        // =========================
        float volatility = EstimateVolatility(_tickGasMixture);

        // =========================
        // APPLY TO PARTICLE
        // =========================

        // Khói nhiều hay ít
        emission.rateOverTime = intensity * 100f;

        // Độ to hạt
        main.startSize = Mathf.Lerp(0.02f, 0.04f, intensity);

        // Bay nhanh (nhiệt)
        main.startSpeed = Mathf.Lerp(0.2f, 1f, tempNorm);

        // Tan nhanh (chất)
        float maxStartLifeTime = Mathf.Lerp(0.5f, 1.5f, currentVolume / maxVolume);
        float lifetime = maxStartLifeTime * Mathf.Lerp(1f, 0.4f, volatility);

        main.startLifetime = Mathf.Max(lifetime, 0.3f);
    }   

    private float EstimateVolatility(Mixture gas)
    {
        if (gas == null) return 0.5f;

        float score = 0f;
        int count = 0;

        foreach (var mol in gas.GetMolecules())
        {
            count++;

            // Rule đơn giản (bạn có thể refine sau)
            if (mol == Molecules.Water) score += 0.3f;
            else if (mol == Molecules.Hydrogen) score += 1f;
            else if (mol == Molecules.Ammonia) score += 0.9f;
            else score += 0.6f; // default
        }

        return count > 0 ? score / count : 0.5f;
    }
}