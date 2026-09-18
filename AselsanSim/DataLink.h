#pragma once
#include <iostream>
#include <string>
#include <winsock2.h>
#include <ws2tcpip.h>
#include "TelemetryData.h"

// Windows Socket kütüphanesini projeye bağlar
#pragma comment(lib, "ws2_32.lib") 

namespace TacticalNetwork {

    class DataLink {
    private:
        WSADATA wsaData;
        SOCKET broadcastSocket;
        sockaddr_in recvAddr;
        bool isInitialized;

    public:
        DataLink() {
            isInitialized = false;
        }

        bool Initialize(int port = 14530) {
            if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
                std::cerr << "[SISTEM HATASI] Telsiz gucu acilamadi!\n";
                return false;
            }

            broadcastSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
            if (broadcastSocket == INVALID_SOCKET) {
                WSACleanup();
                return false;
            }

            char broadcast = '1';
            if (setsockopt(broadcastSocket, SOL_SOCKET, SO_BROADCAST, &broadcast, sizeof(broadcast)) < 0) {
                closesocket(broadcastSocket);
                WSACleanup();
                return false;
            }

            recvAddr.sin_family = AF_INET;
            recvAddr.sin_port = htons(port);
            recvAddr.sin_addr.s_addr = INADDR_BROADCAST;

            isInitialized = true;
            return true;
        }

        // Telsizden Veriyi Havaya Fırlatma (Kriptolu ve FHSS Eklenmiş Hali)
        bool Transmit(TelemetryData data) { // Verinin kopyasını alıyoruz
            if (!isInitialized) return false;

            // 1. FHSS (Frekans Atlamalı Yayılı Spektrum) Simülasyonu
            std::cout << "[FHSS AKTIF] Frekanslar atliyor: 14530 -> " << (14500 + rand() % 100) << " -> " << (14500 + rand() % 100) << " MHz...\n";

            // 2. Kriptolama (Askeri XOR Simülasyonu - Sözde AES)
            char* rawBytes = (char*)&data;
            char secretKey = 0xAA; // Gizli Askeri Anahtarımız
            for (size_t i = 0; i < sizeof(TelemetryData); i++) {
                rawBytes[i] ^= secretKey; // XOR işlemiyle kilitler
            }
            std::cout << "[SIBER GUVENLIK] Veri paketi AES anahtari ile KRIPTOLANDI! (Dusman okuyamaz)\n";

            // Şifrelenmiş veriyi havaya fırlat
            int sendResult = sendto(broadcastSocket, (const char*)&data, sizeof(TelemetryData), 0, (sockaddr*)&recvAddr, sizeof(recvAddr));

            return (sendResult != SOCKET_ERROR);
        }

        void Shutdown() {
            if (isInitialized) {
                closesocket(broadcastSocket);
                WSACleanup();
                isInitialized = false;
            }
        }

        ~DataLink() {
            Shutdown();
        }
    };
}