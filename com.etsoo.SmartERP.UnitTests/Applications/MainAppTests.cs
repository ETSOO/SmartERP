using com.etsoo.Core.Database;
using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Applications;
using com.etsoo.SmartERP.User;
using NUnit.Framework;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace com.etsoo.SmartERP.UnitTests.Applications
{
    [TestFixture]
    public class MainAppTests
    {
        // SmartERP Configuration
        MainConfiguration configs = new MainConfiguration(
            privateKey: "]nB9]gY!)sL?",
            symmetricKey: "*sk29)oO@s9d?lsodi49s830Vn!si39x",
            modelValidated: false,
            serviceUserId: "1002"
        );

        // Database
        SqlServerDatabase database = new SqlServerDatabase("Initial Catalog=smarterp_new;Server=(local);User ID=smarterp;Password=smarterp;Enlist=false");

        MainApp app;

        [SetUp]
        public void Setup()
        {
            app = new MainApp(configs, database);
        }

        [Test]
        public void SetupPerformanceTest()
        {
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < 10000; i++)
            {
                var app = new MainApp(configs, database);
            }

            sw.Stop();

            var ms = sw.ElapsedMilliseconds;

            Assert.IsTrue(ms < 50, $"{ms} is more than 50 ms");
        }

        [Test]
        public async Task UserLogin()
        {
            // Current user
            var user = new CurrentUser(Thread.CurrentPrincipal, IPAddress.Parse("127.0.0.1"));

            // Service
            var service = new UserSerivce();
            service.Setup(app, user);

            // Login
            var result = await service.LoginAsync(new Login.LoginModel() { Id = "1001", Password = "1234" });

            // Assert
            Assert.IsTrue(result.OK, result.ToString());
        }
    }
}