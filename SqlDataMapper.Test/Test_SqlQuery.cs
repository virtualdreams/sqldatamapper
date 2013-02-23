using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace SqlDataMapper.Test
{
	class MockObject
	{
		public string name { get; set; }
		public string value { get; set; }
	}

	[TestFixture]
	public class Test_SqlQuery
	{
		public Test_SqlQuery()
		{
		
		}
		
		[SetUp]
		public void Setup()
		{
		
		}
		
		[Test(Description="Create a empty sql qury object")]
		public void TestCreateEmptySqlQuery()
		{
			SqlQuery query = new SqlQuery();
			
			Assert.AreEqual("", query.QueryString);
		}
		
		[Test(Description="Initialize sql query with a custom query")]
		public void TestCreateDefaultSqlQuery()
		{
			SqlQuery query1 = new SqlQuery("select * from test");
			SqlQuery query2 = SqlQuery.CreateQuery("select * from test");
			
			Assert.AreEqual("select * from test", query1.QueryString);
			Assert.AreEqual("select * from test", query2.QueryString);
		}
		
		[Test(Description="Add queries, but don't affect the instances itself")]
		public void TestAdd()
		{
			SqlQuery query1 = new SqlQuery("select * from test");
			SqlQuery query2 = new SqlQuery("where t = 1");
			
			SqlQuery result1 = query1 + query2; //create 1 new instance
			SqlQuery result2 = new SqlQuery(query1).Add(query2); //new creates a new instance and add creates a new instance
			SqlQuery result3 = query1;
			result3 += query2;
			SqlQuery result4 = query1.Add(query2);

			Assert.AreEqual("select * from test where t = 1", result1.QueryString);
			Assert.AreEqual("select * from test where t = 1", result2.QueryString);
			Assert.AreEqual("select * from test where t = 1", result3.QueryString);
			Assert.AreEqual("select * from test where t = 1", result4.QueryString);
			
			Assert.AreEqual("select * from test", query1.QueryString);
			Assert.AreEqual("where t = 1", query2.QueryString);
		}
		
		[Test(Description="Append a query to the instance")]
		public void TestAppend()
		{
			SqlQuery query1 = new SqlQuery("select * from test");
			SqlQuery query2 = new SqlQuery("where t = 1");
			
			SqlQuery result1 = query1.Append(query2);
			
			Assert.AreEqual("select * from test where t = 1", query1.QueryString);
			Assert.AreEqual("select * from test where t = 1", result1.QueryString);
		}
		
		[Test(Description="ToString() is overriden and return the sql query")]
		public void TestToString()
		{
			SqlQuery query1 = new SqlQuery("select * from test");
			SqlQuery query2 = new SqlQuery(query1).Append("where t = 1");
			
			Assert.AreEqual("select * from test", query1.ToString());
			Assert.AreEqual("select * from test where t = 1", query2.ToString());
		}
		
		[Test(Description="Replace named parameters")]
		public void TestParameterSubstitute()
		{
			SqlQuery query1 = new SqlQuery("select * from test where t = #name#");
			SqlQuery query2 = new SqlQuery(query1);
			SqlQuery query3 = new SqlQuery("select * from test where t in (#name#)");
			SqlQuery query4 = new SqlQuery(query3);
			SqlQuery query5 = new SqlQuery(query1);
			
			query1.SetString("name", "1");
			query2.SetEntity("name", 1);
			query3.SetEntity("name", new string[] {"Test1", "Test2", "Test3"});
			query4.SetEntity("name", new int[] {1, 2, 3});
			query5.SetEntity("name", null);
			
			Assert.AreEqual("select * from test where t = '1'", query1.QueryString);
			Assert.AreEqual("select * from test where t = 1", query2.QueryString);
			Assert.AreEqual("select * from test where t in ('Test1', 'Test2', 'Test3')", query3.QueryString);
			Assert.AreEqual("select * from test where t in (1, 2, 3)", query4.QueryString);
			Assert.AreEqual("select * from test where t = null", query5.QueryString);
		}
		
		[Test(Description="Replace parameters, but now binary arrays and DateTime")]
		public void TestParameterSubstituteSpecial()
		{
			SqlQuery query1 = new SqlQuery("select * from test where t = #name#");
			SqlQuery query2 = new SqlQuery(query1);
			SqlQuery query3 = new SqlQuery(query1);
			SqlQuery query4 = new SqlQuery(query1);
			
			byte[] binary1 = new byte[] {0xDE, 0xAD, 0xBE, 0xEF};
			byte[] binary2 = new byte[] { 0xDE, 0xAD, 0xC0, 0xDE };

			query1.SetDateTime("name", new DateTime(2012, 12, 12));
			query2.SetEntity("name", new DateTime(2012, 12, 12, 6, 34, 22));
			query3.SetBinary("name", binary1);
			query4.SetEntity("name", binary2);
			
			Assert.AreEqual("select * from test where t = '2012-12-12 00:00:00'", query1.QueryString);
			Assert.AreEqual("select * from test where t = '2012-12-12 06:34:22'", query2.QueryString);
			Assert.AreEqual("select * from test where t = 0xDEADBEEF", query3.QueryString);
			Assert.AreEqual("select * from test where t = 0xDEADC0DE", query4.QueryString);
		}
		
		[Test(Description="Try replace a named parameter, but it throws an exception")]
		[ExpectedException(ExpectedException=typeof(SqlDataMapperException))]
		public void TestNamedParameterNotFoundException()
		{
			SqlQuery query1 = new SqlQuery("select * from test where t = #name#");
			SqlQuery query2 = new SqlQuery(query1).Add("and x = #value#");
			SqlQuery query3 = new SqlQuery(query1);

			query1.SetString("nmae", "Test");
			query2.SetString("valeu", "Test");
			query3.SetEntity("nmae", new string[] { "Test1", "Test2", "Test3" });
		}

		[Test(Description="Try replace a named parameter, but it throws not an exception. This is disabled for objects and SqlParameter")]
		public void TestNamedParameterNoNotFoundException()
		{
			SqlParameter param = new SqlParameter();
			param.Add("name", "Test");
			param.Add("value", "Test"); //this parameter can't found in the query, but it throws no excepetion

			SqlQuery query1 = new SqlQuery("select * from test where t = #name#");
			SqlQuery query2 = new SqlQuery(query1);

			MockObject t = new MockObject();
			t.name = "Test";
			t.value = "Test"; //this parameter can't found in the query, but it throws no excepetion

			query1.SetEntities(param);
			query2.SetEntities<MockObject>(t);
		}
		
		[Test(Description="Check for unresolved named parameters and throw an exception if found one or more")]
		[ExpectedException(ExpectedException=typeof(SqlDataMapperException))]
		public void TestUnresolvedNamedParameters()
		{
			SqlQuery query1 = new SqlQuery("select * from test where t = #name#");
			SqlQuery query2 = new SqlQuery(query1).Add("and x = #value#");

			query1.Check();
			query2.Check();
		}
		
		[Test(Description="Initialize a query and replace the content with a new query")]
		public void TestSet()
		{
			SqlQuery query = new SqlQuery("select * from test");
			
			query.Set("select * from values");
			
			Assert.AreEqual("select * from values", query.QueryString);		
		}
		
		[Test(Description="SqlQuery has a build in sql autoformatter...")]
		public void TestAutoformatter()
		{
			string queryRaw1 = "select\n  * \n    from\ntest\n--\twhere t = 1\n\r  where v = /* v is not null,\nmust be one.. */  1/* and x  =   \t'100' */order by v  ";
			string queryRaw2 = " \t\n,   x\tdesc  ";
			
			SqlQuery query1 = new SqlQuery(queryRaw1);
			SqlQuery query2 = query1.Add(queryRaw2);
			
			Assert.AreEqual("select * from test where v = 1 order by v", query1.QueryString);
			Assert.AreEqual("select * from test where v = 1 order by v , x desc", query2.QueryString);
		}
	}
}
