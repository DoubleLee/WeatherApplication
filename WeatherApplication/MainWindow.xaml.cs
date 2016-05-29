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
using System.Windows.Shell;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;

namespace WeatherApplication
	{
	[Serializable]
	public class Configuration
		{
		[System.Xml.Serialization.XmlElement("LastZipCode")]
		public string lastZipCode;

		public static Configuration Default
			{
			get 
				{
				return new Configuration{lastZipCode = "65804"};
				}
			}
		}
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
		public Label clouds;

		public ForecastHours( Label hourRange, Label temperature, Label precipitation, Label windSpeed, Label clouds )
			{
			this.hourRange = hourRange;
			this.temperature = temperature;
			this.precipitation = precipitation;
			this.windSpeed = windSpeed;
			this.clouds = clouds;
			}
		}

	public partial class MainWindow : Window
		{
		ForecastDay[] forecastDays;
		ForecastHours[] forecastHours;
		DispatcherTimer weatherUpdateTimer;
		DispatcherTimer clockUpdateTimer;

		DateTime lastDateTime;

		DateTime lastApiUpdate = DateTime.Now;
		TaskbarItemInfo taskBarInfo;

		Configuration configuration;

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

			string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string configFilePath = dataFolder + "weatherConfig.txt";

			if (File.Exists(configFilePath))
				{
				XmlSerializer serializer = new XmlSerializer(typeof(Configuration));

				StreamReader reader = new StreamReader(configFilePath);

				configuration = (Configuration)serializer.Deserialize(reader);
				}
			else
				{
				configuration = Configuration.Default;
				}

			textBoxZip.Text = configuration.lastZipCode;

			// Setup the task bar info
			taskBarInfo = new TaskbarItemInfo();
			taskBarInfo.ProgressState = TaskbarItemProgressState.Normal;
			Application.Current.MainWindow.TaskbarItemInfo = taskBarInfo;

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
				WebRequest request = WebRequest.Create(String.Format("http://api.openweathermap.org/data/2.5/weather?zip={0},us&mode=xml&APPID=930964919a915aefc90d0d5e3b0f4bd2", textBoxZip.Text));
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
				lastApiUpdate = DateTime.Parse(lastUpdate);
				// tell the struct that it is UTC time.
				lastApiUpdate = DateTime.SpecifyKind(lastApiUpdate, DateTimeKind.Utc);
				lastApiUpdate = lastApiUpdate.ToLocalTime();
				// convert UTC time to local time and parse it to string and set the label.
				labelLastWeatherApiUpdate.Content = lastApiUpdate.ToLongTimeString();

				// load the current weather image icon.
				string weatherIconString = weatherNode.XPathSelectElement("weather").Attribute("icon").Value;
				LoadWeatherIcon( imageCurrentWeather, weatherIconString );
				taskBarInfo.Overlay = imageCurrentWeather.Source;
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
				WebRequest request = WebRequest.Create(String.Format("http://api.openweathermap.org/data/2.5/forecast/daily?zip={0},us&mode=xml&APPID=930964919a915aefc90d0d5e3b0f4bd2", textBoxZip.Text));
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
				WebRequest request = WebRequest.Create(String.Format("http://api.openweathermap.org/data/2.5/forecast?zip={0},us&mode=xml&APPID=930964919a915aefc90d0d5e3b0f4bd2", textBoxZip.Text));
				WebResponse response = await request.GetResponseAsync();
				Stream dataStream = response.GetResponseStream();

				var xdoc = XDocument.Load( dataStream );

				DateTime nowMidnight = DateTime.Now.Date;

				var forecastNode = xdoc.Root.XPathSelectElement("forecast");
				
				listBox.Items.Clear();

				StringBuilder forecastBuilder = new StringBuilder();
				foreach ( XElement timeElement in forecastNode.Elements() )
					{
					forecastBuilder.Clear();

					DateTime startDate = DateTime.Parse(timeElement.Attribute("from").Value).ToLocalTime();
					forecastBuilder.Append("Days: " + (Math.Max((startDate.Date - nowMidnight).Days, 0)).ToString());
					forecastBuilder.Append("  ");
					forecastBuilder.Append(startDate.ToShortTimeString());
					forecastBuilder.Append('-');
					forecastBuilder.Append(DateTime.Parse(timeElement.Attribute("to").Value).ToLocalTime().ToShortTimeString());
					forecastBuilder.Append("\t");

					var temp = CelciusToDegrees(float.Parse(timeElement.XPathSelectElement("temperature").Attribute("value").Value));
						
					forecastBuilder.Append(String.Format("Temp: {0}",temp.ToString("F0")));
					forecastBuilder.Append("\t");

					var precipElement = timeElement.XPathSelectElement("precipitation");
                    var precipValue = precipElement.Attribute("value");
                    if (precipValue != null)
						{
                        double precipMilliMetersPerHour = double.Parse(precipValue.Value);
						double precipMilliMetersInTimeFrame = precipMilliMetersPerHour * 3.0; // 3 because it's 3 hours in a per hour format.
						double precipInches = precipMilliMetersInTimeFrame * 0.03937; // this number is the conversion to inches.

						forecastBuilder.Append(String.Format("Precip: {0:f3}\" {1}", precipInches, precipElement.Attribute("type").Value));
						}
					else
						{
						forecastBuilder.Append("Precip: 0.000\" none");
						}
					forecastBuilder.Append("\t");

					var windDirectionElement = timeElement.XPathSelectElement("windDirection");
					var windSpeedElement = timeElement.XPathSelectElement("windSpeed");

					double windSpeedMetersPerSecond = float.Parse(windSpeedElement.Attribute("mps").Value);
					float windSpeedMilesPerHour = (float)Math.Round(2.23694 * windSpeedMetersPerSecond); // Convert Meters Per Second to Miles Per Hour
						
					string windDirection = windDirectionElement.Attribute("code").Value;

					forecastBuilder.Append(String.Format("Wind: {0} {1}", windSpeedMilesPerHour, windDirection));
					forecastBuilder.Append("\t");

					forecastBuilder.Append(String.Format("Clouds: {0}%", timeElement.XPathSelectElement("clouds").Attribute("all").Value));
					forecastBuilder.Append("\t");

					listBox.Items.Add(forecastBuilder.ToString());
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
			var now = DateTime.Now;
			CurrentDisplayedDate = now;

			var timeSinceApiUpdate = now - lastApiUpdate;

			taskBarInfo.ProgressValue = timeSinceApiUpdate.TotalHours;
			}

		private void buttonUpdate_Click(object sender, RoutedEventArgs e)
			{
			UpdateWeather(buttonUpdate, EventArgs.Empty);
			}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e)
			{
			configuration.lastZipCode = textBoxZip.Text;
			}

		private void OnClosingApp(object sender, System.ComponentModel.CancelEventArgs e)
			{
			string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string configFilePath = dataFolder + "weatherConfig.txt";

			XmlSerializer serializer = new XmlSerializer(typeof(Configuration));

			StreamWriter writer = new StreamWriter(configFilePath);

			serializer.Serialize(writer, configuration);
			}
		}
	}
