
(function () {
    'use strict';

    const MIN_LOG_SECONDS = 5; 

    const Player = {
        audio: null,
        queue: [],      
        index: -1,
        loggedFor: null,  

        init() {
            this.buildUI();
            this.bindEvents();
        },

        // ------------------------------------------------------------- UI
        buildUI() {
            const bar = document.getElementById('mp-player');
            if (!bar) return;

            this.audio = bar.querySelector('audio');

            this.el = {
                bar:      bar,
                cover:    bar.querySelector('.mp-cover'),
                title:    bar.querySelector('.mp-title'),
                artist:   bar.querySelector('.mp-artist'),
                playBtn:  bar.querySelector('.mp-play'),
                prevBtn:  bar.querySelector('.mp-prev'),
                nextBtn:  bar.querySelector('.mp-next'),
                seek:     bar.querySelector('.mp-seek'),
                current:  bar.querySelector('.mp-current'),
                duration: bar.querySelector('.mp-duration'),
                volume:   bar.querySelector('.mp-volume')
            };
        },

        bindEvents() {
            // Sarki kartlarindaki play butonlari (dinamik icerik icin delegasyon)
            document.addEventListener('click', (e) => {
                const btn = e.target.closest('.btn-play');
                if (!btn) return;

                e.preventDefault();
                const item = btn.closest('[data-song-id]');
                if (item) this.playFromElement(item);
            });

            if (!this.audio) return;

            this.el.playBtn?.addEventListener('click', () => this.toggle());
            this.el.prevBtn?.addEventListener('click', () => this.prev());
            this.el.nextBtn?.addEventListener('click', () => this.next());

            this.audio.addEventListener('timeupdate', () => this.onTimeUpdate());
            this.audio.addEventListener('loadedmetadata', () => this.onLoaded());
            this.audio.addEventListener('ended', () => this.onEnded());
            this.audio.addEventListener('play', () => this.setPlayIcon(true));
            this.audio.addEventListener('pause', () => this.setPlayIcon(false));

            this.audio.addEventListener('error', () => {
                if (this.audio.src) MP.toast('Şarkı yüklenemedi.', 'danger');
            });

            this.el.seek?.addEventListener('input', (e) => {
                if (this.audio.duration) {
                    this.audio.currentTime = (e.target.value / 100) * this.audio.duration;
                }
            });

            this.el.volume?.addEventListener('input', (e) => {
                this.audio.volume = e.target.value / 100;
            });

            // Bosluk tusuyla duraklat (input alaninda degilsen)
            document.addEventListener('keydown', (e) => {
                if (e.code !== 'Space') return;
                const tag = document.activeElement?.tagName;
                if (tag === 'INPUT' || tag === 'TEXTAREA') return;
                if (this.index < 0) return;

                e.preventDefault();
                this.toggle();
            });
        },

        // -------------------------------------------------- Calma akisi
        async playFromElement(el) {
            const song = {
                id:       parseInt(el.dataset.songId, 10),
                title:    el.dataset.songTitle,
                artist:   el.dataset.songArtist,
                cover:    el.dataset.songCover,
                duration: parseInt(el.dataset.songDuration || '0', 10)
            };

            // Ayni sarki calıyorsa duraklat/devam et
            if (this.index >= 0 && this.queue[this.index]?.id === song.id) {
                this.toggle();
                return;
            }

            // Karttaki data-can-play sadece bir on filtre.
            // Asil karar API'den geliyor: token eskimis olabilir.
            const allowed = await this.checkAccess(song.id);
            if (!allowed) return;

            // Ayni satirdaki (row) tum sarkilari kuyruga al
            this.buildQueue(el);

            const idx = this.queue.findIndex(s => s.id === song.id);
            this.index = idx >= 0 ? idx : 0;

            this.load(this.queue[this.index]);
        },

        /**
         * Paket kontrolu. Izin yoksa popup acar ve false doner.
         */
        async checkAccess(songId) {
            try {
                const res = await fetch(`/Stream/Check/${songId}`, {
                    headers: { 'Accept': 'application/json' }
                });

                if (res.status === 401) {
                    window.location.href = '/Auth/Login';
                    return false;
                }

                const body = await res.json().catch(() => null);

                if (res.ok && body?.success) return true;

                // 403 veya success:false → paket yetersiz
                MP.showPackageModal(body);
                return false;

            } catch (err) {
                console.error('Erisim kontrolu basarisiz:', err);
                MP.toast('Sunucuya ulaşılamadı.', 'danger');
                return false;
            }
        },

        /**
         * Ayni satirdaki dinlenebilir sarkilari kuyruga alir.
         * Kilitli olanlari atlar: sirayla calarken 403'e takilmasin.
         */
        buildQueue(el) {
            const container = el.closest('[data-playlist]') || el.parentElement;
            const items = container.querySelectorAll('[data-song-id]');

            this.queue = Array.from(items)
                .filter(i => i.dataset.canPlay === 'true')
                .map(i => ({
                    id:       parseInt(i.dataset.songId, 10),
                    title:    i.dataset.songTitle,
                    artist:   i.dataset.songArtist,
                    cover:    i.dataset.songCover,
                    duration: parseInt(i.dataset.songDuration || '0', 10)
                }));

            // Tiklanan sarki kilitliyse (token yenilenmis olabilir) kuyruga ekle
            const id = parseInt(el.dataset.songId, 10);
            if (!this.queue.some(s => s.id === id)) {
                this.queue.unshift({
                    id:       id,
                    title:    el.dataset.songTitle,
                    artist:   el.dataset.songArtist,
                    cover:    el.dataset.songCover,
                    duration: parseInt(el.dataset.songDuration || '0', 10)
                });
            }
        },

        load(song) {
            if (!song) return;

            this.loggedFor = null;
            this.audio.src = `/Stream/Play/${song.id}`;
            this.audio.play().catch(err => {
                // Tarayici otomatik oynatmayi engellemis olabilir
                console.warn('Oynatma engellendi:', err);
            });

            this.render(song);
            this.markPlaying(song.id);
            this.el.bar.classList.add('active');
        },

        toggle() {
            if (!this.audio.src) return;
            this.audio.paused ? this.audio.play() : this.audio.pause();
        },

        next() {
            if (this.queue.length === 0) return;
            this.index = (this.index + 1) % this.queue.length;
            this.load(this.queue[this.index]);
        },

        prev() {
            if (this.queue.length === 0) return;

            // 3 saniyeden fazla calmissa basa sar, degilse onceki sarkiya git
            if (this.audio.currentTime > 3) {
                this.audio.currentTime = 0;
                return;
            }

            this.index = (this.index - 1 + this.queue.length) % this.queue.length;
            this.load(this.queue[this.index]);
        },

        // ------------------------------------------------------ Olaylar
        onTimeUpdate() {
            const { currentTime, duration } = this.audio;
            if (!duration) return;

            if (this.el.seek) this.el.seek.value = (currentTime / duration) * 100;
            if (this.el.current) this.el.current.textContent = MP.formatTime(currentTime);

            // Dinleme kaydi: 5 saniyeyi gecince bir kez
            const song = this.queue[this.index];
            if (song && currentTime > MIN_LOG_SECONDS && this.loggedFor !== song.id) {
                this.loggedFor = song.id;
                this.logListening(song.id, Math.floor(currentTime));
            }
        },

        onLoaded() {
            if (this.el.duration) {
                this.el.duration.textContent = MP.formatTime(this.audio.duration);
            }
        },

        onEnded() {
            const song = this.queue[this.index];
            if (song) this.logListening(song.id, song.duration);

            this.next();
        },

        logListening(songId, seconds) {
            fetch(`/Stream/Log/${songId}?seconds=${seconds}`, { method: 'POST' })
                .catch(() => { /* sessizce gec: kritik degil */ });
        },

        // -------------------------------------------------------- Gorunum
        render(song) {
            if (this.el.title)  this.el.title.textContent = song.title;
            if (this.el.artist) this.el.artist.textContent = song.artist;

            if (this.el.cover) {
                if (song.cover) {
                    this.el.cover.style.backgroundImage = `url('${song.cover}')`;
                    this.el.cover.textContent = '';
                    this.el.cover.classList.remove('letter-cover');
                } else {
                    this.el.cover.style.backgroundImage = '';
                    this.el.cover.textContent = (song.artist || '?').substring(0, 2).toUpperCase();
                    this.el.cover.classList.add('letter-cover', 'letter-cover-sm');
                }
            }

            document.title = `${song.title} · ${song.artist}`;
        },

        markPlaying(songId) {
            document.querySelectorAll('[data-song-id]').forEach(el => {
                el.classList.toggle('playing', parseInt(el.dataset.songId, 10) === songId);
            });
        },

        setPlayIcon(playing) {
            if (!this.el.playBtn) return;

            this.el.playBtn.innerHTML = playing
                ? '<i data-feather="pause"></i>'
                : '<i data-feather="play"></i>';

            if (window.feather) feather.replace();
        }
    };

    window.MPPlayer = Player;
    document.addEventListener('DOMContentLoaded', () => Player.init());
})();