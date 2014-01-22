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
		//private string assemblyName = "System.Data.SQLite";
		//private string connectionClass = "System.Data.SQLite.SQLiteConnection";
		//private string connectionString = "Data Source=sqlite.db; Version=3;";
        private SqlMapper _dal = null;

        public Test_SqlContext()
		{
		
		}
		
		[SetUp]
		public void Setup()
		{
            _dal = new SqlMapper();
            SqlContext _ctx = _dal.CreateContext();

            SqlQuery _qry = new SqlQuery("create table if not exists test (id integer primary key, name text, date datetime)");

            _ctx.Insert(_qry);
		}
		
		[Test(Description="Create context")]
		public void TestCreateDefaultContext()
		{
            SqlContext _ctx = _dal.CreateContext();
		}
	}
}
