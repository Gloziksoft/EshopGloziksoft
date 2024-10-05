using eshopgloziksoft.lib.Models;
using eshopgloziksoft.lib.Models.Ecommerce;
using eshopgloziksoft.lib.Repositories;
using eshopgloziksoft.lib.Util;
using System;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshopgloziksoft.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class ProductAttributeController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = null, string sortDir = null)
        {
            try
            {
                return GetRecordsView(page, sort, sortDir);
            }
            catch
            {
                ProductAttributeFilterModel filter = GetEshopgloziksoftProductAttributeFilterForEdit();
                if (filter != null)
                {
                    filter.SearchText = string.Empty;
                    EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
                    repository.Save(this.CurrentSessionId, ProductAttributeFilterModel.CreateCopyFrom(filter));
                }
                return GetRecordsView(page, sort, sortDir);
            }
        }
        ActionResult GetRecordsView(int page, string sort, string sortDir)
        {
            ProductAttributeFilterModel filter = GetEshopgloziksoftProductAttributeFilterForEdit();

            EshopgloziksoftProductAttributeRepository repository = new EshopgloziksoftProductAttributeRepository();
            ProductAttributePagingListModel model = ProductAttributePagingListModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir,
                    new EshopgloziksoftProductAttributeFilter()
                    {
                        SearchText = filter.SearchText
                    })
                );

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", new ProductAttributeModel());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            ProductAttributeModel model = ProductAttributeModel.CreateCopyFrom(new EshopgloziksoftProductAttributeRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(ProductAttributeModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshopgloziksoftProductAttributeRepository repository = new EshopgloziksoftProductAttributeRepository();
            if (!repository.Save(ProductAttributeModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceProductAttributesFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            ProductAttributeModel model = ProductAttributeModel.CreateCopyFrom(new EshopgloziksoftProductAttributeRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(ProductAttributeModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshopgloziksoftProductAttributeRepository repository = new EshopgloziksoftProductAttributeRepository();
            if (!repository.Delete(ProductAttributeModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceProductAttributesFormId);
        }



        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetFilter()
        {
            return View(GetEshopgloziksoftProductAttributeFilterForEdit());
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFilter(ProductAttributeFilterModel model)
        {
            EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
            if (!repository.Save(this.CurrentSessionId, ProductAttributeFilterModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return RedirectToCurrentUmbracoPage();
        }
        ProductAttributeFilterModel GetEshopgloziksoftProductAttributeFilterForEdit()
        {
            return ProductAttributeFilterModel.CreateCopyFrom(new EshopgloziksoftUserPropRepository().Get(this.CurrentSessionId, ConfigurationUtil.PropId_ProductAttributeFilterModel));
        }
    }
}
