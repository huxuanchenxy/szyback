using Dapper;
using SZY.Platform.WebApi.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MSS.API.Common.MyDictionary;


// Coded By admin 2019/10/21 13:06:57
namespace SZY.Platform.WebApi.Data
{
    public interface ITransportCameraRepo<T> where T : BaseEntity
    {
        Task<TransportCarCameraToTunnelView> GetPageList();
    }

    public class TransportCameraRepo : BaseRepo, ITransportCameraRepo<TransportCarCameraToTunnel>
    {
        public TransportCameraRepo(DapperOptions options) : base(options) { }

        public async Task<TransportCarCameraToTunnelView> GetPageList()
        {
            return await WithConnection(async c =>
            {

                StringBuilder sql = new StringBuilder();
                sql.Append($@"  SELECT * FROM transport_camera");
                var ets = await c.QueryAsync<TransportCarCameraToTunnel>(sql.ToString());

                TransportCarCameraToTunnelView ret = new TransportCarCameraToTunnelView();
                ret.rows = ets.ToList();
                return ret;
            });
        }

    }
}



