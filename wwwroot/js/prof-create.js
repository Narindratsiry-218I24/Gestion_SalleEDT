/* ═══════════════════════════════════════════════════════════════
   PROF CREATE — Interactive Logic
   Tabs, Validation, OTP, Email check, Toasts, Animations
   ═══════════════════════════════════════════════════════════════ */

(function () {
    'use strict';

    // ── Toast System ────────────────────────────────────────────
    let toastContainer;
    function initToasts() {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container';
        document.body.appendChild(toastContainer);
    }
    function showToast(message, type = 'success', duration = 4000) {
        if (!toastContainer) initToasts();
        const icons = { success: '✓', error: '✕', info: 'ℹ' };
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.innerHTML = `<span class="toast-icon">${icons[type] || 'ℹ'}</span><span>${message}</span><button class="toast-close" onclick="this.parentElement.classList.add('toast-exit');setTimeout(()=>this.parentElement.remove(),300)">×</button>`;
        toastContainer.appendChild(toast);
        setTimeout(() => { toast.classList.add('toast-exit'); setTimeout(() => toast.remove(), 300); }, duration);
    }

    // ── Ripple Effect ───────────────────────────────────────────
    function addRipple(e) {
        const btn = e.currentTarget;
        const r = document.createElement('span');
        const rect = btn.getBoundingClientRect();
        const sz = Math.max(rect.width, rect.height);
        r.className = 'ripple';
        r.style.width = r.style.height = sz + 'px';
        r.style.left = (e.clientX - rect.left - sz / 2) + 'px';
        r.style.top = (e.clientY - rect.top - sz / 2) + 'px';
        btn.appendChild(r);
        setTimeout(() => r.remove(), 500);
    }

    // ── Tab Management ──────────────────────────────────────────
    let currentTab = 0;
    const tabEls = () => document.querySelectorAll('.tab[data-tab]');
    const panelEls = () => document.querySelectorAll('.tab-panel');

    function switchTab(idx) {
        const tabs = tabEls(), panels = panelEls();
        if (idx < 0 || idx >= panels.length) return;
        tabs.forEach((t, i) => { t.classList.toggle('active', i === idx); });
        panels.forEach((p, i) => { p.classList.toggle('active', i === idx); });
        currentTab = idx;
        updateButtons();
    }

    function updateButtons() {
        const panels = panelEls();
        const btnPrev = document.getElementById('btnPrev');
        const btnNext = document.getElementById('btnNext');
        const btnSave = document.getElementById('btnSave');
        const btnSaveCreate = document.getElementById('btnSaveCreate');
        if (btnPrev) btnPrev.style.display = currentTab > 0 ? '' : 'none';
        if (btnNext) btnNext.style.display = currentTab < panels.length - 1 ? '' : 'none';
        if (btnSave) btnSave.style.display = currentTab === panels.length - 1 ? '' : 'none';
        if (btnSaveCreate) btnSaveCreate.style.display = currentTab === panels.length - 1 ? '' : 'none';
    }

    // ── Field Validation ────────────────────────────────────────
    const validators = {
        required(v) { return v.trim() !== ''; },
        email(v) { return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v); },
        phone(v) { return /^[\d\s\+\-\(\)]{6,20}$/.test(v); },
        minlen(v, n) { return v.length >= n; },
        match(v, other) { return v === other; },
        cin(v) { return v.trim().length >= 6; }
    };

    function validateField(input) {
        const field = input.closest('.field');
        if (!field) return true;
        const rules = (field.dataset.rules || '').split(',').filter(Boolean);
        let valid = true, msg = '';

        for (const rule of rules) {
            const [name, param] = rule.split(':');
            if (name === 'required' && !validators.required(input.value)) { valid = false; msg = 'Ce champ est obligatoire'; break; }
            if (name === 'email' && input.value && !validators.email(input.value)) { valid = false; msg = 'Format email invalide'; break; }
            if (name === 'phone' && input.value && !validators.phone(input.value)) { valid = false; msg = 'Numéro invalide'; break; }
            if (name === 'minlen' && input.value && !validators.minlen(input.value, parseInt(param))) { valid = false; msg = `Minimum ${param} caractères`; break; }
            if (name === 'cin' && input.value && !validators.cin(input.value)) { valid = false; msg = 'Minimum 6 caractères'; break; }
            if (name === 'match') {
                const other = document.getElementById(param);
                if (other && input.value && !validators.match(input.value, other.value)) { valid = false; msg = 'Les valeurs ne correspondent pas'; break; }
            }
        }

        // Update UI
        field.classList.remove('valid', 'error');
        let msgEl = field.querySelector('.field-message');
        if (!input.value && !rules.includes('required')) {
            if (msgEl) msgEl.remove();
            return true;
        }
        if (input.value || field.classList.contains('touched')) {
            field.classList.add(valid ? 'valid' : 'error');
            if (!msgEl) { msgEl = document.createElement('div'); field.appendChild(msgEl); }
            if (valid && input.value) {
                msgEl.className = 'field-message success';
                msgEl.innerHTML = '<span class="field-icon success">✓</span> Valide';
            } else if (!valid) {
                msgEl.className = 'field-message error';
                msgEl.innerHTML = `<span class="field-icon error">✕</span> ${msg}`;
            } else {
                msgEl.remove();
            }
        }
        return valid;
    }

    function validateCurrentTab() {
        const panel = document.querySelector('.tab-panel.active');
        if (!panel) return true;
        const inputs = panel.querySelectorAll('input[name], select[name]');
        let allValid = true;
        inputs.forEach(inp => {
            const f = inp.closest('.field');
            if (f) f.classList.add('touched');
            if (!validateField(inp)) allValid = false;
        });
        return allValid;
    }

    // ── Profile Card Live Update ────────────────────────────────
    function updateProfileCard() {
        const nom = document.querySelector('[name="Nom"]')?.value || '';
        const prenom = document.querySelector('[name="Prenom"]')?.value || '';
        const specialite = document.querySelector('[name="Specialite"]')?.value || '';
        const grade = document.querySelector('[name="Grade"]')?.value || '';
        const statut = document.querySelector('[name="Statut"]')?.value || 'Actif';

        const nameEl = document.querySelector('.profile-name');
        if (nameEl) nameEl.textContent = (prenom + ' ' + nom).trim() || 'Nouveau professeur';

        const specValue = document.getElementById('profileSpecialite');
        if (specValue) specValue.textContent = specialite || '-';
        const gradeValue = document.getElementById('profileGrade');
        if (gradeValue) gradeValue.textContent = grade || '-';
    }

    // ── Email Availability Check ────────────────────────────────
    let emailCheckTimeout;
    function checkEmailAvailability(email) {
        const statusEl = document.getElementById('emailStatus');
        if (!statusEl) return;
        if (!email || !validators.email(email)) { statusEl.innerHTML = ''; return; }

        statusEl.innerHTML = '<span class="btn-loader-dark btn-loader" style="width:14px;height:14px;"></span> Vérification...';

        clearTimeout(emailCheckTimeout);
        emailCheckTimeout = setTimeout(() => {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            fetch('/Professeurs/CheckEmailAvailability', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `email=${encodeURIComponent(email)}&__RequestVerificationToken=${encodeURIComponent(token || '')}`
            })
            .then(r => r.json())
            .then(data => {
                if (data.available) {
                    statusEl.innerHTML = `<span class="email-status available">✔ ${data.message}</span>`;
                } else {
                    statusEl.innerHTML = `<span class="email-status unavailable">✖ ${data.message}</span>`;
                }
            })
            .catch(() => { statusEl.innerHTML = ''; });
        }, 600);
    }

    // ── OTP System ──────────────────────────────────────────────
    let otpTimer, otpSeconds = 90, emailVerified = false;

    function sendOtp() {
        // Toujours utiliser le champ Email principal (asp-for="Email", lié au modèle)
        const email = document.getElementById('Email')?.value
                   || document.querySelector('[name="Email"]')?.value;
        if (!email || !validators.email(email)) {
            showToast('Veuillez saisir un email valide dans l\'onglet "Informations personnelles".', 'error');
            return;
        }

        const btn = document.getElementById('btnSendCode');
        if (!btn) return;
        btn.disabled = true;
        btn.innerHTML = '<span class="btn-loader"></span> Envoi...';

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        fetch('/Professeurs/SendVerificationCode', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `email=${encodeURIComponent(email)}&__RequestVerificationToken=${encodeURIComponent(token || '')}`
        })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                showToast('Email envoyé.', 'success');
                document.getElementById('otpSentMsg')?.classList.remove('hidden');
                document.getElementById('otpInputSection')?.classList.remove('hidden');
                startOtpTimer();
                setTimeout(() => document.querySelector('.otp-input')?.focus(), 200);
            } else {
                showToast(data.message || 'Impossible d\'envoyer le mail.', 'error');
                btn.disabled = false;
            }
            btn.innerHTML = 'Envoyer un code';
        })
        .catch(() => {
            showToast('Impossible d\'envoyer le mail.', 'error');
            btn.disabled = false;
            btn.innerHTML = 'Envoyer un code';
        });
    }

    function startOtpTimer() {
        otpSeconds = 90;
        const timerEl = document.getElementById('otpTimerText');
        const resendBtn = document.getElementById('otpResendBtn');
        if (resendBtn) { resendBtn.classList.add('disabled'); resendBtn.textContent = ''; }
        clearInterval(otpTimer);
        otpTimer = setInterval(() => {
            otpSeconds--;
            const m = String(Math.floor(otpSeconds / 60)).padStart(2, '0');
            const s = String(otpSeconds % 60).padStart(2, '0');
            if (timerEl) timerEl.textContent = `${m}:${s}`;
            if (otpSeconds <= 0) {
                clearInterval(otpTimer);
                if (timerEl) timerEl.textContent = '';
                if (resendBtn) { resendBtn.classList.remove('disabled'); resendBtn.textContent = 'Renvoyer le code'; }
                const sendBtn = document.getElementById('btnSendCode');
                if (sendBtn) sendBtn.disabled = false;
            }
        }, 1000);
    }

    function verifyOtp() {
        const inputs = document.querySelectorAll('.otp-input');
        const code = Array.from(inputs).map(i => i.value).join('');
        if (code.length !== 6) return;

        // Utiliser le champ Email principal (lié au modèle via asp-for)
        const email = document.getElementById('Email')?.value
                   || document.querySelector('[name="Email"]')?.value;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        fetch('/Professeurs/VerifyOtpCode', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `email=${encodeURIComponent(email)}&code=${encodeURIComponent(code)}&__RequestVerificationToken=${encodeURIComponent(token || '')}`
        })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                emailVerified = true;
                inputs.forEach(i => { i.classList.remove('error-shake'); i.classList.add('success'); i.disabled = true; });
                document.getElementById('otpSuccessMsg')?.classList.remove('hidden');
                document.getElementById('otpInputSection')?.querySelector('.otp-resend')?.classList.add('hidden');
                document.getElementById('btnSendCode')?.classList.add('hidden');
                document.getElementById('hiddenVerificationCode').value = code;
                showToast('Adresse Email vérifiée', 'success');
                updateSaveButtonState();
            } else {
                inputs.forEach(i => { i.classList.add('error-shake'); i.value = ''; });
                setTimeout(() => inputs.forEach(i => i.classList.remove('error-shake')), 400);
                inputs[0]?.focus();
                showToast(data.message || 'Code incorrect', 'error');
                if (data.maxAttempts) {
                    inputs.forEach(i => i.disabled = true);
                    showToast('Nombre maximum de tentatives atteint.', 'error');
                }
            }
        })
        .catch(() => showToast('Erreur de vérification.', 'error'));
    }

    // ── Password Generator ──────────────────────────────────────
    function generatePassword() {
        const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789@#!$';
        let pwd = '';
        for (let i = 0; i < 12; i++) pwd += chars.charAt(Math.floor(Math.random() * chars.length));
        return pwd;
    }

    function updatePasswordStrength(pwd) {
        const bar = document.getElementById('strengthBar');
        const label = document.getElementById('strengthLabel');
        if (!bar || !label) return;
        let score = 0;
        if (pwd.length >= 8) score++;
        if (pwd.length >= 12) score++;
        if (/[A-Z]/.test(pwd)) score++;
        if (/[0-9]/.test(pwd)) score++;
        if (/[^A-Za-z0-9]/.test(pwd)) score++;

        const levels = [
            { w: '0%', c: '#edf0f5', t: '' },
            { w: '20%', c: '#ef4444', t: 'Très faible' },
            { w: '40%', c: '#f97316', t: 'Faible' },
            { w: '60%', c: '#eab308', t: 'Moyen' },
            { w: '80%', c: '#22c55e', t: 'Fort' },
            { w: '100%', c: '#10b981', t: 'Très fort' }
        ];
        const l = levels[score] || levels[0];
        bar.style.width = l.w;
        bar.style.background = l.c;
        label.textContent = l.t;
        label.style.color = l.c;
    }

    function updateSaveButtonState() {
        const btn = document.getElementById('btnSaveCreate');
        if (btn) btn.disabled = !emailVerified;
    }

    // ── Preview Update ──────────────────────────────────────────
    function updatePreview() {
        const fields = {
            'previewMatricule': '[name="Matricule"]',
            'previewGrade': '[name="Grade"]',
            'previewSpecialite': '[name="Specialite"]',
            'previewStatut': '[name="Statut"]',
            'previewDateEmbauche': '[name="DateEmbauche"]'
        };
        for (const [id, sel] of Object.entries(fields)) {
            const el = document.getElementById(id);
            const inp = document.querySelector(sel);
            if (el && inp) el.textContent = inp.value || '-';
        }
    }

    // ── Init ────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        initToasts();

        // Tab clicks
        tabEls().forEach((tab, idx) => {
            tab.addEventListener('click', () => switchTab(idx));
        });
        updateButtons();

        // Navigation buttons
        document.getElementById('btnPrev')?.addEventListener('click', (e) => { addRipple(e); switchTab(currentTab - 1); });
        document.getElementById('btnNext')?.addEventListener('click', (e) => {
            addRipple(e);
            if (validateCurrentTab()) {
                // Mark tab completed
                const tabs = tabEls();
                if (tabs[currentTab]) tabs[currentTab].classList.add('completed');
                switchTab(currentTab + 1);
            } else {
                showToast('Veuillez remplir tous les champs obligatoires.', 'error');
            }
        });

        // Ripple on all buttons
        document.querySelectorAll('.btn-primary-like, .btn-success-like').forEach(btn => {
            btn.addEventListener('click', addRipple);
        });

        // Field validation on blur/input
        document.querySelectorAll('.field input, .field select').forEach(input => {
            input.addEventListener('blur', function () {
                this.closest('.field')?.classList.add('touched');
                validateField(this);
            });
            input.addEventListener('input', function () {
                if (this.closest('.field')?.classList.contains('touched')) validateField(this);
                updateProfileCard();
                updatePreview();
            });
        });

        // Suivi du champ Email principal (Panel 1, asp-for="Email")
        // → synchronise le display Panel 3 et réinitialise l'OTP si l'email change
        document.getElementById('Email')?.addEventListener('input', function () {
            const display = document.getElementById('AccountEmailDisplay');
            if (display) display.value = this.value;
            checkEmailAvailability(this.value);
            emailVerified = false;
            updateSaveButtonState();
            // Reset OTP
            document.getElementById('otpSentMsg')?.classList.add('hidden');
            document.getElementById('otpInputSection')?.classList.add('hidden');
            document.getElementById('otpSuccessMsg')?.classList.add('hidden');
            document.querySelectorAll('.otp-input').forEach(i => { i.value = ''; i.disabled = false; i.classList.remove('success', 'error-shake'); });
            const sendBtn = document.getElementById('btnSendCode');
            if (sendBtn) { sendBtn.classList.remove('hidden'); sendBtn.disabled = false; }
        });

        // Synchroniser également au moment où on arrive sur le Tab 3
        tabEls().forEach((tab, idx) => {
            tab.addEventListener('click', () => {
                if (idx === 2) {
                    const email = document.getElementById('Email')?.value || '';
                    const display = document.getElementById('AccountEmailDisplay');
                    if (display && email) display.value = email;
                }
            });
        });

        // Send OTP
        document.getElementById('btnSendCode')?.addEventListener('click', sendOtp);
        document.getElementById('otpResendBtn')?.addEventListener('click', function (e) {
            e.preventDefault();
            if (!this.classList.contains('disabled')) sendOtp();
        });

        // OTP Inputs
        document.querySelectorAll('.otp-input').forEach((inp, idx, all) => {
            inp.addEventListener('input', function () {
                this.value = this.value.replace(/\D/g, '').slice(0, 1);
                if (this.value) {
                    this.classList.add('filled');
                    if (idx < all.length - 1) all[idx + 1].focus();
                    if (idx === all.length - 1) verifyOtp();
                } else {
                    this.classList.remove('filled');
                }
            });
            inp.addEventListener('keydown', function (e) {
                if (e.key === 'Backspace' && !this.value && idx > 0) { all[idx - 1].focus(); all[idx - 1].value = ''; }
            });
            inp.addEventListener('paste', function (e) {
                e.preventDefault();
                const paste = (e.clipboardData || window.clipboardData).getData('text').replace(/\D/g, '').slice(0, 6);
                paste.split('').forEach((ch, i) => { if (all[i]) { all[i].value = ch; all[i].classList.add('filled'); } });
                if (paste.length === 6) verifyOtp();
                else if (all[paste.length]) all[paste.length].focus();
            });
        });

        // Auto-generate password checkbox
        document.getElementById('autoGenPassword')?.addEventListener('change', function () {
            const pwdField = document.getElementById('Password');
            const confirmField = document.getElementById('ConfirmPassword');
            if (this.checked) {
                const pwd = generatePassword();
                if (pwdField) { pwdField.value = pwd; pwdField.type = 'text'; pwdField.readOnly = true; }
                if (confirmField) { confirmField.value = pwd; confirmField.readOnly = true; confirmField.closest('.field')?.classList.add('hidden'); }
                updatePasswordStrength(pwd);
            } else {
                if (pwdField) { pwdField.value = ''; pwdField.type = 'password'; pwdField.readOnly = false; }
                if (confirmField) { confirmField.value = ''; confirmField.readOnly = false; confirmField.closest('.field')?.classList.remove('hidden'); }
                updatePasswordStrength('');
            }
        });

        // Password strength indicator
        document.getElementById('Password')?.addEventListener('input', function () {
            updatePasswordStrength(this.value);
        });

        // Photo preview
        window.previewImage = function (input) {
            if (!input.files || !input.files[0]) return;
            const reader = new FileReader();
            reader.onload = function (e) {
                const preview = document.getElementById('photoPreview');
                const placeholder = document.getElementById('photoPlaceholder');
                if (preview) { preview.src = e.target.result; preview.classList.remove('hidden'); }
                placeholder?.classList.add('hidden');
            };
            reader.readAsDataURL(input.files[0]);
        };

        // Form submit handler
        document.getElementById('profForm')?.addEventListener('submit', function (e) {
            const submitBtn = document.querySelector('button[type="submit"]:focus') || document.getElementById('btnSave');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="btn-loader"></span> Enregistrement...';
            }
        });


        // Init profile card
        updateProfileCard();
        updateSaveButtonState();

        // Lucide icons refresh
        if (window.lucide) setTimeout(() => lucide.createIcons(), 100);
    });
})();
