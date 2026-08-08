/**
 * Ortak yardimcilar: bildirim, popup, favori ve playlist islemleri.
 */
window.MP = (function () {
    'use strict';

    return {
        formatTime(seconds) {
            if (!seconds || isNaN(seconds)) return '0:00';
            const m = Math.floor(seconds / 60);
            const s = Math.floor(seconds % 60);
            return `${m}:${s.toString().padStart(2, '0')}`;
        },

        /**
         * Paket yetersizligi popup'i.
         * API'nin 403 cevabindaki detaylari gosterir.
         */
        showPackageModal(body) {
            const modal   = document.getElementById('packageModal');
            const message = document.getElementById('packageModalMessage');
            const detail  = document.getElementById('packageModalDetail');
            const title   = document.getElementById('packageModalTitle');

            if (!modal) {
                alert(body?.message || 'Bu şarkı paketinizde bulunmuyor.');
                return;
            }

            message.textContent = body?.message
                || 'Mevcut paketiniz bu şarkıyı desteklememektedir. Lütfen paketinizi yükseltin.';

            const d = body?.data;

            if (d) {
                title.textContent = `"${d.songTitle}" için ${d.requiredPackageName} gerekli`;

                detail.innerHTML = `
                    <span class="badge badge-pill pkg-${d.userPackage} px-3 py-2">${d.userPackageName}</span>
                    <i data-feather="arrow-right" class="mx-3 text-muted" style="width:18px;height:18px"></i>
                    <span class="badge badge-pill pkg-${d.requiredPackage} px-3 py-2">${d.requiredPackageName}</span>
                `;
            } else {
                title.textContent = 'Bu şarkı paketinizde yok';
                detail.innerHTML = '';
            }

            if (window.feather) feather.replace();
            $(modal).modal('show');
        },

        /**
         * Kisa bildirim.
         */
        toast(message, type = 'info') {
            let host = document.getElementById('mp-toasts');

            if (!host) {
                host = document.createElement('div');
                host.id = 'mp-toasts';
                document.body.appendChild(host);
            }

            const el = document.createElement('div');
            el.className = `mp-toast mp-toast-${type}`;
            el.textContent = message;
            host.appendChild(el);

            requestAnimationFrame(() => el.classList.add('show'));

            setTimeout(() => {
                el.classList.remove('show');
                setTimeout(() => el.remove(), 300);
            }, 3000);
        },

        /**
         * Favori ekle/cikar.
         */
        async toggleFavorite(songId, button) {
            try {
                const res = await fetch(`/Favorites/Toggle/${songId}`, { method: 'POST' });

                if (res.status === 401) {
                    window.location.href = '/Auth/Login';
                    return;
                }

                const body = await res.json();

                if (body.success) {
                    const isFav = body.data === true;
                    button.classList.toggle('is-favorite', isFav);
                    this.toast(body.message, 'success');
                } else {
                    this.toast(body.message || 'İşlem başarısız.', 'danger');
                }
            } catch {
                this.toast('Sunucuya ulaşılamadı.', 'danger');
            }
        },

        /**
         * Sarkiyi playlist'e ekler.
         */
        async addToPlaylist(playlistId, songId, button) {
            try {
                const res = await fetch(`/Playlists/${playlistId}/AddSong/${songId}`, {
                    method: 'POST'
                });

                if (res.status === 401) {
                    window.location.href = '/Auth/Login';
                    return;
                }

                const body = await res.json();

                if (body.success) {
                    this.toast(body.message || 'Playlist\'e eklendi.', 'success');
                    if (window.jQuery) $('#addToPlaylistModal').modal('hide');
                } else {
                    this.toast(body.message || 'Eklenemedi.', 'danger');
                }
            } catch {
                this.toast('Sunucuya ulaşılamadı.', 'danger');
            }
        }
    };
})();

// -------------------------------------------------------------- baglama
document.addEventListener('DOMContentLoaded', function () {

    // Favori butonlari
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-favorite');
        if (!btn) return;

        e.preventDefault();
        e.stopPropagation();

        const songId = btn.dataset.songId;
        if (songId) MP.toggleFavorite(songId, btn);
    });

    // Sayfadaki favorileri isaretle
    fetch('/Favorites/Ids')
        .then(r => r.ok ? r.json() : null)
        .then(body => {
            if (!body?.success || !Array.isArray(body.data)) return;

            const favorites = new Set(body.data);
            document.querySelectorAll('.btn-favorite').forEach(btn => {
                if (favorites.has(parseInt(btn.dataset.songId, 10))) {
                    btn.classList.add('is-favorite');
                }
            });
        })
        .catch(() => { /* giris yapilmamis olabilir */ });

    if (window.feather) feather.replace();
});