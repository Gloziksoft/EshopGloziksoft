using eshopgloziksoft.lib.Models.Ecommerce;
using NPoco;
using System;
using System.Collections;
using System.Linq;

namespace eshopgloziksoft.lib.Repositories
{
    public class EshopgloziksoftCustomerRepository : _BaseRepository
    {
        public Page<EshopgloziksoftCustomer> GetPage(long page, long itemsPerPage, string sortBy = "Name", string sortDir = "ASC", EshopgloziksoftCustomerFilter filter = null)
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

            return GetPage<EshopgloziksoftCustomer>(page, itemsPerPage, sql);
        }

        public EshopgloziksoftCustomer Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<EshopgloziksoftCustomer>(sql).FirstOrDefault();
        }

        public EshopgloziksoftCustomer GetForOwner(int ownerId)
        {
            var sql = GetBaseQuery().Where(GetOwnerWhereClause(), new { Id = ownerId });

            return Fetch<EshopgloziksoftCustomer>(sql).FirstOrDefault();
        }

        public bool Save(EshopgloziksoftCustomer dataRec)
        {
            if (dataRec.DeliveryCountryKey == Guid.Empty)
            {
                dataRec.DeliveryCountryKey = null;
            }
            if (IsNew(dataRec))
            {
                return Insert(dataRec);
            }
            else
            {
                return Update(dataRec);
            }
        }

        bool Insert(EshopgloziksoftCustomer dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            EshopgloziksoftCustomerCache.RemoveFromCache(dataRec.OwnerId);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(EshopgloziksoftCustomer dataRec)
        {
            bool result = UpdateInstance(dataRec);
            EshopgloziksoftCustomerCache.RemoveFromCache(dataRec.OwnerId);

            return result;
        }

        public bool Delete(EshopgloziksoftCustomer dataRec)
        {
            bool result = DeleteInstance(dataRec);
            EshopgloziksoftCustomerCache.RemoveFromCache(dataRec.OwnerId);

            return result;
        }

        Sql GetBaseQuery()
        {
            var sql = new Sql(string.Format("SELECT * FROM {0}", EshopgloziksoftCustomer.DbTableName));

            return sql;
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", EshopgloziksoftCustomer.DbTableName);
        }

        string GetOwnerWhereClause()
        {
            return string.Format("{0}.ownerId = @Id", EshopgloziksoftCustomer.DbTableName);
        }

        string GetSearchTextWhereClause(string searchText)
        {
            return string.Format("{0}.Name LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.Email LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.Phone LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.Street LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.City LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.Zip LIKE '%{1}%' collate Latin1_general_CI_AI", EshopgloziksoftCustomer.DbTableName, searchText);
        }
    }

    [TableName(EshopgloziksoftCustomer.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshopgloziksoftCustomer : _BaseRepositoryRec
    {
        public const string DbTableName = "epCustomer";

        public int OwnerId { get; set; }

        public string Name { get; set; }
        public Guid CountryKey { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string Ico { get; set; }
        public string Dic { get; set; }
        public string Icdph { get; set; }
        public string ContactName { get; set; }

        public bool IsDeliveryAddress { get; set; }
        public string DeliveryName { get; set; }
        public Guid? DeliveryCountryKey { get; set; }
        public string DeliveryStreet { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryZip { get; set; }
        public string DeliveryPhone { get; set; }

        public static EshopgloziksoftCustomer CreateCopyFrom(CustomerRegisterModel src, EshopgloziksoftMember member)
        {
            EshopgloziksoftCustomer trg = new EshopgloziksoftCustomer();
            trg.OwnerId = member.MemberId;
            trg.Name = src.Name;
            trg.CountryKey = new Guid(src.CountryCollectionKey);
            trg.Street = src.Street;
            trg.City = src.City;
            trg.Zip = src.Zip;
            trg.Phone = src.Phone;
            trg.Email = src.Email;

            trg.Ico = src.Ico;
            trg.Dic = src.Dic;
            trg.Icdph = src.Icdph;
            trg.ContactName = src.ContactName;

            trg.IsDeliveryAddress = src.IsDeliveryAddress;
            trg.DeliveryName = src.DeliveryName;
            if (string.IsNullOrEmpty(src.DeliveryCountryCollectionKey) || src.DeliveryCountryCollectionKey == Guid.Empty.ToString())
            {
                trg.DeliveryCountryKey = null;
            }
            else
            {
                trg.DeliveryCountryKey = new Guid(src.DeliveryCountryCollectionKey);
            }
            trg.DeliveryStreet = src.DeliveryStreet;
            trg.DeliveryCity = src.DeliveryCity;
            trg.DeliveryZip = src.DeliveryZip;
            trg.DeliveryPhone = src.DeliveryPhone;

            return trg;
        }
    }

    public class EshopgloziksoftCustomerFilter
    {
        public string SearchText { get; set; }
    }

    public class EshopgloziksoftCustomerCache
    {
        private static Hashtable htCustomers = new Hashtable();

        public static string CurrentMemberId
        {
            get
            {
                return EshopgloziksoftCustomerCache.IsCustomerAuthenticated ? CustomerModel.GetCurrentMemberId().ToString() : string.Empty;
            }
        }

        public static EshopgloziksoftCustomer GetCurrentCustomer()
        {
            return GetCustomer(CustomerModel.GetCurrentMemberId());
        }

        public static EshopgloziksoftCustomer GetCustomer(int memberId)
        {
            if (!htCustomers.ContainsKey(memberId))
            {
                htCustomers.Add(memberId, new EshopgloziksoftCustomerRepository().GetForOwner(memberId));
            }

            return (EshopgloziksoftCustomer)htCustomers[memberId];
        }

        public static void RemoveFromCache(int memberId)
        {
            if (htCustomers.ContainsKey(memberId))
            {
                htCustomers.Remove(memberId);
            }
        }

        public static bool IsCustomerAuthenticated
        {
            get
            {
                return System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }
    }
}
