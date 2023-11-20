using Dapper;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SZY.Platform.WebApi.Model;

namespace SZY.Platform.WebApi.Data
{
    public interface ISimulationInfoSumRepo<T> where T : BaseEntity
    {
        Task<SimulationInfoSumPageView> GetPageList(SimulationInfoSumParm param);
    }

    public class SimulationInfoSumRepo : BaseRepo, ISimulationInfoSumRepo<SimulationInfoSum>
    {
        public SimulationInfoSumRepo(DapperOptions options) : base(options) { }

        public async Task<SimulationInfoSumPageView> GetPageList(SimulationInfoSumParm parm)
        {
            return await WithConnection(async c =>
            {

                StringBuilder sql = new StringBuilder();
                sql.Append($@"  SELECT 
                id,
                roadpart,
                camera,
                area_id,
                item_type,
                Item_count,
                speed,
                queue_length,
                time,
                proj_id,add_time FROM simulation_info_sum
                 ");
                StringBuilder whereSql = new StringBuilder();
                whereSql.Append(" WHERE 1 = 1 ");

                if (parm.time != null)
                {
                    whereSql.Append(" and time >= '" + parm.time + " 00:00:00' and time <= '" + parm.time + " 23:59:59' ");
                }
                if (parm.item_type != 0)
                {
                    whereSql.Append(" and item_type = "+ parm.item_type+"");
                }

                sql.Append(whereSql);

                var data = await c.QueryAsync<SimulationInfoSum>(sql.ToString());
                var total = data.ToList().Count;
                sql.Append(" order by " + parm.sort + " " + parm.order)
                .Append(" limit " + (parm.page - 1) * parm.rows + "," + parm.rows);
                var ets = await c.QueryAsync<SimulationInfoSum>(sql.ToString());

                SimulationInfoSumPageView ret = new SimulationInfoSumPageView();
                ret.rows = ets.ToList();
                ret.total = total;
                return ret;
            });
        }

    }
}



