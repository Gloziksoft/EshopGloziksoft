using dufeksoft.lib.Model;
using dufeksoft.lib.UI;
using eshopgloziksoft.lib.Repositories;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eshopgloziksoft.lib.Models.Ecommerce
{
    public class LoginModel : _BaseModel
    {
        [Display(Name = "Užívateľské meno")]
        [Required(ErrorMessage = "Užívateľské meno musí byť zadané")]
        public string Username { get; set; }

        [Display(Name = "Heslo")]
        [Required(ErrorMessage = "Heslo musí byť zadané")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class LostPasswordModel : _BaseModel
    {
        [Display(Name = "Váš e-mail")]
        [Email(ErrorMessage = "Neplatný tvar e-mailovej adresy")]
        [Required(ErrorMessage = "Váš e-mail musí byť zadaný")]
        public string Email { get; set; }
    }

    public class ChangePasswordModel : _BaseModel
    {
        [Required(ErrorMessage = "Staré heslo musí byť zadané")]
        [Display(Name = "Staré heslo")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Nové heslo musí byť zadané")]
        [Display(Name = "Nové heslo")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Zopakované nové heslo musí byť zadané")]
        [Display(Name = "Zopakované nové heslo")]
        public string NewPasswordRepeat { get; set; }
    }

    public class EshopgloziksoftMemberModel : _BaseModel
    {
        [Display(Name = "Id")]
        [Required(ErrorMessage = "Id musí byť zadané")]
        public int MemberId { get; set; }
        [Email(ErrorMessage = "Neplatný tvar e-mailovej adresy")]
        [Display(Name = "Užívateľské meno (e-mail)")]
        [Required(ErrorMessage = "Užívateľské meno (e-mail) musí byť zadané")]
        public string Name { get; set; }

        [Display(Name = "Povolený")]
        public bool IsApproved { get; set; }
        [Display(Name = "Uzamknutý")]
        public bool IsLockedOut { get; set; }


        [Display(Name = "Administrátor")]
        public bool IsAdminUser { get; set; }
        [Display(Name = "Zákazník")]
        public bool IsCustomerUser { get; set; }

        [Display(Name = "Heslo")]
        public string Password { get; set; }
        [Display(Name = "Zopakované heslo")]
        public string PasswordRepeat { get; set; }

        public string IsCustomerEdit { get; set; }

        public override bool IsNew
        {
            get
            {
                return this.MemberId <= 0;
            }
        }

        public void CopyDataFrom(EshopgloziksoftMember src)
        {
            this.MemberId = src.MemberId;
            this.Name = src.Name;
            this.IsApproved = src.IsApproved;
            this.IsLockedOut = src.IsLockedOut;
            this.IsAdminUser = src.IsAdminUser;
            this.IsCustomerUser = src.IsCustomerUser;
            this.Password = src.Password;
            this.PasswordRepeat = src.PasswordRepeat;
        }

        public void CopyDataTo(EshopgloziksoftMember trg)
        {
            trg.MemberId = this.MemberId;
            trg.Name = this.Name;
            trg.Username = this.Name;
            trg.Email = this.Name;
            trg.IsApproved = this.IsApproved;
            trg.IsLockedOut = this.IsLockedOut;
            trg.IsAdminUser = this.IsAdminUser;
            trg.IsCustomerUser = this.IsCustomerUser;
            trg.Password = this.Password;
            trg.PasswordRepeat = this.PasswordRepeat;
        }

        public static EshopgloziksoftMemberModel CreateCopyFrom(EshopgloziksoftMember src)
        {
            EshopgloziksoftMemberModel trg = new EshopgloziksoftMemberModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static EshopgloziksoftMember CreateCopyFrom(EshopgloziksoftMemberModel src)
        {
            EshopgloziksoftMember trg = new EshopgloziksoftMember();
            src.CopyDataTo(trg);

            return trg;
        }

        public static EshopgloziksoftMember CreateCopyFrom(CustomerRegisterModel src)
        {
            EshopgloziksoftMember trg = new EshopgloziksoftMember();
            trg.MemberId = 0;
            trg.Name = src.Email;
            trg.Username = src.Email;
            trg.Email = src.Email;
            trg.IsApproved = true;
            trg.IsLockedOut = false;
            trg.IsAdminUser = false;
            trg.IsCustomerUser = true;

            trg.Password = src.RegisterPassword;
            trg.PasswordRepeat = src.RepeatRegisterPassword;

            return trg;
        }
    }

    public class EshopgloziksoftMemberListModel : _PagingModel
    {
        public List<EshopgloziksoftMemberModel> Items { get; set; }

        public static EshopgloziksoftMemberListModel CreateCopyFrom(List<EshopgloziksoftMember> srcArray)
        {
            EshopgloziksoftMemberListModel trgArray = new EshopgloziksoftMemberListModel();
            trgArray.ItemsPerPage = (int)srcArray.Count;
            trgArray.TotalItems = (int)srcArray.Count;
            trgArray.Items = new List<EshopgloziksoftMemberModel>(srcArray.Count + 1);

            foreach (EshopgloziksoftMember src in srcArray)
            {
                trgArray.Items.Add(EshopgloziksoftMemberModel.CreateCopyFrom(src));
            }

            return trgArray;
        }
    }

    public class EshopgloziksoftMemberDropDown : CmpDropDown
    {
        public EshopgloziksoftMemberDropDown()
        {
        }

        public static EshopgloziksoftMemberDropDown CreateCustomerUserListFromRepository(bool allowNull, string emptyText = "[ žiadny užívateľ ]", bool allowAll = false, string allText = "[ všetci užívatelia ]")
        {
            EshopgloziksoftMemberRepository memberRepository = new EshopgloziksoftMemberRepository();
            return EshopgloziksoftMemberDropDown.CreateCopyFrom(memberRepository.GetCustomerUsers(), allowNull, emptyText, allowAll, allText);
        }

        public static EshopgloziksoftMemberDropDown CreateCopyFrom(List<EshopgloziksoftMember> memberList, bool allowNull, string emptyText, bool allowAll, string allText)
        {
            EshopgloziksoftMemberDropDown ret = new EshopgloziksoftMemberDropDown();

            if (allowNull)
            {
                ret.AddItem(emptyText, "0", null);
            }
            if (allowAll)
            {
                ret.AddItem(allText, "-1", null);
            }
            foreach (EshopgloziksoftMember memberItem in memberList)
            {
                EshopgloziksoftMemberModel memberModel = EshopgloziksoftMemberModel.CreateCopyFrom(memberItem);
                ret.AddItem(memberItem.Name, memberItem.MemberId.ToString(), memberModel);
            }

            return ret;
        }
    }
}
