using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SymconDashboard;

internal static class Strings
{
    private static readonly ResourceManager s_rm =
        new("SymconDashboard.Strings", typeof(Strings).Assembly);

    private static string Get([CallerMemberName] string key = "") =>
        s_rm.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    // Tray-Menü
    public static string TrayShowTitleBar    => Get();
    public static string TrayHideTitleBar    => Get();
    public static string TrayChangeUrl       => Get();
    public static string TrayReload          => Get();
    public static string TrayResetPosition   => Get();
    public static string TrayBorderless      => Get();
    public static string TrayColor           => Get();
    public static string TrayColorSystem     => Get();
    public static string TrayColorAuto       => Get();
    public static string TrayCustom          => Get();
    public static string TrayBorderWidth     => Get();
    public static string TrayZoom            => Get();
    public static string TrayKioskMode      => Get();
    public static string TrayExit            => Get();

    // Dialoge
    public static string DlgUrlTitle         => Get();
    public static string DlgUrlPrompt        => Get();
    public static string DlgUrlInvalid       => Get();
    public static string DlgError            => Get();
    public static string DlgColorTitle       => Get();
    public static string DlgColorPrompt      => Get();
    public static string DlgBorderTitle      => Get();
    public static string DlgBorderPrompt     => Get();
    public static string DlgBorderInvalid    => Get();
    public static string DlgZoomTitle        => Get();
    public static string DlgZoomPrompt       => Get();
    public static string DlgZoomInvalid      => Get();
    public static string DlgInvalidInput     => Get();
    public static string DlgCancel           => Get();

    // HTTP-Fehler
    public static string ErrHttp401          => Get();
    public static string ErrHttp403          => Get();
    public static string ErrHttp404          => Get();
    public static string ErrHttp500          => Get();
    public static string ErrHttp502          => Get();
    public static string ErrHttp503          => Get();
    public static string ErrHttpGeneric      => Get();
    public static string ErrNetworkTitle     => Get();
    public static string ErrDnsNotResolved   => Get();
    public static string ErrCannotConnect    => Get();
    public static string ErrConnectionReset  => Get();
    public static string ErrDisconnected     => Get();
    public static string ErrTimeout          => Get();
    public static string ErrServerUnreachable => Get();
    public static string ErrOperationCanceled => Get();
    public static string ErrNetworkGeneric   => Get();

    // Fehlerseite
    public static string ErrPageRetry        => Get();
}
