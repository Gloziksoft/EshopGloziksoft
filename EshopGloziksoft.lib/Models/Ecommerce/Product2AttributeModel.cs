﻿using eshopgloziksoft.lib.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;

namespace eshopgloziksoft.lib.Models.Ecommerce
{
    public class Product2AttributeModel : _BaseModel
    {
        public List<Product2AttributeItem> Items { get; set; }

        public static List<Product2AttributeItem> LoadItems(Guid productKey)
        {
            List<Product2AttributeItem> allItems = new List<Product2AttributeItem>();
            Hashtable htAttributes = new Hashtable();

            // Load all attributes
            EshopgloziksoftProductAttributeRepository repAttr = new EshopgloziksoftProductAttributeRepository();
            foreach (EshopgloziksoftProductAttribute attr in repAttr.GetPage(1, _PagingModel.AllItemsPerPage).Items)
            {
                Product2AttributeItem item = new Product2AttributeItem()
                {
                    ProductKey = productKey,
                    AttributeKey = attr.pk,
                    Title = attr.ProductAttributeName,
                    Group = attr.ProductAttributeGroup,
                    IsSelected = false
                };

                htAttributes.Add(attr.pk, item);
                allItems.Add(item);
            }

            // Update attributes with values for the product
            EshopgloziksoftProduct2AttributeRepository rep = new EshopgloziksoftProduct2AttributeRepository();
            List<EshopgloziksoftProduct2Attribute> dataList = rep.GetForProduct(productKey);
            foreach (EshopgloziksoftProduct2Attribute dataRec in dataList)
            {
                if (htAttributes.ContainsKey(dataRec.PkAttribute))
                {
                    Product2AttributeItem item = (Product2AttributeItem)htAttributes[dataRec.PkAttribute];
                    item.IsSelected = true;
                }
            }

            return allItems;
        }

        public List<List<int>> GetItemIndexesInGroups()
        {
            List<List<int>> ret = new List<List<int>>();
            Hashtable htGroup = new Hashtable();
            for (int idx = 0; idx < this.Items.Count; idx++)
            {
                if (!htGroup.ContainsKey(this.Items[idx].Group))
                {
                    ret.Add(new List<int>());
                    htGroup.Add(this.Items[idx].Group, ret[ret.Count - 1]);
                }
                ((List<int>)htGroup[this.Items[idx].Group]).Add(idx);
            }

            return ret;
        }
    }

    public class Product2AttributeItem : _BaseModel
    {
        public Guid ProductKey { get; set; }
        public Guid AttributeKey { get; set; }
        public bool IsSelected { get; set; }
        public string Title { get; set; }
        public string Group { get; set; }

        public Product2AttributeItem()
        {
        }

        public void CopyDataFrom(EshopgloziksoftProduct2Attribute src)
        {
            this.pk = src.pk;
            this.ProductKey = src.PkProduct;
            this.AttributeKey = src.PkAttribute;
        }

        public void CopyDataTo(EshopgloziksoftProduct2Attribute trg)
        {
            trg.pk = this.pk;
            trg.PkProduct = this.ProductKey;
            trg.PkAttribute = this.AttributeKey;
        }

        public static Product2AttributeItem CreateCopyFrom(EshopgloziksoftProduct2Attribute src)
        {
            Product2AttributeItem trg = new Product2AttributeItem();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static EshopgloziksoftProduct2Attribute CreateCopyFrom(Product2AttributeItem src)
        {
            EshopgloziksoftProduct2Attribute trg = new EshopgloziksoftProduct2Attribute();
            src.CopyDataTo(trg);

            return trg;
        }
    }
}
