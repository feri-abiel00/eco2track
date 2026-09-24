# Arsitektur Ekosistem ECO₂Track

Ekosistem ECO₂Track dirancang untuk menghadirkan pelacakan emisi karbon secara real-time melalui web dan 4 platform sistem operasi native.

## Komponen Utama
1. **Halaman Depan (Landing Page)**: `index.html`
   - Terinspirasi oleh estetika gamified futuristik [sacredoctagon.id](https://sacredoctagon.id/).
   - Menyediakan Pusat Unduhan 4 OS resmi (WinOS, MacOS, Android, iOS).
   - Animasi daun swirl interaktif saat masuk dan berpindah halaman.
   - Simulasi emisi karbon langsung dan FAQ interaktif.

2. **Aplikasi Pelacak Utama**: `ECO2Track.html`
   - Integrasi peta Leaflet GPS pelacakan mobilitas multi-moda.
   - Algoritma TerraStep untuk konversi langkah kaki menjadi reduksi gram CO₂.
   - Sinkronisasi Cloud dengan Firebase Firestore & Storage.

3. **Simulator Pendukung**:
   - `kalkulator_kimia.html`: Perhitungan stoikiometri pembakaran hidrokarbon.
   - `Physics.html`: Simulasi mekanika gaya gesek dan efisiensi energi kendaraan.
   - `Mood.html`: Pemantau kebugaran emosional dan gaya hidup hijau.
