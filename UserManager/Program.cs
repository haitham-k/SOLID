var userManager = new UserManager();

userManager.AddUser("Alice", UserRole.Admin);
userManager.AddUser("Bob", UserRole.Manager);
userManager.AddUser("Charlie", UserRole.Employee);

Console.WriteLine(userManager.HasPermission("Alice", "Delete")); // true
Console.WriteLine(userManager.HasPermission("Bob", "Delete"));   // false
Console.WriteLine(userManager.HasPermission("Charlie", "Read")); // true

userManager.ChangeUserRole("Bob", UserRole.Admin);
Console.WriteLine(userManager.HasPermission("Bob", "Delete"));   // true

Console.WriteLine(UserManager.Factorial(4));

Console.WriteLine(UserManager.IsPalindrome("PoloP"));