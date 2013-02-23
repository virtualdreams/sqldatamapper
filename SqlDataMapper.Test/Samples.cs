using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SqlDataMapper;

namespace SqlDataMapper.Test
{
	[SqlMapperObjectDebug(false, typeof(Debug), "Reader")]
	public class TestTable
	{
		public long Id { get; set; }
		
		[SqlMapperAttributes("id")]
		public long OtherNameForId { get; set; }
		
		public string Name { get; set; }
		
		[SqlMapperAttributes("date", SqlMapperProperty.NotNull)]
		public DateTime Updated { get; set; }
		
		public string Content { get; set; }
		
		[SqlMapperAttributes("comments")]
		public string Comment { get; set; }
	}
	
	public class Samples
	{
		private readonly SqlMapper _dal;
		
		public Samples()
		{
			string assemblyName = "System.Data.SQLite";
			string connectionClass = "System.Data.SQLite.SQLiteConnection";
			string connectionString = "Data Source=sqlite.db; Version=3;";
			
			ISqlProvider provider = new SqlProvider(assemblyName, connectionClass, connectionString);
			
			_dal = new SqlMapper(provider);
		}
		
		public int CreateTableIfNotExists()
		{
			_dal.AddStatement("createTable", "CREATE TABLE IF NOT EXISTS test (id INTEGER PRIMARY KEY, name TEXT, date DATETIME, content TEXT, contentid INTEGER, comments TEXT);");
			
			int res = 0;
			res = _dal.Update(_dal.CreateQuery("createTable"));
			return res;
		}
		
		public int Insert()
		{
			_dal.AddStatement("insertData", "insert into test (name, date, content, contentid, comments) values(#name#, #date#, #content#, #contentid#, #comments#)");
			
			SqlQuery query = _dal.CreateQuery("insertData");
			query.SetDateTime("date", DateTime.Now).SetEntity("name", "Test").SetEntity("contentid", 10).SetEntity("content", null).SetEntity("comments", null);
			
			int res = 0;
			res = _dal.Insert(query);
			return res;
		}
		
		public int Update()
		{
			SqlQuery query = new SqlQuery("update test set content = #content#, comments = #comments#, date = #date# where contentid = 10");
			
			SqlParameter paramter = new SqlParameter();
			paramter.Add("content", "some new content");
			paramter.Add("comments", "refresh comment");
			paramter.Add("date", DateTime.Now.AddDays(10));
			
			query.SetEntities(paramter);
			
			int res = 0;
			res = _dal.Update(query);
			return res;
		}
		
		public int Get()
		{
			SqlQuery query = SqlQuery.CreateQuery("select * from test");
			
			List<TestTable> table = _dal.QueryForList<TestTable>(query);
			
			return table.Count;	
		}
		
		public int Dynamic()
		{
			string[] terms = { "Test", "Hello", "World" };
			
			_dal.AddStatement("searchBase", "select * from test where name like #name#");
			_dal.AddStatement("searchFrag", "or name like #name#");
			
			SqlQuery query = _dal.CreateQuery("searchBase");
			
			int len = 0;
			foreach(string term in terms)
			{
				if(len > 0)
					query.Add(_dal.CreateQuery("searchFrag"));
				query.SetEntity("name", term);
				len++;
			}
			
			List<TestTable> table = _dal.QueryForList<TestTable>(query);
			
			return table.Count;
		}
		
		public void UnknownParameter()
		{
			SqlQuery query = new SqlQuery("select * from test where name = #name#").SetString("nmae", "Text");
		}
		
		public void Overload()
		{
			SqlQuery query1 = SqlQuery.CreateQuery("select * from test");
			SqlQuery query2 = query1 + "where name like 'Hello'";
			SqlQuery query3 = query1;
			query3 += "where name like 'Hello'";
			SqlQuery query4 = query1 + query2;
			SqlQuery query5 = new SqlQuery(query2);
			
			Console.WriteLine("query1: " + query1.QueryString);
			Console.WriteLine("query2: (+) " + query2.QueryString);
			Console.WriteLine("query3: (+=) " + query3.QueryString);
			Console.WriteLine("query4: (+) " + query4.QueryString);
			Console.WriteLine("query5: (ctor) " + query5.QueryString);
			
			Console.WriteLine("ToString(): " + query1.ToString());
		}
		
		public void Exception()
		{
			_dal.ParameterCheck = true;
			_dal.AddStatement("exception", "select * from table where contentid = #contentid#");
			
			_dal.QueryForList<TestTable>(_dal.CreateQuery("exception"));
		}
	}

	public static class Debug
	{
		public static void Reader(System.Data.Common.DbDataReader reader)
		{
			for (int i = 0; i < reader.FieldCount; ++i)
			{
				Console.WriteLine(String.Format("Name: {0}\tType: {1}\tValue: {2}", reader.GetName(i), reader[i].GetType(), reader[i]));
			}
			Console.WriteLine();
		}
	}
}
