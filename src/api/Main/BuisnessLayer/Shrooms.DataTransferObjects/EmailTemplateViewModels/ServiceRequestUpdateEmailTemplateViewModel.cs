﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.EmailTemplateViewModels
{
    public class ServiceRequestUpdateEmailTemplateViewModel : ServiceRequestEmailTemplateViewModel
    {
        public string StatusName { get; set; }
        public ServiceRequestUpdateEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string serviceRequestTitle,
            string fullName,
            string statusName,
            string url)
            : base(userNotificationSettingsUrl, serviceRequestTitle, fullName, url)
        {
            this.StatusName = statusName;
        }
    }
}
