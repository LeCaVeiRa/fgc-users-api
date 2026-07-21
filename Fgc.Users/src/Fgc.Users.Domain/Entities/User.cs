using Fgc.Users.Domain.ValueObjects;

namespace Fgc.Users.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set ; }

    private User() 
    {
        Name = null!;
        Email = null!; 
        PasswordHash = null!;
        Role = null!;
    }

    private User(string name, Email email, string passwordhash, string role)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PasswordHash = passwordhash;
        Role = role;
    }

    public static User Create(
        string name,
        Email email,
        string passwordHash,
        string role = "User")

    {
        Validate(name, email, passwordHash, role);
        return new User(name, email, passwordHash, role);
    }
    private static void Validate(
        string name,
        Email email,
        string passwordHash,
        string role)
    {

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password is required.");

        if (role != "User" && role != "Admin")
            throw new ArgumentException("Invalid role.");
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.");

        Name = name;
    }

    public void UpdateRole(string role)
    {
        if (role != "User" && role != "Admin")
            throw new ArgumentException("Invalid role.");

        Role = role;
    }
    public void UpdateEmail(Email email)
    {
        Email = email;
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password is required.");

        PasswordHash = passwordHash;
    }

}
