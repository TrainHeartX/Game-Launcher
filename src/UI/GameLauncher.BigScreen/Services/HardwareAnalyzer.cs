using System;
using System.Text.RegularExpressions;
using GameLauncher.BigScreen.Models;
using GameLauncher.BigScreen.ViewModels;

namespace GameLauncher.BigScreen.Services;

public class RequirementsAnalyzer
{
    private readonly SystemInfoViewModel _systemInfo;

    public RequirementsAnalyzer(SystemInfoViewModel systemInfo)
    {
        _systemInfo = systemInfo;
    }

    public CompatibilityVerification Verify(GameRequirementInfo requirements)
    {
        var result = new CompatibilityVerification();

        // Check RAM
        result.RamCheck = CheckRam(requirements.Ram);
        
        // OS Check
        result.OsCheck = CheckOs(requirements.Os);

        // CPU Check
        result.CpuCheck = CheckCpu(requirements.Cpu);

        // GPU Check
        result.GpuCheck = CheckGpu(requirements.Gpu);

        return result;
    }

    private CompatibilityStatus CheckRam(string reqRam)
    {
        var reqGb = ExtractGb(reqRam);
        if (reqGb <= 0) return CompatibilityStatus.ManualCheck;
        var sysGb = ExtractGb(_systemInfo.RamTotal);
        if (sysGb <= 0) return CompatibilityStatus.ManualCheck;
        return sysGb >= reqGb ? CompatibilityStatus.Pass : CompatibilityStatus.Fail;
    }

    private CompatibilityStatus CheckOs(string reqOs)
    {
        var sysOs = _systemInfo.OsVersion;
        if (string.IsNullOrEmpty(reqOs)) return CompatibilityStatus.ManualCheck;
        if (reqOs.Contains("Windows 10") && sysOs.Contains("Windows 10")) return CompatibilityStatus.Pass;
        if (reqOs.Contains("Windows 11") && (sysOs.Contains("Windows 11") || sysOs.Contains("Windows 10"))) return CompatibilityStatus.Pass;
        return CompatibilityStatus.ManualCheck; 
    }

    private CompatibilityStatus CheckCpu(string reqCpu)
    {
        var sysCpu = _systemInfo.CpuName;
        
        // Simple Brand Check
        bool reqIntel = reqCpu.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0;
        bool reqAmd = reqCpu.IndexOf("AMD", StringComparison.OrdinalIgnoreCase) >= 0;
        bool sysIntel = sysCpu.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0;
        bool sysAmd = sysCpu.IndexOf("AMD", StringComparison.OrdinalIgnoreCase) >= 0;

        // If brands don't match, we can't easily compare (e.g. Intel vs AMD req) without a database.
        // But if requirement lists BOTH (Intel / AMD), we just need to match one.
        
        if (sysIntel && reqIntel) return CheckIntelCpu(reqCpu, sysCpu);
        if (sysAmd && reqAmd) return CheckAmdCpu(reqCpu, sysCpu);
        
        // Fallback: If we have separate checks for both, try them
        if (sysIntel && !reqIntel && reqAmd) return CompatibilityStatus.ManualCheck; // User has Intel, Req only says AMD? Warning.
        
        return CompatibilityStatus.ManualCheck;
    }

    private CompatibilityStatus CheckIntelCpu(string req, string sys)
    {
        // Extract Core iX
        int reqTier = ExtractIntelTier(req); // 3, 5, 7, 9
        int sysTier = ExtractIntelTier(sys);

        if (sysTier > 0 && reqTier > 0)
        {
            if (sysTier >= reqTier) return CompatibilityStatus.Pass;
            return CompatibilityStatus.Fail;
        }
        return CompatibilityStatus.ManualCheck;
    }

    private int ExtractIntelTier(string text)
    {
        if (text.Contains("i9")) return 9;
        if (text.Contains("i7")) return 7;
        if (text.Contains("i5")) return 5;
        if (text.Contains("i3")) return 3;
        return 0;
    }

    private CompatibilityStatus CheckAmdCpu(string req, string sys)
    {
        int reqTier = ExtractAmdTier(req); // 3, 5, 7, 9
        int sysTier = ExtractAmdTier(sys);
        
        if (sysTier > 0 && reqTier > 0)
        {
            if (sysTier >= reqTier) return CompatibilityStatus.Pass;
            return CompatibilityStatus.Fail;
        }
        return CompatibilityStatus.ManualCheck;
    }

    private int ExtractAmdTier(string text)
    {
        if (text.Contains("Ryzen 9")) return 9;
        if (text.Contains("Ryzen 7")) return 7;
        if (text.Contains("Ryzen 5")) return 5;
        if (text.Contains("Ryzen 3")) return 3;
        return 0;
    }

    private CompatibilityStatus CheckGpu(string reqGpu)
    {
        var sysGpu = _systemInfo.GpuName;
        var sysVram = _systemInfo.GpuVram;

        // 1. VRAM Check (Strongest indicator we have right now)
        // Extract VRAM from requirement if possible (e.g. "4GB VRAM")
        var reqVram = ExtractGb(reqGpu);
        var sysVramGb = ExtractGb(sysVram);

        if (reqVram > 0 && sysVramGb > 0)
        {
            if (sysVramGb >= reqVram) return CompatibilityStatus.Pass;
             // If VRAM fails, it's likely a fail, but let's be cautious and say Fail
            return CompatibilityStatus.Fail;
        }

        // 2. Series Check (Simple)
        // Nvidia GTX/RTX
        if (sysGpu.Contains("RTX") && reqGpu.Contains("GTX")) return CompatibilityStatus.Pass; // RTX > GTX generally
        
        // Number extraction (very loose)
        // If extracted numbers exist, compare? Too risky without normalization.
        
        return CompatibilityStatus.ManualCheck;
    }

    private double ExtractGb(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        var match = Regex.Match(text, @"(\d+)(\s*GB|\s*MB)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            double val = double.Parse(match.Groups[1].Value);
            string unit = match.Groups[2].Value.Trim().ToUpper();
            if (unit == "MB") val = val / 1024.0;
            return val;
        }
        return 0;
    }
}

public class CompatibilityVerification
{
    public CompatibilityStatus RamCheck { get; set; }
    public CompatibilityStatus OsCheck { get; set; }
    public CompatibilityStatus CpuCheck { get; set; }
    public CompatibilityStatus GpuCheck { get; set; }
    
    public bool CanRun => RamCheck != CompatibilityStatus.Fail && OsCheck != CompatibilityStatus.Fail;
}

public enum CompatibilityStatus
{
    Pass,
    Fail,
    ManualCheck
}
