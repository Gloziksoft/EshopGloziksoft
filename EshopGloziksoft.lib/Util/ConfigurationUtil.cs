using System.Configuration;

namespace eshopgloziksoft.lib.Util
{
    public abstract class ConfigurationUtil
    {
        public const string Ecommerce_Document_Contract_1 = "Platba";
        public const string Ecommerce_Document_Contract_2 = "Platba";
        public const string GetCfgValue = "Platba";



        public const string Ecommerce_Quote_TransportItemCode = "DOPRAVA";
        public const string Ecommerce_Quote_PaymentItemCode = "PLATBA";
        public const string Ecommerce_Quote_DiscountItemCode = "ZĽAVA";

        public const string Ecommerce_Quote_InfMsgId = "quote-info-msg";
        public const string Ecommerce_Quote_ModalMsgId = "quote-modal-msg";

        public const string Ecommerce_Quote_InitialState = "eshopgloziksoft.Ecommerce.Quote.InitialState";
        public const string Ecommerce_Quote_PaidPriceState = "eshopgloziksoft.Ecommerce.Quote.PaidPriceState";


        public const string EcommerceAfterLoginFormId = "eshopgloziksoft.Ecommerce.AfterLoginFormId";
        public const string EcommerceRegistrationOkFormId = "eshopgloziksoft.Ecommerce.RegistrationOkFormId";
        public const string EcommerceMembersFormId = "eshopgloziksoft.Ecommerce.MembersFormId";
        public const string EcommerceCustomersFormId = "eshopgloziksoft.Ecommerce.CustomersFormId";
        public const string EcommerceCountriesFormId = "eshopgloziksoft.Ecommerce.CountriesFormId";
        public const string EcommerceProducersFormId = "eshopgloziksoft.Ecommerce.ProducersFormId";
        public const string EcommerceAvailabilitiesFormId = "eshopgloziksoft.Ecommerce.AvailabilitiesFormId";
        public const string EcommerceTransportTypesFormId = "eshopgloziksoft.Ecommerce.TransportTypesFormId";
        public const string EcommercePaymentTypesFormId = "eshopgloziksoft.Ecommerce.PaymentTypesFormId";
        public const string EcommerceSimpleStringsFormId = "eshopgloziksoft.Ecommerce.SimpleStringsFormId";
        public const string EcommercePaymentStatesFormId = "eshopgloziksoft.Ecommerce.PaymentStatesFormId";
        public const string EcommerceQuoteStatesFormId = "eshopgloziksoft.Ecommerce.QuoteStatesFormId";
        public const string EcommerceCategoriesFormId = "eshopgloziksoft.Ecommerce.CategoriesFormId";
        public const string EcommerceProductAttributesFormId = "eshopgloziksoft.Ecommerce.ProductAttributesFormId";
        public const string EcommerceProductsFormId = "eshopgloziksoft.Ecommerce.ProductsFormId";
        public const string EcommerceProductPricesFormId = "eshopgloziksoft.Ecommerce.ProductPricesFormId";
        public const string EcommerceQuotesFormId = "eshopgloziksoft.Ecommerce.QuotesFormId";
        public const string EcommerceQuotesEditFormId = "eshopgloziksoft.Ecommerce.QuotesEditFormId";

        public const string Ecommerce_ProductPublic_DetailPageId = "eshopgloziksoft.Ecommerce.ProductPublic_DetailPageId";
        public const string Ecommerce_ProductPublic_CategoryPageId = "eshopgloziksoft.Ecommerce.ProductPublic_CategoryPageId";

        public const string Ecommerce_Basket_DeliveryDataPageId = "eshopgloziksoft.Ecommerce.Basket_DeliveryDataPageId";
        public const string Ecommerce_Basket_ReviewAndSendPageId = "eshopgloziksoft.Ecommerce.Basket_ReviewAndSendPageId";
        public const string Ecommerce_Basket_FinishedPageId = "eshopgloziksoft.Ecommerce.Basket_FinishedPageId";

        public const string PropId_CustomerFilterModel = "PropId_CustomerFilterModel";
        public const string PropId_ProducerFilterModel = "PropId_ProducerFilterModel";
        public const string PropId_ProductFilterModel = "PropId_ProductFilterModel";
        public const string PropId_ProductInCategoryFilterModel = "PropId_ProductInCategoryFilterModel";
        public const string PropId_ProductNotInCategoryFilterModel = "PropId_ProductNotInCategoryFilterModel";
        public const string PropId_CategoryPublicFilterModel_PageSize = "PropId_CategoryPublicFilterModel_PageSize";
        public const string PropId_CategoryPublicFilterModel_ProductView = "PropId_CategoryPublicFilterModel_ProductView";
        public const string PropId_CategoryPublicFilterModel_ProductSort = "PropId_CategoryPublicFilterModel_ProductSort";
        public const string PropId_CategoryPublicFilterModel_CurrentCategory = "PropId_CategoryPublicFilterModel_CurrentCategory";
        public const string PropId_CategoryPublicFilterModel_Producer = "PropId_CategoryPublicFilterModel_Producer";
        public const string PropId_CategoryPublicFilterModel_ProductAttribute = "PropId_CategoryPublicFilterModel_ProductAttribute";
        public const string PropId_ProductAttributeFilterModel = "PropId_ProductAttributeFilterModel";
        public const string PropId_QuoteListFilterModel = "PropId_QuoteListFilterModel";

        public static string InitialQuoteState()
        {
            return ConfigurationManager.AppSettings[ConfigurationUtil.Ecommerce_Quote_InitialState];
        }
        public static string PaiedQuotePriceState()
        {
            return ConfigurationManager.AppSettings[ConfigurationUtil.Ecommerce_Quote_PaidPriceState];
        }

        public static int GetPageId(string pageKey)
        {
            return int.Parse(ConfigurationManager.AppSettings[pageKey]);
        }

        public static string EshopRootUrl
        {
            get
            {
                return "/eshop/kategoria/-vsetko-";
            }
        }
        public static string QuoteViewUrl
        {
            get
            {
                return "/e-shop/detail-objednavky";
            }
        }
    }
}
