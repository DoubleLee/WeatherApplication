using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		private string dayGrade;

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

		public string DayGrade
			{
			get { return dayGrade; }
			set 
				{
				dayGrade = value;
				}
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
					dateTime.Text = "Your Local Time: " + value.ToShortTimeString(); // this line is why there is a property, to save the ToString() call on the DateTime, unless really neccessary.
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
			if(Application.Current != null)
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

			mediaElement1.Play();

			DBUtils.Conn.CreateTableAsync<Location>();
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

				var source = mediaElement1.Source;
				mediaElement1.Source = null;
				mediaElement1.Source = source;

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
				WebRequest request = WebRequest.Create(String.Format("http://api.openweathermap.org/data/2.5/weather?zip={0},us&mode=xml&APPID=930964919a915aefc90d0d5e3b0f4bd2&units=imperial", textBoxZip.Text));
				WebResponse response = request.GetResponse();
				Stream dataStream = response.GetResponseStream();

				XmlSerializer serializer = new XmlSerializer(typeof(current));
				StreamReader reader = new StreamReader(dataStream);

				current curr = (current)serializer.Deserialize(reader);

				this.city.Text = curr.city.name;

				// Get temperature and convert to ferenheit
				float ferenheit = (float)curr.temperature.value;
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
				WebRequest request = WebRequest.Create(String.Format("http://api.openweathermap.org/data/2.5/forecast?zip={0}&mode=xml&APPID=930964919a915aefc90d0d5e3b0f4bd2&units=imperial", textBoxZip.Text));
				WebResponse response = request.GetResponse();
				Stream dataStream = response.GetResponseStream();

				XmlSerializer serializer = new XmlSerializer(typeof(weatherdata));

				StreamReader reader = new StreamReader(dataStream);
				weatherdata hourlyForecast = (weatherdata)serializer.Deserialize(reader);

				DateTime nowMidnight = DateTime.Now.Date;

				List<HourlyDisplayData> displayData = new List<HourlyDisplayData>();
				for(int i = 0; i < hourlyForecast.forecast.Length; ++i)
					{
					weatherdataTime hoursForecast = hourlyForecast.forecast[i];
					HourlyDisplayData d = new HourlyDisplayData();
					
					displayData.Add(d);

					DateTime startDate = hoursForecast.from.ToLocalTime();
					d.Day = Math.Max((startDate.Date - nowMidnight).Days, 0);
					d.TimeStart = startDate.ToShortTimeString();
					d.TimeEnd = hoursForecast.to.ToLocalTime().ToShortTimeString();
					d.Temp = (int)hoursForecast.temperature.value;
					d.Precip = PrecipMMToInches((double)hoursForecast.precipitation.value).ToString("F3");
					d.wind = String.Format("{0}mph {1}", hoursForecast.windSpeed.mps.ToString("F0"), hoursForecast.windDirection.code);
					d.clouds = hoursForecast.clouds.all + "%";
					d.grade = GetGrade(hoursForecast).ToString("F0");
					}
				listBox.AutoGenerateColumns = true;
				listBox.ItemsSource = displayData;
				
				//foreach (var col in listBox.Columns) 
				//	{ 
				//	if (double.IsNaN(col.Width.Value)) col.Width = col.ActualWidth; 
				//	col.Width = double.NaN; 
				//	} 
				
				/*
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

					var temp = (float)hoursForecast.temperature.value;
						
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

					float windSpeedMilesPerHour = (float)hoursForecast.windSpeed.mps;

					string windDirection = hoursForecast.windDirection.code;

					forecastBuilder.Append(String.Format("Wind: {0} {1}", windSpeedMilesPerHour.ToString("F0"), windDirection));
					forecastBuilder.Append("\t");

					forecastBuilder.Append(String.Format("Clouds: {0}%", hoursForecast.clouds.all));
					forecastBuilder.Append("\t");

					listBox.Items.Add(forecastBuilder.ToString());
					}
					*/
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
				WebRequest request = WebRequest.Create(String.Format("http://api.openweathermap.org/data/2.5/forecast/daily?zip={0},us&mode=xml&APPID=930964919a915aefc90d0d5e3b0f4bd2&units=imperial", textBoxZip.Text));
				WebResponse response = request.GetResponse();
				Stream dataStream = response.GetResponseStream();

				XmlSerializer serializer = new XmlSerializer(typeof(Daily.weatherdata));

				StreamReader reader = new StreamReader(dataStream);
				Daily.weatherdata dailyForecast = (Daily.weatherdata)serializer.Deserialize(reader);

				int i = 0;
				// use the forecast days array to update the forecast data.
				foreach ( Daily.weatherdataTime dayForecast in dailyForecast.forecast )
					{
					DateTime parsedDate = DateTime.SpecifyKind(dayForecast.day, DateTimeKind.Local);

					string dateString = String.Format("{0} {1}/{2}",parsedDate.DayOfWeek, parsedDate.Date.Month, parsedDate.Date.Day);
					forecasts[i].Date = dateString;

					forecasts[i].Image = LoadOrGetImageSource(dayForecast.symbol.var);
					forecasts[i].Cond = dayForecast.symbol.name;

					if ( dayForecast.precipitation.valueSpecified)
						{
						forecasts[i].Precip = String.Format("{0:F3}\" {1}",  PrecipMMToInches((double)dayForecast.precipitation.value), dayForecast.precipitation.type);
						}
					else
						{
						forecasts[i].Precip = "0.00\" None";
						}
					
					float maxTemp = (float)dayForecast.temperature.max;
					forecasts[i].Temp = String.Format("H:{0} L:{1}", maxTemp.ToString("F0"), ((float)dayForecast.temperature.min).ToString("F0"));

					float windSpeedMilesPerHour = (float)dayForecast.windSpeed.mps;

					forecasts[i].Wind = string.Format("Wind: {0}MPH {1}", windSpeedMilesPerHour.ToString("F0"), dayForecast.windDirection.code);
					
					if (!dayForecast.symbol.var.Contains('n'))
						{
						float grade = GetGrade(dayForecast);

						forecasts[i].DayGrade = "Grade: " + grade.ToString("F0") + "%";
						}
					else
						{
						forecasts[i].DayGrade = "Grade: Night";
						}
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

		private float GetGrade(Daily.weatherdataTime dayForecast)
			{
			float tempGrade = GetTemperatureFishingGrade((float)dayForecast.temperature.max);
			float conditionsGrade = GetConditionsGrade(dayForecast.symbol.var);
			float windGrade = GetWindGrade((float)dayForecast.windSpeed.mps);
			float cloudinessGrade = Math.Max(50.0f, (float)dayForecast.clouds.all);

			float totalGrade = tempGrade + conditionsGrade + windGrade + cloudinessGrade;
			float grade = totalGrade / 4.0f;
			return grade;
			}

		private float GetGrade(weatherdataTime timeForecast)
			{
			float tempGrade = GetTemperatureFishingGrade((float)timeForecast.temperature.max);
			float conditionsGrade = GetConditionsGrade(timeForecast.symbol.var);
			float windGrade = GetWindGrade((float)timeForecast.windSpeed.mps);
			float cloudinessGrade = Math.Max(50.0f, (float)timeForecast.clouds.all);

			float totalGrade = tempGrade + conditionsGrade + windGrade + cloudinessGrade;
			float grade = totalGrade / 4.0f;
			return grade;
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

		public void UpdateToThisZipCode(string zipCode)
			{
			configuration.lastZipCode = textBoxZip.Text = zipCode;
			UpdateWeather(buttonUpdate, EventArgs.Empty);
			}

		private void OnClosingApp(object sender, System.ComponentModel.CancelEventArgs e)
			{
			string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string configFilePath = dataFolder + "weatherConfig.txt";

			XmlSerializer serializer = new XmlSerializer(typeof(Configuration));

			StreamWriter writer = new StreamWriter(configFilePath);

			serializer.Serialize(writer, configuration);
			}

		private void MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
			{
			mediaElement1.Position = new TimeSpan(0,0,0,0,1);
			mediaElement1.Play();
			}

		private float GetTemperatureFishingGrade(float temp)
			{
			if (temp >= 110)
				{
				return 70.0f;
				}
			if (temp > 105.0f && temp < 110)
				{
				return 80.0f;
				}
			if (temp >= 95.0f && temp < 105.0f)
				{
				return 90.0f;
				}
			if (temp >= 70.0f && temp < 95.0f)
				{
				return 100.0f;
				}
			if (temp >= 65.0f && temp < 70.0f)
				{
				return 80.0f;
				}
			if (temp >= 50 && temp < 65.0f)
				{
				return 70.0f;
				}
				
			return 0.0f; //way to hot or cold.
			}

		private float GetConditionsGrade(string condition)
			{
			if (String.CompareOrdinal(condition, "01d") == 0)
				{
				return 100.0f;
				}
			if (String.CompareOrdinal(condition, "02d") == 0)
				{
				return 95.0f;
				}
			if (String.CompareOrdinal(condition, "03d") == 0)
				{
				return 90.0f;
				}
			if (String.CompareOrdinal(condition, "04d") == 0)
				{
				return 85.0f;
				}
			if (String.CompareOrdinal(condition, "50d") == 0)
				{
				return 75.0f;
				}
			if (String.CompareOrdinal(condition, "09d") == 0)
				{
				return 70.0f;
				}
			if (String.CompareOrdinal(condition, "10d") == 0)
				{
				return 60.0f;
				}
			if (String.CompareOrdinal(condition, "11d") == 0)
				{
				return 0.0f;
				}
			if (String.CompareOrdinal(condition, "13d") == 0)
				{
				return 0.0f;
				}

			return 0;

			//throw new InvalidDataException("Condition string unhandled, " + condition);
			}

		float GetWindGrade(float speedmph)
			{
			if (speedmph <= 5.0f)
				{
				return 100.0f;
				}
			if (speedmph <= 10.0f)
				{
				return 90.0f;
				}
			if (speedmph <= 15.0f)
				{
				return 75.0f;
				}
			if(speedmph <= 25.0f)
				{
				return 75.0f;
				}

			if(speedmph <= 40f)
				{
				return 50.0f;
				}
			
			return 0;
			}

		private void buttonFavorites_Click(object sender, RoutedEventArgs e)
			{
			var loc = new Locations(this);
			loc.ShowDialog();
			}
		}
	}
