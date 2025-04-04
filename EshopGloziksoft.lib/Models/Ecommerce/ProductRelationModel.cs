﻿using eshopgloziksoft.lib.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;

namespace eshopgloziksoft.lib.Models.Ecommerce
{
    public class ProductRelationModel : _BaseModel
    {
        public List<ProductRelationItem> Items { get; set; }

        public static List<ProductRelationItem> LoadItems(Guid productKey)
        {
            List<ProductRelationItem> allItems = new List<ProductRelationItem>();

            // Load all relations
            foreach (EshopgloziksoftProductRelation relation in new EshopgloziksoftProductRelationRepository().GetForProduct(productKey))
            {
                ProductRelationItem item = new ProductRelationItem()
                {
                    PkProductMain = productKey,
                    PkProductRelated = relation.PkProductRelated,
                };

                allItems.Add(item);
            }

            // Set related products
            SetRelatedProducts(allItems);

            // Sort result list by product name
            allItems.Sort(new ProductRelationItemComparer());

            return allItems;
        }

        public void SetRelatedProducts()
        {
            SetRelatedProducts(this.Items);
        }

        public static void SetRelatedProducts(List<ProductRelationItem> itemList)
        {
            List<string> relatedProductsKeyList = new List<string>();
            Hashtable htRelated = new Hashtable();
            foreach (ProductRelationItem item in itemList)
            {
                htRelated.Add(item.PkProductRelated, item);
                relatedProductsKeyList.Add(item.PkProductRelated.ToString());
            }


            List<EshopgloziksoftProduct> dataList = new EshopgloziksoftProductRepository().GetPage(1, _PagingModel.AllItemsPerPage,
                filter: new EshopgloziksoftProductFilter()
                {
                    ProductKeyList = relatedProductsKeyList
                }).Items;
            foreach (EshopgloziksoftProduct relatedProduct in dataList)
            {
                if (htRelated.ContainsKey(relatedProduct.pk))
                {
                    ProductRelationItem item = (ProductRelationItem)htRelated[relatedProduct.pk];
                    item.RelatedProduct = relatedProduct;
                }
            }
        }
    }

    public class ProductRelationItem : _BaseModel
    {
        public Guid PkProductMain { get; set; }
        public Guid PkProductRelated { get; set; }
        public EshopgloziksoftProduct RelatedProduct { get; set; }

        public ProductRelationItem()
        {
        }

        public void CopyDataFrom(EshopgloziksoftProductRelation src)
        {
            this.pk = src.pk;
            this.PkProductMain = src.PkProductMain;
            this.PkProductRelated = src.PkProductRelated;
        }

        public void CopyDataTo(EshopgloziksoftProductRelation trg)
        {
            trg.pk = this.pk;
            trg.PkProductMain = this.PkProductMain;
            trg.PkProductRelated = this.PkProductRelated;
        }

        public static ProductRelationItem CreateCopyFrom(EshopgloziksoftProductRelation src)
        {
            ProductRelationItem trg = new ProductRelationItem();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static EshopgloziksoftProductRelation CreateCopyFrom(ProductRelationItem src)
        {
            EshopgloziksoftProductRelation trg = new EshopgloziksoftProductRelation();
            src.CopyDataTo(trg);

            return trg;
        }
    }

    public class ProductRelationItemComparer : IComparer<ProductRelationItem>
    {
        public int Compare(ProductRelationItem x, ProductRelationItem y)
        {
            return string.Compare(x.RelatedProduct.ProductName, y.RelatedProduct.ProductName);
        }
    }
}
