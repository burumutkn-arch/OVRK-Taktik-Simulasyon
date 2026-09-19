#pragma once
#include <cstdint>

namespace TacticalNetwork {
    enum class DeviceStatus : uint8_t {
        Normal = 0,
        Jammed = 1,
        HardwareFault = 2,
        ThreatLocked = 3
    };

#pragma pack(push, 1) 
    struct TelemetryData {
        uint32_t deviceId;
        uint8_t swarmId;
        double latitude;
        double longitude;
        float altitude;
        float speed;
        float heading;
        uint64_t timestamp;
        uint8_t status;
        float aiConfidence;
        bool isLaserActive;

        // FAZ-2 EKLENTİLERİ (Sensör Füzyonu)
        float thermalSignature;  // Termal Isı İzi (Santigrat)
        bool isDecoy;            // Hedef Sahte mi? (Şişme Maket)
    };
#pragma pack(pop)
}