using NPoco;
using System;
using System.Collections.Generic;

namespace eshopgloziksoft.lib.Repositories
{
    public class EshopgloziksoftProductRelationRepository : _BaseRepository
    {
        public List<EshopgloziksoftProductRelation> GetForProduct(Guid productKey)
        {
            var sql = GetBaseQuery().Where(GetProductMainWhereClause(), new { KeyProductMain = productKey });

            return Fetch<EshopgloziksoftProductRelation>(sql);
        }

        public bool Insert(EshopgloziksoftProductRelation dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("INSERT INTO {0} (pkProductMain, pkProductRelated) VALUES (@PkProductMain, @PkProductRelated)",
                EshopgloziksoftProductRelation.DbTableName),
                new { PkProductMain = dataRec.PkProductMain, PkProductRelated = dataRec.PkProductRelated });

            return Execute(sql) > 0;
        }

        public bool Delete(EshopgloziksoftProductRelation dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("DELETE {0} WHERE pkProductMain=@PkProductMain AND pkProductRelated=@PkProductRelated", EshopgloziksoftProductRelation.DbTableName),
                new { PkProductMain = dataRec.PkProductMain, PkProductRelated = dataRec.PkProductRelated });

            return Execute(sql) > 0;
        }

        public bool DeleteForProduct(Guid productKey)
        {
            bool isOK = true;
            List<EshopgloziksoftProductRelation> dataList = GetForProduct(productKey);
            foreach (EshopgloziksoftProductRelation dataRec in dataList)
            {
                if (!Delete(dataRec))
                {
                    isOK = false;
                }
            }

            return isOK;
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshopgloziksoftProductRelation.DbTableName));
        }

        string GetProductMainWhereClause()
        {
            return string.Format("{0}.pkProductMain = @KeyProductMain", EshopgloziksoftProductRelation.DbTableName);
        }
    }

    [TableName(EshopgloziksoftProductRelation.DbTableName)]
    public class EshopgloziksoftProductRelation : _BaseRepositoryRec
    {
        public const string DbTableName = "epProductRelation";

        public Guid PkProductMain { get; set; }
        public Guid PkProductRelated { get; set; }
    }
}
