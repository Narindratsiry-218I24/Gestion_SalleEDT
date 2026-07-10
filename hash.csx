using System;
using System.Security.Cryptography;
using System.Text;

var password = "password123";
var bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password + "EMIT_SALT_2026"));
Console.WriteLine(Convert.ToBase64String(bytes));
