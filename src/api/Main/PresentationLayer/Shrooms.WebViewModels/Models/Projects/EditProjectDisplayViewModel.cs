﻿using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models.Projects
{
    public class EditProjectDisplayViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public UserViewModel Owner { get; set; }

        public string Logo { get; set; }

        public IEnumerable<UserViewModel> Members { get; set; }

        public IEnumerable<string> Attributes { get; set; }
    }
}
