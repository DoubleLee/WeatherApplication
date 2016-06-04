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

	public class ForecastDay
		{
		private string date;
		private ImageSource image;
		private string cond;
		private string precip;
		private string temp;
		private string wind;

		public string Date
			{
			get
				{
				return date;
				}

			set
				{
				date = value;
				}
			}

		public ImageSource Image
			{
			get
				{
				return image;
				}

			set
				{
				image = value;
				}
			}

		public string Temp
			{
			get
				{
				return temp;
				}

			set
				{
				temp = value;
				}
			}

		public string Wind
			{
			get
				{
				return wind;
				}

			set
				{
				wind = value;
				}
			}

		public string Cond
			{
			get { return cond; }
			set { cond = value; }
			}

		public string Precip
			{
			get { return precip; }
			set { precip = value; }
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
		Dictionary<Uri, ImageSource> weatherImageSources = new Dictionary<Uri,ImageSource>();

		List<ForecastDay> forecasts = new List<ForecastDay>(7);

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
					dateTime.Text = value.ToShortTimeString(); // this line is why there is a property, to save the ToString() call on the DateTime, unless really neccessary.
					lastDateTime = value;
					}
				}
			}

		public MainWindow()
			{
			InitializeComponent();

			for(int i = 0; i < forecasts.Capacity; ++i)
				{
				forecasts.Add(new ForecastDay());
				}

			listBox1.ItemsSource = forecasts;

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

			// call the update weather methods now.
			UpdateWeather(null, null);
			// call the clock update method now.
			UpdateClockString(null, null);
			}

		public void UpdateWeather( object sender, EventArgs args )
			{
			try
				{
				progressText.Text = "Status working...";
				UpdateCurrentWeather();
				UpdateHourlyForecast();
				UpdateDailyForecast();
				UpdateLastApplicationUpdate();
				progressText.Text = "Status good.";
				}
			catch (Exception e)
				{
				
				}
			
			}

		/// <summary>
		/// Updates the current weather by calling the openweathermap api.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void UpdateCurrentWeather()
			{
			try
				{
				// The url contains at the end options for the call, including my unique api key, as well as the type of api call and any other settings.
				WebRequest request = WebRequest.Create(String.Format("http://api.openweathermap.org/data/2.5/weather?zip={0},us&mode=xml&APPID=930964919a915aefc90d0d5e3b0f4bd2", textBoxZip.Text));
				WebResponse response = request.GetResponse();
				Stream dataStream = response.GetResponseStream();

				XmlSerializer serializer = new XmlSerializer(typeof(current));
				StreamReader reader = new StreamReader(dataStream);

				current curr = (current)serializer.Deserialize(reader);

				this.city.Text = curr.city.name;

				// Get temperature and convert to ferenheit
				float kelvin = (float)curr.temperature.value;
				float ferenheit = KelvinToFerenheit(kelvin);
				labelTemp.Content = ferenheit.ToString("F0");

				// get current weather description string
				labelCurrentWeather.Content = curr.weather.value;

				// get humidity
				labelHumidity.Content = curr.humidity.value.ToString() + curr.humidity.unit;

				// get pressure
				labelPressure.Content = curr.pressure.value + curr.pressure.unit;

				// parse the last api update string to a datetime struct.
				lastApiUpdate = curr.lastupdate.value;
				// tell the struct that it is UTC time.
				lastApiUpdate = DateTime.SpecifyKind(lastApiUpdate, DateTimeKind.Utc);
				lastApiUpdate = lastApiUpdate.ToLocalTime();
				// convert UTC time to local time and parse it to string and set the label.
				apiUpdate.Text = "Api Update: " + lastApiUpdate.ToLongTimeString();

				// load the current weather image icon.
				imageCurrentWeather.Source = LoadOrGetImageSource(curr.weather.icon);
				taskBarInfo.Overlay = imageCurrentWeather.Source;
				Icon = imageCurrentWeather.Source;
				}
			catch( Exception e )
				{
				progressText.Text = String.Format("Error in UpdateCurrentWeather: [{0}]\nAt: [{1}]", e.Message, DateTime.Now.ToLongTimeString());
				throw e;
				}
			}

		public void UpdateHourlyForecast()
			{
			try
				{
				WebRequest request = WebRequest.Create(String.Format("http://api.openweathermap.org/data/2.5/forecast?zip={0},us&mode=xml&APPID=930964919a915aefc90d0d5e3b0f4bd2", textBoxZip.Text));
				WebResponse response = request.GetResponse();
				Stream dataStream = response.GetResponseStream();

				XmlSerializer serializer = new XmlSerializer(typeof(weatherdata));

				StreamReader reader = new StreamReader(dataStream);
				weatherdata hourlyForecast = (weatherdata)serializer.Deserialize(reader);

				DateTime nowMidnight = DateTime.Now.Date;
				
				
				listBox.Items.Clear();

				StringBuilder forecastBuilder = new StringBuilder();
				foreach ( weatherdataTime hoursForecast in hourlyForecast.forecast )
					{
					forecastBuilder.Clear();

					DateTime startDate = hoursForecast.from.ToLocalTime();
					forecastBuilder.Append("Days: " + (Math.Max((startDate.Date - nowMidnight).Days, 0)).ToString());
					forecastBuilder.Append("  ");
					forecastBuilder.Append(startDate.ToShortTimeString());
					forecastBuilder.Append('-');
					forecastBuilder.Append(hoursForecast.to.ToLocalTime().ToShortTimeString());
					forecastBuilder.Append("\t");

					var temp = CelciusToDegrees((float)hoursForecast.temperature.value);
						
					forecastBuilder.Append(String.Format("Temp: {0}", temp.ToString("F0")));
					forecastBuilder.Append("\t");

					
                    if (hoursForecast.precipitation.valueSpecified)
						{
						double precipInches = PrecipMMToInches((double)hoursForecast.precipitation.value);

						forecastBuilder.Append(String.Format("Precip: {0:f3}\" {1}", precipInches, hoursForecast.precipitation.type));
						}
					else
						{
						forecastBuilder.Append("Precip: 0.000\" none");
						}
					forecastBuilder.Append("\t");

					double windSpeedMetersPerSecond = (double)hoursForecast.windSpeed.mps;
					float windSpeedMilesPerHour = (float)Math.Round(2.23694 * windSpeedMetersPerSecond); // Convert Meters Per Second to Miles Per Hour
						
					string windDirection = hoursForecast.windDirection.code;

					forecastBuilder.Append(String.Format("Wind: {0} {1}", windSpeedMilesPerHour, windDirection));
					forecastBuilder.Append("\t");

					forecastBuilder.Append(String.Format("Clouds: {0}%", hoursForecast.clouds.all));
					forecastBuilder.Append("\t");

					listBox.Items.Add(forecastBuilder.ToString());
					}
				}
			catch (Exception e)
				{
				progressText.Text = String.Format("Error in UpdateHourlyForecast: [{0}]\nAt: [{1}]", e.Message, DateTime.Now.ToLongTimeString());
				throw e;
				}
			}

		/// <summary>
		/// Updates the 5 day forecast data from the open weather map api forecast.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void UpdateDailyForecast()
			{
			try
				{
				WebRequest request = WebRequest.Create(String.Format("http://api.openweathermap.org/data/2.5/forecast/daily?zip={0},us&mode=xml&APPID=930964919a915aefc90d0d5e3b0f4bd2", textBoxZip.Text));
				WebResponse response = request.GetResponse();
				Stream dataStream = response.GetResponseStream();

				var xdoc = XDocument.Load( dataStream );

				var forecastNode = xdoc.Root.XPathSelectElement("forecast");

				int i = 0;
				// use the forecast days array to update the forecast data.
				foreach ( XElement element in forecastNode.Elements() )
					{
					DateTime parsedDate = DateTime.SpecifyKind(DateTime.Parse(element.Attribute("day").Value), DateTimeKind.Local);

					string dateString = String.Format("{0} {1}/{2}",parsedDate.DayOfWeek, parsedDate.Date.Month, parsedDate.Date.Day);
					forecasts[i].Date = dateString;

					var symbolElement = element.XPathSelectElement("symbol");
					string id = symbolElement.Attribute("var").Value;
					forecasts[i].Image = LoadOrGetImageSource(id);
					forecasts[i].Cond = symbolElement.Attribute("name").Value;

					var precipElement = element.XPathSelectElement("precipitation");
					if ( precipElement != null )
						{
						var precipAttrib = precipElement.Attribute("value");
						if (precipAttrib != null)
							{
							string typeOfPrecip = precipElement.Attribute("type").Value;

							double precipInches = PrecipMMToInches(Double.Parse(precipAttrib.Value));

							forecasts[i].Precip = String.Format("{0:F3}\" {1}", precipInches, typeOfPrecip);
							}
						else
							{
							forecasts[i].Precip = "0.00\" None";
							}
						}
					else
						{
						forecasts[i].Precip = "0.00\" None";
						}

					var tempNode = element.XPathSelectElement("temperature");
					forecasts[i].Temp = String.Format("H:{0} L:{1}", KelvinToFerenheit(float.Parse(tempNode.Attribute("max").Value)).ToString("F0"), KelvinToFerenheit(float.Parse(tempNode.Attribute("min").Value)).ToString("F0"));
					
					var windSpeedNode = element.XPathSelectElement("windSpeed");
					var windDirNode = element.XPathSelectElement("windDirection");

					double windSpeedMetersPerSecond = float.Parse(windSpeedNode.Attribute("mps").Value);
					float windSpeedMilesPerHour = (float)Math.Round(2.23694 * windSpeedMetersPerSecond); // Convert Meters Per Second to Miles Per Hour

					forecasts[i].Wind = string.Format("Wind: {0}MPH {1}", windSpeedMilesPerHour, windDirNode.Attribute("code").Value);
					
					++i;
					}
				listBox1.Items.Refresh();
				}
			catch( Exception e )
				{
				progressText.Text = String.Format("Error in UpdateDailyForecast: [{0}]\nAt: [{1}]", e.Message, DateTime.Now.ToLongTimeString());
				throw e;
				}
			}

		private static double PrecipMMToInches(double originalValue)
			{
			double precipMilliMetersPerHour = originalValue;
			double precipMilliMetersInTimeFrame = precipMilliMetersPerHour * 3.0; // 3 because it's 3 hours in a per hour format.
			double precipInches = precipMilliMetersInTimeFrame * 0.03937; // this number is the conversion to inches.
			return precipInches;
			}

		/// <summary>
		/// Updates the last application update string
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void UpdateLastApplicationUpdate()
			{
			appUpdates.Text = "App Update: " + DateTime.Now.ToLongTimeString();
			}

		public ImageSource LoadOrGetImageSource(string id)
			{
			const string url = "http://openweathermap.org/img/w/";
			string fullUrl = url + id + ".png";
			
			Uri uri = new Uri(fullUrl);
			ImageSource source;
			if (weatherImageSources.TryGetValue(uri, out source))
				{
				return source;
				}
			else
				{
				BitmapImage bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.UriSource = uri;
				bitmap.EndInit();

				weatherImageSources.Add(uri, bitmap);

				return bitmap;
				}
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
			progressBarHoursSinceUpdate.Value = timeSinceApiUpdate.TotalHours;
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
