using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eshopgloziksoft.lib.Repositories
{
    public class EshopgloziksoftProduct2AttributeRepository : _BaseRepository
    {
        public List<EshopgloziksoftProduct2Attribute> GetForProduct(Guid keyProduct)
        {
            var sql = GetBaseQuery().Where(GetProductWhereClause(), new { KeyProduct = keyProduct });

            return Fetch<EshopgloziksoftProduct2Attribute>(sql);
        }

        public List<EshopgloziksoftProduct2AttributeEx> GetForProducts(List<string> productKeyList)
        {
            var sql = new Sql();
            sql.Append(string.Format("SELECT {0}.PkAttribute, {0}.PkProduct, {1}.ProductAttributeName, {1}.ProductAttributeOrder FROM {0}, {1}", EshopgloziksoftProduct2Attribute.DbTableName, EshopgloziksoftProductAttribute.DbTableName));
            sql.Where(GetProductInWhereClause(productKeyList));
            sql.Where(string.Format("{0}.PkAttribute = {1}.pk", EshopgloziksoftProduct2Attribute.DbTableName, EshopgloziksoftProductAttribute.DbTableName));
            sql.Append(string.Format("ORDER BY {0}.PkProduct, {1}.ProductAttributeOrder", EshopgloziksoftProduct2Attribute.DbTableName, EshopgloziksoftProductAttribute.DbTableName));


            return Fetch<EshopgloziksoftProduct2AttributeEx>(sql);
        }

        public List<Guid> GetProductAttributeKeysForProductCategories(List<string> productCategoryKeyList)
        {
            var sql = new Sql(string.Format("SELECT DISTINCT({0}.PkAttribute) FROM {0}", EshopgloziksoftProduct2Attribute.DbTableName));
            if (productCategoryKeyList != null)
            {
                sql.Where(GetProductCategoryInWhereClause(productCategoryKeyList));
            }

            return Fetch<Guid>(sql);
        }


        public EshopgloziksoftProduct2Attribute Get(Guid keyAttribute, Guid keyProduct)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { KeyAttribute = keyAttribute, KeyProduct = keyProduct });

            return Fetch<EshopgloziksoftProduct2Attribute>(sql).FirstOrDefault();
        }

        public bool Insert(EshopgloziksoftProduct2Attribute dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("INSERT INTO {0} (PkAttribute, PkProduct) VALUES (@PkAttribute, @PkProduct)",
                EshopgloziksoftProduct2Attribute.DbTableName),
                new { PkAttribute = dataRec.PkAttribute, PkProduct = dataRec.PkProduct });

            return Execute(sql) > 0;
        }

        public bool Delete(EshopgloziksoftProduct2Attribute dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("DELETE {0} WHERE {1}=@PkAttribute AND {2}=@PkProduct",
                EshopgloziksoftProduct2Attribute.DbTableName, "PkAttribute", "PkProduct"),
                new { PkAttribute = dataRec.PkAttribute, PkProduct = dataRec.PkProduct });

            return Execute(sql) > 0;
        }

        public bool DeleteForProduct(Guid productKey)
        {
            bool isOK = true;
            List<EshopgloziksoftProduct2Attribute> dataList = GetForProduct(productKey);
            foreach (EshopgloziksoftProduct2Attribute dataRec in dataList)
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
            return new Sql(string.Format("SELECT * FROM {0}", EshopgloziksoftProduct2Attribute.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.PkAttribute = @KeyAttribute AND {0}.PkProduct = @KeyProduct", EshopgloziksoftProduct2Attribute.DbTableName);
        }

        string GetProductWhereClause()
        {
            return string.Format("{0}.PkProduct = @KeyProduct", EshopgloziksoftProduct2Attribute.DbTableName);
        }
        string GetAttributeWhereClause()
        {
            return string.Format("{0}.PkAttribute = @KeyAttribute", EshopgloziksoftProduct2Attribute.DbTableName);
        }
        string GetProductInWhereClause(List<string> productKeyList)
        {
            StringBuilder strIn = new StringBuilder();
            foreach (string productKey in productKeyList)
            {
                if (strIn.Length > 0)
                {
                    strIn.Append(",");
                }
                strIn.Append(string.Format("'{0}'", productKey));
            }
            return string.Format("{0}.PkProduct IN ({1})", EshopgloziksoftProduct2Attribute.DbTableName, strIn.ToString());
        }
        string GetProductCategoryInWhereClause(List<string> productCategoryKeyList)
        {
            return string.Format("{0}.PkProduct IN (SELECT {1}.pkProduct FROM {1} WHERE {1}.pkCategory IN ({2}))", EshopgloziksoftProduct2Attribute.DbTableName, EshopgloziksoftProduct2Category.DbTableName, GetKeysForInClause(productCategoryKeyList));
        }
    }

    [TableName(EshopgloziksoftProduct2Attribute.DbTableName)]
    public class EshopgloziksoftProduct2Attribute : _BaseRepositoryRec
    {
        public const string DbTableName = "epProduct2Attribute";

        public Guid PkAttribute { get; set; }
        public Guid PkProduct { get; set; }
    }

    public class EshopgloziksoftProduct2AttributeEx : _BaseRepositoryRec
    {
        public Guid PkAttribute { get; set; }
        public Guid PkProduct { get; set; }
        public string ProductAttributeName { get; set; }
        public int ProductAttributeOrder { get; set; }
    }
}
