namespace FclEx.EfCore;

public readonly record struct DatabaseUser(string Username, string Password, string DefaultSchema);