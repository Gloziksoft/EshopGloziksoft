using NPoco;
using System;
using System.Linq;

namespace eshopgloziksoft.lib.Repositories
{
    public class EshopgloziksoftProductAttributeRepository : _BaseRepository
    {
        static readonly object _productAttributeOrderLock = new object();

        public Page<EshopgloziksoftProductAttribute> GetPage(long page, long itemsPerPage, string sortBy = null, string sortDir = null, EshopgloziksoftProductAttributeFilter filter = null)
        {
            var sql = GetBaseQuery();
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sql.Where(GetSearchTextWhereClause(filter.SearchText), new { SearchText = filter.SearchText });
                }
            }
            if (string.IsNullOrEmpty(sortBy))
            {
                sortBy = "ProductAttributeOrder";
            }
            if (string.IsNullOrEmpty(sortDir))
            {
                sortDir = "ASC";
            }
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<EshopgloziksoftProductAttribute>(page, itemsPerPage, sql);
        }

        public EshopgloziksoftProductAttribute Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<EshopgloziksoftProductAttribute>(sql).FirstOrDefault();
        }

        public bool Save(EshopgloziksoftProductAttribute dataRec)
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

        bool Insert(EshopgloziksoftProductAttribute dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                if ((Guid)result == dataRec.pk)
                {
                    // Record saved
                    // Set product attribute order
                    bool isOk = SetNewProductAttributeOrder(dataRec);

                    return isOk;
                }
                return (bool)result;
            }

            return false;
        }

        bool Update(EshopgloziksoftProductAttribute dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(EshopgloziksoftProductAttribute dataRec)
        {
            if (DeleteInstance(dataRec))
            {
                return ReorderProductAttributes(dataRec.ProductAttributeOrder, -1);
            }

            return false;
        }

        public bool SetNewProductAttributeOrder(EshopgloziksoftProductAttribute dataRec)
        {
            return true;
        }
        public bool ReorderProductAttributes(int orderFrom, int orderOffset)
        {
            return true;
        }

        public bool SetProductAttributeOrder(Guid productAttributeKey, int order)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1}=@Order WHERE {2}=@Key",
                EshopgloziksoftProductAttribute.DbTableName, "ProductAttributeOrder", "pk"),
                new { Order = order, Key = productAttributeKey });

            return Execute(sql) > 0;
        }
        public bool SetProductAttributeOrder(int oldOrder, int newOrder)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1}=@NewOrder WHERE {1}=@OldOrder",
                EshopgloziksoftProductAttribute.DbTableName, "ProductAttributeOrder"),
                new { NewOrder = newOrder, OldOrder = oldOrder });

            return Execute(sql) > 0;
        }

        public int GetProductAttributeOrder(Guid productAttributeKey)
        {
            var sql = new Sql();
            sql.Append(
                string.Format("SELECT ProductAttributeOrder FROM {0} WHERE pk=@ProductAttributeKey",
                EshopgloziksoftProductAttribute.DbTableName),
                new { ProductAttributeKey = productAttributeKey });
            return ExecuteScalar<int>(sql);
        }

        public int GetMaxOrder()
        {
            var sql = new Sql();
            sql.Append(
                string.Format("SELECT MAX(ProductAttributeOrder) FROM {0}",
                EshopgloziksoftProductAttribute.DbTableName));
            return ExecuteScalar<int>(sql);
        }
        public bool MoveProductAttributeUp(Guid productAttributeKey)
        {
            lock (_productAttributeOrderLock)
            {
                int oldOrder = GetProductAttributeOrder(productAttributeKey);
                if (oldOrder > 1)
                {
                    SetProductAttributeOrder(oldOrder - 1, oldOrder);
                    SetProductAttributeOrder(productAttributeKey, oldOrder - 1);
                }
            }

            return true;
        }
        public bool MoveProductAttributeDown(Guid productAttributeKey)
        {
            lock (_productAttributeOrderLock)
            {
                int oldOrder = GetProductAttributeOrder(productAttributeKey);
                if (oldOrder < GetMaxOrder())
                {
                    SetProductAttributeOrder(oldOrder + 1, oldOrder);
                    SetProductAttributeOrder(productAttributeKey, oldOrder + 1);
                }
            }

            return true;
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshopgloziksoftProductAttribute.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", EshopgloziksoftProductAttribute.DbTableName);
        }
        string GetSearchTextWhereClause(string searchText)
        {
            return string.Format("{0}.productAttributeGroup LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.productAttributeName LIKE '%{1}%' collate Latin1_general_CI_AI", EshopgloziksoftProductAttribute.DbTableName, searchText);
        }
    }


    [TableName(EshopgloziksoftProductAttribute.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshopgloziksoftProductAttribute : _BaseRepositoryRec
    {
        public const string DbTableName = "epProductAttribute";

        public string ProductAttributeGroup { get; set; }
        public string ProductAttributeName { get; set; }
        public int ProductAttributeOrder { get; set; }
    }

    public class EshopgloziksoftProductAttributeFilter
    {
        public string SearchText { get; set; }
    }
}
