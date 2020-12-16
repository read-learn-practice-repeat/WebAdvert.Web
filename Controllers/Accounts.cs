using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models.Accounts;

namespace WebAdvert.Web.Controllers
{
    
    public class Accounts : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public Accounts(SignInManager<CognitoUser>signInManager, UserManager<CognitoUser>userManager,CognitoUserPool pool)
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
            this._pool = pool;
        }


        [HttpGet]
        public async Task<ActionResult>SignUp ()
        {
            var model = new SignUpModel();
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> SignUp(SignUpModel model )
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);
                if ( user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User with this email already exists");
                }
                user.Attributes.Add(CognitoAttribute.Name.AttributeName, model.Email);
                var createdUser = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);
                if ( createdUser.Succeeded)
                {
                    return RedirectToAction("confirm");
                }

            }
            return View(model);
        }
        [HttpGet]
        public async Task<ActionResult> Confirm(ConfirmModel model)
        {
            return View(model);
        }
        [HttpPost]
        [ActionName("Confirm")]
        public async Task<ActionResult> Confirm_Post(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if ( user == null)
                {
                    ModelState.AddModelError("NotFound","A user with the given email address was not found");
                    return View(model);
                }
                var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, model.Code,true);
                //var result = await _userManager.ConfirmEmailAsync(user, model.Code);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return View(model);
                }
            }

            return View(model);
            

        }
        [HttpGet]
        public async Task<ActionResult> Login(LoginModel model)
        {
            return View(model);
        }
        [HttpPost]
        [ActionName("Login")]
        public async Task<ActionResult> LoginPost(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email,
                    model.Password, model.RememberMe=false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                   ModelState.AddModelError("LoginError","Email and Password do not match");
                  
                }
            }

            return View("Login",model);
        }
    }
}

