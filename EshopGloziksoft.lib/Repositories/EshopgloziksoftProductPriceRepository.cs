using eshopgloziksoft.lib.Models.Ecommerce;
using eshopgloziksoft.lib.Util;
using NPoco;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace eshopgloziksoft.lib.Repositories
{
    public class EshopgloziksoftProductPriceRepository : _BaseRepository
    {
        public List<EshopgloziksoftProductPrice> GetForProduct(Guid productKey)
        {
            var sql = GetBaseQuery().Where(GetProductWhereClause(), new { ProductKey = productKey });
            sql.Append("ORDER BY ValidFrom DESC, ValidTo DESC");

            return Fetch<EshopgloziksoftProductPrice>(sql);
        }

        public List<EshopgloziksoftProductPrice> GetForDate(DateTime dateAt)
        {
            var sql = GetBaseQuery().Where(GetValidAtWhereClause(), new { ValidAt = dateAt });

            return Fetch<EshopgloziksoftProductPrice>(sql);
        }

        public EshopgloziksoftProductPrice Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<EshopgloziksoftProductPrice>(sql).FirstOrDefault();
        }

        public EshopgloziksoftProductPrice GetStandardPrice(Guid productKey)
        {
            var sql = GetBaseQuery().Where(GetProductWhereClause(), new { ProductKey = productKey });
            sql.Where(GetStandardPriceWhereClause());

            return Fetch<EshopgloziksoftProductPrice>(sql).FirstOrDefault();
        }

        public bool Save(EshopgloziksoftProductPrice dataRec)
        {
            EshopgloziksoftProductPriceCache.ClearCache();

            if (IsNew(dataRec))
            {
                return Insert(dataRec);
            }
            else
            {
                return Update(dataRec);
            }
        }

        bool Insert(EshopgloziksoftProductPrice dataRec)
        {
            bool finalResult = false;

            dataRec.pk = Guid.NewGuid();

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                bool transOk = false;
                try
                {
                    scope.Database.BeginTransaction();
                    object result = scope.Database.Insert<EshopgloziksoftProductPrice>(dataRec);

                    if (result is Guid)
                    {
                        if ((Guid)result == dataRec.pk)
                        {
                            if (dataRec.ValidTo == null)
                            {
                                // New standard price
                                // Set valid to date for previous standard price
                                var sql = new Sql(string.Format("UPDATE {0} SET validTo=@ValidTo", EshopgloziksoftProductPrice.DbTableName),
                                    new { ValidTo = dataRec.ValidFrom.AddDays(-1) });
                                sql.Where(GetNegativeBaseWhereClause(), new { Key = dataRec.pk });
                                sql.Where(GetProductWhereClause(), new { ProductKey = dataRec.ProductKey });
                                sql.Where(GetStandardPriceWhereClause());
                                scope.Database.Execute(sql);
                            }
                            scope.Database.CompleteTransaction();
                            transOk = true;
                            finalResult = true;
                        }
                    }

                }
                finally
                {
                    if (!transOk)
                    {
                        scope.Database.AbortTransaction();
                    }
                    scope.Complete();
                }
            }

            return finalResult;
        }

        bool Update(EshopgloziksoftProductPrice dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(EshopgloziksoftProductPrice dataRec)
        {
            EshopgloziksoftProductPriceCache.ClearCache();

            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshopgloziksoftProductPrice.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", EshopgloziksoftProductPrice.DbTableName);
        }
        string GetNegativeBaseWhereClause()
        {
            return string.Format("{0}.pk <> @Key", EshopgloziksoftProductPrice.DbTableName);
        }
        string GetProductWhereClause()
        {
            return string.Format("{0}.productKey = @ProductKey", EshopgloziksoftProductPrice.DbTableName);
        }
        string GetStandardPriceWhereClause()
        {
            return string.Format("{0}.validTo IS NULL", EshopgloziksoftProductPrice.DbTableName);
        }
        string GetValidAtWhereClause()
        {
            return string.Format("{0}.validFrom <= @ValidAt AND ({0}.validTo IS NULL OR {0}.validTo >= @ValidAt)", EshopgloziksoftProductPrice.DbTableName);
        }
    }

    [TableName(EshopgloziksoftProductPrice.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshopgloziksoftProductPrice : _BaseRepositoryRec
    {
        public const string DbTableName = "epProductPrice";

        public Guid ProductKey { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public decimal VatRate { get; set; }
        public decimal Price_1_NoVat { get; set; }
        public decimal Price_1_WithVat { get; set; }

        public static EshopgloziksoftProductPrice CreateUnknownPrice(Guid procutKey)
        {
            return new EshopgloziksoftProductPrice()
            {
                ProductKey = procutKey,
                ValidFrom = new DateTime(2019, 1, 1),
                Price_1_NoVat = 1000000,
                Price_1_WithVat = 1000000,
            };
        }
    }

    public class EshopgloziksoftProductPriceCache
    {
        static readonly object _cacheLock = new object();
        private static Hashtable htCache = null;
        private static DateTime htCreatedAt;

        public static void ClearCache()
        {
            lock (_cacheLock)
            {
                EshopgloziksoftProductPriceCache.htCache = null;
            }
        }

        public static EshopgloziksoftProductPriceInfo GetProductPrice(Guid productKey)
        {
            EshopgloziksoftProductPricesAtDate pricesCollection;

            lock (_cacheLock)
            {
                if (htCache != null && htCreatedAt < DateTime.Now.AddDays(-1))
                {
                    // Clear cache older than 24 hours
                    htCache = null;
                }
                if (htCache == null)
                {
                    htCache = new Hashtable();
                    htCreatedAt = DateTime.Now;
                }

                DateTime today = DateTime.Today;
                string key = DateTimeUtil.GetDateId(today);
                if (!htCache.ContainsKey(key))
                {
                    htCache.Add(key, new EshopgloziksoftProductPricesAtDate(today));
                }

                pricesCollection = (EshopgloziksoftProductPricesAtDate)htCache[key];
            }

            return pricesCollection.GetProductPrice(productKey);
        }
    }

    public class EshopgloziksoftProductPricesAtDate
    {
        private Hashtable htProduct;

        public EshopgloziksoftProductPricesAtDate(DateTime dateAt)
        {
            this.htProduct = new Hashtable();
            foreach (EshopgloziksoftProductPrice price in new EshopgloziksoftProductPriceRepository().GetForDate(dateAt))
            {
                EshopgloziksoftProductPriceInfo priceInfo;
                string key = price.ProductKey.ToString();
                if (!this.htProduct.ContainsKey(key))
                {
                    this.htProduct.Add(key, priceInfo = new EshopgloziksoftProductPriceInfo(dateAt));
                }
                else
                {
                    priceInfo = (EshopgloziksoftProductPriceInfo)htProduct[key];
                }

                priceInfo.SetPrice(price);
            }
        }

        public EshopgloziksoftProductPriceInfo GetProductPrice(Guid productKey)
        {
            string key = productKey.ToString();

            EshopgloziksoftProductPriceInfo productPriceInfo = null;
            if (!htProduct.ContainsKey(key))
            {
                this.htProduct.Add(key, productPriceInfo = new EshopgloziksoftProductPriceInfo(DateTime.Today));
                productPriceInfo.SetPrice(EshopgloziksoftProductPrice.CreateUnknownPrice(productKey));
            }

            productPriceInfo = (EshopgloziksoftProductPriceInfo)htProduct[key];
            productPriceInfo.EnsureStandardPrice();

            return productPriceInfo;
        }
    }

    public class EshopgloziksoftProductPriceInfo
    {
        public DateTime ValidAt { get; private set; }
        public EshopgloziksoftProductPrice StandardPrice { get; private set; }
        public ProductPriceModel StandardPriceModel
        {
            get
            {
                return ProductPriceModel.CreateCopyFrom(this.StandardPrice);
            }
        }
        public EshopgloziksoftProductPrice ActionPrice { get; private set; }
        public ProductPriceModel ActionPriceModel
        {
            get
            {
                return ProductPriceModel.CreateCopyFrom(this.ActionPrice);
            }
        }
        public EshopgloziksoftProductPrice CurrentPrice
        {
            get
            {
                return this.ActionPrice != null ? this.ActionPrice : this.StandardPrice;
            }
        }
        public ProductPriceModel CurrentPriceModel
        {
            get
            {
                return ProductPriceModel.CreateCopyFrom(this.CurrentPrice);
            }
        }

        public EshopgloziksoftProductPriceInfo(DateTime dateAt)
        {
            this.ValidAt = dateAt;
        }

        public void SetPrice(EshopgloziksoftProductPrice price)
        {
            if (price.ValidTo == null)
            {
                // Standard price
                if (this.StandardPrice == null || this.StandardPrice.ValidFrom < price.ValidFrom)
                {
                    this.StandardPrice = price;
                }
            }
            else
            {
                // Action price
                if (this.ActionPrice == null || this.ActionPrice.ValidFrom < price.ValidFrom)
                {
                    this.ActionPrice = price;
                }
            }
        }
        public void EnsureStandardPrice()
        {
            if (this.StandardPrice == null)
            {
                if (this.ActionPrice == null)
                {
                    this.StandardPrice = new EshopgloziksoftProductPrice();
                }
                else
                {
                    this.StandardPrice = this.ActionPrice;
                }
            }
        }

        public decimal GetCurrentPriceNoVat()
        {
            return this.CurrentPrice.Price_1_NoVat;
        }
        public decimal GetCurrentPriceVatPerc()
        {
            return this.CurrentPrice.VatRate;
        }

        public decimal GetCurrentPriceWithVat()
        {
            return this.CurrentPrice.Price_1_WithVat;
        }
    }

    public class EshopgloziksoftProductPriceComparer : IComparer<EshopgloziksoftProductPrice>
    {
        public int Compare(EshopgloziksoftProductPrice x, EshopgloziksoftProductPrice y)
        {
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }

            return decimal.Compare(x.Price_1_NoVat, y.Price_1_NoVat);
        }
    }
}
