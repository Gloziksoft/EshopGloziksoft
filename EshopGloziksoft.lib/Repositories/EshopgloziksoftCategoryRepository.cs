using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eshopgloziksoft.lib.Repositories
{
    public class EshopgloziksoftCategoryRepository : _BaseRepository
    {
        static readonly object _categoryOrderLock = new object();

        public Page<EshopgloziksoftCategory> GetPage(long page, long itemsPerPage, Guid parentKey, string sortBy = "CategoryOrder", string sortDir = "ASC")
        {
            var sql = GetBaseQuery();
            if (parentKey != null && parentKey != Guid.Empty)
            {
                sql.Where(GetParentWhereClause(), new { Key = parentKey });
            }
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<EshopgloziksoftCategory>(page, itemsPerPage, sql);
        }

        public List<EshopgloziksoftCategory> GetRecordsForParent(Guid parentKey)
        {
            var sql = GetBaseQuery();
            sql.Where(GetParentWhereClause(), new { Key = parentKey });
            sql.Append("ORDER BY CategoryOrder ASC");

            return Fetch<EshopgloziksoftCategory>(sql);
        }

        public EshopgloziksoftCategory Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<EshopgloziksoftCategory>(sql).FirstOrDefault();
        }

        public EshopgloziksoftCategory GetForCategoryCode(string code)
        {
            var sql = GetBaseQuery().Where(GetCodeWhereClause(), new { Code = code });

            return Fetch<EshopgloziksoftCategory>(sql).FirstOrDefault();
        }

        public EshopgloziksoftCategory GetForCategoryUrl(string url)
        {
            var sql = GetBaseQuery().Where(GetUrlWhereClause(), new { Url = url });

            return Fetch<EshopgloziksoftCategory>(sql).FirstOrDefault();
        }

        public bool Save(EshopgloziksoftCategory dataRec)
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

        bool Insert(EshopgloziksoftCategory dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                if ((Guid)result == dataRec.pk)
                {
                    // Record saved
                    // Set category order
                    bool isOk = SetNewCategoryOrder(dataRec);

                    return isOk;
                }
                return (bool)result;
            }

            return false;
        }

        bool Update(EshopgloziksoftCategory dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(EshopgloziksoftCategory dataRec)
        {
            return DeleteInstance(dataRec);
        }

        public bool DeleteRecursive(EshopgloziksoftCategory dataRec, bool reorder)
        {
            // Delete childen
            List<EshopgloziksoftCategory> children = GetRecordsForParent(dataRec.pk);
            foreach (EshopgloziksoftCategory child in children)
            {
                if (!DeleteRecursive(child, false))
                {
                    return false;
                }
            }
            // Delete product links
            DeleteProduct2Category(dataRec.pk);
            // Delete the category
            if (DeleteInstance(dataRec))
            {
                if (reorder)
                {
                    return ReorderChildren(dataRec.ParentCategoryKey);
                }
                return true;
            }

            return false;
        }

        public bool DeleteProduct2Category(Guid categoryKey)
        {
            var sql = new Sql();
            sql.Append(
                string.Format("DELETE {0} WHERE {1}=@CategoryKey", "epProduct2Category", "pkCategory"),
                new { CategoryKey = categoryKey });

            Execute(sql);

            return true;
        }

        public bool ReorderChildren(Guid parentCategoryKey)
        {
            if (parentCategoryKey == null)
            {
                parentCategoryKey = Guid.Empty;
            }

            var sql = new Sql();
            sql.Append(
                string.Format("SELECT pk FROM {0} WHERE ParentCategoryKey=@CategoryKey ORDER BY CategoryOrder ASC", EshopgloziksoftCategory.DbTableName),
                new { CategoryKey = parentCategoryKey });
            List<Guid> guidList = Fetch<Guid>(sql);

            int i = 0;
            foreach (Guid key in guidList)
            {
                SetCategoryOrder(key, ++i);
            }

            return true;
        }

        public bool SetNewCategoryOrder(EshopgloziksoftCategory dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1} = (SELECT MAX({1})+1 FROM {0} WHERE {2}=@ParentCategoryKey) WHERE {3}=@Key",
                EshopgloziksoftCategory.DbTableName, "CategoryOrder", "ParentCategoryKey", "pk"),
                new { ParentCategoryKey = dataRec.ParentCategoryKey, Key = dataRec.pk });

            int cnt = 0;
            lock (_categoryOrderLock)
            {
                cnt = Execute(sql);
            }
            if (cnt > 0)
            {
                return true;
            }

            return false;
        }

        public bool SetCategoryOrder(Guid categoryKey, int order)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1}=@Order WHERE {2}=@Key",
                EshopgloziksoftCategory.DbTableName, "CategoryOrder", "pk"),
                new { Order = order, Key = categoryKey });

            return Execute(sql) > 0;
        }
        public bool SetCategoryOrder(Guid parentKey, int oldOrder, int newOrder)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1}=@NewOrder WHERE {2}=@ParentKey AND {1}=@OldOrder",
                EshopgloziksoftCategory.DbTableName, "CategoryOrder", "ParentCategoryKey"),
                new { ParentKey = parentKey, NewOrder = newOrder, OldOrder = oldOrder });

            return Execute(sql) > 0;
        }

        public int GetCategoryOrder(Guid categoryKey)
        {
            var sql = new Sql();
            sql.Append(
                string.Format("SELECT CategoryOrder FROM {0} WHERE pk=@CategoryKey",
                EshopgloziksoftCategory.DbTableName),
                new { CategoryKey = categoryKey });
            return ExecuteScalar<int>(sql);
        }

        public int GetMaxOrder(Guid parentKey)
        {
            if (parentKey == null)
            {
                parentKey = Guid.Empty;
            }

            var sql = new Sql();
            sql.Append(
                string.Format("SELECT MAX(CategoryOrder) FROM {0} WHERE ParentCategoryKey=@ParentKey",
                EshopgloziksoftCategory.DbTableName),
                new { ParentKey = parentKey });
            return ExecuteScalar<int>(sql);
        }
        public bool MoveCategoryUp(Guid parentKey, Guid categoryKey)
        {
            if (parentKey == null)
            {
                parentKey = Guid.Empty;
            }

            lock (_categoryOrderLock)
            {
                int oldOrder = GetCategoryOrder(categoryKey);
                if (oldOrder > 1)
                {
                    SetCategoryOrder(parentKey, oldOrder - 1, oldOrder);
                    SetCategoryOrder(categoryKey, oldOrder - 1);
                }
            }

            return true;
        }
        public bool MoveCategoryDown(Guid parentKey, Guid categoryKey)
        {
            if (parentKey == null)
            {
                parentKey = Guid.Empty;
            }

            lock (_categoryOrderLock)
            {
                int oldOrder = GetCategoryOrder(categoryKey);
                if (oldOrder < GetMaxOrder(parentKey))
                {
                    SetCategoryOrder(parentKey, oldOrder + 1, oldOrder);
                    SetCategoryOrder(categoryKey, oldOrder + 1);
                }
            }

            return true;
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshopgloziksoftCategory.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", EshopgloziksoftCategory.DbTableName);
        }

        string GetCodeWhereClause()
        {
            return string.Format("{0}.CategoryCode = @Code", EshopgloziksoftCategory.DbTableName);
        }

        string GetUrlWhereClause()
        {
            return string.Format("{0}.CategoryUrl = @Url", EshopgloziksoftCategory.DbTableName);
        }

        string GetParentWhereClause()
        {
            return string.Format("{0}.ParentCategoryKey = @Key", EshopgloziksoftCategory.DbTableName);
        }

        public static string GetParentId(string id)
        {
            EshopgloziksoftCategoryRepository rep = new EshopgloziksoftCategoryRepository();
            EshopgloziksoftCategory cat = rep.Get(new Guid(id));

            return cat != null ? cat.ParentCategoryKey.ToString() : string.Empty;
        }
    }
    [TableName(EshopgloziksoftCategory.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshopgloziksoftCategory : _BaseRepositoryRec
    {
        public const string DbTableName = "epCategory";

        public bool CategoryIsVisible { get; set; }
        public Guid ParentCategoryKey { get; set; }
        public int CategoryOrder { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public string CategoryImg { get; set; }

        public string CategoryOfferText { get; set; }

        public string CategoryUrl { get; set; }
        public string CategoryMetaTitle { get; set; }
        public string CategoryMetaKeywords { get; set; }
        public string CategoryMetaDescription { get; set; }

        public static EshopgloziksoftCategory RootEshopgloziksoftCategory()
        {
            EshopgloziksoftCategory root = new EshopgloziksoftCategory();
            root.pk = Guid.Empty;
            root.CategoryName = "Všetky produkty";
            root.CategoryUrl = CategoryContentFinder.CategoryUrl_All;

            return root;
        }
    }
}
