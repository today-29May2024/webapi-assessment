using System.Security.Cryptography;
using System.Text;
using AssessmentInnocel.Models;

namespace AssessmentInnocel.CustomExtensions;

public static class TransactionExtension
{
    public static string EncodePartnerPassword(string partnerPassword) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(partnerPassword));

    public static string FormatSigTimestamp(string timestamp) =>
        timestamp.Split(".")[0].Replace("-", "").Replace(":", "").Replace("T", "");

    public static string GenSigMsg(List<Transaction> transactions, string partnerKey)
    {
        var query = transactions
                    .Where(t => t.PartnerKey == partnerKey)
                    .Select(i =>
                        new
                        {
                            Timestamp = TransactionExtension.FormatSigTimestamp(i.Timestamp),
                            i.PartnerKey,
                            i.PartnerRefNo,
                            i.TotalAmount,
                            PartnerPassowrd = i.PartnerPassword
                        });

        Console.WriteLine("\n\n\n===> INSIDE the GenSigMsg method of TransactionExtension ---\n");
        Console.WriteLine("\nResult of generated sig msg without formatting:");

        string concatFieldStr = "";
        foreach (var obj in query)
        {
            Console.Write($"{obj}\n");
            concatFieldStr += obj.Timestamp + obj.PartnerKey + obj.PartnerRefNo + obj.TotalAmount + obj.PartnerPassowrd;
        }
        return concatFieldStr;
    }

    public static string EncodeSHA265HashSig(string sigmsg)
    {
        Console.WriteLine("\n\n\n===> INSIDE the EncodeSHAS265HashSig method of TransactionExtension ---\n");
        var encodedSHA265Hash = "";

        using (SHA256 sha256Hash = SHA256.Create())
        {
            string hash = TransactionExtension.GetHash(sha256Hash, sigmsg);
            // Console.WriteLine(hash);
            Console.WriteLine($"\n\nThe SHA256 hash of {sigmsg} is: {hash}");
            encodedSHA265Hash = hash;
        }

        Console.WriteLine("hashed hex string before encoded : " + encodedSHA265Hash);
        encodedSHA265Hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(encodedSHA265Hash));
        Console.WriteLine("hashed hex string after encoded to base 64 string: " + encodedSHA265Hash);

        return encodedSHA265Hash;
    }

    // Ref. https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=net-9.0
    public static string GetHash(HashAlgorithm hashAlgorithm, string input)
    {

        // Convert the input string to a byte array and compute the hash.
        byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        var sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

}