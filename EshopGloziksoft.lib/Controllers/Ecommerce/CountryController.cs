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
    public class CountryController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = "Code", string sortDir = "ASC")
        {
            CountryPagingListModel model = CountryPagingListModel.CreateCopyFrom(
                new CountryRepository().GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir));

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", GetCountryForEdit(null));
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            return View(GetCountryForEdit(id));
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(CountryModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (!new CountryRepository().Save(CountryModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceCountriesFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            return View(GetCountryForEdit(id));
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(CountryModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (!new CountryRepository().Delete(CountryModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }

            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshopgloziksoftUmbracoPage(ConfigurationUtil.EcommerceCountriesFormId);
        }


        CountryModel GetCountryForEdit(string id)
        {
            return string.IsNullOrEmpty(id) ? new CountryModel() : CountryModel.CreateCopyFrom(new CountryRepository().Get(new Guid(id)));
        }
    }
}
