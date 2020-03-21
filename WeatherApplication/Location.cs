using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApplication
	{
	[Table("Locations")]
	public class Location
		{
        [PrimaryKey, AutoIncrement, Column("_id")]
		public int Id{get;set;}
		public string ZipCode {get;set;}
		public string Description {get;set;}
		}
	}
