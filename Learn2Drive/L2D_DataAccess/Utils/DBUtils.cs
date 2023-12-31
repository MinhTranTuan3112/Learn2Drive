﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace L2D_DataAccess.Utils;

/*
dotnet ef dbcontext scaffold "Server=(local);uid=sa;pwd=12345;database=DrivingLicense;TrustServerCertificate=true" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --force --project "D:\Dev\C#\Learn2Drive\L2D_DataAccess\L2D_DataAccess.csproj"
 */
public static class DBUtils
{
    public static SqlConnection getConnection()
    {
        const string connectionString = $@"Data Source={ServerName};Initial Catalog={databaseName};User ID={username};Password={password};TrustServerCertificate=True";
        SqlConnection connection = null;
        try
        {
            connection = new SqlConnection(connectionString);
        }
        catch (SqlException e)
        {
            Console.WriteLine($"Connection error: {e.Message}");
        }
        return connection;
    }
    public static SqlParameter CreateParameter(string name, int size, object value, DbType dbType,
    ParameterDirection direction = ParameterDirection.Input)
    {
        return new SqlParameter()
        {
            DbType = dbType,
            ParameterName = name,
            Size = size,
            Value = value,
            Direction = direction
        };
    }
    public static string getConnectionString()
    {
        //string connectionString = string.Empty;
        //IConfiguration config = new ConfigurationBuilder()
        //                        .SetBasePath(Directory.GetCurrentDirectory())
        //                        .AddJsonFile("appsettings.json", true, true)
        //                        .Build();
        //connectionString = config["ConnectionStrings:DrivingLicenseDB"];
        //return connectionString;
        return $@"Data Source={ServerName};Initial Catalog={databaseName};User ID={username};Password={password};TrustServerCertificate=True";
    }
    private const string ServerName = @"LAPTOP-MG9NF6QU\MSSQLSERVER01";
    private const string databaseName = "DrivingLicense";
    private const string username = "sa";
    private const string password = "12345";
}