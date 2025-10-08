// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MyStore.Models;

namespace MyStore.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        // === التحسين الأمني: تعطيل وظائف التسجيل بالكامل ===

        /// <summary>
        /// عند محاولة الوصول لصفحة التسجيل، يتم إعادة التوجيه فورًا.
        /// </summary>
        public IActionResult OnGet(string returnUrl = null)
        {
            // Registration is disabled. Redirect any GET request to the login page.
            return RedirectToPage("./Login");
        }

        /// <summary>
        /// عند محاولة إرسال بيانات لصفحة التسجيل، يتم إعادة التوجيه فورًا.
        /// </summary>
        public IActionResult OnPost(string returnUrl = null)
        {
            // Registration is disabled. Redirect any POST request to the login page.
            return RedirectToPage("./Login");
        }
    }
}