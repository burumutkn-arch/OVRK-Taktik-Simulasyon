#include <iostream>
#include <thread>
#include <locale>
#include "Vehicle.h"
#include "RingBuffer.h"
#include "DataLink.h" 

using namespace TacticalNetwork;
using namespace std;

int main() {
    setlocale(LC_ALL, "Turkish");

    cout << "========================================================\n";
    cout << "[HAREKAT MERKEZI] ASELSAN Taktik Ucbirim Simulatoru Aktif\n";
    cout << "========================================================\n";

    Vehicle akinci(1453, 39.9666, 32.7666, 25000.0f);
    RingBuffer memoryBuffer(100);
    DataLink radio;
    radio.Initialize(14530);

    bool jammerActive = true;
    bool threatDetected = false;
    bool messengerBirdLaunched = false;
    int birdFlightTimer = 0;

    cout << "[SISTEM UYARISI] Jammer Devrede! Telsiz Baglantisi YOK.\n";
    cout << "[SISTEM UYARISI] TİHA Otonom Ucus Modunda (Kör Ucus)...\n\n";

    for (int tick = 1; tick <= 100; tick++) {
        akinci.Update();
        TelemetryData data = akinci.GetData();

        if (tick == 20) {
            threatDetected = true;
            data.status = (uint8_t)DeviceStatus::ThreatLocked;
            cout << "\n\n*** [FÜZE İKAZ - MAWS] S-400 RADAR KİLİDİ TESPİT EDİLDİ! ***\n";
            cout << "-> S-400 Kordinati RAM'e kopyalaniyor: Enlem " << data.latitude << " | Boylam " << data.longitude << "\n";
        }

        if (jammerActive) {
            memoryBuffer.Push(data);

            if (threatDetected && !messengerBirdLaunched) {
                cout << "-> [ÖLÜ ADAM ANAHTARI] SİHA Kaçınılmaz Vuruşu Hesapladı!\n";
                cout << "-> [SÜRÜ ZEKASI] 3 Adet OVRK (Alfa, Bravo, Charlie) Kapsulu FIRLATILDI!\n";
                cout << "-> Haberci Kuslar S-400 verisini paylasimli olarak RAM'lerine aldi...\n\n";
                messengerBirdLaunched = true;
            }
        }

        if (messengerBirdLaunched) {
            birdFlightTimer++;
            cout << "[OVRK SÜRÜSÜ] Jammer'dan kaciliyor... Irtifa kaybediliyor. Gecen sure: " << birdFlightTimer << " sn\r";

            if (birdFlightTimer == 30) {
                cout << "\n\n[OVRK SÜRÜSÜ] JAMMER SINIRINDAN ÇIKILDI!\n";
                cout << "[YAPAY ZEKA] OVRK Alfa Kamerasi Hedefi Teyit Etti: %98.5 Gercek S-400\n";
                cout << "[YÖNLÜ ANTEN] Uyduya dogru Ince Hüzme (Pencil Beam) RF kilidi atildi...\n\n";

                data.aiConfidence = 98.5f;

                for (int i = 1; i <= 3; i++) {
                    data.swarmId = i;
                    data.isLaserActive = (i == 3);

                    cout << "[OVRK " << i << "] Kriptolu Veri paketi hazirlaniyor...\n";
                    bool sendSuccess = radio.Transmit(data);

                    if (sendSuccess && i == 3) {
                        cout << "\n[LAZER İŞARETLEYİCİ] OVRK Charlie hedefe dalisa gecti! Lazer AKTIF!\n";
                        cout << "[HAREKAT MERKEZI] S-400 Lazerle Isaretlendi! F-16 Taarruzu Icin Bekleniyor...\n";
                        cout << "=================== GOREV KUSURSUZ ===================\n";
                    }
                    this_thread::sleep_for(chrono::milliseconds(200));
                }
                break;
            }
        }
        this_thread::sleep_for(chrono::milliseconds(200));
    }

    radio.Shutdown();
    return 0;
}