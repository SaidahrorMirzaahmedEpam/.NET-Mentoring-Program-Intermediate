public class PasswordHashUsingSalt
{
    public string GeneratePasswordHashUsingSalt(string passwordText, byte[] salt)
    {
        const int iterate = 10000;
        const int derivedBytesLength = 32;

        using var pbkdf2 = new Rfc2898DeriveBytes(
            passwordText, salt, iterate, HashAlgorithmName.SHA256);

        byte[] hash = pbkdf2.GetBytes(derivedBytesLength);
        byte[] hashBytes = new byte[salt.Length + hash.Length];

        Buffer.BlockCopy(salt, 0, hashBytes, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, hashBytes, salt.Length, hash.Length);

        try
        {
            return Convert.ToBase64String(hashBytes);
        }
        finally
        {
            Array.Clear(hash, 0, hash.Length);
            Array.Clear(hashBytes, 0, hashBytes.Length);
        }
    }
}