(function () {
    let firebasePromise = null;

    function loadScript(src) {
        return new Promise(function (resolve, reject) {
            if (document.querySelector('script[src="' + src + '"]')) {
                resolve();
                return;
            }
            var s = document.createElement('script');
            s.src = src;
            s.async = true;
            s.onload = resolve;
            s.onerror = reject;
            document.head.appendChild(s);
        });
    }

    async function ensureFirebase() {
        if (window._nawafDb) return window._nawafDb;

        if (!firebasePromise) {
            firebasePromise = (async function () {
                await loadScript('https://www.gstatic.com/firebasejs/9.22.0/firebase-app-compat.js');
                await loadScript('https://www.gstatic.com/firebasejs/9.22.0/firebase-database-compat.js');
                firebase.initializeApp({
                    databaseURL: 'https://nawafappointments-default-rtdb.firebaseio.com/'
                });
                window._nawafDb = firebase.database();
                return window._nawafDb;
            })().catch(function (err) {
                firebasePromise = null;
                throw err;
            });
        }

        return firebasePromise;
    }

    window.saveToFirebase = async function (appointment) {
        var db = await ensureFirebase();
        var data = {
            id: appointment.id,
            doctorName: appointment.doctorName,
            hospitalName: appointment.hospitalName,
            specialty: appointment.specialty || null,
            date: appointment.date,
            note: appointment.note || null,
            isCancelled: appointment.isCancelled || false
        };

        if (data.id) {
            return db.ref('appointments/' + data.id).set(data);
        }

        var newRef = db.ref('appointments').push();
        data.id = newRef.key;
        return newRef.set(data);
    };

    window.deleteFromFirebase = async function (id) {
        var db = await ensureFirebase();
        return db.ref('appointments/' + id).remove();
    };

    window.loadFromFirebase = async function () {
        var db = await ensureFirebase();
        var snapshot = await db.ref('appointments').once('value');
        var data = snapshot.val();
        if (!data) return [];

        return Object.keys(data)
            .map(function (key) {
                return {
                    id: key,
                    doctorName: data[key].doctorName,
                    hospitalName: data[key].hospitalName,
                    specialty: data[key].specialty || null,
                    date: data[key].date,
                    note: data[key].note || null,
                    isCancelled: data[key].isCancelled || false
                };
            })
            .sort(function (a, b) { return new Date(a.date) - new Date(b.date); });
    };

    window.loadSettings = function () {
        try {
            var raw = localStorage.getItem('nawaf_settings');
            return raw ? JSON.parse(raw) : { remindersEnabled: true, darkMode: false };
        } catch {
            return { remindersEnabled: true, darkMode: false };
        }
    };

    window.saveSettings = function (settings) {
        localStorage.setItem('nawaf_settings', JSON.stringify(settings));
    };

    window.showBrowserNotification = async function (message) {
        if (!('Notification' in window)) return;
        if (Notification.permission === 'default') {
            await Notification.requestPermission();
        }
        if (Notification.permission === 'granted') {
            new Notification('مواعيد نواف', { body: message, icon: 'favicon.png?v=2' });
        }
    };
})();
