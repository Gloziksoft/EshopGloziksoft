using NPoco;
using System;
using System.Linq;

namespace eshopgloziksoft.lib.Repositories
{
    public class EshopgloziksoftUserPropRepository : _BaseRepository
    {
        public EshopgloziksoftUserProp Get(string sessionId, string propId)
        {
            System.Web.Security.MembershipUser user = System.Web.Security.Membership.GetUser();
            if (user == null)
            {
                return Get(0, sessionId, propId);
            }

            return Get((int)user.ProviderUserKey, sessionId, propId);
        }

        public EshopgloziksoftUserProp Get(int userId, string sessionId, string propId)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { PropId = propId });
            // Use session id identifier
            sql.Where(GetSessionWhereClause(), new { SessionId = sessionId });

            return Fetch<EshopgloziksoftUserProp>(sql).FirstOrDefault();
        }

        public bool Save(string sessionId, EshopgloziksoftUserProp dataRec)
        {
            if (IsNew(dataRec))
            {
                return Insert(sessionId, dataRec);
            }
            else
            {
                return Update(dataRec);
            }
        }

        bool Insert(string sessionId, EshopgloziksoftUserProp dataRec)
        {
            // Use session id identifier
            dataRec.UserId = 0;
            dataRec.SessionId = sessionId;

            dataRec.pk = Guid.NewGuid();
            dataRec.DateCreate = DateTime.Now;

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(EshopgloziksoftUserProp dataRec)
        {
            dataRec.DateCreate = DateTime.Now; // update date create
            return UpdateInstance(dataRec);
        }

        public bool Delete(EshopgloziksoftUserProp dataRec)
        {
            return DeleteInstance(dataRec);
        }
        public void Delete(string sessionId, string propId)
        {
            EshopgloziksoftUserProp dataRec = Get(sessionId, propId);
            if (dataRec != null)
            {
                Delete(dataRec);
            }
        }

        Sql GetBaseQuery()
        {
            var sql = new Sql(string.Format("SELECT * FROM {0}", EshopgloziksoftUserProp.DbTableName));

            return sql;
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.propId = @PropId", EshopgloziksoftUserProp.DbTableName);
        }
        string GetUserWhereClause()
        {
            return string.Format("{0}.userId = @UserId", EshopgloziksoftUserProp.DbTableName);
        }
        string GetSessionWhereClause()
        {
            return string.Format("{0}.sessionId = @SessionId", EshopgloziksoftUserProp.DbTableName);
        }

        public bool DeleteOldSessionData(DateTime dt)
        {
            var sql = new Sql();
            sql.Append(string.Format("DELETE FROM {0}", EshopgloziksoftUserProp.DbTableName));
            sql.Where(string.Format("{0}.sessionId IS NOT NULL AND {0}.dateCreate < @DateCreate", EshopgloziksoftUserProp.DbTableName), new { DateCreate = dt });
            Execute(sql);

            return true;
        }

    }

    [TableName(EshopgloziksoftUserProp.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshopgloziksoftUserProp : _BaseRepositoryRec
    {
        public const string DbTableName = "epUserProp";

        public int UserId { get; set; }
        public string SessionId { get; set; }
        public DateTime DateCreate { get; set; }
        public string PropId { get; set; }
        public string PropValue { get; set; }
    }
}
