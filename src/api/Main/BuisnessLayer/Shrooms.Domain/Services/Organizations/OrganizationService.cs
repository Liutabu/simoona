﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using Shrooms.Constants.ErrorCodes;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Users;
using Shrooms.Domain.Services.Roles;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.DomainExceptions.Exceptions.Organization;
using Shrooms.EntityModels.Models;

namespace Shrooms.Domain.Services.Organizations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IRoleService _roleService;
        private readonly IUnitOfWork2 _uow;

        public OrganizationService(IUnitOfWork2 uow, IRoleService roleService)
        {
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _roleService = roleService;
            _uow = uow;
        }

        public Organization GetOrganizationById(int id)
        {
            return _organizationsDbSet.Find(id);
        }

#pragma warning disable S1449 //EF does does not understand Equals(invarient) or ToLowerInvarient()
        public Organization GetOrganizationByName(string organizationName)
        {
            return _organizationsDbSet
                .Single(organization => organization.ShortName.ToLower() == organizationName.ToLower());
        }

        public string GetOrganizationHostName(string organizationName)
        {
            var hostName = _organizationsDbSet
                .Single(organization => organization.ShortName.ToLower() == organizationName.ToLower())
                .HostName;
            return hostName;
        }

        public bool HasOrganizationEmailDomainRestriction(string organizationName)
        {
            var organization = _organizationsDbSet
                .SingleOrDefault(o => o.ShortName.ToLower() == organizationName.ToLower());

            if (organization == null)
            {
                throw new InvalidOrganizationException();
            }

            return organization.HasRestrictedAccess;
        }
#pragma warning restore S1449

        public Organization GetUserOrganization(ApplicationUser user)
        {
            return _usersDbSet
                .Find(user.Id).Organization;
        }

        public bool IsOrganizationHostValid(string email, string requestedOrganizationName)
        {
            if (HasOrganizationEmailDomainRestriction(requestedOrganizationName))
            {
                var validEmailHostName = GetOrganizationHostName(requestedOrganizationName);
                if (GetHostFromEmail(email) != validEmailHostName)
                {
                    return false;
                }
            }

            return true;
        }

        public bool RequiresUserConfirmation(int organizationId)
        {
            return _organizationsDbSet
                .Where(o => o.Id == organizationId)
                .Select(o => o.RequiresUserConfirmation)
                .First();
        }

        public UserDto GetManagingDirector(int organizationId)
        {
            var managingDirector = _usersDbSet
                .Where(x => x.OrganizationId == organizationId &&
                            x.IsManagingDirector)
                .Select(x => new UserDto()
                {
                    UserId = x.Id,
                    FullName = x.FirstName + " " + x.LastName
                })
                .FirstOrDefault();

            return managingDirector;
        }

        public void SetManagingDirector(string userId, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            if (!_roleService.HasRole(userId, Shrooms.Constants.Authorization.Roles.Manager))
            {
                throw new ValidationException(ErrorCodes.UserIsNotAManager, "User need to have manager role to become a managing director");
            }

            var managingDirectors = _usersDbSet
                .Include(x => x.Roles)
                .Where(x => x.OrganizationId == userAndOrganizationDTO.OrganizationId)
                .Where(x => x.IsManagingDirector || x.Id == userId)
                .ToList();

            foreach (var currentDirector in managingDirectors.Where(x => x.IsManagingDirector))
            {
                currentDirector.IsManagingDirector = false;
            }

            var newManagingDirector = managingDirectors.Where(x => x.Id == userId).First();

            newManagingDirector.IsManagingDirector = true;

            _uow.SaveChanges(userAndOrganizationDTO.UserId);
        }

        private static string GetHostFromEmail(string email)
        {
            var mailAddress = new MailAddress(email);
            return mailAddress.Host.ToLowerInvariant();
        }
    }
}
