using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dowding.Api;
using Dowding.Client;
using Dowding.Model;
using Newtonsoft.Json;
using WebSocket4Net;

namespace MissionPlanner.WebAPIs
{
    public class Dowding
    {
        private Configuration Configuration;
        private string token;
        public string URL { get; set; } = "https://test.dowding.cuas.dds.mil/api/1.0";
        public string WS { get; set; } = "wss://test.dowding.cuas.dds.mil/ws";

        public Dowding()
        {
            Configuration.Default.DefaultHeader["User-Agent"] =
                "MissionPlanner " + Assembly.GetExecutingAssembly().GetName().Version;
        }

        public async Task Auth(string email, string password)
        {
            Configuration = new Configuration(new ApiClient(URL));

            var auth = new AuthenticationApi(Configuration);

            var tokenres = await auth.AuthenticationLoginPostAsync(new LoginDto(email, password));

            token = tokenres?.Token;

            Configuration.AddApiKey("Authorization", "Bearer " + tokenres?.Token);
        }

        public void SetToken(string customtoken)
        {
            Configuration = new Configuration(new ApiClient(URL));

            token = customtoken;

            Configuration.AddApiKey("Authorization", "Bearer " + customtoken);
        }

        public async Task<List<AgentTick>> GetAgents()
        {
            var agent = new AgentTickApi(Configuration);
            var list = await agent.AgentTickGetAsync();
            return list;
        }

        public async Task<List<VehicleTick>> GetVehicle(string contactIds = null)
        {
            var vehicle = new VehicleTickApi(Configuration);
            var list = await vehicle.VehicleTickGetAsync(contactIds);
            return list;
        }

        //https://test.dowding.cuas.dds.mil/api/1.0/contact?min_ts=1619582433732&max_ts=1619654399999&includeThreats=false&thin=true
        public async Task<List<Contact>> GetContact(decimal? offset = null, decimal? limit = null, bool? thin = null,
            string format = null, string maxLon = null, string minLon = null, string maxLat = null,
            string minLat = null, string maxTs = null, string minTs = null)
        {
            var contact = new ContactApi(Configuration);
            var list = await contact.ContactGetAsync(offset, limit, thin, format, maxLon, minLon, maxLat, minLat, maxTs,
                minTs);
            return list;
        }

        public async Task<List<Zone>> GetZone()
        {
            var zone = new ZoneApi(Configuration);
            var list = await zone.ZoneGetAsync();
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">contacts/vehicle_ticks/operator_ticks/homepoints/events</param>
        /// <returns></returns>
        public async Task<WebSocket> StartWS<T>(string type = "vehicle_ticks")
        {
            var ws = new WebSocket(WS);
            await ws.OpenAsync();
            var connectmsg = JsonConvert.SerializeObject(new
            {
                @event = type,
                data = new
                {
                    headers = new
                    {
                        authorization = "Bearer " + token
                    }
                }
            });

            ws.Opened += (sender, args) => { ws.Send(connectmsg); };

            ws.MessageReceived += (sender, args) =>
            {
                var item = JsonConvert.DeserializeObject<T>(args.Message);
            };

            ws.Closed += (sender, args) => { };

            ws.Error += (sender, args) => { };

            return ws;
        }
    }
}