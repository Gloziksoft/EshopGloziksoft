using eshopgloziksoft.lib.Models;
using System.Collections;
using System.Collections.Generic;
using System.Web.Security;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;

namespace eshopgloziksoft.lib.Repositories
{
    public class EshopgloziksoftMemberRepository : _BaseRepository
    {
        public const string EshopgloziksoftMemberTypeAlias = "EcommerceUser";
        public const string EshopgloziksoftMemberAdminRole = "EcommerceAdmin";
        public const string EshopgloziksoftMemberCustomerRole = "EcommerceCustomer";

        public List<EshopgloziksoftMember> GetAll(string sortBy = "Name", string sortDir = "ASC")
        {
            List<EshopgloziksoftMember> dataList = new List<EshopgloziksoftMember>();
            EshopgloziksoftMemberRolesInfo rolesInfo = new EshopgloziksoftMemberRolesInfo();

            foreach (IMember member in GetAllMembers(sortBy, sortDir))
            {
                EshopgloziksoftMember dataRec = EshopgloziksoftMember.CreateCopyFrom(member);
                dataRec.IsAdminUser = rolesInfo.IsAdmin(this.MemberService, member);
                dataRec.IsCustomerUser = rolesInfo.IsCustomer(this.MemberService, member);
                dataList.Add(dataRec);
            }

            return dataList;
        }

        public List<EshopgloziksoftMember> GetCustomerUsers(string sortBy = "Name", string sortDir = "ASC")
        {
            List<EshopgloziksoftMember> dataList = new List<EshopgloziksoftMember>();
            EshopgloziksoftMemberRolesInfo rolesInfo = new EshopgloziksoftMemberRolesInfo();

            foreach (IMember member in GetAllMembers(sortBy, sortDir))
            {
                EshopgloziksoftMember dataRec = EshopgloziksoftMember.CreateCopyFrom(member);
                if (rolesInfo.IsCustomer(this.MemberService, member))
                {
                    dataList.Add(dataRec);
                }
            }

            return dataList;
        }

        IEnumerable<IMember> GetAllMembers(string sortBy = "Name", string sortDir = "ASC")
        {
            long totalRecords;

            return this.MemberService.GetAll(0, _PagingModel.AllItemsPerPage, out totalRecords, sortBy, sortDir == "DESC" ? Direction.Descending : Direction.Ascending, EshopgloziksoftMemberRepository.EshopgloziksoftMemberTypeAlias);
        }

        public EshopgloziksoftMember Get(int id)
        {
            return CreateCopyFrom(this.MemberService.GetById(id));
        }

        public EshopgloziksoftMember Get(string id)
        {
            return Get(int.Parse(id));
        }

        public EshopgloziksoftMember GetMemberByEmail(string email)
        {
            IMember member = this.MemberService.GetByEmail(email);
            return member == null ? null : CreateCopyFrom(member);
        }

        EshopgloziksoftMember CreateCopyFrom(IMember imember)
        {
            EshopgloziksoftMember member = EshopgloziksoftMember.CreateCopyFrom(imember);

            member.IsAdminUser = System.Web.Security.Roles.IsUserInRole(member.Username, EshopgloziksoftMemberRepository.EshopgloziksoftMemberAdminRole);
            member.IsCustomerUser = System.Web.Security.Roles.IsUserInRole(member.Username, EshopgloziksoftMemberRepository.EshopgloziksoftMemberCustomerRole);

            return member;

        }
        public MembershipCreateStatus Save(PluginController ctrl, EshopgloziksoftMember member, bool updatePermissions = true)
        {
            if (member.IsNew)
            {
                return Insert(ctrl, member);
            }
            else
            {
                return Update(member, updatePermissions);
            }
        }

        public MembershipCreateStatus Insert(PluginController ctrl, EshopgloziksoftMember member)
        {
            if (this.MemberService.GetById(member.MemberId) != null)
            {
                return MembershipCreateStatus.DuplicateProviderUserKey;
            }

            var registerModel = ctrl.Members.CreateRegistrationModel(EshopgloziksoftMemberRepository.EshopgloziksoftMemberTypeAlias);
            registerModel.Name = member.Name;
            registerModel.Email = member.Email;
            registerModel.Password = member.Password;
            registerModel.Username = member.Email;
            registerModel.UsernameIsEmail = true;

            MembershipCreateStatus status;
            var newMember = ctrl.Members.RegisterMember(registerModel, out status, false);

            if (status == MembershipCreateStatus.Success)
            {
                // Assign user roles
                if (member.IsAdminUser)
                {
                    System.Web.Security.Roles.AddUserToRole(member.Username, EshopgloziksoftMemberRepository.EshopgloziksoftMemberAdminRole);
                }
                if (member.IsCustomerUser)
                {
                    System.Web.Security.Roles.AddUserToRole(member.Username, EshopgloziksoftMemberRepository.EshopgloziksoftMemberCustomerRole);
                }
            }

            return status;
            ;
        }

