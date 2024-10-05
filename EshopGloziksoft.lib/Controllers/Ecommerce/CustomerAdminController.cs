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
    public class CustomerAdminController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = "Name", string sortDir = "ASC")
        {
            try
            {
                return GetRecordsView(page, sort, sortDir);
            }
            catch
            {
                CustomerFilterModel filter = GetCustomerFilterForEdit();
                if (filter != null)
                {
                    filter.SearchText = string.Empty;
                    EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
                    repository.Save(this.CurrentSessionId, CustomerFilterModel.CreateCopyFrom(filter));
                }
                return GetRecordsView(page, sort, sortDir);
            }
        }
        ActionResult GetRecordsView(int page, string sort, string sortDir)
        {
            CustomerFilterModel filter = GetCustomerFilterForEdit();

            EshopgloziksoftCustomerRepository repository = new EshopgloziksoftCustomerRepository();
            CustomerListModel model = CustomerListModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir,
                    new EshopgloziksoftCustomerFilter()
                    {
                        SearchText = filter.SearchText,
                    }),
                    new CustomerDropDowns()
                );

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", GetCustomerForEdit());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            EshopgloziksoftCustomerRepository repository = new EshopgloziksoftCustomerRepository();
            CustomerModel model = CustomerModel.CreateCopyFrom(repository.Get(new Guid(id)), new CustomerDropDowns());

            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(CustomerModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsDeliveryAddress)
                {
                    if (string.IsNullOrEmpty(model.DeliveryName))
                    {
                        ModelState.AddModelError("DeliveryName", "Doručovacia firma (meno a priezvisko) musí byť zadané.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryCountryCollectionKey) || model.DeliveryCountryCollectionKey == Guid.Empty.ToString())
                    {
                        ModelState.AddModelError("DeliveryCountryCollectionKey", "Doručovacia krajina musí byť zadaná.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryStreet))
                    {
                        ModelState.AddModelError("DeliveryStreet", "Doručovacia ulica a číslo domu musí byť zadané.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryCity))
                    {
                        ModelState.AddModelError("DeliveryCity", "Doručovacia obec musí byť zadaná.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryZip))
                    {
                        ModelState.AddModelError("DeliveryZip", "Doručovacie PSČ musí byť zadané.");
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshopgloziksoftCustomerRepository repository = new EshopgloziksoftCustomerRepository();
            if (repository.Save(CustomerModel.CreateCopyFrom(model, new CustomerDropDowns())))
            {
                //model.SaveNewsletterSettings();
            }
            else
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceCustomersFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            EshopgloziksoftCustomerRepository repository = new EshopgloziksoftCustomerRepository();
            CustomerModel model = CustomerModel.CreateCopyFrom(repository.Get(new Guid(id)), new CustomerDropDowns());

            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(string keepMember, CustomerModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshopgloziksoftCustomerRepository repository = new EshopgloziksoftCustomerRepository();
            if (!repository.Delete(CustomerModel.CreateCopyFrom(model, new CustomerDropDowns())))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (keepMember == "no")
            {
                EshopgloziksoftMemberRepository memberRep = new EshopgloziksoftMemberRepository();
                EshopgloziksoftMember member = memberRep.Get(model.OwnerId);
                if (member != null)
                {
                    memberRep.Delete(member);
                }
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceCustomersFormId);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetFilter()
        {
            return View(GetCustomerFilterForEdit());
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFilter(CustomerFilterModel model)
        {
            model.ModelErrors.Clear();
            if (model.ModelErrors.Count == 0)
            {
                EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
                if (!repository.Save(this.CurrentSessionId, CustomerFilterModel.CreateCopyFrom(model)))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveFilter(model);
            }

            return RedirectToCurrentUmbracoPageAfterSaveFilter();
        }


        CustomerModel GetCustomerForEdit()
        {
            CustomerModel model = new CustomerModel();
            model.DropDowns = new CustomerDropDowns();

            return model;
        }

        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveFilter(CustomerFilterModel rec = null)
        {
            SetCustomerFilterForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetCustomerFilterForEdit(CustomerFilterModel rec = null)
        {
            TempData["CustomerFilterForEdit"] = rec;
        }
        CustomerFilterModel GetCustomerFilterForEdit()
        {
            if (TempData["CustomerFilterForEdit"] == null)
            {
                EshopgloziksoftUserPropRepository repository = new EshopgloziksoftUserPropRepository();
                TempData["CustomerFilterForEdit"] = CustomerFilterModel.CreateCopyFrom(repository.Get(this.CurrentSessionId, ConfigurationUtil.PropId_CustomerFilterModel));
            }

            return (CustomerFilterModel)TempData["CustomerFilterForEdit"];
        }
    }
}
