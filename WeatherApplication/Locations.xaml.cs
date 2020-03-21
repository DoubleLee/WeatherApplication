using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WeatherApplication
	{
	/// <summary>
	/// Interaction logic for Locations.xaml
	/// </summary>
	public partial class Locations : Window
		{
		MainWindow window;

		public Locations(MainWindow window)
			{
			InitializeComponent();
			this.window = window;
			}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
			{
			var locations = await DBUtils.Conn.Table<Location>().ToListAsync();
			
			if(locations == null || locations.Count == 0)
				{
				locations.Add(new Location{ZipCode="65807", Description="Home"});
				locations.Add(new Location{ZipCode="65804", Description="Other Side"});

				await DBUtils.Conn.InsertAllAsync(locations);
				}
			listBox.SelectedValuePath = "Id";
			listBox.DisplayMemberPath = "Description";
			listBox.ItemsSource = locations;
			}

		private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
			{
			listBox = sender as ListBox;

			var selection = listBox.SelectedItem as Location;
			if(selection != null)
				window.UpdateToThisZipCode(selection.ZipCode);
			}

		private async void buttonDelete_Click(object sender, RoutedEventArgs e)
			{
			var location = listBox.SelectedItem as Location;

			await DBUtils.Conn.DeleteAsync(location);

			Window_Loaded(null, null);
			}

		private void buttonAdd_Click(object sender, RoutedEventArgs e)
			{
			var loc = new LocationEditWindow(null);
			loc.ShowDialog();
			Window_Loaded(null,null);
			}

		private void buttonModify_Click(object sender, RoutedEventArgs e)
			{
			var sel = listBox.SelectedItem as Location;
			if(sel != null)
				{
				var loc = new LocationEditWindow(sel);
				loc.ShowDialog();
				Window_Loaded(null,null);
				}
			}
		}
	}
