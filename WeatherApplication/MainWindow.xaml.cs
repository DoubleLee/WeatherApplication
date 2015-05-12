using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;

namespace WeatherApplication
	{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public class ForecastDay
		{
		// references for each forecasted day.
		public Image forecastImage;
		public Label dateLabel;
		public Label labelHighTemp;
		public Label labelLowTemp;

		public ForecastDay( Image forecastImage, Label dateLabel, Label labelHighTemp, Label labelLowTemp )
			{
			this.forecastImage = forecastImage;
			this.dateLabel = dateLabel;
			this.labelHighTemp = labelHighTemp;
			this.labelLowTemp = labelLowTemp;
			}
		}

	public class ForecastHours
		{
		public Label hourRange;
		public Label temperature;
		public Label precipitation;
		public Label windSpeed;

		public ForecastHours( Label hourRange, Label temperature, Label precipitation, Label windSpeed )
			{
			this.hourRange = hourRange;
			this.temperature = temperature;
			this.precipitation = precipitation;
			this.windSpeed = windSpeed;
			}
		}

	public partial class MainWindow : Window
		{
		ForecastDay[] forecastDays;
		ForecastHours[] forecastHours;
		DispatcherTimer weatherUpdateTimer;
		DispatcherTimer clockUpdateTimer;

		DateTime lastDateTime;

		public DateTime CurrentDisplayedDate
			{
			get
				{
				return lastDateTime;
				}
			set
				{
				if ( !DateTime.Equals(lastDateTime, value) && value.Hour != lastDateTime.Hour || value.Minute != lastDateTime.Minute )
					{
					labelTimeString.Content = value; // this line is why there is a property, to save the ToString() call on the DateTime, unless really neccessary.
					lastDateTime = value;
					}
				}
			}

		public MainWindow()
			{
			InitializeComponent();
			
			// setup the weather update timer. Runs every 10 minutes.
			weatherUpdateTimer = new DispatcherTimer();
			weatherUpdateTimer.Interval = new TimeSpan(0, 0, 10, 0);
			weatherUpdateTimer.Tick += UpdateWeather;
			weatherUpdateTimer.Start();

			// setup the clock update timer. Runs every 1 second.
			clockUpdateTimer = new DispatcherTimer();
			clockUpdateTimer.Interval = new TimeSpan(0, 0, 0, 1);
			clockUpdateTimer.Tick += UpdateClockString;
			clockUpdateTimer.Start();

			// Setup the array of forecast days, the references come from the main window's controls.
			// We use this array for easy iteration when updating forecasts.
			forecastDays = new ForecastDay[]
				{ 
				new ForecastDay(imageDay1, labelDate1, labelDayHigh1, labelDayLow1),
				new ForecastDay(imageDay2, labelDate2, labelDayHigh2, labelDayLow2),
				new ForecastDay(imageDay3, labelDate3, labelDayHigh3, labelDayLow3),
				new ForecastDay(imageDay4, labelDate4, labelDayHigh4, labelDayLow4),
				new ForecastDay(imageDay5, labelDate5, labelDayHigh5, labelDayLow5)
				};

			forecastHours = new ForecastHours[]
				{
				new ForecastHours(labelHourRange1, labelTemp1, labelPrecipitation1, labelWind1),
				new ForecastHours(labelHourRange2, labelTemp2, labelPrecipitation2, labelWind2),
				new ForecastHours(labelHourRange3, labelTemp3, labelPrecipitation3, labelWind3),
				new ForecastHours(labelHourRange4, labelTemp4, labelPrecipitation4, labelWind4),
				new ForecastHours(labelHourRange5, labelTemp5, labelPrecipitation5, labelWind5),
				};

			// call the update weather methods now.
			UpdateWeather(null, null);
			// call the clock update method now.
			UpdateClockString(null, null);
			}

		public void UpdateWeather( object sender, EventArgs args )
			{
			UpdateCurrentWeather();
			UpdateForecast();
			UpdateHourlyForecast();
			UpdateLastApplicationUpdate();
			}

		/// <summary>
		/// Updates the current weather by calling the openweathermap api.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public async void UpdateCurrentWeather()
			{
			try
				{
				labelErrors.Content = "Status working...";
				// The url contains at the end options for the call, including my unique api key, as well as the type of api call and any other settings.
				WebRequest request = WebRequest.Create("http://api.openweathermap.org/data/2.5/weather?id=4409896&MODE=XML&APPID=930964919a915aefc90d0d5e3b0f4bd2");
				WebResponse response = await request.GetResponseAsync();
				Stream dataStream = response.GetResponseStream();

				var xdoc = XDocument.Load( dataStream );

				var weatherNode = xdoc.Root;

				// Get City
				string city = weatherNode.XPathSelectElement("city").Attribute("name").Value;
				labelCity.Content = city;

				// Get temperature and convert to ferenheit
				var tempValue = weatherNode.XPathSelectElement("temperature").Attribute("value");
				float kelvin = float.Parse(tempValue.Value);
				float ferenheit = KelvinToFerenheit(kelvin);
				labelTemp.Content = ferenheit.ToString("F0");

				// get current weather description string
				string currentWeather = weatherNode.XPathSelectElement("weather").Attribute("value").Value;
				labelCurrentWeather.Content = currentWeather;

				// get humidity
				string humidity = weatherNode.XPathSelectElement("humidity").Attribute("value").Value;
				string humidityUnitString = weatherNode.XPathSelectElement("humidity").Attribute("unit").Value;
				labelHumidity.Content = humidity + humidityUnitString;

				// get pressure
				var pressureNode = weatherNode.XPathSelectElement("pressure");
				string pressure = pressureNode.Attribute("value").Value;
				string pressureUnitString = pressureNode.Attribute("unit").Value;
				labelPressure.Content = pressure + pressureUnitString;

				// get last update of the open weather api as a string
				string lastUpdate = weatherNode.XPathSelectElement("lastupdate").Attribute("value").Value;

				// parse the last api update string to a datetime struct.
				DateTime dateTime = DateTime.Parse(lastUpdate);
				// tell the struct that it is UTC time.
				dateTime = DateTime.SpecifyKind( dateTime, DateTimeKind.Utc);

				// convert UTC time to local time and parse it to string and set the label.
				labelLastWeatherApiUpdate.Content = dateTime.ToLocalTime().ToLongTimeString();

				// load the current weather image icon.
				string weatherIconString = weatherNode.XPathSelectElement("weather").Attribute("icon").Value;
				LoadWeatherIcon( imageCurrentWeather, weatherIconString );
				LoadIcon( weatherIconString );

				labelErrors.Content = "Status good.";
				}
			catch( Exception e )
				{
				labelErrors.Content = String.Format("Error: [{0}]\nAt: [{1}]", e.Message, DateTime.Now.ToLongTimeString());
				}
			}

		/// <summary>
		/// Updates the 5 day forecast data from the open weather map api forecast.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public async void UpdateForecast()
			{
			try
				{
				labelErrors.Content = "Status working...";
				WebRequest request = WebRequest.Create("http://api.openweathermap.org/data/2.5/forecast/daily?id=4409896&MODE=XML&APPID=930964919a915aefc90d0d5e3b0f4bd2");
				WebResponse response = await request.GetResponseAsync();
				Stream dataStream = response.GetResponseStream();

				var xdoc = XDocument.Load( dataStream );

				var forecastNode = xdoc.Root.XPathSelectElement("forecast");

				int index = 0;
				int maxIndex = 5;

				// use the forecast days array to update the forecast data.
				foreach ( XElement element in forecastNode.Elements() )
					{
					if ( index >= maxIndex )
						break;

					forecastDays[index].dateLabel.Content = element.Attribute("day").Value;

					string id = element.XPathSelectElement("symbol").Attribute("var").Value;
					LoadWeatherIcon( forecastDays[index].forecastImage, id );
					var tempNode = element.XPathSelectElement("temperature");
					forecastDays[index].labelHighTemp.Content = "H " + KelvinToFerenheit(float.Parse(tempNode.Attribute("max").Value)).ToString("F0");
					forecastDays[index].labelLowTemp.Content = "L " + KelvinToFerenheit(float.Parse(tempNode.Attribute("min").Value)).ToString("F0");;

					++index;
					}
				labelErrors.Content = "Status good.";
				}
			catch( Exception e )
				{
				labelErrors.Content = String.Format("Error: [{0}]\nAt: [{1}]", e.Message, DateTime.Now.ToLongTimeString());
				}
			}

		public async void UpdateHourlyForecast()
			{
			try
				{
				labelErrors.Content = "Status working...";
				WebRequest request = WebRequest.Create("http://api.openweathermap.org/data/2.5/forecast?id=4409896&MODE=XML&APPID=930964919a915aefc90d0d5e3b0f4bd2");
				WebResponse response = await request.GetResponseAsync();
				Stream dataStream = response.GetResponseStream();

				var xdoc = XDocument.Load( dataStream );

				var forecastNode = xdoc.Root.XPathSelectElement("forecast");

				int i = 0;
				int max = forecastHours.Length;

				foreach ( XElement timeElement in forecastNode.Elements() )
					{
					if ( i < max )
						{
						DateTime from = DateTime.Parse(timeElement.Attribute("from").Value).ToLocalTime();
						DateTime to = DateTime.Parse(timeElement.Attribute("to").Value).ToLocalTime();

						forecastHours[i].hourRange.Content = String.Format("{0}-{1}", from.ToShortTimeString(), to.ToShortTimeString());

						var temp = CelciusToDegrees(float.Parse(timeElement.XPathSelectElement("temperature").Attribute("value").Value));

						forecastHours[i].temperature.Content = String.Format("T: {0}",temp.ToString("F0"));

						var precipElement = timeElement.XPathSelectElement("precipitation");

						forecastHours[i].precipitation.Content = (precipElement.Attribute("value")) != null ? String.Format("P: {0}\" {1}", Math.Round(double.Parse(precipElement.Attribute("value").Value) * 0.03937) /*convert to inches*/, precipElement.Attribute("type").Value) : "P: 0\"";

						var windDirectionElement = timeElement.XPathSelectElement("windDirection");
						var windSpeedElement = timeElement.XPathSelectElement("windSpeed");

						double windSpeedMetersPerSecond = float.Parse(windSpeedElement.Attribute("mps").Value);
						float windSpeedMilesPerHour = (float)Math.Round(2.23694 * windSpeedMetersPerSecond); // Convert Meters Per Second to Miles Per Hour
						
						string windDirection = windDirectionElement.Attribute("code").Value;

						forecastHours[i].windSpeed.Content = String.Format("W: {0} {1}", windSpeedMilesPerHour, windDirection);
						++i;
						}
					else
						{
						break;
						}
					}
				}
			catch (Exception e)
				{
				labelErrors.Content = String.Format("Error: [{0}]\nAt: [{1}]", e.Message, DateTime.Now.ToLongTimeString());
				}
			}

		/// <summary>
		/// Updates the last application update string
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void UpdateLastApplicationUpdate()
			{
			labelLastApplicationUpdate.Content = DateTime.Now.ToLongTimeString();
			}

		/// <summary>
		/// Static method for loading a bitmapimage from the openweathermap api.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="id"></param>
		private static void LoadWeatherIcon(Image image, string id)
			{
			const string url = "http://openweathermap.org/img/w/";
			string fullUrl = url + id + ".png";

			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(fullUrl);
			bitmap.EndInit();

			image.Source = bitmap;
			}

		public void LoadIcon(string uri)
			{
			const string url = "http://openweathermap.org/img/w/";
			string fullUrl = url + uri + ".png";

			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(fullUrl);
			bitmap.EndInit();

			Icon = bitmap;
			}

		public static float KelvinToFerenheit(float kelvin)
			{
			return ((kelvin - 273.15f)*1.8f + 32.0f);
			}

		public static float CelciusToDegrees(float celcius)
			{
			return ((celcius * (9.0f/5.0f)) + 32.0f);
			}

		/// <summary>
		/// Updates the string that represents the date and time.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void UpdateClockString(object sender, EventArgs args )
			{
			CurrentDisplayedDate = DateTime.Now;
			}

		private void buttonUpdate_Click(object sender, RoutedEventArgs e)
			{
			UpdateWeather(buttonUpdate, EventArgs.Empty);
			}
		}
	}
