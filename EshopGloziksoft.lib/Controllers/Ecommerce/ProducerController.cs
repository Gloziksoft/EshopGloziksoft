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
    public class ProducerController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = "ProducerName", string sortDir = "ASC")
        {
            try
            {
                return GetRecordsView(page, sort, sortDir);
            }
            catch
            {
                ProducerFilterModel filter = GetEshopgloziksoftProducerFilterForEdit();
                if (filter != null)
                {
                    filter.SearchText = string.Empty;
                    EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
                    repository.Save(this.CurrentSessionId, ProducerFilterModel.CreateCopyFrom(filter));
                }
                return GetRecordsView(page, sort, sortDir);
            }
        }
        ActionResult GetRecordsView(int page, string sort, string sortDir)
        {
            ProducerFilterModel filter = GetEshopgloziksoftProducerFilterForEdit();

            EshopgloziksoftProducerRepository repository = new EshopgloziksoftProducerRepository();
            ProducerPagingListModel model = ProducerPagingListModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir,
                    new EshopgloziksoftProducerFilter()
                    {
                        SearchText = filter.SearchText
                    })
                );

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", new ProducerModel());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            ProducerModel model = ProducerModel.CreateCopyFrom(new EshopgloziksoftProducerRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(ProducerModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (!new EshopgloziksoftProducerRepository().Save(ProducerModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceProducersFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            ProducerModel model = ProducerModel.CreateCopyFrom(new EshopgloziksoftProducerRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(ProducerModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshopgloziksoftProducerRepository repository = new EshopgloziksoftProducerRepository();
            try
            {
                if (!repository.Delete(ProducerModel.CreateCopyFrom(model)))
                {
                    ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", "Výrobcu nie je možné odstrániť pretože je priradený k niektorým produktom.");
                this.Logger.Error(typeof(ProducerController), "DeleteRecord error", exc);
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceProducersFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetFilter()
        {
            return View(GetEshopgloziksoftProducerFilterForEdit());
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFilter(ProducerFilterModel model)
        {
            model.ModelErrors.Clear();
            if (model.ModelErrors.Count == 0)
            {
                EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
                if (!repository.Save(this.CurrentSessionId, ProducerFilterModel.CreateCopyFrom(model)))
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
        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecordFilter(ProducerFilterModel rec = null)
        {
            SetEshopgloziksoftProducerFilterForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetEshopgloziksoftProducerFilterForEdit(ProducerFilterModel rec = null)
        {
            TempData["stirilabProducerFilterForEdit"] = rec;
        }
        ProducerFilterModel GetEshopgloziksoftProducerFilterForEdit()
        {
            if (TempData["stirilabProducerFilterForEdit"] == null)
            {
                EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
                TempData["stirilabProducerFilterForEdit"] = ProducerFilterModel.CreateCopyFrom(repository.Get(this.CurrentSessionId, ConfigurationUtil.PropId_ProducerFilterModel));
            }

            return (ProducerFilterModel)TempData["stirilabProducerFilterForEdit"];
        }
    }
}
