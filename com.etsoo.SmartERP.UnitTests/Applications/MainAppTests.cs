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
        // Create app
        private MainApp CreateApp()
        {
            return new MainApp((configuration) =>
                {
                    configuration
                        .ModelValidated(false)
                        .SetKeys("]nB9]gY!)sL?", "*sk29)oO@s9d?lsodi49s830Vn!si39x")
                        .ServiceUser(1002)
                    ;
                },
                new SqlServerDatabase("Initial Catalog=smarterp_new;Server=(local);User ID=smarterp;Password=smarterp;Enlist=false")
            );
        }


        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SetupPerformanceTest()
        {
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < 10000; i++)
            {
                CreateApp();
            }

            sw.Stop();

            var ms = sw.ElapsedMilliseconds;

            Assert.IsTrue(ms < 50, $"{ms} is more than 50 ms");
        }

        [Test]
        public async Task UserLogin()
        {
            // Current user
            var user = new CurrentUser(3, IPAddress.Parse("127.0.0.1"));

            // Service
            var service = UserSerivce.Create(CreateApp(), user);

            // Login
            var result = await service.LoginAsync(new Login.LoginModel() { Id = "1001", Password = "1234" });

            // Assert
            Assert.IsTrue(result.OK, result.ToString());
        }
    }
}