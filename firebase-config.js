// ===================================================
// ECO2TRACK FIREBASE CONFIGURATION & CLOUD SERVICE
// Project: eco2track-new
// Storage Bucket: eco2track-new.firebasestorage.app
// ===================================================

var firebaseConfig = window.firebaseConfig || {
  apiKey: "AIzaSyAxVOQg7rnrgUaOyvSXxrgelEqyLEcUo6c",
  authDomain: "eco2track-new.firebaseapp.com",
  projectId: "eco2track-new",
  storageBucket: "eco2track-new.firebasestorage.app",
  messagingSenderId: "647174491993",
  appId: "1:647174491993:web:20359ca5bdd587877d0648"
};

// Global reference
window.firebaseConfig = firebaseConfig;

var EcoTrackFirebase = window.EcoTrackFirebase || {
  config: firebaseConfig,
  app: null,
  auth: null,
  db: null,
  storage: null,
  isInitialized: false,

  init() {
    try {
      if (typeof firebase !== 'undefined') {
        if (!firebase.apps || !firebase.apps.length) {
          this.app = firebase.initializeApp(firebaseConfig);
        } else {
          this.app = firebase.app();
        }
        if (firebase.auth) {
          this.auth = firebase.auth();
        }
        if (firebase.firestore) {
          this.db = firebase.firestore();
        }
        if (firebase.storage) {
          this.storage = firebase.storage();
        }
        this.isInitialized = true;
        console.log("🔥 [EcoTrack] Firebase Connected successfully to project: " + firebaseConfig.projectId);
        if (this.storage) {
          console.log("📦 [EcoTrack] Firebase Storage ready: " + firebaseConfig.storageBucket);
        }
      } else {
        console.warn("⚠️ Firebase SDK not loaded in global window object.");
      }
    } catch (e) {
      console.warn("⚠️ Firebase Init Notice:", e);
    }
    return this;
  },

  // ==========================================
  // FIREBASE CLOUD STORAGE METHODS
  // ==========================================

  // Upload any general file or blob to Firebase Storage
  async uploadFile(file, folder = 'uploads') {
    if (!this.storage) {
      console.warn("⚠️ Firebase Storage not ready or SDK not loaded.");
      return { success: false, error: "Firebase Storage not initialized" };
    }
    try {
      const sanitizedName = file.name ? file.name.replace(/[^a-zA-Z0-9._-]/g, '_') : 'file_' + Date.now();
      const filePath = `${folder}/${Date.now()}_${sanitizedName}`;
      const storageRef = this.storage.ref().child(filePath);
      
      const snapshot = await storageRef.put(file);
      const downloadURL = await snapshot.ref.getDownloadURL();
      console.log("🔥 [Firebase Storage] File successfully uploaded:", downloadURL);
      return { success: true, url: downloadURL, fullPath: snapshot.ref.fullPath };
    } catch (err) {
      console.error("❌ [Firebase Storage] Upload error:", err);
      return { success: false, error: err.message, code: err.code };
    }
  },

  // Upload user profile avatar to Firebase Storage
  async uploadAvatar(userId, file) {
    if (!this.storage || !userId) {
      return { success: false, error: "Storage or User ID missing" };
    }
    try {
      const ext = (file.name && file.name.split('.').pop()) || 'png';
      const filePath = `avatars/${userId}_${Date.now()}.${ext}`;
      const storageRef = this.storage.ref().child(filePath);

      const snapshot = await storageRef.put(file);
      const downloadURL = await snapshot.ref.getDownloadURL();

      // Update Firestore user document if db available
      if (this.db && !userId.startsWith('guest_')) {
        await this.db.collection('users').doc(userId).set({
          photoURL: downloadURL,
          lastUpdated: new Date().toISOString()
        }, { merge: true });
      }

      console.log("🔥 [Firebase Storage] Avatar updated:", downloadURL);
      return { success: true, url: downloadURL };
    } catch (err) {
      console.error("❌ [Firebase Storage] Avatar upload error:", err);
      return { success: false, error: err.message, code: err.code };
    }
  },

  // Upload Mood Image / Art generated to Firebase Storage
  async uploadMoodImage(userId, blobOrFile, moodType = 'mood') {
    if (!this.storage) return { success: false, error: "Storage not ready" };
    try {
      const filePath = `mood_arts/${userId || 'guest'}_${moodType}_${Date.now()}.png`;
      const storageRef = this.storage.ref().child(filePath);
      const snapshot = await storageRef.put(blobOrFile, { contentType: 'image/png' });
      const downloadURL = await snapshot.ref.getDownloadURL();
      return { success: true, url: downloadURL };
    } catch (err) {
      console.error("❌ [Firebase Storage] Mood image upload error:", err);
      return { success: false, error: err.message };
    }
  },

  // ==========================================
  // FIREBASE AUTH METHODS
  // ==========================================

  async registerUser(email, password, displayName = '') {
    if (!this.auth) return { success: false, fallback: true, error: "Firebase Auth not ready" };
    try {
      const userCredential = await this.auth.createUserWithEmailAndPassword(email, password);
      const user = userCredential.user;
      if (displayName && user.updateProfile) {
        try { await user.updateProfile({ displayName }); } catch(e){}
      }
      const userData = {
        id: user.uid,
        name: displayName || email.split('@')[0],
        email: user.email,
        type: 'registered',
        createdAt: new Date().toISOString(),
        profile: {
          name: displayName || email.split('@')[0],
          age: 22,
          city: 'jakarta',
          height: 170,
          weight: 65,
          vision: 'normal',
          completed: false
        }
      };
      if (this.db) {
        try {
          await this.db.collection('users').doc(user.uid).set(userData, { merge: true });
        } catch(e){}
      }
      return { success: true, user: userData };
    } catch (err) {
      return { success: false, error: err.message, code: err.code };
    }
  },

  async loginUser(email, password) {
    if (!this.auth) return { success: false, fallback: true, error: "Firebase Auth not ready" };
    try {
      const userCredential = await this.auth.signInWithEmailAndPassword(email, password);
      const user = userCredential.user;
      let profileData = { 
        id: user.uid, 
        name: user.displayName || email.split('@')[0], 
        email: user.email, 
        type: 'registered' 
      };
      
      if (this.db) {
        try {
          const doc = await this.db.collection('users').doc(user.uid).get();
          if (doc.exists) {
            const data = doc.data();
            profileData = { ...profileData, ...data };
          }
        } catch(e){}
      }
      return { success: true, user: profileData };
    } catch (err) {
      return { success: false, error: err.message, code: err.code };
    }
  },

  // ==========================================
  // FIRESTORE DATABASE METHODS
  // ==========================================

  async saveUserProfile(userId, profileData) {
    if (!this.db || !userId) return false;
    try {
      await this.db.collection('users').doc(userId).set({
        name: profileData.name || '',
        profile: profileData,
        lastUpdated: new Date().toISOString()
      }, { merge: true });
      console.log("🔥 [Firebase] User profile synced to Cloud Firestore:", userId);
      return true;
    } catch(e) { 
      console.warn("Firebase saveUserProfile error:", e);
      return false; 
    }
  },

  async saveRecord(userId, record) {
    if (!this.db) return false;
    try {
      const recordPayload = {
        ...record,
        syncedAt: new Date().toISOString()
      };

      // 1. Save in user's subcollection if registered
      if (userId && !userId.startsWith('guest_')) {
        await this.db.collection('users').doc(userId).collection('records').doc(record.id).set(recordPayload);
      }
      
      // 2. Also save in root activity_records collection for global dashboard & backup
      try {
        await this.db.collection('activity_records').doc(record.id).set(recordPayload);
      } catch(rootErr) {
        console.warn("activity_records backup write notice:", rootErr);
      }

      console.log("🔥 [Firebase] Activity record successfully saved to Cloud Firestore:", record.id);
      return true;
    } catch(e) {
      console.error("❌ [Firebase] Failed to save record to Firestore:", e);
      return false;
    }
  },

  async getRecords(userId) {
    if (!this.db || !userId) return [];
    try {
      // Try user subcollection first
      const snap = await this.db.collection('users').doc(userId).collection('records').orderBy('timestamp', 'desc').get();
      if (!snap.empty) {
        return snap.docs.map(d => d.data());
      }
      // Fallback: search in root activity_records by userId
      const snapRoot = await this.db.collection('activity_records').where('userId', '==', userId).get();
      return snapRoot.docs.map(d => d.data()).sort((a,b) => new Date(b.timestamp) - new Date(a.timestamp));
    } catch(e) {
      console.warn("Firebase getRecords notice:", e);
      return [];
    }
  },

  async deleteRecord(userId, recordId) {
    if (!this.db) return false;
    try {
      if (userId && !userId.startsWith('guest_')) {
        await this.db.collection('users').doc(userId).collection('records').doc(recordId).delete();
      }
      try {
        await this.db.collection('activity_records').doc(recordId).delete();
      } catch(e){}
      console.log("🔥 [Firebase] Record deleted from Cloud Firestore:", recordId);
      return true;
    } catch(e) {
      console.warn("Firebase deleteRecord error:", e);
      return false;
    }
  },

  async clearAllRecords(userId) {
    if (!this.db || !userId) return false;
    try {
      const snap = await this.db.collection('users').doc(userId).collection('records').get();
      const batch = this.db.batch();
      snap.docs.forEach(doc => {
        batch.delete(doc.ref);
      });
      await batch.commit();
      console.log("🔥 [Firebase] All user records cleared from Cloud Firestore");
      return true;
    } catch(e) {
      console.warn("Firebase clearAllRecords error:", e);
      return false;
    }
  },

  async recordMoodEntry(moodData) {
    if (!this.db) return false;
    try {
      await this.db.collection('mood_records').add({
        ...moodData,
        timestamp: new Date().toISOString()
      });
      console.log("🔥 [Firebase] Mood analysis recorded to Cloud Firestore");
      return true;
    } catch(e) {
      console.warn("Firebase recordMoodEntry error:", e);
      return false;
    }
  },

  async recordCalculation(calcData) {
    if (!this.db) return false;
    try {
      await this.db.collection('calculation_records').add({
        ...calcData,
        timestamp: new Date().toISOString()
      });
      console.log("🔥 [Firebase] Calculation recorded to Cloud Firestore");
      return true;
    } catch(e) {
      console.warn("Firebase recordCalculation error:", e);
      return false;
    }
  }
};

window.EcoTrackFirebase = EcoTrackFirebase;

// Auto-initialize if SDK already loaded
if (typeof firebase !== 'undefined') {
  EcoTrackFirebase.init();
}
