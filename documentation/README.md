# TerraStep & ECO₂Track Ecosystem

Platform Ekosistem Mobilitas Cerdas, Pelacakan Jejak Karbon Real-Time, dan Simulasi Emisi Multi-Platform.

Di dalam website **TerraStep**, terdapat modul utama yang bernama **ECO₂Track**.

---

## 👥 Tim Pembuat
Proyek inovasi ramah lingkungan ini diciptakan dan dikembangkan oleh:
1. **CHARISSA LIONI CLAUDIA NAPITUPULU**
2. **FERI ABIEL SIMAMORA**
3. **YOBEL TARIAS TONGGO SINAMBELA**

---

## 📱 Pusat Unduhan Resmi Multi-Platform (Tanpa ZIP)
Empat berkas mandiri resmi yang langsung tersimpan di media penyimpanan lokal (folder Downloads) perangkat pengguna:

| Sistem Operasi | Format Berkas | Path Berkas | Deskripsi |
| :--- | :--- | :--- | :--- |
| **Windows (WinOS)** | `.exe` | [`downloads/ECO2Track_Windows_Setup.exe`](file:///d:/Webtest%20HTML-AI/ECO(2)Track%20(html%20website%20test)/ECO2TRACK%20NEW/downloads/ECO2Track_Windows_Setup.exe) | Installer 64-bit untuk Windows 10 & 11 |
| **macOS (Apple)** | `.dmg` | [`downloads/ECO2Track_macOS.dmg`](file:///d:/Webtest%20HTML-AI/ECO(2)Track%20(html%20website%20test)/ECO2TRACK%20NEW/downloads/ECO2Track_macOS.dmg) | Apple Disk Image Universal (M1/M2/M3 & Intel) |
| **Android** | `.apk` | [`downloads/ECO2Track_Android.apk`](file:///d:/Webtest%20HTML-AI/ECO(2)Track%20(html%20website%20test)/ECO2TRACK%20NEW/downloads/ECO2Track_Android.apk) | Paket APK native dengan sensor TerraStep |
| **iOS (iPhone/iPad)** | `.ipa` | [`downloads/ECO2Track_iOS.ipa`](file:///d:/Webtest%20HTML-AI/ECO(2)Track%20(html%20website%20test)/ECO2TRACK%20NEW/downloads/ECO2Track_iOS.ipa) | Paket arsip IPA untuk AltStore/Sideload |

---

## 🗂️ Struktur Organisasi Repositori
Repositori ini dikelompokkan ke dalam folder terpisah dengan tetap mempertahankan kelancaran fungsi aplikasi:

- **[`documentation/`](file:///d:/Webtest%20HTML-AI/ECO(2)Track%20(html%20website%20test)/ECO2TRACK%20NEW/documentation/)**: Berisi seluruh dokumentasi arsitektur, panduan multi-platform, dan ringkasan proyek.
- **[`javascript/`](file:///d:/Webtest%20HTML-AI/ECO(2)Track%20(html%20website%20test)/ECO2TRACK%20NEW/javascript/)**: Modul skrip JavaScript (`firebase-config.js`).
- **[`firebase/`](file:///d:/Webtest%20HTML-AI/ECO(2)Track%20(html%20website%20test)/ECO2TRACK%20NEW/firebase/)**: Konfigurasi deployment hosting, Cloud Firestore (`firestore.rules`), dan Cloud Storage (`storage.rules`).
- **[`setup/`](file:///d:/Webtest%20HTML-AI/ECO(2)Track%20(html%20website%20test)/ECO2TRACK%20NEW/setup/)**: Skrip pembangun installer (`TerraStep_Setup.cs`, `make_packages.ps1`, `build_apk.ps1`).
- **[`json/`](file:///d:/Webtest%20HTML-AI/ECO(2)Track%20(html%20website%20test)/ECO2TRACK%20NEW/json/)**: Konfigurasi berbasis JSON (`package.json`, `firebase.json`, `.firebaserc`).
- **[`downloads/`](file:///d:/Webtest%20HTML-AI/ECO(2)Track%20(html%20website%20test)/ECO2TRACK%20NEW/downloads/)**: Tempat penyimpanan 4 berkas binary installer OS.

---

## 🌐 Alur Penggunaan & Aplikasi Web
1. **Halaman Depan TerraStep** (`index.html`): Penjelasan ringkas, nama pembuat, berkas setup 4 OS, cara kerja, dan tombol Start menuju MP3 Player dengan animasi daun terbang alami.
2. **Virtual MP3 Player Portal** (`mp3.html`): Portal interaktif bergaya perangkat retro MP3 untuk meluncurkan modul aplikasi.
3. **ECO₂Track Web App** (`ECO2Track.html`): Aplikasi pelacak emisi mobilitas real-time dengan integrasi GPS Leaflet, kalori langkah TerraStep, dan sinkronisasi Firebase.
4. **Simulator Kimia** (`kalkulator_kimia (4).html` / `kalkulator_kimia.html`): Perhitungan stoikiometri pembakaran hidrokarbon.
5. **Simulator Fisika** (`Physics.html`): Analisis mekanika gaya gesek dan efisiensi energi kendaraan.
6. **Eco-Mood Tracker** (`Mood.html`): Pemantauan kebugaran dan kebiasaan ramah lingkungan.

---

## 🚀 Deployment & Integrasi
- **GitHub Repository**: [https://github.com/feri-abiel00/eco2track](https://github.com/feri-abiel00/eco2track)
- **Firebase Web App**: [https://eco2track-new.web.app](https://eco2track-new.web.app)
