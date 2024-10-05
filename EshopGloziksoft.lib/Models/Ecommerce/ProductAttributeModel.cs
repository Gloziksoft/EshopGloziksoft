using dufeksoft.lib.Model.Grid;
using dufeksoft.lib.ParamSet;
using dufeksoft.lib.UI;
using eshopgloziksoft.lib.Repositories;
using eshopgloziksoft.lib.Util;
using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Xml;

namespace eshopgloziksoft.lib.Models.Ecommerce
{
    public class ProductAttributeModel : _BaseModel
    {
        [Display(Name = "Poradie")]
        public int ProductAttributeOrder { get; set; }

        [Required(ErrorMessage = "Skupina musí byť zadaná")]
        [Display(Name = "Skupina")]
        public string ProductAttributeGroup { get; set; }

        [Required(ErrorMessage = "Názov vlastnosti musí byť zadaný")]
        [Display(Name = "Názov vlastnosti")]
        public string ProductAttributeName { get; set; }

        public void CopyDataFrom(EshopgloziksoftProductAttribute src)
        {
            this.pk = src.pk;
            this.ProductAttributeOrder = src.ProductAttributeOrder;
            this.ProductAttributeName = src.ProductAttributeName;
            this.ProductAttributeGroup = src.ProductAttributeGroup;
        }

        public void CopyDataTo(EshopgloziksoftProductAttribute trg)
        {
            trg.pk = this.pk;
            trg.ProductAttributeOrder = this.ProductAttributeOrder;
            trg.ProductAttributeName = this.ProductAttributeName;
            trg.ProductAttributeGroup = this.ProductAttributeGroup;
        }

        public static ProductAttributeModel CreateCopyFrom(EshopgloziksoftProductAttribute src)
        {
            ProductAttributeModel trg = new ProductAttributeModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static EshopgloziksoftProductAttribute CreateCopyFrom(ProductAttributeModel src)
        {
            EshopgloziksoftProductAttribute trg = new EshopgloziksoftProductAttribute();
            src.CopyDataTo(trg);

            return trg;
        }
    }

    public class ProductAttributeListModel : List<ProductAttributeModel>
    {
        public HttpRequest CurrentRequest { get; private set; }
        public string SessionId { get; set; }
        public int PageSize { get; private set; }

        private GridPagerModel currentPager;
        public GridPagerModel Pager
        {
            get
            {
                return GetPager();
            }
        }

        public ProductAttributeListModel(HttpRequest request, int pageSize = 25)
        {
            this.CurrentRequest = request;
            this.PageSize = pageSize;
        }

        public List<ProductAttributeModel> GetPageItems()
        {
            GridPageInfo cpi = this.Pager.GetCurrentPageInfo();

            List<ProductAttributeModel> resultList = new List<ProductAttributeModel>();
            for (int i = cpi.FirsItemIndex; i < this.Count && i < cpi.LastItemIndex + 1; i++)
            {
                resultList.Add(this[i]);
            }

            return resultList;
        }

        GridPagerModel GetPager()
        {
            if (this.currentPager == null || this.currentPager.ItemCnt != this.Count)
            {
                this.currentPager = new GridPagerModel(this.CurrentRequest, this.Count, this.PageSize);
            }

            return this.currentPager;
        }
    }

    public class ProductAttributePagingListModel : _PagingModel
    {
        public List<ProductAttributeModel> Items { get; set; }

        public static ProductAttributePagingListModel CreateCopyFrom(Page<EshopgloziksoftProductAttribute> srcArray)
        {
            ProductAttributePagingListModel trgArray = new ProductAttributePagingListModel();
            trgArray.ItemsPerPage = (int)srcArray.ItemsPerPage;
            trgArray.TotalItems = (int)srcArray.TotalItems;
            trgArray.Items = new List<ProductAttributeModel>(srcArray.Items.Count + 1);

            foreach (EshopgloziksoftProductAttribute src in srcArray.Items)
            {
                trgArray.Items.Add(ProductAttributeModel.CreateCopyFrom(src));
            }

            return trgArray;
        }
    }

    public class ProductAttributeFilterModel : _BaseUserPropModel
    {

        [Display(Name = "Vyhľadávanie (skupina, názov ...)")]
        public string SearchText { get; set; }


        public ProductAttributeFilterModel()
        {
            this.PropId = ConfigurationUtil.PropId_ProductAttributeFilterModel;
        }

        public static ProductAttributeFilterModel CreateCopyFrom(EshopgloziksoftUserProp src)
        {
            ProductAttributeFilterModel trg = new ProductAttributeFilterModel();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshopgloziksoftUserProp CreateCopyFrom(ProductAttributeFilterModel src)
        {
            src.UpdateAfterEdit();
            EshopgloziksoftUserProp trg = new EshopgloziksoftUserProp();
            src.CopyDataTo(trg);

            return trg;
        }


        public void UpdateBeforeEdit()
        {
            LoadPropValue(this.PropValue);
        }

        public void UpdateAfterEdit()
        {
            this.PropValue = SavePropValue();
        }

        private string SavePropValue()
        {
            // Create XML document
            XmlDocument doc = new XmlDocument();
            // Create main element
            XmlElement mainNode = doc.CreateElement("ProductAttributeFilterModel");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            // Search text
            XmlParamSet.SaveItem(doc, mainNode, "SearchText", this.SearchText);

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "ProductAttributeFilterModel";

                // Search text
                this.SearchText = XmlParamSet.LoadItem(doc, fullParent, "SearchText", string.Empty);
            }
        }
    }

    public class ProductAttributeDropDown : CmpDropDown
    {
        public ProductAttributeDropDown()
        {
        }

        public static ProductAttributeDropDown CreateFromRepository(bool allowNull, string emptyText = "< žiadna vlastnosť >")
        {
            EshopgloziksoftProductAttributeRepository repository = new EshopgloziksoftProductAttributeRepository();
            return ProductAttributeDropDown.CreateCopyFrom(repository.GetPage(1, _PagingModel.AllItemsPerPage), allowNull, emptyText);
        }

        public static ProductAttributeDropDown CreateCopyFrom(Page<EshopgloziksoftProductAttribute> dataList, bool allowNull, string emptyText)
        {
            ProductAttributeDropDown ret = new ProductAttributeDropDown();

            if (allowNull)
            {
                ret.AddItem(emptyText, Guid.Empty.ToString(), null);
            }
            foreach (EshopgloziksoftProductAttribute dataItem in dataList.Items)
            {
                ProductAttributeModel dataModel = ProductAttributeModel.CreateCopyFrom(dataItem);
                ret.AddItem(string.Format("{0} / {1}", dataModel.ProductAttributeGroup, dataModel.ProductAttributeName), dataModel.pk.ToString(), dataModel);
            }

            return ret;
        }
    }
}
