using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApplication
	{

namespace Daily
	{
	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class weatherdata
		{

		private weatherdataLocation locationField;

		private object creditField;

		private weatherdataMeta metaField;

		private weatherdataSun sunField;

		private weatherdataTime[] forecastField;

		/// <remarks/>
		public weatherdataLocation location
			{
			get
				{
				return this.locationField;
				}
			set
				{
				this.locationField = value;
				}
			}

		/// <remarks/>
		public object credit
			{
			get
				{
				return this.creditField;
				}
			set
				{
				this.creditField = value;
				}
			}

		/// <remarks/>
		public weatherdataMeta meta
			{
			get
				{
				return this.metaField;
				}
			set
				{
				this.metaField = value;
				}
			}

		/// <remarks/>
		public weatherdataSun sun
			{
			get
				{
				return this.sunField;
				}
			set
				{
				this.sunField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("time", IsNullable = false)]
		public weatherdataTime[] forecast
			{
			get
				{
				return this.forecastField;
				}
			set
				{
				this.forecastField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataLocation
		{

		private string nameField;

		private object typeField;

		private string countryField;

		private object timezoneField;

		private weatherdataLocationLocation locationField;

		/// <remarks/>
		public string name
			{
			get
				{
				return this.nameField;
				}
			set
				{
				this.nameField = value;
				}
			}

		/// <remarks/>
		public object type
			{
			get
				{
				return this.typeField;
				}
			set
				{
				this.typeField = value;
				}
			}

		/// <remarks/>
		public string country
			{
			get
				{
				return this.countryField;
				}
			set
				{
				this.countryField = value;
				}
			}

		/// <remarks/>
		public object timezone
			{
			get
				{
				return this.timezoneField;
				}
			set
				{
				this.timezoneField = value;
				}
			}

		/// <remarks/>
		public weatherdataLocationLocation location
			{
			get
				{
				return this.locationField;
				}
			set
				{
				this.locationField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataLocationLocation
		{

		private byte altitudeField;

		private decimal latitudeField;

		private decimal longitudeField;

		private string geobaseField;

		private uint geobaseidField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public byte altitude
			{
			get
				{
				return this.altitudeField;
				}
			set
				{
				this.altitudeField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal latitude
			{
			get
				{
				return this.latitudeField;
				}
			set
				{
				this.latitudeField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal longitude
			{
			get
				{
				return this.longitudeField;
				}
			set
				{
				this.longitudeField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string geobase
			{
			get
				{
				return this.geobaseField;
				}
			set
				{
				this.geobaseField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public uint geobaseid
			{
			get
				{
				return this.geobaseidField;
				}
			set
				{
				this.geobaseidField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataMeta
		{

		private object lastupdateField;

		private decimal calctimeField;

		private object nextupdateField;

		/// <remarks/>
		public object lastupdate
			{
			get
				{
				return this.lastupdateField;
				}
			set
				{
				this.lastupdateField = value;
				}
			}

		/// <remarks/>
		public decimal calctime
			{
			get
				{
				return this.calctimeField;
				}
			set
				{
				this.calctimeField = value;
				}
			}

		/// <remarks/>
		public object nextupdate
			{
			get
				{
				return this.nextupdateField;
				}
			set
				{
				this.nextupdateField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataSun
		{

		private System.DateTime riseField;

		private System.DateTime setField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public System.DateTime rise
			{
			get
				{
				return this.riseField;
				}
			set
				{
				this.riseField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public System.DateTime set
			{
			get
				{
				return this.setField;
				}
			set
				{
				this.setField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataTime
		{

		private weatherdataTimeSymbol symbolField;

		private weatherdataTimePrecipitation precipitationField;

		private weatherdataTimeWindDirection windDirectionField;

		private weatherdataTimeWindSpeed windSpeedField;

		private weatherdataTimeTemperature temperatureField;

		private weatherdataTimePressure pressureField;

		private weatherdataTimeHumidity humidityField;

		private weatherdataTimeClouds cloudsField;

		private System.DateTime dayField;

		/// <remarks/>
		public weatherdataTimeSymbol symbol
			{
			get
				{
				return this.symbolField;
				}
			set
				{
				this.symbolField = value;
				}
			}

		/// <remarks/>
		public weatherdataTimePrecipitation precipitation
			{
			get
				{
				return this.precipitationField;
				}
			set
				{
				this.precipitationField = value;
				}
			}

		/// <remarks/>
		public weatherdataTimeWindDirection windDirection
			{
			get
				{
				return this.windDirectionField;
				}
			set
				{
				this.windDirectionField = value;
				}
			}

		/// <remarks/>
		public weatherdataTimeWindSpeed windSpeed
			{
			get
				{
				return this.windSpeedField;
				}
			set
				{
				this.windSpeedField = value;
				}
			}

		/// <remarks/>
		public weatherdataTimeTemperature temperature
			{
			get
				{
				return this.temperatureField;
				}
			set
				{
				this.temperatureField = value;
				}
			}

		/// <remarks/>
		public weatherdataTimePressure pressure
			{
			get
				{
				return this.pressureField;
				}
			set
				{
				this.pressureField = value;
				}
			}

		/// <remarks/>
		public weatherdataTimeHumidity humidity
			{
			get
				{
				return this.humidityField;
				}
			set
				{
				this.humidityField = value;
				}
			}

		/// <remarks/>
		public weatherdataTimeClouds clouds
			{
			get
				{
				return this.cloudsField;
				}
			set
				{
				this.cloudsField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
		public System.DateTime day
			{
			get
				{
				return this.dayField;
				}
			set
				{
				this.dayField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataTimeSymbol
		{

		private ushort numberField;

		private string nameField;

		private string varField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ushort number
			{
			get
				{
				return this.numberField;
				}
			set
				{
				this.numberField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string name
			{
			get
				{
				return this.nameField;
				}
			set
				{
				this.nameField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string var
			{
			get
				{
				return this.varField;
				}
			set
				{
				this.varField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataTimePrecipitation
		{

		private decimal valueField;

		private bool valueFieldSpecified;

		private string typeField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal value
			{
			get
				{
				return this.valueField;
				}
			set
				{
				this.valueField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool valueSpecified
			{
			get
				{
				return this.valueFieldSpecified;
				}
			set
				{
				this.valueFieldSpecified = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string type
			{
			get
				{
				return this.typeField;
				}
			set
				{
				this.typeField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataTimeWindDirection
		{

		private ushort degField;

		private string codeField;

		private string nameField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ushort deg
			{
			get
				{
				return this.degField;
				}
			set
				{
				this.degField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string code
			{
			get
				{
				return this.codeField;
				}
			set
				{
				this.codeField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string name
			{
			get
				{
				return this.nameField;
				}
			set
				{
				this.nameField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataTimeWindSpeed
		{

		private decimal mpsField;

		private string nameField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal mps
			{
			get
				{
				return this.mpsField;
				}
			set
				{
				this.mpsField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string name
			{
			get
				{
				return this.nameField;
				}
			set
				{
				this.nameField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataTimeTemperature
		{

		private decimal dayField;

		private decimal minField;

		private decimal maxField;

		private decimal nightField;

		private decimal eveField;

		private decimal mornField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal day
			{
			get
				{
				return this.dayField;
				}
			set
				{
				this.dayField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal min
			{
			get
				{
				return this.minField;
				}
			set
				{
				this.minField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal max
			{
			get
				{
				return this.maxField;
				}
			set
				{
				this.maxField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal night
			{
			get
				{
				return this.nightField;
				}
			set
				{
				this.nightField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal eve
			{
			get
				{
				return this.eveField;
				}
			set
				{
				this.eveField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal morn
			{
			get
				{
				return this.mornField;
				}
			set
				{
				this.mornField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataTimePressure
		{

		private string unitField;

		private decimal valueField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string unit
			{
			get
				{
				return this.unitField;
				}
			set
				{
				this.unitField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal value
			{
			get
				{
				return this.valueField;
				}
			set
				{
				this.valueField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataTimeHumidity
		{

		private byte valueField;

		private string unitField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public byte value
			{
			get
				{
				return this.valueField;
				}
			set
				{
				this.valueField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string unit
			{
			get
				{
				return this.unitField;
				}
			set
				{
				this.unitField = value;
				}
			}
		}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class weatherdataTimeClouds
		{

		private string valueField;

		private byte allField;

		private string unitField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string value
			{
			get
				{
				return this.valueField;
				}
			set
				{
				this.valueField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public byte all
			{
			get
				{
				return this.allField;
				}
			set
				{
				this.allField = value;
				}
			}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string unit
			{
			get
				{
				return this.unitField;
				}
			set
				{
				this.unitField = value;
				}
			}
		}

		}
	}
