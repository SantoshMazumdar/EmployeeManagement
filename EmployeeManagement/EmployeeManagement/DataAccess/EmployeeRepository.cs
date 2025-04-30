using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using EmployeeManagement.Models;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace EmployeeManagement.DataAccess
{
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["EmployeeDB"].ConnectionString;
        }

        public List<Employee> GetAllEmployees()
        {
            var employees = new List<Employee>();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("GetAllEmployees", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string mobileNum = Decrypt(reader["Mobile"].ToString());
                        string Email = Decrypt(reader["Email"].ToString());
                        employees.Add(new Employee
                        {
                            EmpId = Convert.ToInt32(reader["EmpId"]),
                            Name = reader["Name"].ToString(),
                            DOB = Convert.ToDateTime(reader["DOB"]),
                            Mobile = mobileNum,
                            Email = Email,
                            Address = reader["Address"]?.ToString()
                        });
                    }
                }
            }

            return employees;
        }

        public Employee GetEmployeeById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("GetEmployeeById", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@EmpId", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string mobileNum = Decrypt(reader["Mobile"].ToString());
                        string Email = Decrypt(reader["Email"].ToString());
                        return new Employee
                        {
                            EmpId = Convert.ToInt32(reader["EmpId"]),
                            Name = reader["Name"].ToString(),
                            DOB = Convert.ToDateTime(reader["DOB"]),
                            Mobile = mobileNum,
                            Email = Email,
                            Address = reader["Address"]?.ToString()
                        };
                    }
                }
            }
            return null;
        }

        

       
        public void AddEmployee(Employee employee)
        {
            string mobileNum = Encrypt(employee.Mobile);
            string Email = Encrypt(employee.Email);

            string test = Decrypt(mobileNum);
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("InsertEmployee", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Name", employee.Name);
                command.Parameters.AddWithValue("@DOB", employee.DOB);
                command.Parameters.AddWithValue("@Mobile", mobileNum);
                command.Parameters.AddWithValue("@Email", Email);
                command.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateEmployee(Employee employee)
        {
            string mobileNum = Encrypt(employee.Mobile);
            string Email = Encrypt(employee.Email);
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("UpdateEmployee", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@EmpId", employee.EmpId);
                command.Parameters.AddWithValue("@Name", employee.Name);
                command.Parameters.AddWithValue("@DOB", employee.DOB);
                command.Parameters.AddWithValue("@Mobile", mobileNum);
                command.Parameters.AddWithValue("@Email", Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteEmployee(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("DeleteEmployee", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@EmpId", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        #region AES Encryption
        private static string key = "A3S9f1Z4g6H8d3K9"; // 16 bytes
        private static string iv = "1Hbfh667adfDEJ78"; // 16 bytes

        public static string Encrypt(string plainText)
        {
            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = Encoding.UTF8.GetBytes(iv);
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.FlushFinalBlock();
                        }

                        byte[] encryptedBytes = ms.ToArray();
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error encrypting: {ex.Message}";
            }
        }

        public static string Decrypt(string encryptedText)
        {
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = Encoding.UTF8.GetBytes(iv);
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                            cs.FlushFinalBlock();
                        }

                        byte[] decryptedBytes = ms.ToArray();
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error decrypting: {ex.Message}";
            }
        }
        ////private static string key = "A3S9f1Z4g6H8d3K9";
        ////private static string iv = "1Hbfh667adfDEJ78";

        //private static string key = "1234567890123456"; 
        //private static string iv = "abcdefghijklmnop";
        //public static string Encrypt(string plainText)
        //{
        //    try
        //    {
        //        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        //        using (Aes aes = Aes.Create())
        //        {
        //            aes.Key = Encoding.UTF8.GetBytes(key);
        //            aes.IV = Encoding.UTF8.GetBytes(iv);

        //            using (MemoryStream ms = new MemoryStream())
        //            {
        //                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        //                {
        //                    cs.Write(plainBytes, 0, plainBytes.Length);
        //                    cs.Close();
        //                }

        //                byte[] encryptedBytes = ms.ToArray();
        //                return Convert.ToBase64String(encryptedBytes);
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return "Error encrypting!";
        //    }
        //}

        //public static string Decrypt(string encryptedText)
        //{
        //    try
        //    {
        //        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        //        using (Aes aes = Aes.Create())
        //        {
        //            aes.Key = Encoding.UTF8.GetBytes(key);
        //            aes.IV = Encoding.UTF8.GetBytes(iv);

        //            using (MemoryStream ms = new MemoryStream())
        //            {
        //                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
        //                {
        //                    cs.Write(encryptedBytes, 0, encryptedBytes.Length);
        //                    cs.Close();
        //                }

        //                byte[] decryptedBytes = ms.ToArray();
        //                return Encoding.UTF8.GetString(decryptedBytes);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return "Error decrypting!";
        //    }
        //}
        #endregion
    }
}