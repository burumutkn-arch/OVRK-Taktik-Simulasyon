#pragma once
#include <vector>
#include "TelemetryData.h"

namespace TacticalNetwork {

    // Dairesel Tampon (Ring Buffer) Sınıfımız
    class RingBuffer {
    private:
        std::vector<TelemetryData> buffer; // Verileri tutacağımız dizi
        size_t head;       // Yeni verinin yazılacağı indeks (Kalem)
        size_t tail;       // En eski verinin okunacağı indeks (Silgi)
        size_t capacity;   // Kutunun maksimum kapasitesi
        bool isFull;       // Kutu ağzına kadar doldu mu?

    public:
        // Constructor: Kutu yaratılırken maksimum kaç veri alacağını belirtiriz
        RingBuffer(size_t size) {
            capacity = size;
            buffer.resize(capacity); // Bellekte (RAM) yeri en baştan tek seferde ayır! (Çok kritik)
            head = 0;
            tail = 0;
            isFull = false;
        }

        // Yeni veri eklendiğinde çalışacak fonksiyon
        void Push(const TelemetryData& data) {
            // 1. Yeni veriyi 'head' (kalem) noktasına yaz
            buffer[head] = data;

            // 2. Eğer kutu doluysa ve yeni veri yazdıysak, en eski veriyi "ezmiş" oluruz.
            // Bu yüzden 'tail' (okuma noktası) bir adım ileri kaymalı ki ezilen veriyi okumaya çalışmasın.
            if (isFull) {
                tail = (tail + 1) % capacity;
            }

            // 3. Kalemi bir sonraki boş kutuya kaydır.
            // '%' (Mod Alma) işlemi çemberi sağlar: Kutu sonuna (örneğin 100) gelirse başa (0) döner.
            head = (head + 1) % capacity;

            // 4. Eğer kalem (head), silgiye (tail) yetiştiyse kutu dolmuş demektir.
            isFull = (head == tail);
        }

        // Kutuda hiç veri var mı?
        bool IsEmpty() const {
            return (!isFull && (head == tail));
        }

        // İçeride gönderilmeyi bekleyen kaç tane veri var?
        size_t Size() const {
            if (isFull) return capacity;
            if (head >= tail) return head - tail;
            return capacity + head - tail; // Çemberin başa sardığı durum hesabı
        }
    };

}