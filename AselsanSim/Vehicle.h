#pragma once
#include "TelemetryData.h"
#include <cmath>
#include <chrono>

namespace TacticalNetwork {

    class Vehicle {
    private:
        TelemetryData currentData;
        const double PI = 3.14159265358979323846;
        const double EARTH_RADIUS = 6371000.0;

    public:
        Vehicle(uint32_t id, double startLat, double startLon, float startAlt) {
            currentData.deviceId = id;
            currentData.latitude = startLat;
            currentData.longitude = startLon;
            currentData.altitude = startAlt;
            currentData.speed = 25.0f;
            currentData.heading = 45.0f;
            currentData.status = (uint8_t)DeviceStatus::Normal;
        }

        void Update() {
            auto now = std::chrono::system_clock::now();
            currentData.timestamp = std::chrono::duration_cast<std::chrono::milliseconds>(now.time_since_epoch()).count();

            float headingRad = currentData.heading * (PI / 180.0f);
            float distancePerTick = currentData.speed / 10.0f;

            double latOffset = (distancePerTick * cos(headingRad)) / EARTH_RADIUS * (180.0 / PI);
            double lonOffset = (distancePerTick * sin(headingRad)) / (EARTH_RADIUS * cos(currentData.latitude * PI / 180.0)) * (180.0 / PI);

            currentData.latitude += latOffset;
            currentData.longitude += lonOffset;
        }

        TelemetryData GetData() const {
            return currentData;
        }
    };
}
