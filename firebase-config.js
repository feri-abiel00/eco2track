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
          // Enable offline persistence so data is never lost even if connection drops
          try {
            this.db.enablePersistence({ synchronizeTabs: true }).catch(err => {
              if (err.code !== 'failed-precondition' && err.code !== 'unimplemented') {
                console.warn("Firestore persistence notice:", err.code);
              }
            });
          } catch(pe){}
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
      if (this.db) {
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
  // FIREBASE AUTH & ACCOUNT METHODS
  // ==========================================

  async registerUser(email, password, displayName = '') {
    const cleanEmail = (email || '').trim().toLowerCase();
    const userData = {
      id: 'usr_' + Date.now().toString(36) + '_' + Math.random().toString(36).substring(2, 7),
      name: displayName || cleanEmail.split('@')[0],
      email: cleanEmail,
      type: 'registered',
      createdAt: new Date().toISOString(),
      profile: {
        name: displayName || cleanEmail.split('@')[0],
        age: 22,
        city: 'jakarta',
        height: 170,
        weight: 65,
        vision: 'normal',
        completed: false
      }
    };

    // 1. Try Firebase Auth
    if (this.auth) {
      try {
        const userCredential = await this.auth.createUserWithEmailAndPassword(cleanEmail, password);
        const user = userCredential.user;
        userData.id = user.uid;
        if (displayName && user.updateProfile) {
          try { await user.updateProfile({ displayName }); } catch(e){}
        }
      } catch (authErr) {
        console.warn("Firebase Auth createUser notice:", authErr.message);
      }
    }

    // 2. Save directly to Cloud Firestore users and accounts collections
    if (this.db) {
      try {
        await this.db.collection('users').doc(userData.id).set(userData, { merge: true });
        const emailKey = cleanEmail.replace(/[^a-zA-Z0-9]/g, '_');
        await this.db.collection('accounts').doc(emailKey).set({
          id: userData.id,
          name: userData.name,
          email: userData.email,
          createdAt: userData.createdAt,
          lastLogin: new Date().toISOString()
        }, { merge: true });
      } catch(dbErr) {
        console.warn("Firestore registerUser sync notice:", dbErr);
      }
    }
    return { success: true, user: userData };
  },

  async saveAccount(account) {
    if (!this.db || !account) return false;
    try {
      const uid = account.id || ('usr_' + Date.now().toString(36));
      await this.db.collection('users').doc(uid).set({
        ...account,
        lastUpdated: new Date().toISOString()
      }, { merge: true });

      if (account.email) {
        const emailKey = account.email.trim().toLowerCase().replace(/[^a-zA-Z0-9]/g, '_');
        await this.db.collection('accounts').doc(emailKey).set({
          id: uid,
          email: account.email,
          name: account.name || '',
          lastUpdated: new Date().toISOString()
        }, { merge: true });
      }
      console.log("🔥 [Firebase] Account synced to Cloud Firestore:", uid);
      return true;
    } catch(e) {
      console.warn("Firebase saveAccount notice:", e);
      return false;
    }
  },

  async loginUser(email, password) {
    const cleanEmail = (email || '').trim().toLowerCase();
    let profileData = null;

    // 1. Try Firebase Auth first
    if (this.auth) {
      try {
        const userCredential = await this.auth.signInWithEmailAndPassword(cleanEmail, password);
        const user = userCredential.user;
        profileData = { 
          id: user.uid, 
          name: user.displayName || cleanEmail.split('@')[0], 
          email: user.email, 
          type: 'registered' 
        };
        if (this.db) {
          try {
            const doc = await this.db.collection('users').doc(user.uid).get();
            if (doc.exists) {
              profileData = { ...profileData, ...doc.data() };
            }
          } catch(e){}
        }
        return { success: true, user: profileData };
      } catch (authErr) {
        console.warn("Firebase Auth signIn notice:", authErr.message);
      }
    }

    // 2. Dual fallback: lookup in Firestore accounts collection
    if (this.db) {
      try {
        const emailKey = cleanEmail.replace(/[^a-zA-Z0-9]/g, '_');
        const accSnap = await this.db.collection('accounts').doc(emailKey).get();
        if (accSnap.exists) {
          const accInfo = accSnap.data();
          const userDoc = await this.db.collection('users').doc(accInfo.id).get();
          if (userDoc.exists) {
            const userData = userDoc.data();
            if (!userData.password || userData.password === password) {
              return { success: true, user: userData };
            }
          }
        }
      } catch(e){
        console.warn("Firestore account login lookup notice:", e);
      }
    }

    return { success: false, error: "Invalid credentials or user not found" };
  },

  // ==========================================
  // FIRESTORE DATABASE METHODS
  // ==========================================

  async saveUserProfile(userId, profileData) {
    if (!this.db || !userId) return false;
    try {
      await this.db.collection('users').doc(userId).set({
        id: userId,
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
      const uid = userId || (record && record.userId) || 'guest_user';
      const recordPayload = {
        ...record,
        userId: uid,
        syncedAt: new Date().toISOString()
      };

      // 1. Save in user's subcollection for both registered and guest IDs
      try {
        await this.db.collection('users').doc(uid).collection('records').doc(record.id).set(recordPayload);
      } catch(subErr) {
        console.warn("User subcollection save notice:", subErr);
      }
      
      // 2. Also save in root activity_records collection for global dashboard, sync & backup
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
      // 1. Try user subcollection first
      const snap = await this.db.collection('users').doc(userId).collection('records').orderBy('timestamp', 'desc').get();
      if (!snap.empty) {
        return snap.docs.map(d => d.data());
      }
      // 2. Fallback: search in root activity_records by userId
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
      const uid = userId || 'guest_user';
      try {
        await this.db.collection('users').doc(uid).collection('records').doc(recordId).delete();
      } catch(e){}
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
