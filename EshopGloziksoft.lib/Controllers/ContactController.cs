using eshopgloziksoft.lib.Models;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshopgloziksoft.lib.Controllers
{
    [PluginController("EshopGloziksoft")]
    public class ContactController : _BaseController
    {
        public ActionResult Index()
        {
            return View("ContactForm", new EshopgloziksoftContactModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleFormSubmit(EshopgloziksoftContactModel model)
        {
            if (ModelState.IsValid)
            {
                if (!new ApiKeyValidator().IsValid(model.Password, model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Musíte označiť, že nie ste robot.");
                }
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            TempData["success"] = model.SendContactRequest();

            return RedirectToCurrentUmbracoPage();
        }
    }
}
