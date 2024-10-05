using NPoco;
using System;
using System.Linq;

namespace eshopgloziksoft.lib.Repositories
{
    public class EshopgloziksoftProducerRepository : _BaseRepository
    {
        public Page<EshopgloziksoftProducer> GetPage(long page, long itemsPerPage, string sortBy = "ProducerName", string sortDir = "ASC", EshopgloziksoftProducerFilter filter = null)
        {
            var sql = GetBaseQuery();
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sql.Where(GetSearchTextWhereClause(filter.SearchText), new { SearchText = filter.SearchText });
                }
            }
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<EshopgloziksoftProducer>(page, itemsPerPage, sql);
        }

        public EshopgloziksoftProducer Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<EshopgloziksoftProducer>(sql).FirstOrDefault();
        }

        public bool Save(EshopgloziksoftProducer dataRec)
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

        bool Insert(EshopgloziksoftProducer dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(EshopgloziksoftProducer dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(EshopgloziksoftProducer dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshopgloziksoftProducer.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", EshopgloziksoftProducer.DbTableName);
        }
        string GetSearchTextWhereClause(string searchText)
        {
            return string.Format("{0}.producerName LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.producerDescription LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.producerWeb LIKE '%{1}%' collate Latin1_general_CI_AI", EshopgloziksoftProducer.DbTableName, searchText);
        }
    }


    [TableName(EshopgloziksoftProducer.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshopgloziksoftProducer : _BaseRepositoryRec
    {
        public const string DbTableName = "epProducer";

        public string ProducerName { get; set; }
        public string ProducerDescription { get; set; }
        public string ProducerWeb { get; set; }
    }

    public class EshopgloziksoftProducerFilter
    {
        public string SearchText { get; set; }
    }
}
