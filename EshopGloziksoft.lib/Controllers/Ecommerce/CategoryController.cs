using eshopgloziksoft.lib.Models.Ecommerce;
using eshopgloziksoft.lib.Pdf;
using eshopgloziksoft.lib.Repositories;
using eshopgloziksoft.lib.Tasks.Ecommerce;
using eshopgloziksoft.lib.Util;
using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshopgloziksoft.lib.Controllers.Ecommerce
{
    public class _BaseCategoryController : _BaseController
    {
        public const string ReturnToCategoryKey = "ReturnToCategory";

        public const string ReturnToTabKey = "ReturnToTab";
        public const string ReturnToTabVal_Subcateg = "Subcategories";
        public const string ReturnToTabVal_ProdInCat = "ProductsInCategory";
        public const string ReturnToTabVal_ProdNotInCat = "ProductsNotInCategory";

        public const string ReturnToPageKey = "ReturnToPage";

        public void SetReturnToCategory(Guid categoryKey, string tab)
        {
            TempData[_BaseCategoryController.ReturnToCategoryKey] = categoryKey;
            TempData[_BaseCategoryController.ReturnToTabKey] = tab;
        }
        public void SetReturnToPage(int page)
        {
            TempData[_BaseCategoryController.ReturnToCategoryKey] = page.ToString();
        }
        public string GetReturnToCategory()
        {
            Guid categoryKey = TempData[_BaseCategoryController.ReturnToCategoryKey] == null ? Guid.Empty : (Guid)TempData[_BaseCategoryController.ReturnToCategoryKey];

            return categoryKey == null || categoryKey == Guid.Empty ? null : categoryKey.ToString();
        }
        public string GetReturnToTab()
        {
            return TempData[_BaseCategoryController.ReturnToTabKey] == null ? _BaseCategoryController.ReturnToTabVal_Subcateg : (string)TempData[_BaseCategoryController.ReturnToTabKey];
        }
        public string GetReturnToPage()
        {
            return TempData[_BaseCategoryController.ReturnToPageKey] == null ? "1" : (string)TempData[_BaseCategoryController.ReturnToPageKey];
        }

        public string GetReturnToCategoryQueryString()
        {
            return string.Format("id={0}&tab={1}&page={2}", GetReturnToCategory(), GetReturnToTab(), GetReturnToPage());
        }
    }

    [PluginController("Ecommerce")]
    public class CategoryController : _BaseCategoryController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(string id, string tab)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Try to get category ID after category edit event
                id = GetReturnToCategory();
            }
            EshopgloziksoftCategoryRepository repository = new EshopgloziksoftCategoryRepository();
            CategoryModel model = string.IsNullOrEmpty(id) ? new CategoryModel() : CategoryModel.CreateCopyFrom(repository.Get(new Guid(id)));
            model.LoadRelatives(repository);
            model.TabId = string.IsNullOrEmpty(tab) ? GetReturnToTab() : tab;

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord(string id)
        {
            CategoryModel model = GetEshopgloziksoftCategoryForEdit();
            if (!string.IsNullOrEmpty(id))
            {
                model.ParentCategoryKey = new Guid(id);
                model.LoadRelatives(new EshopgloziksoftCategoryRepository());
            }
            return View("EditRecord", model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            EshopgloziksoftCategoryRepository repository = new EshopgloziksoftCategoryRepository();
            CategoryModel model = string.IsNullOrEmpty(id) ? GetEshopgloziksoftCategoryForEdit() : CategoryModel.CreateCopyFrom(repository.Get(new Guid(id)));
            if (model.Children == null || model.Parents == null)
            {
                model.LoadRelatives(repository);
            }

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(CategoryModel model)
        {
            SetReturnToCategory(model.ParentCategoryKey, _BaseCategoryController.ReturnToTabVal_Subcateg);

            if (!ModelState.IsValid)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            model.ModelErrors.Clear();

            EshopgloziksoftCategoryRepository repository = new EshopgloziksoftCategoryRepository();
            // check category CODE
            EshopgloziksoftCategory dupl = repository.GetForCategoryCode(model.CategoryCode);
            if (dupl != null && dupl.pk != model.pk)
            {
                model.ModelErrors.Add("Zadaný kód kategórie už je použitý pre inú kategóriu.");
            }
            // check category URL
            dupl = repository.GetForCategoryUrl(model.CategoryUrl);
            if (dupl != null && dupl.pk != model.pk)
            {
                model.ModelErrors.Add("Zadané URL už je použité pre inú kategóriu.");
            }
            if (model.ModelErrors.Count == 0)
            {
                if (!repository.Save(CategoryModel.CreateCopyFrom(model)))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceCategoriesFormId, GetReturnToCategoryQueryString());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            EshopgloziksoftCategoryRepository repository = new EshopgloziksoftCategoryRepository();
            CategoryModel model = string.IsNullOrEmpty(id) ? GetEshopgloziksoftCategoryForEdit() : CategoryModel.CreateCopyFrom(repository.Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(CategoryModel model)
        {
            SetReturnToCategory(model.ParentCategoryKey, _BaseCategoryController.ReturnToTabVal_Subcateg);

            if (!ModelState.IsValid)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            model.ModelErrors.Clear();

            if (model.ModelErrors.Count == 0)
            {
                EshopgloziksoftCategoryRepository repository = new EshopgloziksoftCategoryRepository();
                if (!repository.DeleteRecursive(CategoryModel.CreateCopyFrom(model), true))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceCategoriesFormId, GetReturnToCategoryQueryString());
        }

        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecord(CategoryModel rec = null)
        {
            SetEshopgloziksoftCategoryForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetEshopgloziksoftCategoryForEdit(CategoryModel rec = null)
        {
            TempData["EshopgloziksoftCategoryForEdit"] = rec;
        }
        CategoryModel GetEshopgloziksoftCategoryForEdit()
        {
            CategoryModel model = TempData["EshopgloziksoftCategoryForEdit"] == null ? new CategoryModel() : (CategoryModel)TempData["EshopgloziksoftCategoryForEdit"];
            if (model.Children == null || model.Parents == null)
            {
                model.LoadRelatives(new EshopgloziksoftCategoryRepository());
            }

            return model;
        }


        public ActionResult GetCategoryOfferPdf(string id)
        {
            PdfFilePrintResult pdfResult = new CategoryOfferToPdf(new Guid(id), this.DefaultImgPath).GetPdf();

            ActionResult ret = PdfDownloadResult.GetActionResult(pdfResult.FileContent, pdfResult.FileName);
            if (ret == null)
            {
                throw new HttpException(404, "Error generating PDF");
            }

            return ret;
        }
    }
}
