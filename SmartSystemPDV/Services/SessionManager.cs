using SmartSystemPDV.Models;

namespace SmartSystemPDV.Services;

public static class SessionManager
{
    private static Usuario? _currentUser;

    public static Usuario? CurrentUser
    {
        get => _currentUser;
        set => _currentUser = value;
    }

    public static bool IsLoggedIn => _currentUser != null;

    public static void Logout()
    {
        _currentUser = null;
    }
}
