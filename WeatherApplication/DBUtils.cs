using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApplication
	{
	public static class DBUtils
		{
		private static SQLiteAsyncConnection conn;

		public static SQLiteAsyncConnection Conn
			{
			get
				{
				if(conn == null)
					{
					conn = new SQLiteAsyncConnection("UserDB");
					}

				return conn;
				}
			}
		
		}
	}
