using System;
using System.Runtime.Remoting.Channels;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WeatherApplication;

namespace UnitTests
	{
	[TestClass]
	public class UnitTest1
		{
		[TestMethod]
		public void TestMethod1()
			{
			MainWindow window = new MainWindow();
			window.Show();
			Thread.Sleep(1000);
			// the constructor updates the weather. So if it works it has tested all features.
			}

		[TestMethod]
		public void TestMethod2()
			{
			var locData = DBUtils.Conn.Table<Location>();
			Assert.IsNotNull(locData);
			// you have to have run the whether application at least once.
			}
		}
	}
