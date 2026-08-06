namespace MusicPlatform.Business.Mail;

public static class MailTemplates
{
    private const string Primary = "#6C4DF6";
    private const string Dark    = "#14121F";
    private const string Muted   = "#8B8798";

    /// <summary>Tüm mailleri saran ortak çerçeve.</summary>
    public static string Layout(string title, string bodyHtml, string? ctaText = null, string? ctaUrl = null)
    {
        var cta = ctaText is not null && ctaUrl is not null
            ? $"""
               <tr><td align="center" style="padding:8px 0 28px;">
                 <a href="{ctaUrl}" style="display:inline-block;background:{Primary};color:#ffffff;
                    text-decoration:none;padding:14px 32px;border-radius:8px;font-weight:600;
                    font-size:15px;">{ctaText}</a>
               </td></tr>
               """
            : "";

        return $"""
            <!DOCTYPE html>
            <html lang="tr">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width,initial-scale=1">
              <title>{title}</title>
            </head>
            <body style="margin:0;padding:0;background:#F4F3F8;
                         font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Arial,sans-serif;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#F4F3F8;padding:32px 16px;">
                <tr><td align="center">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0"
                         style="max-width:560px;background:#ffffff;border-radius:16px;overflow:hidden;
                                box-shadow:0 2px 12px rgba(20,18,31,0.06);">

                    <tr><td style="background:{Dark};padding:28px 32px;">
                      <span style="color:#ffffff;font-size:20px;font-weight:700;letter-spacing:-0.3px;">
                        ♪ MusicPlatform
                      </span>
                    </td></tr>

                    <tr><td style="padding:32px 32px 8px;">
                      <h1 style="margin:0 0 16px;font-size:22px;line-height:1.3;color:{Dark};font-weight:700;">
                        {title}
                      </h1>
                      <div style="font-size:15px;line-height:1.65;color:#4A4658;">
                        {bodyHtml}
                      </div>
                    </td></tr>

                    {cta}

                    <tr><td style="padding:20px 32px 28px;border-top:1px solid #EEECF4;">
                      <p style="margin:0;font-size:12px;line-height:1.6;color:{Muted};">
                        Bu e-posta MusicPlatform tarafından otomatik gönderilmiştir.<br>
                        Bu işlemi siz yapmadıysanız lütfen dikkate almayın.
                      </p>
                    </td></tr>

                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }

    public static string Welcome(string fullName, string confirmUrl) => Layout(
        "Aramıza hoş geldin!",
        $"""
         <p style="margin:0 0 14px;">Merhaba <strong>{fullName}</strong>,</p>
         <p style="margin:0 0 14px;">MusicPlatform hesabın oluşturuldu. Şu anda
         <strong>Basic</strong> paketindesin ve katalogdaki ücretsiz şarkıları dinlemeye
         hemen başlayabilirsin.</p>
         <p style="margin:0 0 14px;">Hesabını aktifleştirmek için aşağıdaki butona tıkla.</p>
         """,
        "E-postamı doğrula", confirmUrl);

    public static string PasswordReset(string fullName, string resetUrl) => Layout(
        "Şifre sıfırlama talebi",
        $"""
         <p style="margin:0 0 14px;">Merhaba <strong>{fullName}</strong>,</p>
         <p style="margin:0 0 14px;">Hesabın için şifre sıfırlama talebinde bulunuldu.
         Yeni şifreni belirlemek için aşağıdaki butonu kullan.</p>
         <p style="margin:0 0 14px;color:#8B8798;font-size:13px;">
         Bu bağlantı <strong>1 saat</strong> boyunca geçerlidir.</p>
         """,
        "Şifremi sıfırla", resetUrl);

    public static string PasswordChanged(string fullName, DateTime changedAt) => Layout(
        "Şifren değiştirildi",
        $"""
         <p style="margin:0 0 14px;">Merhaba <strong>{fullName}</strong>,</p>
         <p style="margin:0 0 14px;">Hesabının şifresi
         <strong>{changedAt.AddHours(3):dd MMMM yyyy HH:mm}</strong> tarihinde değiştirildi.
         Güvenlik amacıyla diğer cihazlardaki oturumların kapatıldı.</p>
         <p style="margin:0;">Bu işlemi sen yapmadıysan hemen şifreni sıfırla.</p>
         """);

    public static string PurchaseReceipt(
        string fullName, string packageName, decimal amount,
        DateTime expiresAt, string reference) => Layout(
        $"{packageName} paketin aktif!",
        $"""
         <p style="margin:0 0 18px;">Merhaba <strong>{fullName}</strong>,
         ödemen alındı ve paketin aktifleştirildi.</p>

         <table role="presentation" width="100%" cellpadding="0" cellspacing="0"
                style="background:#F8F7FC;border-radius:10px;padding:18px;margin-bottom:16px;">
           <tr>
             <td style="font-size:14px;color:#8B8798;padding:5px 0;">Paket</td>
             <td style="font-size:14px;color:#14121F;font-weight:600;text-align:right;">{packageName}</td>
           </tr>
           <tr>
             <td style="font-size:14px;color:#8B8798;padding:5px 0;">Tutar</td>
             <td style="font-size:14px;color:#14121F;font-weight:600;text-align:right;">{amount:N2} ₺</td>
           </tr>
           <tr>
             <td style="font-size:14px;color:#8B8798;padding:5px 0;">Geçerlilik</td>
             <td style="font-size:14px;color:#14121F;font-weight:600;text-align:right;">{expiresAt.AddHours(3):dd MMMM yyyy}</td>
           </tr>
           <tr>
             <td style="font-size:14px;color:#8B8798;padding:5px 0;">İşlem No</td>
             <td style="font-size:13px;color:#14121F;font-family:monospace;text-align:right;">{reference}</td>
           </tr>
         </table>

         <p style="margin:0;">Yeni katalogun açıldı, iyi dinlemeler!</p>
         """);

    public static string PackageExpiring(string fullName, string packageName, int daysLeft, string upgradeUrl) => Layout(
        $"Paketinin bitmesine {daysLeft} gün kaldı",
        $"""
         <p style="margin:0 0 14px;">Merhaba <strong>{fullName}</strong>,</p>
         <p style="margin:0 0 14px;"><strong>{packageName}</strong> paketinin süresi
         <strong>{daysLeft} gün</strong> içinde doluyor. Süre dolduğunda hesabın
         otomatik olarak Basic pakete düşecek ve bazı şarkılara erişimin kapanacak.</p>
         """,
        "Paketimi yenile", upgradeUrl);

    public static string UpgradeInvitation(
        string fullName, string songTitle, string requiredPackage, string upgradeUrl) => Layout(
        "Bu şarkıyı kaçırma",
        $"""
         <p style="margin:0 0 14px;">Merhaba <strong>{fullName}</strong>,</p>
         <p style="margin:0 0 14px;">Az önce <strong>{songTitle}</strong> şarkısını dinlemek istedin
         ama bu parça <strong>{requiredPackage}</strong> paketine ait.</p>
         <p style="margin:0 0 14px;">Paketini yükselterek bu şarkıya ve
         katalogdaki yüzlerce parçaya anında erişebilirsin.</p>
         """,
        "Paketleri incele", upgradeUrl);

    public static string WeeklyRecommendations(
        string fullName, List<(string Title, string Artist)> songs, string appUrl)
    {
        var rows = string.Join("", songs.Select(s => $"""
            <tr>
              <td style="padding:10px 0;border-bottom:1px solid #EEECF4;">
                <div style="font-size:15px;color:#14121F;font-weight:600;">{s.Title}</div>
                <div style="font-size:13px;color:#8B8798;margin-top:2px;">{s.Artist}</div>
              </td>
            </tr>
            """));

        return Layout(
            "Bunları da sevebilirsin",
            $"""
             <p style="margin:0 0 18px;">Merhaba <strong>{fullName}</strong>,
             dinleme geçmişine göre bu hafta senin için seçtiklerimiz:</p>
             <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
               {rows}
             </table>
             """,
            "Dinlemeye başla", appUrl);
    }

    public static string NewDeviceLogin(string fullName, string ip, DateTime loginAt) => Layout(
        "Yeni bir cihazdan giriş yapıldı",
        $"""
         <p style="margin:0 0 14px;">Merhaba <strong>{fullName}</strong>,</p>
         <p style="margin:0 0 14px;">Hesabına yeni bir cihazdan giriş yapıldı:</p>
         <table role="presentation" width="100%" cellpadding="0" cellspacing="0"
                style="background:#F8F7FC;border-radius:10px;padding:16px;margin-bottom:16px;">
           <tr>
             <td style="font-size:14px;color:#8B8798;padding:4px 0;">Tarih</td>
             <td style="font-size:14px;color:#14121F;text-align:right;">{loginAt.AddHours(3):dd.MM.yyyy HH:mm}</td>
           </tr>
           <tr>
             <td style="font-size:14px;color:#8B8798;padding:4px 0;">IP adresi</td>
             <td style="font-size:14px;color:#14121F;font-family:monospace;text-align:right;">{ip}</td>
           </tr>
         </table>
         <p style="margin:0;">Bu giriş sana ait değilse şifreni hemen değiştir.</p>
         """);
}