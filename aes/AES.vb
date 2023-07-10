Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Security.Cryptography
Imports System.IO

Public Class Aes
    Private Const KeySize As Integer = 256
    Private Const DerivationIterations As Integer = 1000

    Private Shared Function Generate256BitsOfRandomEntropy() As Byte()
        Dim randomBytes(31) As Byte
        Using rngcsp = New RNGCryptoServiceProvider()
            rngcsp.GetBytes(randomBytes)
        End Using
        Return randomBytes
    End Function

    Public Function Encrypt(ByVal seed As String, ByVal password As String) As String
        Dim saltBytes() As Byte = Generate256BitsOfRandomEntropy()
        Dim ivBytes() As Byte = Generate256BitsOfRandomEntropy()
        Dim seedByte() As Byte = Encoding.UTF8.GetBytes(seed)

        Using key = New Rfc2898DeriveBytes(password, saltBytes, DerivationIterations)
            Dim keyBytes = key.GetBytes(KeySize \ 8)
            Using symmetricKey = New RijndaelManaged()
                symmetricKey.BlockSize = 256
                symmetricKey.Mode = CipherMode.CBC
                symmetricKey.Padding = PaddingMode.PKCS7
                Using encryptor = symmetricKey.CreateEncryptor(keyBytes, ivBytes)
                    Using memoryStream As New MemoryStream()
                        Using cryptoStream As New CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)
                            cryptoStream.Write(seedByte, 0, seedByte.Length)
                            cryptoStream.FlushFinalBlock()
                            Dim cipherBytes = saltBytes
                            cipherBytes = cipherBytes.Concat(ivBytes).ToArray()
                            cipherBytes = cipherBytes.Concat(memoryStream.ToArray()).ToArray()
                            memoryStream.Close()
                            cryptoStream.Close()
                            Return Convert.ToBase64String(cipherBytes)
                        End Using
                    End Using
                End Using
            End Using
        End Using
    End Function

    Public Function Decrypt(ByVal cipher As String, ByVal password As String) As String
        Dim cipherSaltIv() As Byte = Convert.FromBase64String(cipher)
        Dim saltBytes() As Byte = cipherSaltIv.Take(KeySize \ 8).ToArray()
        Dim ivBytes() As Byte = cipherSaltIv.Skip(KeySize \ 8).Take(KeySize \ 8).ToArray()
        Dim cipherBytes() As Byte = cipherSaltIv.Skip((KeySize \ 8) * 2).Take(cipherSaltIv.Length - ((KeySize \ 8) * 2)).ToArray()

        Using key = New Rfc2898DeriveBytes(password, saltBytes, DerivationIterations)
            Dim keyBytes = key.GetBytes(KeySize \ 8)
            Using symmetricKey = New RijndaelManaged()
                symmetricKey.BlockSize = 256
                symmetricKey.Mode = CipherMode.CBC
                symmetricKey.Padding = PaddingMode.PKCS7
                Using decryptor = symmetricKey.CreateDecryptor(keyBytes, ivBytes)
                    Using memoryStream As New MemoryStream(cipherBytes)
                        Using cryptoStream As New CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)
                            Dim seedBytes(cipherBytes.Length - 1) As Byte
                            Dim decryptedByteCount = cryptoStream.Read(seedBytes, 0, seedBytes.Length)
                            memoryStream.Close()
                            cryptoStream.Close()
                            Return Encoding.UTF8.GetString(seedBytes, 0, decryptedByteCount)
                        End Using
                    End Using
                End Using
            End Using
        End Using
    End Function
End Class
