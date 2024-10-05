using eshopgloziksoft.lib.Models.Ecommerce;
using eshopgloziksoft.lib.Repositories;
using eshopgloziksoft.lib.Util;
using System;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshopgloziksoft.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class ProductPriceController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(string productId)
        {
            ProductPriceListModel model = ProductPriceListModel.CreateCopyFrom(
                new EshopgloziksoftProductPriceRepository().GetForProduct(new Guid(productId)));
            model.Product = ProductModel.CreateCopyFrom(new EshopgloziksoftProductRepository().Get(new Guid(productId)), null);

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord(string productId)
        {
            return View("EditRecord", GetEshopgloziksoftProductPriceForEdit(null, productId));
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            return View(GetEshopgloziksoftProductPriceForEdit(id, null));
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(ProductPriceModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            model.ModelErrors.Clear();
            EshopgloziksoftProductPriceRepository repository = new EshopgloziksoftProductPriceRepository();
            EshopgloziksoftProductPrice dataRec = ProductPriceModel.CreateCopyFrom(model);
            if (dataRec.ValidFrom < DateTime.Today)
            {
                ModelState.AddModelError("ValidFrom", "Dátum platí od nie je možné zadávať do minulosti. Najnižší možný dátum je dnešný dátum.");
            }
            if (dataRec.ValidTo != null)
            {
                if (dataRec.ValidTo.Value < dataRec.ValidFrom)
                {
                    ModelState.AddModelError("ValidTo", "Dátum platí do nemôže byť menší ako dátum platí od.");
                }
            }

            if (ModelState.IsValid)
            {
                if (!repository.Save(dataRec))
                {
                    ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceProductPricesFormId, string.Format("productId={0}", model.ProductKey.ToString()));
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            return View(GetEshopgloziksoftProductPriceForEdit(id, null));
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(ProductPriceModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshopgloziksoftProductPriceRepository repository = new EshopgloziksoftProductPriceRepository();
            if (!repository.Delete(ProductPriceModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceProductPricesFormId, string.Format("productId={0}", model.ProductKey.ToString()));
        }


        ProductPriceModel GetEshopgloziksoftProductPriceForEdit(string pk, string productId)
        {
            ProductPriceModel model;
            if (string.IsNullOrEmpty(pk))
            {
                // Create new price as a copy of current standard price 
                model = ProductPriceModel.CreateCopyFrom(new EshopgloziksoftProductPriceRepository().GetStandardPrice(new Guid(productId)));
                model.pk = Guid.Empty;
                model.ProductKey = new Guid(productId);
                model.ValidFrom = DateTimeUtil.GetDisplayDate(DateTime.Today.AddDays(1));
            }
            else
            {
                model = ProductPriceModel.CreateCopyFrom(new EshopgloziksoftProductPriceRepository().Get(new Guid(pk)));
            }

            if (model.Product == null)
            {
                model.Product = ProductModel.CreateCopyFrom(new EshopgloziksoftProductRepository().Get(model.ProductKey), null);
            }

            return model;
        }
    }
}
