using eshopgloziksoft.lib.Models;
using eshopgloziksoft.lib.Models.Ecommerce;
using eshopgloziksoft.lib.Repositories;
using eshopgloziksoft.lib.Util;
using System;
using System.Collections;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshopgloziksoft.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class ProductController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = "ProductOrder", string sortDir = "DESC")
        {
            try
            {
                return GetRecordsView(page, sort, sortDir);
            }
            catch
            {
                ProductFilterModel filter = GetEshopgloziksoftProductFilterForEdit();
                if (filter != null)
                {
                    filter.SearchText = string.Empty;
                    EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
                    repository.Save(this.CurrentSessionId, ProductFilterModel.CreateCopyFrom(filter));
                }
                return GetRecordsView(page, sort, sortDir);
            }
        }
        ActionResult GetRecordsView(int page, string sort, string sortDir)
        {
            ProductFilterModel filter = GetEshopgloziksoftProductFilterForEdit();

            EshopgloziksoftProductRepository repository = new EshopgloziksoftProductRepository();
            ProductPagingListModel model = ProductPagingListModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir,
                    new EshopgloziksoftProductFilter()
                    {
                        ProductCode = filter.ProductCode,
                        SearchText = filter.SearchText
                    }),
                GetEshopgloziksoftProductDropDowns()
                );

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", GetEshopgloziksoftProductForEdit());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            EshopgloziksoftProductRepository repository = new EshopgloziksoftProductRepository();
            ProductModel model;
            if (string.IsNullOrEmpty(id))
            {
                model = GetEshopgloziksoftProductForEdit();
            }
            else
            {
                model = ProductModel.CreateCopyFrom(repository.Get(new Guid(id)), GetEshopgloziksoftProductDropDowns());
                model.ProductAttributes.pk = model.pk;
                model.ProductAttributes.Items = Product2AttributeModel.LoadItems(model.pk);
                model.ProductRelations.pk = model.pk;
                model.ProductRelations.Items = ProductRelationModel.LoadItems(model.pk);
                model.ProductCategories.LoadCategories(this, model.pk);
            }

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            model.ModelErrors.Clear();

            EshopgloziksoftProductRepository repository = new EshopgloziksoftProductRepository();
            // check product CODE
            EshopgloziksoftProduct dupl = repository.GetForProductCode(model.ProductCode);
            if (dupl != null && dupl.pk != model.pk)
            {
                model.ModelErrors.Add("Zadaný kód produktu už je použitý pre iný produkt.");
            }
            // check product URL
            dupl = repository.GetForProductUrl(model.ProductUrl);
            if (dupl != null && dupl.pk != model.pk)
            {
                model.ModelErrors.Add("Zadané URL už je použité pre iný produkt.");
            }

            if (model.ModelErrors.Count == 0)
            {
                EshopgloziksoftProduct dataRecord = ProductModel.CreateCopyFrom(model, GetEshopgloziksoftProductDropDowns());
                if (repository.Save(dataRecord))
                {
                    model.pk = dataRecord.pk;
                }
                else
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
                if (model.ModelErrors.Count == 0)
                {
                    #region Product attributes
                    model.ProductAttributes.pk = dataRecord.pk; // set the current product key

                    EshopgloziksoftProduct2AttributeRepository repAttr = new EshopgloziksoftProduct2AttributeRepository();
                    if (!repAttr.DeleteForProduct(model.ProductAttributes.pk))
                    {
                        model.ModelErrors.Add("Nastala chyba pri zápise vlastností produktu systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                    }
                    else
                    {
                        if (model.ProductAttributes.Items != null)
                        {
                            foreach (Product2AttributeItem item in model.ProductAttributes.Items)
                            {
                                item.ProductKey = dataRecord.pk; // set the current product key
                                if (item.IsSelected)
                                {
                                    if (!repAttr.Insert(Product2AttributeItem.CreateCopyFrom(item)))
                                    {
                                        model.ModelErrors.Add("Nastala chyba pri zápise vlastností produktu systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                if (model.ModelErrors.Count == 0)
                {
                    #region Product relations
                    model.ProductRelations.pk = dataRecord.pk; // set the current product key

                    EshopgloziksoftProductRelationRepository repRel = new EshopgloziksoftProductRelationRepository();
                    if (!repRel.DeleteForProduct(model.ProductRelations.pk))
                    {
                        model.ModelErrors.Add("Nastala chyba pri zápise súvisiacich produktov do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                    }
                    else
                    {
                        if (model.ProductRelations.Items != null)
                        {
                            Hashtable htDupl = new Hashtable();
                            foreach (ProductRelationItem item in model.ProductRelations.Items)
                            {
                                if (htDupl.ContainsKey(item.PkProductRelated))
                                {
                                    continue;
                                }
                                htDupl.Add(item.PkProductRelated, item);
                                item.PkProductMain = dataRecord.pk; // set the current product key
                                item.PkProductRelated = item.PkProductRelated; // set the related product key
                                if (!repRel.Insert(ProductRelationItem.CreateCopyFrom(item)))
                                {
                                    model.ModelErrors.Add("Nastala chyba pri zápise súvisiacich produktov do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                                    break;
                                }
                            }
                        }
                    }
                    #endregion
                }
                if (model.ModelErrors.Count == 0)
                {
                    #region Product categories
                    Guid pkProduct = model.pk;
                    EshopgloziksoftProduct2CategoryRepository repCat = new EshopgloziksoftProduct2CategoryRepository();
                    if (model.ProductCategories.SelectedCategories != null && model.ProductCategories.SelectedCategories.Count > 0)
                    {
                        // Set product to selected categories
                        Hashtable htCatSaved = new Hashtable();
                        foreach (string categoryKey in model.ProductCategories.SelectedCategories)
                        {
                            if (htCatSaved.ContainsKey(categoryKey))
                            {
                                continue;
                            }
                            htCatSaved.Add(categoryKey, categoryKey);

                            Guid pkCategory = new Guid(categoryKey);
                            if (repCat.Get(pkCategory, pkProduct) == null)
                            {
                                // Insert product to category
                                EshopgloziksoftProduct2Category item = new EshopgloziksoftProduct2Category();
                                item.PkProduct = pkProduct;
                                item.PkCategory = pkCategory;
                                if (!repCat.Insert(item))
                                {
                                    model.ModelErrors.Add("Nastala chyba pri zápise kategórií produktu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                                    break;
                                }
                            }
                        }

                        // Delete product from not selected categories
                        foreach (EshopgloziksoftProduct2Category item in repCat.GetForProduct(pkProduct))
                        {
                            if (!htCatSaved.ContainsKey(item.PkCategory.ToString()))
                            {
                                // Delete product from category
                                repCat.Delete(item);
                            }
                        }
                    }
                    else
                    {
                        // Remove product from all categories
                        if (!repCat.DeleteForProduct(model.pk))
                        {
                            model.ModelErrors.Add("Nastala chyba pri zápise kategórií produktu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                        }
                    }
                    #endregion
                }
            }

            if (model.ModelErrors.Count > 0)
            {
                if (model.ProductRelations.Items != null)
                {
                    model.ProductRelations.SetRelatedProducts();
                }
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            ProductFilterModel filter = GetEshopgloziksoftProductFilterForEdit();
            if (filter != null)
            {
                filter.SearchText = string.Empty;
                filter.ProductCode = string.Empty;
                new EshopgloziksoftUserPropRepository().Save(this.CurrentSessionId, ProductFilterModel.CreateCopyFrom(filter));
            }


            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceProductsFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            EshopgloziksoftProductRepository repository = new EshopgloziksoftProductRepository();
            ProductModel model = string.IsNullOrEmpty(id) ? GetEshopgloziksoftProductForEdit() : ProductModel.CreateCopyFrom(repository.Get(new Guid(id)), GetEshopgloziksoftProductDropDowns());

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            model.ModelErrors.Clear();

            if (model.ModelErrors.Count == 0)
            {
                EshopgloziksoftProductRepository repository = new EshopgloziksoftProductRepository();
                if (!repository.Delete(ProductModel.CreateCopyFrom(model, GetEshopgloziksoftProductDropDowns())))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceProductsFormId);
        }


        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecord(ProductModel rec = null)
        {
            SetEshopgloziksoftProductForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetEshopgloziksoftProductForEdit(ProductModel rec = null)
        {
            TempData["EshopgloziksoftProductForEdit"] = rec;
        }
        ProductModel GetEshopgloziksoftProductForEdit()
        {
            ProductModel model = TempData["EshopgloziksoftProductForEdit"] == null ? new ProductModel() : (ProductModel)TempData["EshopgloziksoftProductForEdit"];
            if (model.ProductAttributes.Items == null)
            {
                model.ProductAttributes.pk = model.pk;
                model.ProductAttributes.Items = Product2AttributeModel.LoadItems(model.ProductAttributes.pk);
            }
            if (model.ProductRelations.Items == null)
            {
                model.ProductRelations.pk = model.pk;
                model.ProductRelations.Items = ProductRelationModel.LoadItems(model.ProductRelations.pk);
            }
            if (model.ProductCategories.SelectedCategories == null)
            {
                model.ProductCategories.LoadCategories(this, model.pk);
            }
            if (model.ProductCategories.AllCategories == null)
            {
                model.ProductCategories.UpdateSelectedCategories(this);
            }
            if (model.DropDowns == null)
            {
                model.DropDowns = GetEshopgloziksoftProductDropDowns();
            }

            return model;
        }
        ProductModelDropDowns GetEshopgloziksoftProductDropDowns()
        {
            return new ProductModelDropDowns();
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetFilter()
        {
            return View(GetEshopgloziksoftProductFilterForEdit());
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFilter(ProductFilterModel model)
        {
            model.ModelErrors.Clear();
            if (model.ModelErrors.Count == 0)
            {
                EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
                if (!repository.Save(this.CurrentSessionId, ProductFilterModel.CreateCopyFrom(model)))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecordFilter(model);
            }

            return RedirectToCurrentUmbracoPageAfterSaveRecordFilter();
        }
        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecordFilter(ProductFilterModel rec = null)
        {
            SetEshopgloziksoftProductFilterForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetEshopgloziksoftProductFilterForEdit(ProductFilterModel rec = null)
        {
            TempData["takfajnProductFilterForEdit"] = rec;
        }
        ProductFilterModel GetEshopgloziksoftProductFilterForEdit()
        {
            if (TempData["takfajnProductFilterForEdit"] == null)
            {
                EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
                TempData["takfajnProductFilterForEdit"] = ProductFilterModel.CreateCopyFrom(repository.Get(this.CurrentSessionId, ConfigurationUtil.PropId_ProductFilterModel));
            }

            return (ProductFilterModel)TempData["takfajnProductFilterForEdit"];
        }



        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditImages(string id)
        {
            ProductImagesModel model = ProductImagesModel.LoadModel(new Guid(id));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveImages(ProductImagesModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            model.Save();

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceProductsFormId);
        }
    }
}
