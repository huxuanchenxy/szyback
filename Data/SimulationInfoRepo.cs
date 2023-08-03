using Dapper;
using SZY.Platform.WebApi.Model;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;


// Coded By admin 2019/11/9 13:45:19
namespace SZY.Platform.WebApi.Data
{
    public interface ISimulationInfoRepo<T> where T : BaseEntity
    {
        Task<SimulationInfoPageView> GetPageList(SimulationInfoParm param);
    }

    public class SimulationInfoRepo : BaseRepo, ISimulationInfoRepo<SimulationInfo>
    {
        public SimulationInfoRepo(DapperOptions options) : base(options) { }

        public async Task<SimulationInfoPageView> GetPageList(SimulationInfoParm parm)
        {
            return await WithConnection(async c =>
            {

                StringBuilder sql = new StringBuilder();
                sql.Append($@"  SELECT 
                id,
                road_part,
                camera,
                time,
                summarize,content FROM simulation_info
                 ");
                StringBuilder whereSql = new StringBuilder();
                whereSql.Append(" WHERE 1 = 1 ");

                if (parm.time != null)
                {
                    //whereSql.Append(" and DATE(time) = '"+parm.time+"'");
                    whereSql.Append(" and time >= '" + parm.time + " 00:00:00' and time <= '" + parm.time + " 23:59:59' ");
                }

                sql.Append(whereSql);
                //验证是否有参与到流程中
                //string sqlcheck = sql.ToString();
                //sqlcheck += ("AND ai.CreatedByUserID = '" + parm.UserID + "'");
                //var checkdata = await c.QueryFirstOrDefaultAsync<TaskViewModel>(sqlcheck);
                //if (checkdata == null)
                //{
                //    return null;
                //}

                var data = await c.QueryAsync<SimulationInfo>(sql.ToString());
                var total = data.ToList().Count;
                sql.Append(" order by " + parm.sort + " " + parm.order)
                .Append(" limit " + (parm.page - 1) * parm.rows + "," + parm.rows);
                var ets = await c.QueryAsync<SimulationInfo>(sql.ToString());

                SimulationInfoPageView ret = new SimulationInfoPageView();
                ret.rows = ets.ToList();
                ret.total = total;
                return ret;
            });
        }

    }
}



