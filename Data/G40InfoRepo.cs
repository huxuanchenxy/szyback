using Dapper;
using SZY.Platform.WebApi.Model;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Coded By admin 2019/11/9 13:45:19
namespace SZY.Platform.WebApi.Data
{
    public interface IG40InfoRepo<T> where T : BaseEntity
    {
        Task<G40InfoPageView> GetPageList(G40InfoParm param);
        Task<G40Info> Save(G40Info obj);
        Task<G40Info> GetByID(long id);
        Task<int> Update(G40Info obj);
        Task<int> Delete(string[] ids, int userID);
    }

    public class G40InfoRepo : BaseRepo, IG40InfoRepo<G40Info>
    {
        public G40InfoRepo(DapperOptions options) : base(options) { }

        public async Task<G40InfoPageView> GetPageList(G40InfoParm parm)
        {
            return await WithConnection(async c =>
            {

                StringBuilder sql = new StringBuilder();
                sql.Append($@"  SELECT 
                id,
                road_part,
                camera,
                time,
                carcount,timespan FROM g40_info
                 ");
                StringBuilder whereSql = new StringBuilder();
                //whereSql.Append(" WHERE ai.ProcessInstanceID = '" + parm.ProcessInstanceID + "'");

                //if (parm.AppName != null)
                //{
                //    whereSql.Append(" and ai.AppName like '%" + parm.AppName.Trim() + "%'");
                //}

                sql.Append(whereSql);
                //验证是否有参与到流程中
                //string sqlcheck = sql.ToString();
                //sqlcheck += ("AND ai.CreatedByUserID = '" + parm.UserID + "'");
                //var checkdata = await c.QueryFirstOrDefaultAsync<TaskViewModel>(sqlcheck);
                //if (checkdata == null)
                //{
                //    return null;
                //}

                var data = await c.QueryAsync<G40Info>(sql.ToString());
                var total = data.ToList().Count;
                sql.Append(" order by " + parm.sort + " " + parm.order)
                .Append(" limit " + (parm.page - 1) * parm.rows + "," + parm.rows);
                var ets = await c.QueryAsync<G40Info>(sql.ToString());

                G40InfoPageView ret = new G40InfoPageView();
                ret.rows = ets.ToList();
                ret.total = total;
                return ret;
            });
        }

        public async Task<G40Info> Save(G40Info obj)
        {
            return await WithConnection(async c =>
            {
                string sql = $@" INSERT INTO `g40_info`(
                    
                    road_part,
                    camera,
                    time,
                    carcount,
                    timespan
                ) VALUES 
                (
                    @RoadPart,
                    @Camera,
                    @Time,
                    @Carcount,
                    @Timespan
                    );
                    ";
                sql += "SELECT LAST_INSERT_ID() ";
                int newid = await c.QueryFirstOrDefaultAsync<int>(sql, obj);
                obj.Id = newid;
                return obj;
            });
        }

        public async Task<G40Info> GetByID(long id)
        {
            return await WithConnection(async c =>
            {
                var result = await c.QueryFirstOrDefaultAsync<G40Info>(
                    "SELECT * FROM g40_info WHERE id = @id", new { id = id });
                return result;
            });
        }

        public async Task<int> Update(G40Info obj)
        {
            return await WithConnection(async c =>
            {
                var result = await c.ExecuteAsync($@" UPDATE g40_info set 
                    
                    road_part=@RoadPart,
                    camera=@Camera,
                    time=@Time,
                    carcount=@Carcount,
                    timespan=@Timespan
                 where id=@Id", obj);
                return result;
            });
        }

        public async Task<int> Delete(string[] ids, int userID)
        {
            return await WithConnection(async c =>
            {
                var result = await c.ExecuteAsync(" Update g40_info set is_del=1" +
                ",updated_time=@updated_time,updated_by=@updated_by" +
                " WHERE id in @ids ", new { ids = ids, updated_time = DateTime.Now, updated_by = userID });
                return result;
            });
        }
    }
}