        public MembershipCreateStatus Update(EshopgloziksoftMember member, bool updatePermissions)
        {
            IMember updateMember = this.MemberService.GetById(member.MemberId);
            if (updateMember == null)
            {
                return MembershipCreateStatus.UserRejected;
            }

            bool wasChange = false;

            if (updateMember.Name != member.Name)
            {
                updateMember.Name = member.Name;
                wasChange = true;
            }
            if (updateMember.Email != member.Email)
            {
                updateMember.Username = member.Email;
                updateMember.Email = member.Email;
                IMember checkMember = this.MemberService.GetByEmail(updateMember.Email);
                if (checkMember != null)
                {
                    return MembershipCreateStatus.DuplicateEmail;
                }
                wasChange = true;
            }
            if (updatePermissions)
            {
                if (updateMember.IsApproved != member.IsApproved)
                {
                    updateMember.IsApproved = member.IsApproved;
                    wasChange = true;
                }
                if (updateMember.IsLockedOut != member.IsLockedOut)
                {
                    updateMember.IsLockedOut = member.IsLockedOut;
                    wasChange = true;
                }
            }

            if (wasChange)
            {
                this.MemberService.Save(updateMember);
            }

            if (updatePermissions)
            {
                // Assign user roles
                if (member.IsAdminUser)
                {
                    System.Web.Security.Roles.AddUserToRole(member.Username, EshopgloziksoftMemberRepository.EshopgloziksoftMemberAdminRole);
                }
                else
                {
                    System.Web.Security.Roles.RemoveUserFromRole(member.Username, EshopgloziksoftMemberRepository.EshopgloziksoftMemberAdminRole);
                }
                if (member.IsCustomerUser)
                {
                    System.Web.Security.Roles.AddUserToRole(member.Username, EshopgloziksoftMemberRepository.EshopgloziksoftMemberCustomerRole);
                }
                else
                {
                    System.Web.Security.Roles.RemoveUserFromRole(member.Username, EshopgloziksoftMemberRepository.EshopgloziksoftMemberCustomerRole);
                }
            }

            return MembershipCreateStatus.Success;
        }

        public MembershipCreateStatus SavePassword(EshopgloziksoftMember member)
        {
            IMember updateMember = this.MemberService.GetById(member.MemberId);
            if (updateMember == null)
            {
                return MembershipCreateStatus.UserRejected;
            }

            try
            {
                this.MemberService.SavePassword(updateMember, member.Password);
            }
            catch
            {
                return MembershipCreateStatus.InvalidPassword;
            }

            return MembershipCreateStatus.Success;
        }

        public MembershipCreateStatus Delete(EshopgloziksoftMember member)
        {
            IMember deleteMember = this.MemberService.GetById(member.MemberId);
            if (deleteMember == null)
            {
                return MembershipCreateStatus.UserRejected;
            }
            this.MemberService.Delete(deleteMember);

            return MembershipCreateStatus.Success;
        }

        public string GetErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.Success:
                    return string.Empty;
                case MembershipCreateStatus.DuplicateProviderUserKey:
                    return "Užívateľ už existuje";
                case MembershipCreateStatus.InvalidUserName:
                    return "Neznámy užívateľ";
                case MembershipCreateStatus.InvalidPassword:
                    return "Neplatné heslo. Zadajte heslo aspoň na 8 znakov";
                case MembershipCreateStatus.InvalidQuestion:
                    return "Nesprávna otázka";
                case MembershipCreateStatus.InvalidAnswer:
                    return "Nesprávna odpoveď";
                case MembershipCreateStatus.InvalidEmail:
                    return "Nesprávny email";
                case MembershipCreateStatus.DuplicateUserName:
                case MembershipCreateStatus.DuplicateEmail:
                    return "Užívateľ pre zadaný email už existuje";
                case MembershipCreateStatus.UserRejected:
                case MembershipCreateStatus.InvalidProviderUserKey:
                    return "Neznámy užívateľ";
                case MembershipCreateStatus.ProviderError:
                    return "Neznámy typ chyby";
            }

            return "Neznámy typ chyby";
        }
    }

    public class EshopgloziksoftMember
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }

        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }

        public bool IsAdminUser { get; set; }
        public bool IsCustomerUser { get; set; }

        public bool IsNew
        {
            get
            {
                return this.MemberId <= 0;
            }
        }

        public static EshopgloziksoftMember CreateCopyFrom(IMember member)
        {
            return new EshopgloziksoftMember()
            {
                MemberId = member.Id,
                Name = member.Name,
                Username = member.Username,
                Email = member.Email,
                IsApproved = member.IsApproved,
                IsLockedOut = member.IsLockedOut,
            };
        }
    }

    public class EshopgloziksoftMemberRolesInfo
    {
        Hashtable htAdmin;
        Hashtable htCustomer;

        public bool IsAdmin(IMemberService service, IMember member)
        {
            return IsMemberInRole(service, member, EshopgloziksoftMemberRepository.EshopgloziksoftMemberAdminRole, ref htAdmin);
        }

        public bool IsCustomer(IMemberService service, IMember member)
        {
            return IsMemberInRole(service, member, EshopgloziksoftMemberRepository.EshopgloziksoftMemberCustomerRole, ref htCustomer);
        }

        bool IsMemberInRole(IMemberService service, IMember member, string roleName, ref Hashtable ht)
        {
            if (ht == null)
            {
                ht = LoadRolesInfo(service, roleName);
            }

            return ht.ContainsKey(member.Id);
        }

        Hashtable LoadRolesInfo(IMemberService service, string roleName)
        {
            Hashtable ht = new Hashtable();
            foreach (IMember member in service.GetMembersInRole(roleName))
            {
                ht.Add(member.Id, member);
            }

            return ht;
        }
    }
}
