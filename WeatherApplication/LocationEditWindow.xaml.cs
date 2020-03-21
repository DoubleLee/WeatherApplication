using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WeatherApplication
	{
	/// <summary>
	/// Interaction logic for LocationEditWindow.xaml
	/// </summary>
	public partial class LocationEditWindow : Window
		{
		Location loc;
		public LocationEditWindow(Location sel)
			{
			loc = sel;
			InitializeComponent();

			if(loc != null)
				{
				textBoxDescription.Text = loc.Description;
				textBoxZip.Text = loc.ZipCode;
				}
			}

		private async void buttonSave_Click(object sender, RoutedEventArgs e)
			{
			if(!String.IsNullOrWhiteSpace(textBoxDescription.Text) && !String.IsNullOrWhiteSpace(textBoxZip.Text))
				{
				if(loc == null)
					{
					Location loc = new Location();
					loc.Description = textBoxDescription.Text;
					loc.ZipCode = textBoxZip.Text;

					await DBUtils.Conn.InsertAsync(loc);
					Close();
					}
				else
					{
					loc.Description = textBoxDescription.Text;
					loc.ZipCode = textBoxZip.Text;

					await DBUtils.Conn.UpdateAsync(loc);
					Close();
					}
				}
			else
				{
				MessageBox.Show("Please enter all information.");
				}
			}

		private void buttonCancel_Click(object sender, RoutedEventArgs e)
			{
			Close();
			}
		}
	}
