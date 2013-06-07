using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace SqlDataMapper.Test
{
	[TestFixture]
	class Test_SqlMapper
	{
		//private string assemblyName = "System.Data.SQLite";
		//private string connectionClass = "System.Data.SQLite.SQLiteConnection";
		//private string connectionString = "Data Source=sqlite.db; Version=3;";
		
		public Test_SqlMapper()
		{
		
		}
		
		[SetUp]
		public void Setup()
		{
		
		}
		
		[Test(Description="Create zero config instance.")]
		public void TestCreateDefaultInstance()
		{
			SqlMapper mapper = new SqlMapper();
		}
		
		//[Test(Description="Create custom empty instance")]
		//public void TestCreateCustomEmptyInstance()
		//{
		//    ISqlProvider provider = new SqlProvider(assemblyName, connectionClass, connectionString);
		//    SqlMapper mapper = new SqlMapper(provider);
		//}
		
		//[Test(Description="Create custom instance")]
		//public void TestCreateCustomInstance()
		//{
		//    ISqlProvider provider = new SqlProvider(assemblyName, connectionClass, connectionString);
		//    SqlMapper mapper = new SqlMapper(provider, "./query.xml");
		//}
		
		[Test(Description="Create instance out of custom config")]
		public void TestCreateCunstomInstanceFromConfig()
		{
			SqlMapper mapper = new SqlMapper("./SqlMapperConfig.xml");
		}
		
		[Test(Description="Add statements to object")]
		public void TestAddStatements()
		{
			SqlMapper mapper = new SqlMapper();
			mapper.AddStatement("select1", "select * from test");
			mapper.AddStatement("select2", "select * from value");
			
			Assert.AreEqual("select * from test", mapper.GetStatement("select1"));
			Assert.AreEqual("select * from value", mapper.GetStatement("select2"));
		}
		
		[Test(Description="Throw exception on multiple id")]
		[ExpectedException(ExpectedException=typeof(SqlDataMapperException))]
		public void TestAddStatementThrowException1()
		{
			SqlMapper mapper = new SqlMapper();
			mapper.AddStatement("select1", "select * from test");
			mapper.AddStatement("select1", "select * from value");
		}

		[Test(Description = "Throw exception on multiple id. id is loaded from xml")]
		[ExpectedException(ExpectedException = typeof(SqlDataMapperException))]
		public void TestAddStatementThrowException2()
		{
			SqlMapper mapper = new SqlMapper();
			mapper.AddStatement("test", "select * from test");
		}

		[Test(Description = "Throw exception if id not found")]
		[ExpectedException(ExpectedException = typeof(SqlDataMapperException))]
		public void TestGetStatementRawThrowException()
		{
			SqlMapper mapper = new SqlMapper();
			string query = mapper.GetStatement("select1");
		}

		[Test(Description = "Create sql query from pool")]
		public void TestCreateQuery()
		{
			SqlMapper mapper = new SqlMapper();
			SqlQuery query = mapper.CreateQuery("test");
			
			Assert.AreEqual("select * from test", query.QueryString);
		}
	}
}
