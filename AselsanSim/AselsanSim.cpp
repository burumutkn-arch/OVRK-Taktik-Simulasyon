#include <iostream>
#include <thread>
#include <locale>
#include <vector>
#include <string>
#include "Vehicle.h"
#include "RingBuffer.h"
#include "DataLink.h" 

using namespace TacticalNetwork;
using namespace std;

struct Target {
    double lat; double lon; float thermal; float aiConf; string name;
};

int main() {
    setlocale(LC_ALL, "Turkish");

    cout << "========================================================\n";
    cout << "[HAREKAT MERKEZI] ASELSAN Faz-2 Taktik Simulatoru Aktif\n";
    cout << "========================================================\n";

    // Akıncı TİHA (Karargah: 39.9666, 32.7666)
    Vehicle akinci(1453, 39.9666, 32.7666, 25000.0f);
    RingBuffer memoryBuffer(100);
    DataLink radio;
    radio.Initialize(14530);

    // Uzak Koordinatlı Hedefler (Polatlı ve Kırıkkale Bölgesi)
    vector<Target> threats = {
        {39.5500, 32.0000, 25.5f, 45.0f, "Bilinmeyen Sinyal (Hedef 1)"},
        {39.8500, 33.5000, 850.0f, 98.5f, "S-400 Bataryası (Hedef 2)"}
    };

    bool jammerActive = true;

    cout << "[SISTEM UYARISI] Jammer Devrede! Telsiz Baglantisi YOK.\n";
    cout << "[SISTEM UYARISI] TİHA Otonom Uçuş Modunda (Kör Uçuş)...\n\n";

    // OOM Koruma: Jammer altında RingBuffer dolumu simülasyonu
    for (int tick = 1; tick <= 5; tick++) {
        akinci.Update();
        if (jammerActive) memoryBuffer.Push(akinci.GetData());
        this_thread::sleep_for(chrono::milliseconds(200));
    }

    for (int t = 0; t < threats.size(); t++) {
        cout << "\n*** [FÜZE İKAZ - MAWS] RADAR KİLİDİ TESPİT EDİLDİ: " << threats[t].name << " ***\n";
        cout << "-> [ÖLÜ ADAM ANAHTARI] SİHA Vuruşu Hesapladı, Sürü Fırlatılıyor!\n";
        cout << "-> [SÜRÜ ZEKASI] OVRK Kapsülleri Analiz İçin Gönderiliyor...\n";

        this_thread::sleep_for(chrono::seconds(2));

        TelemetryData data = akinci.GetData();
        data.latitude = threats[t].lat;
        data.longitude = threats[t].lon;
        data.thermalSignature = threats[t].thermal;
        data.aiConfidence = threats[t].aiConf;
        data.status = (uint8_t)DeviceStatus::ThreatLocked;

        // Sensör Füzyonu (Decoy Analizi)
        if (data.thermalSignature < 100.0f) {
            data.isDecoy = true;
            cout << "[SENSÖR FÜZYONU] Isı izi YETERSİZ (" << data.thermalSignature << "C).\n";
            cout << "[YAPAY ZEKA] TESPİT: HEDEF SAHTE (ŞİŞME MAKET DECOY)!\n";
            cout << "[KARAR] F-16 Taarruzu İPTAL EDİLDİ. Mühimmat korunuyor.\n\n";
        }
        else {
            data.isDecoy = false;
            cout << "[SENSÖR FÜZYONU] " << data.thermalSignature << "C Jeneratör Isı İzi TESPİT EDİLDİ.\n";
            cout << "[YAPAY ZEKA] TESPİT: HEDEF GERÇEK S-400!\n";
        }

        // Sürünün Veriyi Kriptolu Göndermesi
        for (int i = 1; i <= 3; i++) {
            data.swarmId = i;
            data.isLaserActive = (!data.isDecoy && i == 3);

            cout << "[OVRK " << i << "] AES-256 Kriptolu Veri Karargaha Aktarılıyor...\n";
            bool sendSuccess = radio.Transmit(data);

            if (sendSuccess && data.isLaserActive) {
                cout << "\n[LAZER İŞARETLEYİCİ] OVRK Charlie GERÇEK hedefe daldı! Lazer AKTİF!\n";
                cout << "=================== GÖREV KUSURSUZ ===================\n";
            }
            this_thread::sleep_for(chrono::milliseconds(500));
        }
        this_thread::sleep_for(chrono::seconds(2));
    }

    radio.Shutdown();
    return 0;
}