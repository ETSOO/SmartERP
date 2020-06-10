using com.etsoo.Core.Database;
using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Applications;
using com.etsoo.SmartERP.User;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace com.etsoo.SmartERPConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            // Database
            var database = new SqlServerDatabase("Initial Catalog=smarterp_new;Server=(local);User ID=smarterp;Password=smarterp;Enlist=false;Min Pool Size=5");

            // SmartERP Configuration
            var configs = new MainConfiguration(
                privateKey: "]nB9]gY!)sL?",
                symmetricKey: "*sk29)oO@s9d?lsodi49s830Vn!si39x",
                modelValidated: true,
                serviceUserId: "1002"
            );

            // App
            var app = new MainApp(configs, database);

            // Current user
            var user = new CurrentUser(null, IPAddress.None);

            // Service
            var service = new UserSerivce();
            service.Setup(app, user);

            for (var t = 0; t < 1; t++)
            {
                // Login
                var model = new SmartERP.Login.LoginModel() { Id = "1001", Password = "1234" };
                var result = await service.LoginAsync(model);
            }

            sw.Stop();

            var ms = sw.ElapsedMilliseconds;

            // Output
            Console.WriteLine($"Milliseconds: {ms}");
        }
    }
}
