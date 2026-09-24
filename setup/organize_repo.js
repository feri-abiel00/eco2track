const fs = require('fs');
const path = require('path');

function ensureDir(dir) {
    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
        console.log(`Created directory: ${dir}`);
    }
}

function copyFile(src, dest) {
    if (fs.existsSync(src)) {
        fs.copyFileSync(src, dest);
        console.log(`Copied ${src} -> ${dest}`);
    }
}

// 1. documentation/ folder
ensureDir('documentation');
copyFile('README.md', 'documentation/README.md');

fs.writeFileSync('documentation/ARCHITECTURE.md', `# Arsitektur Ekosistem ECO₂Track

Ekosistem ECO₂Track dirancang untuk menghadirkan pelacakan emisi karbon secara real-time melalui web dan 4 platform sistem operasi native.

## Komponen Utama
1. **Halaman Depan (Landing Page)**: \`index.html\`
   - Terinspirasi oleh estetika gamified futuristik [sacredoctagon.id](https://sacredoctagon.id/).
   - Menyediakan Pusat Unduhan 4 OS resmi (WinOS, MacOS, Android, iOS).
   - Animasi daun swirl interaktif saat masuk dan berpindah halaman.
   - Simulasi emisi karbon langsung dan FAQ interaktif.

2. **Aplikasi Pelacak Utama**: \`ECO2Track.html\`
   - Integrasi peta Leaflet GPS pelacakan mobilitas multi-moda.
   - Algoritma TerraStep untuk konversi langkah kaki menjadi reduksi gram CO₂.
   - Sinkronisasi Cloud dengan Firebase Firestore & Storage.

3. **Simulator Pendukung**:
   - \`kalkulator_kimia.html\`: Perhitungan stoikiometri pembakaran hidrokarbon.
   - \`Physics.html\`: Simulasi mekanika gaya gesek dan efisiensi energi kendaraan.
   - \`Mood.html\`: Pemantau kebugaran emosional dan gaya hidup hijau.
`, 'utf8');

fs.writeFileSync('documentation/MULTI_PLATFORM_DOWNLOADS.md', `# Pusat Unduhan Multi-Platform ECO₂Track

Berkas unduhan resmi disimpan langsung ke penyimpanan lokal perangkat pengguna (folder Downloads):

| Platform | Format | Berkas | Deskripsi |
| :--- | :--- | :--- | :--- |
| **Windows (WinOS)** | \`.exe\` | \`downloads/ECO2Track_Windows_Setup.exe\` | Installer desktop 64-bit untuk Windows 10 & 11 |
| **macOS** | \`.dmg\` | \`downloads/ECO2Track_macOS.dmg\` | Apple Disk Image Universal (Apple Silicon & Intel) |
| **Android** | \`.apk\` | \`downloads/ECO2Track_Android.apk\` | Paket APK native dengan sensor gerak TerraStep |
| **iOS** | \`.ipa\` | \`downloads/ECO2Track_iOS.ipa\` | Paket arsip IPA untuk iPhone & iPad |
`, 'utf8');

// 2. javascript/ folder
ensureDir('javascript');
copyFile('firebase-config.js', 'javascript/firebase-config.js');

fs.writeFileSync('javascript/README.md', `# Modul JavaScript ECO₂Track

Folder ini berisi berkas dan modul skrip JavaScript yang digunakan pada seluruh aplikasi:

- \`firebase-config.js\`: Konfigurasi Firebase App, Authentication, Firestore, dan Cloud Storage.
- Semua modul skrip dihubungkan secara modular agar tetap kompatibel baik saat dibuka langsung via \`file://\` maupun melalui server web hosting.
`, 'utf8');

// 3. firebase/ folder
ensureDir('firebase');
copyFile('firebase.json', 'firebase/firebase.json');
copyFile('.firebaserc', 'firebase/.firebaserc');
copyFile('firestore.rules', 'firebase/firestore.rules');
copyFile('storage.rules', 'firebase/storage.rules');
copyFile('firebase-config.js', 'firebase/firebase-config.js');

fs.writeFileSync('firebase/README.md', `# Konfigurasi & Aturan Firebase ECO₂Track

Proyek Firebase: \`eco2track-new\`
Bucket Storage: \`eco2track-new.firebasestorage.app\`

## Perintah Deployment
- Deploy Hosting: \`npx firebase-tools deploy --only hosting\`
- Deploy Firestore: \`npx firebase-tools deploy --only firestore\`
- Deploy Storage: \`npx firebase-tools deploy --only storage\`
`, 'utf8');

// 4. setup/ folder
ensureDir('setup');
copyFile('make_packages.ps1', 'setup/make_packages.ps1');
copyFile('build_apk.ps1', 'setup/build_apk.ps1');
if (fs.existsSync('src/windows/TerraStep_Setup.cs')) {
    copyFile('src/windows/TerraStep_Setup.cs', 'setup/TerraStep_Setup.cs');
}
if (fs.existsSync('downloads/TerraStep_Windows.xml')) {
    copyFile('downloads/TerraStep_Windows.xml', 'setup/TerraStep_Windows.xml');
}

fs.writeFileSync('setup/SETUP_GUIDE.md', `# Panduan Build & Setup Multi-Platform

Berkas setup dan skrip pembangun binary:
- \`make_packages.ps1\`: Skrip otomatisasi pembuatan berkas paket macOS DMG dan iOS IPA.
- \`build_apk.ps1\`: Skrip otomatisasi kompilasi Android APK dengan Android SDK dan zipalign.
- \`TerraStep_Setup.cs\`: Kode sumber C# untuk installer Windows dengan GUI kustom.
`, 'utf8');

// 5. json/ folder
ensureDir('json');
copyFile('package.json', 'json/package.json');
copyFile('package-lock.json', 'json/package-lock.json');
copyFile('firebase.json', 'json/firebase.json');
copyFile('.firebaserc', 'json/.firebaserc');

fs.writeFileSync('json/README.md', `# Berkas Konfigurasi JSON ECO₂Track

Folder ini mengorganisir seluruh berkas konfigurasi JSON:
- \`package.json\`: Konfigurasi proyek npm dan dependensi CLI.
- \`package-lock.json\`: Kunci versi dependensi npm.
- \`firebase.json\`: Konfigurasi routing hosting, headers pengunduhan, dan aturan Firebase.
- \`.firebaserc\`: Target proyek default Firebase.
`, 'utf8');

console.log('Semua folder terpisah (javascript, firebase, setup, documentation, json) berhasil dibuat dan diorganisir!');
