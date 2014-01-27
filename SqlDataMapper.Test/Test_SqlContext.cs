using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace SqlDataMapper.Test
{
	[TestFixture]
	class Test_SqlContext
	{
        private string assemblyName = "System.Data.SQLite";
        private string connectionClass = "System.Data.SQLite.SQLiteConnection";
        private string connectionString1 = "Data Source=sqlite.db; Version=3;";
        private string connectionString2 = "Server=jus-as-lit03;Database=svweb;User Id=svweb;Password=svweb;";
        private SqlConfig _dal = null;

        public Test_SqlContext()
		{
		
		}
		
		[SetUp]
		public void Setup()
		{
            _dal = new SqlConfig();
		}
		
		[Test(Description="Create context from config")]
		public void TestCreateDefaultContextFromConfig()
		{
            SqlContext _ctx = _dal.CreateContext();
        }

        [Test(Description = "Create context from provider")]
        public void TestCreateDefaultContextFromProvider()
        {
            SqlContext _ctx = _dal.CreateContext("mssql2.0", connectionString2);
        }

        [Test(Description = "Create context from string")]
        public void TestCreateDefaultContextFromString()
        {
            SqlContext _ctx = new SqlContext(assemblyName, connectionClass, connectionString1);
        }
	}
}
