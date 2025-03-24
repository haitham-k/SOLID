using System.Security;

public enum UserRole
{
    Admin, Manager, Employee
}

public enum Permissions
{
    Read, Write, Delete
}

public class RolePermissions
{
    public static Dictionary<UserRole, HashSet<Permissions>> RolePermissionsDictionnary = new()
        {
            {UserRole.Admin, new HashSet<Permissions>{Permissions.Read, Permissions.Write, Permissions.Delete}},
            {UserRole.Manager, new HashSet<Permissions>{Permissions.Read, Permissions.Write}},
            {UserRole.Employee, new HashSet<Permissions>{Permissions.Read}}
        };
}

public interface IUserManager
{
    void AddUser(string userName, UserRole userRole);
    bool HasPermission(string userName, string permission);
    void ChangeUserRole(string userName, UserRole userRole);
}

public class UserManager : IUserManager
{
    public readonly Dictionary<string, UserRole> Users = new();
    public void AddUser(string userName, UserRole userRole)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Name cannot be empty", nameof(userName));

        var useradded = Users.TryAdd(userName, userRole);
        if (!useradded) throw new ArgumentException("User already exists", nameof(userName));
    }

    public bool HasPermission(string userName, Permissions permission)
    {
        var userExists = Users.TryGetValue(userName, out var userRole);
        if (!userExists) throw new ArgumentException("User not found", nameof(userName));

        RolePermissions.RolePermissionsDictionnary.TryGetValue(userRole, out var permissions);
        return permissions!.Contains(permission);
    }

    public bool HasPermission(string userName, string permission)
    {
        if(!Enum.TryParse(permission, out Permissions permissionEnum)) 
            throw new ArgumentOutOfRangeException(nameof(permission), "Permission not found");

        return HasPermission(userName, permissionEnum);
    }

    public void ChangeUserRole(string userName, UserRole userRole)
    {
        if (!Users.TryGetValue(userName, out var role))
            throw new ArgumentOutOfRangeException(nameof(userName), "userName not found");

        Users[userName] = userRole;
    }

    public static int Factorial(int n)
    {
        return n > 1? n * Factorial(n - 1) : 1;
    }

    public static bool IsPalindrome(string s)
    {
        int start = 0, end = s.Length - 1;
        while (start < end)
        {
            if (s[start] != s[end]) return false;
            start++;
            end--;
        }
        return true;
    }
}