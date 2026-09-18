# OVRK (Autonomous Data Relay Capsule) & Tactical Mesh Network
**Air-Launched Data Mule & C2 Simulator for Electronic Warfare Environments**

![C++](https://img.shields.io/badge/C++-17-blue.svg) ![C#](https://img.shields.io/badge/C%23-.NET-purple.svg) ![Architecture](https://img.shields.io/badge/Architecture-Decentralized-success.svg) ![Status](https://img.shields.io/badge/Status-Prototype-orange.svg)

*(Türkçe dokümantasyon sayfanın aşağısındadır / Turkish documentation is below)*

## 📌 Executive Summary
In modern electronic warfare (EW), Unmanned Aerial Vehicles (UAVs) are highly susceptible to RF jamming. When data links are severed, critical intelligence (such as the coordinates of mobile air defense systems like the S-400) is trapped within the UAV. 

The **OVRK** project introduces a novel "Store-and-Forward" software architecture. When a UAV calculates an unavoidable missile impact, it ejects a swarm of OVRK capsules. These capsules escape the jamming dome and burst-transmit encrypted target coordinates to the Command and Control (C2) center before executing terminal laser designation.

## 🚀 Core Features & Doctrines
* **Fail-Safe Memory (Ring Buffer):** Prevents `Out of Memory (OOM)` crashes during prolonged jamming in C++.
* **Swarm Logic:** Deploys 3 synchronized capsules to ensure redundancy.
* **Edge AI Target Validation:** Capsules utilize onboard edge computing to visually confirm targets (98.5% confidence).
* **Laser Designation Kamikaze:** The final capsule acts as a laser designator for precision-guided munitions.
* **Military-Grade Crypto:** Simulates AES-256 XOR payload encryption and FHSS.
* **Asynchronous C2 Radar (C#):** A multithreaded WinForms radar interface that decrypts incoming telemetry.

## 🔮 Phase-2: Professional Deployment Integrations
* **STANAG 4609 Tactical GIS:** Replacing the dark radar screen with offline topographic satellite imagery and GIS.
* **RSA-2048 Digital Signatures (Anti-Spoofing):** Adding PKI to prevent enemy spoofing attacks.
* **Link-16 Gateway:** Forwarding intelligence directly to F-16s via NATO Link-16.

---
## 💡 Developer's Note
> *"In this prototype, I modeled a dark tactical radar strictly to prove the data-link communication and encryption architecture. However, when the product is deployed to the field, the interface is designed to be completely replaced with a NATO STANAG 4609 standard GIS (Topographic Satellite Map) featuring live camera feeds."*

<br>
<hr>
<br>

# OVRK (Otonom Veri Röle Kapsülü) & Dağıtık Taktik Ağ
**Elektronik Harp Ortamları İçin "Haberci Kuş" ve C2 Simülatörü**

## 📌 Proje Özeti
Modern Elektronik Harp (EH) sahasında, İHA/SİHA'ların karşılaştığı en büyük zafiyet RF Jammer (Sinyal Karıştırıcı) sistemleridir. Veri bağı koptuğunda, istihbarat hava aracının içinde hapis kalır. 

**OVRK** projesi, bu zafiyeti "Store-and-Forward" mimarisi ile çözer. SİHA kaçınılmaz bir füze vuruşu hesapladığında, Jammer kubbesinden dışarı süzülecek bir OVRK sürüsü fırlatır. Bu kapsüller, vurulmadan saniyeler önce veriyi kriptolu olarak Karargaha (C2) iletir ve hedefi lazerle işaretler.

## 🚀 Temel Özellikler
* **Fail-Safe Bellek (Ring Buffer):** Jammer altında uçuş bilgisayarının şişmesini engelleyen mimari (C++).
* **Sürü Zekası (Mesh Relay):** 3 adet kapsülün senkronize fırlatılması.
* **Uç Birim Yapay Zekası (Edge AI):** Sahte hedefleri filtrelemek için %98.5 görsel teyit simülasyonu.
* **Kamikaze Lazer İşaretleyici:** Güdümlü mühimmatlar için lazer kilidi oluşturması.
* **Siber Güvenlik:** AES-256 (XOR) ile şifreleme ve frekans atlama simülasyonu.
* **Asenkron Radar Arayüzü (C#):** Ağdan gelen şifreli verileri anında çözen taktik ekran.

## 🔮 Faz-2: Gelecek Entegrasyonlar
* **STANAG 4609 Taktik GIS:** Siyah radar ekranı yerine çevrimdışı topografik uydu haritaları (CBS) entegrasyonu.
* **RSA-2048 Asimetrik Şifreleme (Anti-Spoofing):** Düşmanın radarımızı kirletmesini önlemek için kimlik doğrulama altyapısı.
* **Link-16 Ağ Geçidi:** Verilerin NATO standartlarındaki Link-16 ağı üzerinden F-16'lara aktarılması.

---
## 💡 Geliştirici Notu
> *"Ben bu prototipte veri bağı haberleşmesini ve şifreleme mimarisini kanıtlamak için siyah bir taktik radar modelledim. Ancak ürün sahaya indiğinde, arayüz tamamen NATO STANAG 4609 standartlarında GIS (Topografik Uydu Haritası) ile değiştirilecek ve canlı kamera akışları entegre edilecek şekilde tasarlanmıştır."*
