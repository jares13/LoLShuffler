using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoLShuffler.Pages
{
    public class TeamShufflerStart : PageModel
    {
        public bool IsTeamAlreadyExist { get; set; }
        public bool IsTeamDoesNotExist { get; set; }
        public bool IsKeyNotValid { get; set; }

        public void OnGet(bool isTeamAlreadyExist, bool isTeamDoesNotExist, bool isKeyNotValid)
        {
            IsTeamAlreadyExist = isTeamAlreadyExist;
            IsTeamDoesNotExist = isTeamDoesNotExist;
            IsKeyNotValid = isKeyNotValid;
        }
    }
}