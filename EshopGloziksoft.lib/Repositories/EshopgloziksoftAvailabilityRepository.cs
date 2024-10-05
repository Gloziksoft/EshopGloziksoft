using NPoco;
using System;
using System.Linq;

namespace eshopgloziksoft.lib.Repositories
{
    public class EshopgloziksoftAvailabilityRepository : _BaseRepository
    {
        public Page<EshopgloziksoftAvailability> GetPage(long page, long itemsPerPage, string sortBy = "AvailabilityName", string sortDir = "ASC")
        {
            var sql = GetBaseQuery();
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<EshopgloziksoftAvailability>(page, itemsPerPage, sql);
        }

        public EshopgloziksoftAvailability Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<EshopgloziksoftAvailability>(sql).FirstOrDefault();
        }

        public bool Save(EshopgloziksoftAvailability dataRec)
        {
            if (IsNew(dataRec))
            {
                return Insert(dataRec);
            }
            else
            {
                return Update(dataRec);
            }
        }

        bool Insert(EshopgloziksoftAvailability dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(EshopgloziksoftAvailability dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(EshopgloziksoftAvailability dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshopgloziksoftAvailability.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", EshopgloziksoftAvailability.DbTableName);
        }
    }


    [TableName(EshopgloziksoftAvailability.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshopgloziksoftAvailability : _BaseRepositoryRec
    {
        public const string DbTableName = "epAvailability";

        public string AvailabilityName { get; set; }
    }
}
