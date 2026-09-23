![TerraStep - ECO₂Track Ecosystem](ECO₂Track0.png)

**TerraStep** is an advanced environmental monitoring and mobility tracking ecosystem created by **Tim STEAM Science Expo SMA Unggul Del 2026: TERRASTEP**. The platform is publicly hosted and accessible at **[terrastep.web.app](https://terrastep.web.app)**. Engineered to bridge real-world physical activity with rigorous ecological quantification, TerraStep allows individuals and communities to measure their kinetic work, caloric burn, fuel expenditure, and carbon footprint with scientific precision rather than broad approximations.

---

## 🌍 Overview & Scientific Foundation

The core philosophy of TerraStep is grounded in empirical mechanics, human metabolism, and atmospheric chemistry. Daily transportation decisions have direct, measurable impacts on greenhouse gas emissions. TerraStep computes these dynamics by integrating live positioning, physiological baselines, dynamic weather resistance, and passenger load variables into a unified calculation engine.

### 1. Metabolic & Caloric Calculations (Eco Modes)
For active mobility (Running, Walking, Cycling), the system calculates caloric expenditure using dynamic Metabolic Equivalent of Task (MET) formulations adjusted for travel velocity:
$$\text{MET}_{\text{dynamic}} = f(v, \text{activity})$$
$$\text{Calories Burned (kcal)} = \text{MET} \times \text{Body Weight (kg)} \times \text{Duration (hours)} \times \mu_{\text{weather}}$$

The avoided greenhouse gas emission is calculated against average internal combustion engine displacement factors scaled by provincial road topography:
$$\Delta\text{CO}_2\text{e Saved (kg)} = \text{Distance (km)} \times \text{Base Displacement Factor} \times C_{\text{provincial}}$$

### 2. Combustion & Passenger Distribution (Motorized Modes)
For motorized vehicles (Motorcycles and Passenger Cars), emissions reflect fuel chemistry, total payload, air conditioning load, and tire-road friction:
$$\text{Total Emission (kg CO}_2\text{e)} = \text{Distance (km)} \times E_{\text{base}} \times M_{\text{payload}} \times M_{\text{weather}} \times C_{\text{provincial}}$$
$$\text{Per-Capita Carbon (kg CO}_2\text{e)} = \frac{\text{Total Emission}}{\text{Passenger Count}}$$
$$\text{Fuel Burned (Liters)} = \frac{\text{Total Emission}}{2.31\,\text{kg CO}_2\text{e / Liter}}$$

---

## 🚀 Key Modules & Ecosystem Architecture

The TerraStep suite integrates four interactive sub-applications accessible from the central MP3-style tactile portal:

1. **ECO₂Track (`ECO2Track.html`)**
   - **Real-time Live GPS & Simulation Engine**: Track outdoor routes with high-accuracy Geolocation API, or test routes indoors with the high-fidelity route simulation mode.
   - **Multi-Modal Mobility Profiling**: Five distinct transportation modes (Running, Walking, Cycling, Motorcycle, and Passenger Car).
   - **Environmental Conditioning**: Dynamic thermoregulation and aerodynamic drag multipliers for Clear (25°C), Hot (>32°C), and Rainy conditions.
   - **Personalized Anthropometric Profiling**: Physical customization (Height, Weight, Age, Vision condition) and regional topography profiles covering all 38 Indonesian provinces.
   - **History & Data Portability**: Instant cloud backup to Firebase Firestore with full CSV and JSON data export capabilities.

2. **Chemistry Calculator (`kalkulator_kimia (4).html`)**
   - Molecular stoichiometry of hydrocarbon combustion.
   - Fuel-to-CO₂ conversion ratios, molar balancing, and greenhouse gas concentration analysis.

3. **Physics Calculator (`Physics.html`)**
   - Mechanical work, aerodynamic drag force, rolling resistance, and kinetic power output equations.

4. **Mood & Mental Well-being Companion (`Mood.html`)**
   - Correlation between green exercise, cardiovascular endurance, and cognitive focus.
   - Integrated mood canvas and mental well-being tracking.

---

## 🛠️ Technology Stack & Cloud Infrastructure

- **Frontend Core**: Vanilla HTML5, CSS3 with responsive glassmorphism and tactile hardware styling, ES6+ JavaScript.
- **Mapping & Geodesy**: Leaflet.js with OpenStreetMap cartography and Haversine geodesic distance computation.
- **Backend & Cloud Services**: Google Firebase (Authentication, Cloud Firestore, Cloud Storage, and Firebase Hosting).
- **Audio Feedback**: Web Audio API synthesizing tactile haptic click responses.

---

## 💻 Local Setup & Development

To run or inspect the project locally:

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/feri-abiel00/eco2track.git
   cd eco2track
   ```

2. **Run Locally**:
   You can open `index.html` directly in any modern web browser, or serve it using any local HTTP server:
   ```bash
   npx serve .
   ```
   or using Python:
   ```bash
   python -m http.server 8080
   ```

3. **Deploy to Firebase Hosting**:
   ```bash
   npm run deploy:hosting
   ```

---

## 👥 Authors & Acknowledgments

This project was developed and presented by **Tim STEAM Science Expo SMA Unggul Del 2026: TERRASTEP** as an interdisciplinary STEAM initiative integrating science, technology, engineering, arts, and mathematics for sustainable climate action.

- **Web Portal**: [terrastep.web.app](https://terrastep.web.app)
- **Repository**: [github.com/feri-abiel00/eco2track](https://github.com/feri-abiel00/eco2track)